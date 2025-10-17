using Microsoft.Extensions.Logging;
using Moq;
using RpgTkoolSaveEditor.Model.SaveDatas;

namespace RpgTkoolSaveEditor.Model.Tests;

public class RmmzSaveDataRepositoryTests
{
    private readonly Mock<ILogger<RmmzSaveDataRepository>> loggerMock_ = new();

    [Fact]
    public async Task LoadAsync_正常なセーブデータ_セーブデータが正しく読み込まれる()
    {
        // Arrange
        var repository = new RmmzSaveDataRepository(loggerMock_.Object);

        // Act
        var saveData = await repository.LoadAsync("testdata/save");

        // Assert
        Assert.NotNull(saveData);
        Assert.NotNull(saveData.Switches);
        Assert.NotNull(saveData.Variables);
        Assert.NotNull(saveData.Items);
        Assert.NotNull(saveData.Weapons);
        Assert.NotNull(saveData.Armors);
        Assert.True(saveData.Gold >= 0);
    }

    [Fact]
    public async Task LoadAsync_存在しないパス_InvalidOperationExceptionがスローされる()
    {
        // Arrange
        var repository = new RmmzSaveDataRepository(loggerMock_.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.LoadAsync("nonexistent/path"));
    }

    [Fact]
    public async Task SaveAsync_正常なセーブデータ_正常に保存される()
    {
        // Arrange
        var repository = new RmmzSaveDataRepository(loggerMock_.Object);
        var originalSaveData = await repository.LoadAsync("testdata/save");
        var modifiedSaveData = new SaveData(
            originalSaveData.Switches,
            originalSaveData.Variables,
            originalSaveData.Gold + 1000,
            originalSaveData.Items,
            originalSaveData.Weapons,
            originalSaveData.Armors);

        // Act
        await repository.SaveAsync(modifiedSaveData, "testdata/save");

        // Assert
        var reloadedSaveData = await repository.LoadAsync("testdata/save");
        Assert.Equal(modifiedSaveData.Gold, reloadedSaveData.Gold);
    }

    [Fact]
    public async Task SaveAsync_存在しないパス_InvalidOperationExceptionがスローされる()
    {
        // Arrange
        var repository = new RmmzSaveDataRepository(loggerMock_.Object);
        var saveData = new SaveData(
            [],
            [],
            0,
            [],
            [],
            []);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.SaveAsync(saveData, "nonexistent/path"));
    }

    [Fact]
    public async Task LoadAsync_データ構造の検証_期待される構造でデータが読み込まれる()
    {
        // Arrange
        var repository = new RmmzSaveDataRepository(loggerMock_.Object);

        // Act
        var saveData = await repository.LoadAsync("testdata/save");

        // Assert
        Assert.All(saveData.Switches, x =>
        {
            Assert.True(x.Id >= 0);
            Assert.NotNull(x.Name);
        });

        Assert.All(saveData.Variables, x =>
        {
            Assert.True(x.Id > 0);
            Assert.NotNull(x.Name);
        });

        Assert.All(saveData.Items, x =>
        {
            Assert.True(x.Id > 0);
            Assert.NotNull(x.Name);
            Assert.True(x.Count >= 0);
        });
    }
}
