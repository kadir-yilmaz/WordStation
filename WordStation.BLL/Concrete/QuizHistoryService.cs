using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Dtos;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class QuizHistoryService : IQuizHistoryService
    {
        private readonly IQuizHistoryRepository _quizHistoryRepository;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public QuizHistoryService(IQuizHistoryRepository quizHistoryRepository)
        {
            _quizHistoryRepository = quizHistoryRepository;
        }

        public async Task<List<QuizHistoryDto>> GetHistoryAsync(string userId, bool? isDailyQuiz = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<QuizHistoryDto>();

            var entities = await _quizHistoryRepository.GetHistoryByUserIdAsync(userId, isDailyQuiz, limit: 50, trackChanges: false);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<QuizHistoryDto> SaveHistoryAsync(CreateQuizHistoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                throw new ArgumentException("UserId zorunludur.", nameof(dto.UserId));

            string resultsJson = dto.ResultsJson ?? "[]";
            if (dto.Results != null && dto.Results.Any())
            {
                resultsJson = JsonSerializer.Serialize(dto.Results, JsonOptions);
            }

            var entity = new QuizHistory
            {
                UserId = dto.UserId,
                Date = dto.Date ?? DateTime.UtcNow,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? "Genel Test" : dto.Title,
                Score = dto.Score,
                MaxScore = dto.MaxScore,
                TotalQuestions = dto.TotalQuestions,
                CorrectCount = dto.CorrectCount,
                WrongCount = dto.WrongCount,
                IsDailyQuiz = dto.IsDailyQuiz,
                ResultsJson = resultsJson,
                CreatedAt = DateTime.UtcNow
            };

            _quizHistoryRepository.CreateHistory(entity);
            await _quizHistoryRepository.SaveAsync();

            return MapToDto(entity);
        }

        public async Task<bool> ClearHistoryAsync(string userId, bool? isDailyQuiz = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            await _quizHistoryRepository.DeleteHistoryAsync(userId, isDailyQuiz);
            await _quizHistoryRepository.SaveAsync();
            return true;
        }

        private static QuizHistoryDto MapToDto(QuizHistory entity)
        {
            List<QuizQuestionResultDto> results;
            try
            {
                results = JsonSerializer.Deserialize<List<QuizQuestionResultDto>>(entity.ResultsJson, JsonOptions)
                    ?? new List<QuizQuestionResultDto>();
            }
            catch
            {
                results = new List<QuizQuestionResultDto>();
            }

            return new QuizHistoryDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Date = entity.Date,
                Title = entity.Title,
                Score = entity.Score,
                MaxScore = entity.MaxScore,
                TotalQuestions = entity.TotalQuestions,
                CorrectCount = entity.CorrectCount,
                WrongCount = entity.WrongCount,
                IsDailyQuiz = entity.IsDailyQuiz,
                Results = results,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
