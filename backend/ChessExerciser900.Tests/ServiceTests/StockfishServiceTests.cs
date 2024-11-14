using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Stockfish.NET;
using System.Threading.Tasks;
using Xunit;
using CHESSPROJ.Services;

namespace ChessExerciser900.Tests.ServiceTests
{
public class StockfishServiceTests
    {
        private readonly Mock<IStockfish> _mockStockfish;
        private readonly StockfishService _service;

        public StockfishServiceTests()
        {
            _mockStockfish = new Mock<IStockfish>();
            _service = new StockfishService(_mockStockfish.Object);
        }

        // 1. Test SetLevelAsync method
        [Fact]
        public async Task SetLevelAsync_ShouldSetSkillLevel()
        {
            // Arrange
            int skillLevel = 10;

            // Use Setup to mock property assignment
            _mockStockfish.SetupSet(s => s.SkillLevel = skillLevel).Verifiable();
            _mockStockfish.SetupSet(s => s.Depth = 3).Verifiable();

            // Act
            await _service.SetLevelAsync(skillLevel);

            // Assert
            // Verify that the SkillLevel and Depth properties were set
            _mockStockfish.Verify();
        }

        // 2. Test SetPositionAsync method
        [Fact]
        public async Task SetPositionAsync_ShouldSetPosition()
        {
            // Arrange
            string movesMade = "e2e4";
            string move = "e7e5";

            // Act
            await _service.SetPositionAsync(movesMade, move);

            // Assert
            _mockStockfish.Verify(s => s.SetPosition(movesMade, move), Times.Once);
        }

        // 3. Test GetFenAsync method - success case
        [Fact]
        public async Task GetFenAsync_ShouldReturnFenPosition()
        {
            // Arrange
            var expectedFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
            _mockStockfish.Setup(s => s.GetFenPosition()).Returns(expectedFen);

            // Act
            var result = await _service.GetFenAsync();

            // Assert
            Assert.Equal(expectedFen, result);
        }

        // 4. Test GetFenAsync method - exception case
        [Fact]
        public async Task GetFenAsync_ShouldReturnErrorMessage_WhenExceptionOccurs()
        {
            // Arrange
            _mockStockfish.Setup(s => s.GetFenPosition()).Throws(new System.Exception("Error"));

            // Act
            var result = await _service.GetFenAsync();

            // Assert
            Assert.Equal("Error getting the FEN", result);
        }

        // 5. Test GetBestMoveAsync method
        [Fact]
        public async Task GetBestMoveAsync_ShouldReturnBestMove()
        {
            // Arrange
            string bestMove = "e7e5";
            _mockStockfish.Setup(s => s.GetBestMove()).Returns(bestMove);

            // Act
            var result = await _service.GetBestMoveAsync();

            // Assert
            Assert.Equal(bestMove, result);
        }

        // 6. Test IsMoveCorrectAsync method - valid move
        [Fact]
        public async Task IsMoveCorrectAsync_ShouldReturnTrue_WhenMoveIsValid()
        {
            // Arrange
            string currentPosition = "e2e4";
            string move = "e7e5";
            _mockStockfish.Setup(s => s.SetPosition(currentPosition));
            _mockStockfish.Setup(s => s.IsMoveCorrect(move)).Returns(true);

            // Act
            var result = await _service.IsMoveCorrectAsync(currentPosition, move);

            // Assert
            Assert.True(result);
        }

        // 7. Test IsMoveCorrectAsync method - invalid move
        [Fact]
        public async Task IsMoveCorrectAsync_ShouldReturnFalse_WhenMoveIsInvalid()
        {
            // Arrange
            string currentPosition = "e2e4";
            string move = "e7e6";
            _mockStockfish.Setup(s => s.SetPosition(currentPosition));
            _mockStockfish.Setup(s => s.IsMoveCorrect(move)).Returns(false);

            // Act
            var result = await _service.IsMoveCorrectAsync(currentPosition, move);

            // Assert
            Assert.False(result);
        }

        // 8. Test IsCheckAsync method
        [Fact]
        public async Task IsCheckAsync_ShouldReturnTrue_WhenInCheck()
        {
            // Arrange
            _mockStockfish.Setup(s => s.IsCheck()).Returns(true);

            // Act
            var result = await _service.IsCheckAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsCheckAsync_ShouldReturnFalse_WhenNotInCheck()
        {
            // Arrange
            _mockStockfish.Setup(s => s.IsCheck()).Returns(false);

            // Act
            var result = await _service.IsCheckAsync();

            // Assert
            Assert.False(result);
        }
    }
}