using Microsoft.Extensions.Logging;
using Moq;
using RpgTkoolSaveEditor.Model.GameDatas;
using RpgTkoolSaveEditor.Model.SaveDatas;
using System.Collections.Immutable;

namespace RpgTkoolSaveEditor.Model.Tests;

public class SaveFileWatcherTests : IDisposable
{
    private readonly Mock<ISaveDataRepository> rpgSaveDataRepositoryMock_;
    private readonly Mock<ISaveDataRepository> rmmzSaveDataRepositoryMock_;
    private readonly Mock<ILogger<SaveFileWatcher>> loggerMock_;
    private readonly SaveFileWatcher watcher_;

    public SaveFileWatcherTests()
    {
        rpgSaveDataRepositoryMock_ = new();
        rmmzSaveDataRepositoryMock_ = new();
        loggerMock_ = new();
        watcher_ = new SaveFileWatcher(
            rpgSaveDataRepositoryMock_.Object,
            rmmzSaveDataRepositoryMock_.Object,
            loggerMock_.Object);
    }

    [Fact]
    public async Task StartAsync_RMMZセーブファイルが存在_RmmzSaveタイプが返される()
    {
        // Arrange
        var testSaveData = new SaveData(
            ImmutableList<Switch>.Empty, 
            ImmutableList<Variable>.Empty, 
            0, 
            ImmutableList<Item>.Empty, 
            ImmutableList<Weapon>.Empty, 
            ImmutableList<Armor>.Empty);
        rmmzSaveDataRepositoryMock_
            .Setup(x => x.LoadAsync(It.IsAny<string>()))
            .ReturnsAsync(testSaveData);

        // Act
        var result = await watcher_.StartAsync("testdata/save");

        // Assert
        Assert.Equal(SaveFileType.RmmzSave, result);
        rmmzSaveDataRepositoryMock_.Verify(
            x => x.LoadAsync("testdata/save"), 
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_存在しないパス_InvalidOperationExceptionがスローされる()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => watcher_.StartAsync("nonexistent/path"));
    }

    [Fact]
    public void LoadSuppressed_設定と取得_正しく動作する()
    {
        // Arrange
        Assert.False(watcher_.LoadSuppressed);

        // Act
        watcher_.LoadSuppressed = true;

        // Assert
        Assert.True(watcher_.LoadSuppressed);
    }

    [Fact]
    public void Dispose_複数回呼び出し_例外がスローされない()
    {
        // Arrange & Act & Assert
        watcher_.Dispose();
        watcher_.Dispose(); // 2回目の呼び出しで例外が発生しないことを確認
    }

    [Fact]
    public async Task StartAsync_Dispose後の呼び出し_ObjectDisposedExceptionがスローされる()
    {
        // Arrange
        watcher_.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => watcher_.StartAsync("testdata/save"));
    }

    public void Dispose()
    {
        watcher_?.Dispose();
    }
}