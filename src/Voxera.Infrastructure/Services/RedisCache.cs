using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Voxera.Application.Interfaces;
using Voxera.Infrastructure.Settings;

namespace Voxera.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly string _prefix;

    public RedisCacheService(IOptions<RedisSettings> settings)
    {
        var redis = ConnectionMultiplexer.Connect(settings.Value.ConnectionString);
        _db = redis.GetDatabase();
        _prefix = settings.Value.InstanceName;
    }

    private string Key(string key) => $"{_prefix}{key}";

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(Key(key));
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(Key(key), json, expiry);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(Key(key));

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => await _db.KeyExistsAsync(Key(key));

    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var result = await _db.StringIncrementAsync(Key(key), value);
        if (expiry.HasValue && result == value)
            await _db.KeyExpireAsync(Key(key), expiry);
        return result;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null) return cached;
        var value = await factory();
        await SetAsync(key, value, expiry, ct);
        return value;
    }
}
