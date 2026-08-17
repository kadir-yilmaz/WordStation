using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using WordStation.BLL.Concrete;
using WordStation.DAL.Abstract;
using WordStation.EL.Dtos;
using WordStation.EL.Models;
using Xunit;

namespace WordStation.Tests
{
    public class QuizHistoryServiceTests
    {
        private readonly Mock<IQuizHistoryRepository> _mockRepo;
        private readonly QuizHistoryService _service;

        public QuizHistoryServiceTests()
        {
            _mockRepo = new Mock<IQuizHistoryRepository>();
            _service = new QuizHistoryService(_mockRepo.Object);
        }

        [Fact]
        public async Task SaveHistoryAsync_ValidDto_SavesAndReturnsDto()
        {
            // Arrange
            var dto = new CreateQuizHistoryDto
            {
                UserId = "user@test.com",
                Date = DateTime.UtcNow,
                Title = "Test Quiz",
                Score = 80,
                MaxScore = 100,
                TotalQuestions = 10,
                CorrectCount = 8,
                WrongCount = 2,
                IsDailyQuiz = true,
                Results = new List<QuizQuestionResultDto>
                {
                    new()
                    {
                        Word = new QuizQuestionWordDto { Id = 1, En = "apple", Tr = "elma" },
                        QuestionText = "apple",
                        CorrectAnswer = "elma",
                        SelectedAnswer = "elma",
                        IsCorrect = true
                    }
                }
            };

            QuizHistory? capturedEntity = null;
            _mockRepo.Setup(r => r.CreateHistory(It.IsAny<QuizHistory>()))
                     .Callback<QuizHistory>(e => capturedEntity = e);
            _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SaveHistoryAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user@test.com", result.UserId);
            Assert.Equal(80, result.Score);
            Assert.Equal(10, result.TotalQuestions);
            Assert.True(result.IsDailyQuiz);
            Assert.Single(result.Results);
            Assert.Equal("apple", result.Results[0].QuestionText);
            _mockRepo.Verify(r => r.CreateHistory(It.IsAny<QuizHistory>()), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetHistoryAsync_ReturnsUserHistory()
        {
            // Arrange
            var list = new List<QuizHistory>
            {
                new()
                {
                    Id = 1,
                    UserId = "user@test.com",
                    Date = DateTime.UtcNow,
                    Title = "Genel Test",
                    Score = 50,
                    MaxScore = 50,
                    TotalQuestions = 5,
                    CorrectCount = 5,
                    WrongCount = 0,
                    IsDailyQuiz = false,
                    ResultsJson = "[]"
                }
            };

            _mockRepo.Setup(r => r.GetHistoryByUserIdAsync("user@test.com", false, 50, false))
                     .ReturnsAsync(list);

            // Act
            var result = await _service.GetHistoryAsync("user@test.com", isDailyQuiz: false);

            // Assert
            Assert.Single(result);
            Assert.Equal("Genel Test", result[0].Title);
            Assert.False(result[0].IsDailyQuiz);
        }

        [Fact]
        public async Task ClearHistoryAsync_DeletesUserHistory()
        {
            // Arrange
            _mockRepo.Setup(r => r.DeleteHistoryAsync("user@test.com", null))
                     .Returns(Task.CompletedTask);
            _mockRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ClearHistoryAsync("user@test.com", null);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.DeleteHistoryAsync("user@test.com", null), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveHistoryAsync_MissingUserId_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateQuizHistoryDto
            {
                UserId = "",
                Title = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveHistoryAsync(dto));
        }
    }
}
