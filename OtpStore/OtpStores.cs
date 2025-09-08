using StackExchange.Redis;

namespace EReceiptAllInOne.OtpStore;

public interface IOtpStore
{
    Task SetAsync(string key, string code, TimeSpan ttl);
    Task<(bool ok, string? code)> GetAsync(string key);
    Task<bool> DeleteAsync(string key);
}

public class RedisOtpStore : IOtpStore
{
    private readonly IDatabase _db;
    public RedisOtpStore(IConnectionMultiplexer mux) { _db = mux.GetDatabase(); }

    public Task SetAsync(string key, string code, TimeSpan ttl)
        => _db.StringSetAsync(key, code, ttl);

    public async Task<(bool ok, string? code)> GetAsync(string key)
    {
        var v = await _db.StringGetAsync(key);
        return v.IsNullOrEmpty ? (false, null) : (true, v!);
    }

    public Task<bool> DeleteAsync(string key) => _db.KeyDeleteAsync(key);
}

public class InMemoryOtpStore : IOtpStore
{
    private readonly Dictionary<string,(string code, DateTimeOffset exp)> _map = new();
    public Task SetAsync(string key, string code, TimeSpan ttl)
    {
        _map[key] = (code, DateTimeOffset.UtcNow.Add(ttl));
        return Task.CompletedTask;
    }
    public Task<(bool ok, string? code)> GetAsync(string key)
    {
        if (_map.TryGetValue(key, out var v))
        {
            if (v.exp > DateTimeOffset.UtcNow) return Task.FromResult((true, v.code));
            _map.Remove(key);
        }
        return Task.FromResult((false, null));
    }
    public Task<bool> DeleteAsync(string key) => Task.FromResult(_map.Remove(key));
}
