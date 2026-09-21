using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using WordStation.BLL.Concrete;
using WordStation.DAL.Abstract;
using WordStation.EL.Dtos;
using WordStation.EL.Models;
using Xunit;

namespace WordStation.Tests
{
    public class DailyQuizServiceTests
    {
        private readonly Mock<IDailyQuizRepository> _mockDailyQuizRepo;
        private readonly Mock<IWordRepository> _mockWordRepo;
        private readonly DailyQuizService _dailyQuizService;

        public DailyQuizServiceTests()
        {
            _mockDailyQuizRepo = new Mock<IDailyQuizRepository>();
            _mockWordRepo = new Mock<IWordRepository>();
            _dailyQuizService = new DailyQuizService(_mockDailyQuizRepo.Object, _mockWordRepo.Object);
        }

        [Fact]
        public async Task CreateOrResetPlanAsync_WhenNoPlanExists_ShouldCreateNewPlan()
        {
            // Arrange
            var dto = new CreateDailyQuizPlanDto
            {
                UserId = "user123",
                ListName = "B2 Words",
                DailyCount = 10,
                IsEnglishToTurkish = true,
            };

            var userWords = new List<Word>
            {
                new Word { Id = 1, En = "abate", Tr = "azalmak", UserId = "user123" },
                new Word { Id = 2, En = "beacon", Tr = "fener", UserId = "user123" },
                new Word { Id = 3, En = "candor", Tr = "açıksözlülük", UserId = "user123" },
                new Word { Id = 4, En = "debate", Tr = "tartışma", UserId = "user123" },
                new Word { Id = 5, En = "eager", Tr = "istekli", UserId = "user123" }
            };

            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("user123", true))
                .ReturnsAsync((DailyQuizPlan?)null);
            _mockWordRepo.Setup(w => w.GetWordsByConditionAsync(It.IsAny<Expression<Func<Word, bool>>>(), false))
                .ReturnsAsync(userWords);

            // Act
            var result = await _dailyQuizService.CreateOrResetPlanAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user123", result.UserId);
            Assert.Equal("B2 Words", result.ListName);
            Assert.Equal(10, result.DailyCount);
            Assert.Equal(5, result.ShuffledWordIds.Count);
            Assert.Equal(0, result.CurrentPointer);
            Assert.Equal(0, result.StreakDays);
            Assert.Null(result.LastCompletedDate);

            _mockDailyQuizRepo.Verify(r => r.CreatePlan(It.IsAny<DailyQuizPlan>()), Times.Once);
            _mockDailyQuizRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrResetPlanAsync_WhenPlanExists_ShouldResetExistingPlan()
        {
            // Arrange
            var existingPlan = new DailyQuizPlan
            {
                Id = 5,
                UserId = "user456",
                ListName = "Tümü",
                DailyCount = 10,
                ShuffledWordIdsJson = "[1,2,3]",
                CurrentPointer = 3,
                StreakDays = 5,
                LastCompletedDate = "2026-09-20"
            };

            var dto = new CreateDailyQuizPlanDto
            {
                UserId = "user456",
                ListName = "Tümü",
                DailyCount = 5,
                IsEnglishToTurkish = true,
            };

            var userWords = new List<Word>
            {
                new Word { Id = 10, En = "abate", Tr = "azalmak", UserId = "user456" },
                new Word { Id = 20, En = "beacon", Tr = "fener", UserId = "user456" },
                new Word { Id = 30, En = "candor", Tr = "açıksözlülük", UserId = "user456" }
            };

            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("user456", true))
                .ReturnsAsync(existingPlan);
            _mockWordRepo.Setup(w => w.GetWordsByConditionAsync(It.IsAny<Expression<Func<Word, bool>>>(), false))
                .ReturnsAsync(userWords);

            // Act
            var result = await _dailyQuizService.CreateOrResetPlanAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user456", result.UserId);
            Assert.Equal(3, result.ShuffledWordIds.Count);
            Assert.Equal(0, result.CurrentPointer);
            Assert.Equal(0, result.StreakDays);
            Assert.Null(result.LastCompletedDate);

            _mockDailyQuizRepo.Verify(r => r.DeletePlan(existingPlan), Times.Once);
            _mockDailyQuizRepo.Verify(r => r.CreatePlan(It.IsAny<DailyQuizPlan>()), Times.Once);
            _mockDailyQuizRepo.Verify(r => r.SaveAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateProgressAsync_ShouldUpdatePointerAndStreak()
        {
            // Arrange
            var existingPlan = new DailyQuizPlan
            {
                Id = 1,
                UserId = "user789",
                ListName = "Tümü",
                DailyCount = 10,
                ShuffledWordIdsJson = "[1,2,3,4,5,6,7,8,9,10,11,12]",
                CurrentPointer = 0,
                StreakDays = 0,
                LastCompletedDate = null
            };

            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("user789", true))
                .ReturnsAsync(existingPlan);

            var updateDto = new UpdateDailyQuizProgressDto
            {
                UserId = "user789",
                NewPointer = 10,
                LastCompletedDate = "2026-08-17",
                StreakDays = 1
            };

            // Act
            var result = await _dailyQuizService.UpdateProgressAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.CurrentPointer);
            Assert.Equal("2026-08-17", result.LastCompletedDate);
            Assert.Equal(1, result.StreakDays);

            _mockDailyQuizRepo.Verify(r => r.UpdatePlan(existingPlan), Times.Once);
            _mockDailyQuizRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPlanByUserIdAsync_WhenPlanExists_ShouldReturnMappedDto()
        {
            // Arrange
            var plan = new DailyQuizPlan
            {
                Id = 5,
                UserId = "user100",
                ListName = "General",
                DailyCount = 10,
                ShuffledWordIdsJson = "[10, 20, 30]",
                CurrentPointer = 3,
                StreakDays = 2,
                LastCompletedDate = "2026-08-16"
            };

            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("user100", false))
                .ReturnsAsync(plan);

            // Act
            var result = await _dailyQuizService.GetPlanByUserIdAsync("user100");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("user100", result.UserId);
            Assert.Equal(3, result.ShuffledWordIds.Count);
            Assert.Equal(3, result.CurrentPointer);
            Assert.Equal(2, result.StreakDays);
            Assert.Equal("2026-08-16", result.LastCompletedDate);
        }

        [Fact]
        public async Task DeletePlanAsync_WhenPlanExists_ShouldRemoveAndReturnTrue()
        {
            // Arrange
            var plan = new DailyQuizPlan { Id = 1, UserId = "userDel" };
            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("userDel", true))
                .ReturnsAsync(plan);

            // Act
            var result = await _dailyQuizService.DeletePlanAsync("userDel");

            // Assert
            Assert.True(result);
            _mockDailyQuizRepo.Verify(r => r.DeletePlan(plan), Times.Once);
            _mockDailyQuizRepo.Verify(r => r.SaveAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetDayHistoriesAsync_ShouldReturnDayHistoriesForActivePlan()
        {
            // Arrange
            var plan = new DailyQuizPlan { Id = 10, UserId = "userDays" };
            var histories = new List<DailyPlanDayHistory>
            {
                new DailyPlanDayHistory
                {
                    Id = 1,
                    DailyQuizPlanId = 10,
                    UserId = "userDays",
                    DayNumber = 2,
                    TotalQuestions = 10,
                    CorrectCount = 8,
                    WrongCount = 2,
                    Score = 80,
                    MaxScore = 100
                },
                new DailyPlanDayHistory
                {
                    Id = 2,
                    DailyQuizPlanId = 10,
                    UserId = "userDays",
                    DayNumber = 1,
                    TotalQuestions = 10,
                    CorrectCount = 7,
                    WrongCount = 3,
                    Score = 70,
                    MaxScore = 100
                }
            };

            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("userDays", false))
                .ReturnsAsync(plan);
            _mockDailyQuizRepo.Setup(r => r.GetDayHistoriesByPlanIdAsync(10))
                .ReturnsAsync(histories);

            // Act
            var result = await _dailyQuizService.GetDayHistoriesAsync("userDays");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].DayNumber);
            Assert.Equal(8, result[0].CorrectCount);
        }

        [Fact]
        public async Task SaveDayHistoryAsync_WhenPlanExists_ShouldAddAndReturnDto()
        {
            // Arrange
            var plan = new DailyQuizPlan { Id = 20, UserId = "userSave" };
            var dto = new SaveDailyPlanDayDto
            {
                DayNumber = 1,
                TotalQuestions = 10,
                CorrectCount = 9,
                WrongCount = 1,
                Score = 90,
                MaxScore = 100,
                ResultsJson = "[{\"question\":\"test\"}]"
            };

            _mockDailyQuizRepo.Setup(r => r.GetPlanByUserIdAsync("userSave", false))
                .ReturnsAsync(plan);

            // Act
            var result = await _dailyQuizService.SaveDayHistoryAsync("userSave", dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.DailyQuizPlanId);
            Assert.Equal(1, result.DayNumber);
            Assert.Equal(9, result.CorrectCount);
            Assert.Equal("userSave", result.UserId);

            _mockDailyQuizRepo.Verify(r => r.AddDayHistory(It.IsAny<DailyPlanDayHistory>()), Times.Once);
            _mockDailyQuizRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
