using System;
using Moq;
using Xunit;
using CHESSPROJ.Services;
using Stockfish.NET;

public class StockfishServiceTests
{
    private readonly Mock<IStockfish> _mockStockfish;
    private readonly StockfishService _stockfishService;

    public StockfishServiceTests()
    {
        _mockStockfish = new Mock<IStockfish>();
        _stockfishService = new StockfishService(_mockStockfish.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenStockfishIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StockfishService(null));
    }

    [Fact]
    public void SetLevel_SetsSkillLevelAndDepth()
    {
        // Arrange
        var level = 5;

        // Act
        _stockfishService.SetLevel(level);

        // Assert
        _mockStockfish.VerifySet(x => x.SkillLevel = level, Times.Once);
        _mockStockfish.VerifySet(x => x.Depth = 1, Times.Once);
    }

    [Fact]
    public void SetPosition_CallsStockfishSetPosition()
    {
        // Arrange
        var movesMade = "e2e4";
        var move = "e7e5";

        // Act
        _stockfishService.SetPosition(movesMade, move);

        // Assert
        _mockStockfish.Verify(x => x.SetPosition(movesMade, move), Times.Once);
    }

    [Fact]
    public void GetFen_ReturnsFenPosition()
    {
        // Arrange
        var expectedFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1";
        _mockStockfish.Setup(x => x.GetFenPosition()).Returns(expectedFen);

        // Act
        var fen = _stockfishService.GetFen();

        // Assert
        Assert.Equal(expectedFen, fen);
    }

    [Fact]
    public void GetFen_ReturnsErrorMessage_WhenExceptionOccurs()
    {
        // Arrange
        _mockStockfish.Setup(x => x.GetFenPosition()).Throws(new Exception("Mocked exception"));

        // Act
        var result = _stockfishService.GetFen();

        // Assert
        Assert.Contains("Error getting the FEN", result);
    }

    [Fact]
    public void GetBestMove_ReturnsBestMove()
    {
        // Arrange
        var bestMove = "e2e4";
        _mockStockfish.Setup(x => x.GetBestMove()).Returns(bestMove);

        // Act
        var result = _stockfishService.GetBestMove();

        // Assert
        Assert.Equal(bestMove, result);
    }

    [Fact]
    public void IsMoveCorrect_ReturnsTrue_WhenMoveIsValid()
    {
        // Arrange
        var position = "e2e4";
        var move = "e7e5";
        _mockStockfish.Setup(x => x.IsMoveCorrect(move)).Returns(true);

        // Act
        var result = _stockfishService.IsMoveCorrect(position, move);

        // Assert
        Assert.True(result);
        _mockStockfish.Verify(x => x.SetPosition(position), Times.Once);
    }

    [Fact]
    public void IsMoveCorrect_ReturnsFalse_WhenMoveIsInvalid()
    {
        // Arrange
        var position = "e2e4";
        var move = "invalid-move";
        _mockStockfish.Setup(x => x.IsMoveCorrect(move)).Returns(false);

        // Act
        var result = _stockfishService.IsMoveCorrect(position, move);

        // Assert
        Assert.False(result);
        _mockStockfish.Verify(x => x.SetPosition(position), Times.Once);
    }


}
