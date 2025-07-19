using Microsoft.Extensions.Logging;
using Moq;
using RpgTkoolSaveEditor.Model.SaveDatas;

namespace RpgTkoolSaveEditor.Model.Tests;

public class RmmzSaveDataRepositoryTests
{
    private readonly Mock<ILogger<RmmzSaveDataRepository>> loggerMock_ = new();

    [Fact]
    public async Task LoadTest()
    {
        // Arrange
        var repo = new RmmzSaveDataRepository(loggerMock_.Object);

        // Act
        var saveData = await repo.LoadAsync("testdata/save");

        // Assert
        Assert.NotNull(saveData);
    }

    [Fact]
    public async Task SaveTest()
    {
        // Arrange
        var repo = new RmmzSaveDataRepository(loggerMock_.Object);
        var saveData = await repo.LoadAsync("testdata/save");

        // Act
        await repo.SaveAsync(saveData, "testdata/save");

        // Assert
        var savedData = await repo.LoadAsync("testdata/save");
        Assert.NotNull(savedData);
    }
}
