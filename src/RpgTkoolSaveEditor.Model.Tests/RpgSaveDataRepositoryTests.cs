using Microsoft.Extensions.Logging;
using Moq;
using RpgTkoolSaveEditor.Model.GameDatas;
using RpgTkoolSaveEditor.Model.SaveDatas;
using System.Collections.Immutable;

namespace RpgTkoolSaveEditor.Model.Tests;

public class RpgSaveDataRepositoryTests
{
    private readonly Mock<ILogger<RpgSaveDataRepository>> loggerMock_ = new();

    [Fact]
    public async Task LoadAsync_存在しないパス_InvalidOperationExceptionがスローされる()
    {
        // Arrange
        var repository = new RpgSaveDataRepository(loggerMock_.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.LoadAsync("nonexistent/path"));
    }

    [Fact]
    public async Task SaveAsync_存在しないパス_InvalidOperationExceptionがスローされる()
    {
        // Arrange
        var repository = new RpgSaveDataRepository(loggerMock_.Object);
        var saveData = new SaveData(
            ImmutableList<Switch>.Empty, 
            ImmutableList<Variable>.Empty, 
            0, 
            ImmutableList<Item>.Empty, 
            ImmutableList<Weapon>.Empty, 
            ImmutableList<Armor>.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.SaveAsync(saveData, "nonexistent/path"));
    }

    [Fact]
    public void Repository_インスタンス化_正常に作成される()
    {
        // Arrange & Act
        var repository = new RpgSaveDataRepository(loggerMock_.Object);

        // Assert
        Assert.NotNull(repository);
    }
}