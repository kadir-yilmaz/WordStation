using Moq;
using WordStation.BLL.Concrete;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;

namespace WordStation.Tests
{
    public class WordServiceTests
    {
        private readonly Mock<IWordRepository> _mockRepo;
        private readonly WordService _wordService;

        public WordServiceTests()
        {
            _mockRepo = new Mock<IWordRepository>();
            _wordService = new WordService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateWordAsync_ShouldCallRepositoryMethods()
        {
            // Arrange
            var word = new Word { En = "Test", Tr = "Deneme", UserId = "1", ListName = "Default" };

            // Act
            await _wordService.CreateWordAsync(word);

            // Assert
            _mockRepo.Verify(x => x.CreateWord(word), Times.Once);
            _mockRepo.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchWordAsync_ShouldReturnWordsFromRepository()
        {
            // Arrange
            var userId = "user123";
            var listName = "General";
            var searchTerm = "ap";
            var mockData = new List<Word>
            {
                new Word { En = "Apple", UserId = userId, ListName = listName },
                new Word { En = "Application", UserId = userId, ListName = listName }
            };

            // Repository'den dönecek veriyi ayarlıyoruz
            _mockRepo.Setup(x => x.GetWordsByConditionAsync(
                    It.IsAny<Expression<Func<Word, bool>>>(), 
                    It.IsAny<bool>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _wordService.SearchWordAsync(searchTerm, userId, listName, "starts");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockRepo.Verify(x => x.GetWordsByConditionAsync(It.IsAny<Expression<Func<Word, bool>>>(), false), Times.Once);
        }

        [Fact]
        public async Task SearchWordAsync_Contains_ShouldCallRepositoryWithCorrectMode()
        {
            // Arrange
            var userId = "user123";
            var listName = "General";
            var searchTerm = "pp"; // "apple" içinde geçer
            
            _mockRepo.Setup(x => x.GetWordsByConditionAsync(
                    It.IsAny<Expression<Func<Word, bool>>>(), 
                    It.IsAny<bool>()))
                .ReturnsAsync(new List<Word>());

            // Act
            await _wordService.SearchWordAsync(searchTerm, userId, listName, "contains");

            // Assert
            _mockRepo.Verify(x => x.GetWordsByConditionAsync(It.IsAny<Expression<Func<Word, bool>>>(), false), Times.Once);
        }

        [Fact]
        public void FailingTest_ShouldFail()
        {
            // Bilerek hata alması için: 1 asıl 2'ye eşit mi?
            Assert.Equal(1, 2);
        }
    }
}
