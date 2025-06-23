using Integrate.EmailVerification.Infrastructure.Constant;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Log = Integration.Util.Logging;



namespace Integrate.EmailVerification.Infrastructure.Redis;
public class RedisSeeder : IRedisSeeder
{
    private readonly IConnectionMultiplexer _redis;
    private readonly Log.ILogger _logger;
    private readonly string _resourcePath;

    public RedisSeeder(
        IConnectionMultiplexer redis,
        Log.ILogger logger,
        IConfiguration configuration
        )
    {
        _redis = redis;
        _logger = logger;
        _resourcePath = configuration["ResourceSettingsResourceFolderPath"] ?? "";
    }

    public async Task SeedAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            string baseDir = AppContext.BaseDirectory;
            string resourceFolderPath = Path.Combine(baseDir, "Features", "Resources");

            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.Tlds), ConstantKeys.Tlds);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.WhitelistedDomains), ConstantKeys.WhitelistedDomains);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.GreylistedDomains), ConstantKeys.GreylistedDomains);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.DisposableDomains), ConstantKeys.DisposableDomains);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.Bogus), ConstantKeys.Bogus);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.VulgarWords), ConstantKeys.VulgarWords);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.AliasNames), ConstantKeys.AliasNames);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.Established), ConstantKeys.Established);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.BlacklistedDomains), ConstantKeys.BlacklistedDomains);
            await CacheIfNotExists(db, Path.Combine(resourceFolderPath, FileNames.Spam), ConstantKeys.Spam);
        }
        catch (Exception ex)
        {
            throw  new Exception(ex.Message);
        }

    }

    private async Task CacheIfNotExists(IDatabase db, string filePath, string key)
    {
        if (!db.KeyExists(key))
        {
            await CacheFileLines(db, filePath, key);
        }
    }

    public async Task SeedAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            string baseDir = AppContext.BaseDirectory;
            string resourceFolderPath = Path.Combine(baseDir, "Features", "Resources");
            var fileMap = new Dictionary<string, (string FileName, string RedisKey)>
            {
                [ConstantKeys.Tlds] = (FileNames.Tlds, ConstantKeys.Tlds),
                [ConstantKeys.WhitelistedDomains] = (FileNames.WhitelistedDomains, ConstantKeys.WhitelistedDomains),
                [ConstantKeys.GreylistedDomains] = (FileNames.GreylistedDomains, ConstantKeys.GreylistedDomains),[ConstantKeys.BlacklistedDomains] = (FileNames.BlacklistedDomains, ConstantKeys.BlacklistedDomains),
                [ConstantKeys.DisposableDomains] = (FileNames.DisposableDomains, ConstantKeys.DisposableDomains),
                [ConstantKeys.Bogus] = (FileNames.Bogus, ConstantKeys.Bogus),
                [ConstantKeys.VulgarWords] = (FileNames.VulgarWords, ConstantKeys.VulgarWords),
                [ConstantKeys.AliasNames] = (FileNames.AliasNames, ConstantKeys.AliasNames),
                [ConstantKeys.Established] = (FileNames.Established, ConstantKeys.Established),
                [ConstantKeys.Spam] = (FileNames.Spam, ConstantKeys.Spam),
            };

            if (fileMap.TryGetValue(key, out var value))
            {
                var filePath = Path.Combine(resourceFolderPath, value.FileName);
                await CacheFileLines(db, filePath, value.RedisKey);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    private async Task CacheFileLines(IDatabase db, string filePath, string redisKey)
    {
        if (!File.Exists(filePath))
        {
            return;
        }
        var lines = await File.ReadAllLinesAsync(filePath);
        const int batchSize = 30000;
        const int maxParallel = 5;

        var tasks = new List<Task>();

        foreach (var batch in lines.Chunk(batchSize))
        {
            var redisValues = batch.Select(l => (RedisValue)l).ToArray();
            tasks.Add(db.SetAddAsync(redisKey, redisValues));

            if (tasks.Count >= maxParallel)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
        await db.KeyExpireAsync(redisKey, TimeSpan.FromDays(7));

        _logger.Info($"Loaded {lines.Length} items into Redis key: {redisKey}");
    }
}
