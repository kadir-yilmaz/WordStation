using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Dtos;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class StudyPlanService : IStudyPlanService
    {
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IWordRepository _wordRepository;
        private readonly Random _random = new();

        public StudyPlanService(
            IStudyPlanRepository studyPlanRepository,
            IWordRepository wordRepository)
        {
            _studyPlanRepository = studyPlanRepository;
            _wordRepository = wordRepository;
        }

        // ──────────────────────────────────────────────────────────────
        // Plan CRUD
        // ──────────────────────────────────────────────────────────────

        public async Task<List<StudyPlanSummaryDto>> GetUserPlansAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<StudyPlanSummaryDto>();

            var plans = await _studyPlanRepository.GetPlansByUserIdAsync(userId);
            return plans.Select(MapToSummaryDto).ToList();
        }

        public async Task<StudyPlanDetailDto?> GetPlanDetailAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var plan = await _studyPlanRepository.GetPlanByIdAsync(planId);
            if (plan == null || plan.UserId != userId)
                return null;

            var summary = MapToSummaryDto(plan);
            return new StudyPlanDetailDto
            {
                Id = summary.Id,
                Name = summary.Name,
                DailyWordLimit = summary.DailyWordLimit,
                TotalDays = summary.TotalDays,
                CompletedDays = summary.CompletedDays,
                TotalWords = summary.TotalWords,
                IsActive = summary.IsActive,
                Progress = summary.Progress,
                IncludedListNames = summary.IncludedListNames,
                CreatedAt = summary.CreatedAt,
                Days = plan.Days.OrderBy(d => d.DayNumber).Select(MapToDayDto).ToList()
            };
        }

        public async Task<StudyPlanSummaryDto> CreatePlanAsync(CreateStudyPlanDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                throw new ArgumentException("UserId zorunludur.", nameof(dto.UserId));

            if (dto.ListNames == null || !dto.ListNames.Any())
                throw new ArgumentException("En az bir liste seçilmelidir.", nameof(dto.ListNames));

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                dto.Name = string.Join(", ", dto.ListNames) + " Planı";
            }

            if (dto.DailyWordLimit < 1)
                throw new ArgumentException("Günlük kelime sınırı en az 1 olmalıdır.", nameof(dto.DailyWordLimit));

            // 1. Seçilen listelerdeki tüm kelimeleri çek
            var allWords = new List<Word>();
            foreach (var listName in dto.ListNames)
            {
                var trimmed = listName.Trim();
                var words = await _wordRepository.GetWordsByConditionAsync(
                    w => (w.UserId == dto.UserId || string.IsNullOrEmpty(w.UserId)) &&
                         (w.ListName == trimmed || w.ListName.ToLower() == trimmed.ToLower()),
                    trackChanges: false);
                allWords.AddRange(words);
            }

            if (!allWords.Any())
                throw new InvalidOperationException("Seçilen listelerde kelime bulunamadı.");

            // Tekrar eden kelimeleri kaldır (farklı listelerde aynı kelime varsa)
            allWords = allWords
                .GroupBy(w => w.Id)
                .Select(g => g.First())
                .ToList();

            // 2. Kelimeleri karıştır (Fisher-Yates shuffle)
            ShuffleList(allWords);

            // 3. Günlere böl
            var dailyLimit = dto.DailyWordLimit;
            var totalDays = (int)Math.Ceiling((double)allWords.Count / dailyLimit);

            var plan = new StudyPlan
            {
                UserId = dto.UserId,
                Name = dto.Name.Trim(),
                DailyWordLimit = dailyLimit,
                TotalDays = totalDays,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IncludedLists = dto.ListNames.Select(ln => new StudyPlanList
                {
                    ListName = ln
                }).ToList(),
                Days = new List<StudyPlanDay>()
            };

            for (int day = 0; day < totalDays; day++)
            {
                var dayWords = allWords
                    .Skip(day * dailyLimit)
                    .Take(dailyLimit)
                    .ToList();

                var planDay = new StudyPlanDay
                {
                    DayNumber = day + 1,
                    IsCompleted = false,
                    Words = dayWords.Select(w => new StudyPlanDayWord
                    {
                        WordId = w.Id,
                        IsCorrectInQuiz = false
                    }).ToList()
                };

                plan.Days.Add(planDay);
            }

            _studyPlanRepository.CreatePlan(plan);
            await _studyPlanRepository.SaveAsync();

            // Kaydedilen planı tekrar çek (navigation'lar ile)
            var saved = await _studyPlanRepository.GetPlanByIdAsync(plan.Id);
            return MapToSummaryDto(saved ?? plan);
        }

        public async Task<bool> DeletePlanAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var plan = await _studyPlanRepository.GetPlanByIdAsync(planId, trackChanges: true);
            if (plan == null || plan.UserId != userId)
                return false;

            _studyPlanRepository.DeletePlan(plan);
            await _studyPlanRepository.SaveAsync();
            return true;
        }

        // ──────────────────────────────────────────────────────────────
        // Gün operasyonları
        // ──────────────────────────────────────────────────────────────

        public async Task<StudyPlanDayDto?> GetCurrentDayAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var plan = await _studyPlanRepository.GetPlanByIdAsync(planId);
            if (plan == null || plan.UserId != userId)
                return null;

            // İlk tamamlanmamış günü bul
            var currentDay = plan.Days
                .Where(d => !d.IsCompleted)
                .OrderBy(d => d.DayNumber)
                .FirstOrDefault();

            if (currentDay == null)
                return null;

            return MapToDayDto(currentDay);
        }

        public async Task<StudyPlanDayDto?> GetDayAsync(int planId, int dayNumber, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            // Hızlı sahiplik kontrolü (bütün planı ve binlerce kelimeyi çekmeden)
            var isOwner = await _studyPlanRepository.IsPlanOwnerAsync(planId, userId);
            if (!isOwner)
                return null;

            var day = await _studyPlanRepository.GetDayAsync(planId, dayNumber);
            return day != null ? MapToDayDto(day) : null;
        }

        public async Task<List<StudyPlanDayDto>> GetAllDaysAsync(int planId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<StudyPlanDayDto>();

            var isOwner = await _studyPlanRepository.IsPlanOwnerAsync(planId, userId);
            if (!isOwner)
                return new List<StudyPlanDayDto>();

            var days = await _studyPlanRepository.GetAllDaysAsync(planId);
            return days.Select(MapToDayDto).ToList();
        }

        // ──────────────────────────────────────────────────────────────
        // Quiz
        // ──────────────────────────────────────────────────────────────

        public async Task<StudyPlanQuizDto> GenerateQuizAsync(int planId, int dayNumber, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId zorunludur.");

            var plan = await _studyPlanRepository.GetPlanByIdAsync(planId);
            if (plan == null || plan.UserId != userId)
                throw new InvalidOperationException("Plan bulunamadı.");

            var day = plan.Days.FirstOrDefault(d => d.DayNumber == dayNumber);
            if (day == null)
                throw new InvalidOperationException($"Gün {dayNumber} bulunamadı.");

            // Quiz şıkları için tüm plandaki kelimeleri havuz olarak kullan
            var allPlanWords = plan.Days
                .SelectMany(d => d.Words)
                .Where(w => w.Word != null)
                .Select(w => w.Word)
                .ToList();

            var questions = new List<StudyPlanQuizQuestionDto>();

            var dayWords = day.Words.Where(w => w.Word != null).ToList();
            ShuffleList(dayWords);

            foreach (var dw in dayWords)
            {
                var word = dw.Word;
                var correctAnswer = word.Tr;
                var questionText = word.En;

                // 3 yanlış şık oluştur
                var wrongOptions = allPlanWords
                    .Where(w => w.Id != word.Id && !string.IsNullOrEmpty(w.Tr) && w.Tr != correctAnswer)
                    .OrderBy(_ => _random.Next())
                    .Take(3)
                    .Select(w => w.Tr)
                    .ToList();

                // Yeterli şık yoksa placeholder ekle
                while (wrongOptions.Count < 3)
                {
                    wrongOptions.Add($"Seçenek {wrongOptions.Count + 1}");
                }

                var options = new List<string> { correctAnswer };
                options.AddRange(wrongOptions);
                ShuffleList(options);

                questions.Add(new StudyPlanQuizQuestionDto
                {
                    WordId = word.Id,
                    QuestionText = questionText,
                    CorrectAnswer = correctAnswer,
                    Options = options
                });
            }

            return new StudyPlanQuizDto
            {
                DayNumber = dayNumber,
                Questions = questions
            };
        }

        public async Task<StudyPlanDayDto> SubmitQuizAsync(SubmitPlanQuizDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                throw new ArgumentException("UserId zorunludur.");

            var plan = await _studyPlanRepository.GetPlanByIdAsync(dto.StudyPlanId, trackChanges: true);
            if (plan == null || plan.UserId != dto.UserId)
                throw new InvalidOperationException("Plan bulunamadı.");

            var day = plan.Days.FirstOrDefault(d => d.DayNumber == dto.DayNumber);
            if (day == null)
                throw new InvalidOperationException($"Gün {dto.DayNumber} bulunamadı.");

            // Cevapları hesapla
            int correctCount = 0;
            int wrongCount = 0;

            foreach (var answer in dto.Answers)
            {
                var dayWord = day.Words.FirstOrDefault(w => w.WordId == answer.WordId);
                if (dayWord != null)
                {
                    if (answer.IsCorrect)
                        correctCount++;
                    else
                        wrongCount++;
                }
            }

            int totalQuestions = day.Words.Count > 0 ? day.Words.Count : (correctCount + wrongCount);
            int percentageScore = totalQuestions > 0 ? (int)Math.Round((double)correctCount / totalQuestions * 100) : 0;

            // Tekrar çözme sonucu eğer daha iyi ise (veya ilk kez çözülüyorsa) skoru yansıt
            bool isBetter = !day.IsCompleted || !day.QuizScore.HasValue || percentageScore > day.QuizScore.Value;

            if (isBetter)
            {
                day.CorrectCount = correctCount;
                day.WrongCount = wrongCount;
                day.QuizScore = percentageScore;
                day.QuizMaxScore = 100;

                foreach (var answer in dto.Answers)
                {
                    var dayWord = day.Words.FirstOrDefault(w => w.WordId == answer.WordId);
                    if (dayWord != null)
                    {
                        dayWord.IsCorrectInQuiz = answer.IsCorrect;
                    }
                }
            }

            day.IsCompleted = true;
            day.CompletedAt = DateTime.UtcNow;

            // Tüm günler tamamlandıysa planı pasif yap
            if (plan.Days.All(d => d.IsCompleted))
            {
                plan.IsActive = false;
            }

            plan.UpdatedAt = DateTime.UtcNow;

            await _studyPlanRepository.SaveAsync();

            // Güncel veriyi döndür
            var updatedDay = await _studyPlanRepository.GetDayAsync(dto.StudyPlanId, dto.DayNumber);
            return MapToDayDto(updatedDay ?? day);
        }

        // ──────────────────────────────────────────────────────────────
        // Private Helpers
        // ──────────────────────────────────────────────────────────────

        private static StudyPlanSummaryDto MapToSummaryDto(StudyPlan plan)
        {
            var completedDays = plan.Days?.Count(d => d.IsCompleted) ?? 0;
            var totalWords = plan.Days?.Sum(d => d.Words?.Count ?? 0) ?? 0;

            return new StudyPlanSummaryDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DailyWordLimit = plan.DailyWordLimit,
                TotalDays = plan.TotalDays,
                CompletedDays = completedDays,
                TotalWords = totalWords,
                IsActive = plan.IsActive,
                Progress = plan.TotalDays > 0 ? (double)completedDays / plan.TotalDays : 0,
                IncludedListNames = plan.IncludedLists?.Select(l => l.ListName).ToList() ?? new List<string>(),
                CreatedAt = plan.CreatedAt
            };
        }

        private static StudyPlanDayDto MapToDayDto(StudyPlanDay day)
        {
            return new StudyPlanDayDto
            {
                Id = day.Id,
                DayNumber = day.DayNumber,
                IsCompleted = day.IsCompleted,
                QuizScore = day.QuizScore.HasValue ? (day.QuizScore.Value > 100 ? (int)Math.Round(day.QuizScore.Value / 2.0) : day.QuizScore.Value) : null,
                QuizMaxScore = day.QuizMaxScore,
                CorrectCount = day.CorrectCount,
                WrongCount = day.WrongCount,
                Words = day.Words?
                    .Where(w => w.Word != null)
                    .Select(w => new StudyPlanWordDto
                    {
                        WordId = w.WordId,
                        En = w.Word.En,
                        Tr = w.Word.Tr,
                        Example = w.Word.Example,
                        IsCorrectInQuiz = w.IsCorrectInQuiz
                    }).ToList() ?? new List<StudyPlanWordDto>()
            };
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
