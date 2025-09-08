using Microsoft.Extensions.Logging;
using Moq;
using RpgTkoolSaveEditor.Model.GameDatas;
using RpgTkoolSaveEditor.Model.SaveDatas;
using System.Collections.Immutable;

namespace RpgTkoolSaveEditor.Model.Tests;

public class ApplicationServiceTests
{
    private readonly Mock<ISaveFileWatcher> saveFileWatcherMock_;
    private readonly Mock<ISaveDataRepository> rpgSaveDataRepositoryMock_;
    private readonly Mock<ISaveDataRepository> rmmzSaveDataRepositoryMock_;
    private readonly Mock<ILogger<ApplicationService>> loggerMock_;

    public ApplicationServiceTests()
    {
        saveFileWatcherMock_ = new();
        rpgSaveDataRepositoryMock_ = new();
        rmmzSaveDataRepositoryMock_ = new();
        loggerMock_ = new();
    }

    [Fact]
    public void Initialize_正常なパス_初期化が成功する()
    {
        // Arrange
        var service = new ApplicationService(
            saveFileWatcherMock_.Object,
            rpgSaveDataRepositoryMock_.Object,
            rmmzSaveDataRepositoryMock_.Object,
            loggerMock_.Object);
        var saveDirPath = "testdata/save";

        // Act
        service.Initialize(saveDirPath);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task StartWatcherAsync_初期化済み_ウォッチャーが開始される()
    {
        // Arrange
        var service = new ApplicationService(
            saveFileWatcherMock_.Object,
            rpgSaveDataRepositoryMock_.Object,
            rmmzSaveDataRepositoryMock_.Object,
            loggerMock_.Object);
        
        saveFileWatcherMock_
            .Setup(x => x.StartAsync(It.IsAny<string>()))
            .ReturnsAsync(SaveFileType.RmmzSave);

        service.Initialize("testdata/save");

        // Act
        await service.StartWatcherAsync();

        // Assert
        saveFileWatcherMock_.Verify(x => x.StartAsync("testdata/save"), Times.Once);
    }

    [Fact]
    public async Task StartWatcherAsync_未初期化_ウォッチャーが開始されない()
    {
        // Arrange
        var service = new ApplicationService(
            saveFileWatcherMock_.Object,
            rpgSaveDataRepositoryMock_.Object,
            rmmzSaveDataRepositoryMock_.Object,
            loggerMock_.Object);

        // Act
        await service.StartWatcherAsync();

        // Assert
        saveFileWatcherMock_.Verify(x => x.StartAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSaveDataAsync_正常なデータ_保存処理が実行される()
    {
        // Arrange
        var service = new ApplicationService(
            saveFileWatcherMock_.Object,
            rpgSaveDataRepositoryMock_.Object,
            rmmzSaveDataRepositoryMock_.Object,
            loggerMock_.Object);
        
        var saveData = new SaveData(
            ImmutableList<Switch>.Empty, 
            ImmutableList<Variable>.Empty, 
            1000, 
            ImmutableList<Item>.Empty, 
            ImmutableList<Weapon>.Empty, 
            ImmutableList<Armor>.Empty);
        service.Initialize("testdata/save");
        
        // StartWatcherAsyncをモック化してRmmzSaveを返すように設定
        saveFileWatcherMock_
            .Setup(x => x.StartAsync(It.IsAny<string>()))
            .ReturnsAsync(SaveFileType.RmmzSave);
        
        await service.StartWatcherAsync();

        // Act
        await service.UpdateSaveDataAsync(saveData);

        // Assert
        rmmzSaveDataRepositoryMock_.Verify(
            x => x.SaveAsync(saveData, "testdata/save"), 
            Times.Once);
    }

    [Fact]
    public async Task UpdateSaveDataAsync_未初期化_保存処理が実行されない()
    {
        // Arrange
        var service = new ApplicationService(
            saveFileWatcherMock_.Object,
            rpgSaveDataRepositoryMock_.Object,
            rmmzSaveDataRepositoryMock_.Object,
            loggerMock_.Object);
        
        var saveData = new SaveData(
            ImmutableList<Switch>.Empty, 
            ImmutableList<Variable>.Empty, 
            1000, 
            ImmutableList<Item>.Empty, 
            ImmutableList<Weapon>.Empty, 
            ImmutableList<Armor>.Empty);

        // Act
        await service.UpdateSaveDataAsync(saveData);

        // Assert
        rpgSaveDataRepositoryMock_.Verify(
            x => x.SaveAsync(It.IsAny<SaveData>(), It.IsAny<string>()), 
            Times.Never);
        rmmzSaveDataRepositoryMock_.Verify(
            x => x.SaveAsync(It.IsAny<SaveData>(), It.IsAny<string>()), 
            Times.Never);
    }
}