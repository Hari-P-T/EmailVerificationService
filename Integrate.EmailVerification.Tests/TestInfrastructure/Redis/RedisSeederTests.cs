using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System.Text;
using Log = Integration.Util.Logging;

namespace Integrate.EmailVerification.Tests.Infrastructure.Redis;

[TestFixture]
public class RedisSeederTests
{
    private Mock<IConnectionMultiplexer> _redisMock;
    private Mock<IDatabase> _databaseMock;
    private Mock<Log.ILogger> _loggerMock;
    private RedisSeeder _seeder;

    private string _resourcePath;

    [SetUp]
    public void SetUp()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _loggerMock = new Mock<Log.ILogger>();

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ResourceSettingsResourceFolderPath"]).Returns(string.Empty);

        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                  .Returns(_databaseMock.Object);

        _seeder = new RedisSeeder(_redisMock.Object, _loggerMock.Object, configMock.Object);

        _resourcePath = Path.Combine(AppContext.BaseDirectory, "Features", "Resources");
        Directory.CreateDirectory(_resourcePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_resourcePath))
            Directory.Delete(_resourcePath, true);
    }

    [Test]
    public async Task SeedAsync_ShouldSeedAllKeys_IfNotExist()
    {
        var testLines = Enumerable.Range(0, 5).Select(i => $"line-{i}").ToArray();
        var keys = new Dictionary<string, string>
        {
            { "tlds.txt", ConstantKeys.Tlds },
            { "whitelistedDomains.txt", ConstantKeys.WhitelistedDomains },
            { "greylistedDomains.txt", ConstantKeys.GreylistedDomains },
            { "disposableDomains.txt", ConstantKeys.DisposableDomains },
            { "vulgarWords.txt", ConstantKeys.VulgarWords },
            { "AliasNames.txt", ConstantKeys.AliasNames },
            { "blacklistedDomains.txt", ConstantKeys.BlacklistedDomains }
        };

        foreach (var file in keys.Keys)
        {
            File.WriteAllLines(Path.Combine(_resourcePath, file), testLines);
        }

        // Setup mocks
        _databaseMock.Setup(d => d.KeyExists(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Returns(false);

        _databaseMock.Setup(d => d.SetAddAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue[]>(),
            It.IsAny<CommandFlags>())).ReturnsAsync(0);

        _databaseMock.Setup(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>())).ReturnsAsync(true);

        // Act
        await _seeder.SeedAsync();

        // Verify
        foreach (var key in keys.Values)
        {
            _databaseMock.Verify(d => d.KeyExists(key, It.IsAny<CommandFlags>()), Times.Once);
            _databaseMock.Verify(d => d.SetAddAsync(key,
                It.Is<RedisValue[]>(arr => arr.Length == testLines.Length),
                It.IsAny<CommandFlags>()), Times.Once);
            _databaseMock.Verify(d => d.KeyExpireAsync(
                key, TimeSpan.FromDays(7), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()), Times.Once);
        }
    }

    [Test]
    public async Task SeedAsync_ShouldNotSeed_IfKeyExists()
    {
        var key = ConstantKeys.Tlds;
        var filePath = Path.Combine(_resourcePath, "tlds.txt");
        File.WriteAllText(filePath, "line-0\nline-1");

        _databaseMock.Setup(d => d.KeyExists(key, It.IsAny<CommandFlags>())).Returns(true);

        await _seeder.SeedAsync();

        _databaseMock.Verify(d => d.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()), Times.Never);
        _databaseMock.Verify(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(), It.IsAny<TimeSpan>(), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Test]
    public async Task SeedAsync_ByKey_ShouldSeedSpecificKey()
    {
        var testLines = new[] { "line-0", "line-1" };
        var fileName = "tlds.txt";
        var redisKey = ConstantKeys.Tlds;

        File.WriteAllLines(Path.Combine(_resourcePath, fileName), testLines);

        _databaseMock.Setup(d => d.SetAddAsync(redisKey, It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>())).ReturnsAsync(0);
        _databaseMock.Setup(d => d.KeyExpireAsync(
            redisKey, TimeSpan.FromDays(7), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);

        await _seeder.SeedAsync(redisKey);

        _databaseMock.Verify(d => d.SetAddAsync(redisKey,
            It.Is<RedisValue[]>(arr => arr.Length == testLines.Length),
            It.IsAny<CommandFlags>()), Times.Once);

        _databaseMock.Verify(d => d.KeyExpireAsync(
            redisKey, TimeSpan.FromDays(7), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task CacheFileLines_ShouldDoNothing_WhenFileDoesNotExist()
    {
        string fakeKey = "fake:key";
        string badPath = Path.Combine(_resourcePath, "nonexistent.txt");

        await _seeder.SeedAsync(fakeKey);

        _databaseMock.Verify(d => d.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()), Times.Never);
        _databaseMock.Verify(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(), It.IsAny<TimeSpan>(), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()), Times.Never);
    }
}
