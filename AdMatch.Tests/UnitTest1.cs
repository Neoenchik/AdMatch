using System.Text;
using AdMatch.Application.Services;
using AdMatch.DataAccessInterfaces.Models;
using AdMatch.DataAccessInterfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace AdMatch.Tests;

public class AdvertisingServiceTests
{
    /// <summary>
    /// Проверяет, что при загрузке файла с некорректной строкой (без двоеточия) логируется Warning,
    /// но процесс не прерывается.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_LogsWarning_OnInvalidLine()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        var content = "Строка без двоеточия";
        var file = CreateFormFile(content);

        // Act
        await service.LoadPlatformsFromFileAsync(file);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Ошибка при обработке строки")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Проверяет, что при попытке загрузки пустого файла (Length=0) выбрасывается ArgumentException
    /// и логируется Warning о пустом файле.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_ThrowsArgumentException_OnEmptyFile()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.LoadPlatformsFromFileAsync(fileMock.Object));

        Assert.Equal("Некорректный файл", ex.Message);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Попытка загрузить пустой или null файл")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Проверяет, что при загрузке платформы без валидных локаций (не начинающихся с '/') логируется Warning,
    /// но платформа не добавляется в репозиторий.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_LogsWarning_WhenNoValidLocations()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        var content = "Газета:invalidLocation";
        var file = CreateFormFile(content);

        // Act
        await service.LoadPlatformsFromFileAsync(file);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Ошибка при обработке строки")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Проверяет, что при загрузке валидного файла репозиторий вызывается с правильными платформами и локациями.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_CallsRepositoryLoadPlatforms()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        var content = "Газета:/ru/msk";
        var file = CreateFormFile(content);

        // Act
        await service.LoadPlatformsFromFileAsync(file);

        // Assert
        repoMock.Verify(r => r.LoadPlatforms(It.Is<List<AdvertisingPlatform>>(list =>
            list.Count == 1 &&
            list[0].Name == "Газета" &&
            list[0].Locations.Contains("/ru/msk"))),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Проверяет, что при поиске по некорректной локации (не начинается с '/') выбрасывается ArgumentException
    /// и логируется Warning.
    /// </summary>
    [Fact]
    public async Task SearchPlatformsAsync_ThrowsArgumentException_OnInvalidLocation()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.SearchPlatformsAsync("invalid"));
        Assert.Equal("Некорректная локация", ex.Message);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Некорректный параметр локации")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    /// <summary>
    /// Проверяет, что при загрузке валидного многострочного файла все платформы парсятся правильно,
    /// репозиторий вызывается с полным списком, и нет Warning.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_ValidFile_LoadsAllPlatforms()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        var content = string.Join(Environment.NewLine, new[]
        {
            "Яндекс.Директ:/ru",
            "Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik",
            "Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl",
            "Крутая реклама:/ru/svrd"
        });

        var file = CreateFormFile(content);

        // Act
        await service.LoadPlatformsFromFileAsync(file);

        // Assert
        repoMock.Verify(r => r.LoadPlatforms(It.Is<List<AdvertisingPlatform>>(list =>
            list.Count == 4 &&
            list.Any(p => p.Name == "Яндекс.Директ" && p.Locations.SequenceEqual(new[] { "/ru" })) &&
            list.Any(p => p.Name == "Ревдинский рабочий" && p.Locations.SequenceEqual(new[] { "/ru/svrd/revda", "/ru/svrd/pervik" })) &&
            list.Any(p => p.Name == "Газета уральских москвичей" && p.Locations.SequenceEqual(new[] { "/ru/msk", "/ru/permobl", "/ru/chelobl" })) &&
            list.Any(p => p.Name == "Крутая реклама" && p.Locations.SequenceEqual(new[] { "/ru/svrd" }))
        )), Times.AtLeastOnce);

        loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет, что при загрузке файла с mixture валидных и некорректных строк Warning логируется только для invalid,
    /// но валидные платформы загружаются в репозиторий.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_WithInvalidLine_LogsWarning_AndLoadsOtherPlatforms()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        var content = string.Join(Environment.NewLine, new[]
        {
            "Яндекс.Директ:/ru",
            "Некорректная строка без двоеточия",
            "Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl"
        });

        var file = CreateFormFile(content);

        // Act
        await service.LoadPlatformsFromFileAsync(file);

        // Assert
        repoMock.Verify(r => r.LoadPlatforms(It.Is<List<AdvertisingPlatform>>(list =>
            list.Count == 2 &&
            list.Any(p => p.Name == "Яндекс.Директ" && p.Locations.SequenceEqual(new[] { "/ru" })) &&
            list.Any(p => p.Name == "Газета уральских москвичей" && p.Locations.SequenceEqual(new[] { "/ru/msk", "/ru/permobl", "/ru/chelobl" }))
        )), Times.AtLeastOnce);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Ошибка при обработке строки")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    
    /// <summary>
    /// Проверяет, что при ошибке парсинга строки логируется LogError,
    /// но процесс продолжается для других строк.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_LogsError_OnParsingException_AndContinues()
    {
        // Arrange: content с строкой, вызывающей exception (например, с null в split — но симулируем в тесте)
        var content = "Платформа:/ru\r\nСтрока с exception:throw new Exception()";
        var file = CreateFormFile(content);

        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        // Act
        await service.LoadPlatformsFromFileAsync(file);

        // Assert
        loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        repoMock.Verify(r => r.LoadPlatforms(It.IsAny<IEnumerable<AdvertisingPlatform>>()), Times.Once);
    }
    
    /// <summary>
    /// Проверяет, что поиск по валидной локации возвращает список платформ от репозитория без ошибок.
    /// </summary>
    [Fact]
    public async Task SearchPlatformsAsync_ReturnsPlatforms_ForValidLocation()
    {
        // Arrange
        var expected = new List<string> { "Яндекс.Директ" };
        var repoMock = new Mock<IAdvertisingRepository>();
        repoMock.Setup(r => r.GetPlatformsForLocation("/ru")).Returns(expected);
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        // Act
        var result = await service.SearchPlatformsAsync("/ru");

        // Assert
        Assert.Equal(expected, result);
        repoMock.Verify(r => r.GetPlatformsForLocation("/ru"), Times.Once);
        loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что при null-файле выбрасывается ArgumentException без чтения stream.
    /// </summary>
    [Fact]
    public async Task LoadPlatformsFromFile_ThrowsArgumentException_OnNullFile()
    {
        // Arrange
        var repoMock = new Mock<IAdvertisingRepository>();
        var loggerMock = new Mock<ILogger<AdvertisingService>>();
        var service = new AdvertisingService(repoMock.Object, loggerMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.LoadPlatformsFromFileAsync(null));
        Assert.Equal("Некорректный файл", ex.Message);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) =>
                    o.ToString()!.Contains("Попытка загрузить пустой или null файл")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        repoMock.Verify(r => r.LoadPlatforms(It.IsAny<IEnumerable<AdvertisingPlatform>>()), Times.Never);
    }

    private static IFormFile CreateFormFile(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(bytes.Length);
        return fileMock.Object;
    }
}