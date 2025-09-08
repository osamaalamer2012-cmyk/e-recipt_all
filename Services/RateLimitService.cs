using System.Collections.Concurrent;
using StackExchange.Redis;

namespace EReceiptAllInOne.Services;

public interface IRateLimitService
{
    Task<bool> HitAsync(string key, int limit, TimeSpan window);
}

public class RedisRateLimitService : IRateLimitService
{
    private readonly IDatabase _db;
    public RedisRateLimitService(IConnectionMultiplexer mux) { _db = mux.GetDatabase(); }

    public async Task<bool> HitAsync(string key, int limit, TimeSpan window)
    {
        var count = await _db.StringIncrementAsync(key);
        if (count == 1) await _db.KeyExpireAsync(key, window);
        return count <= limit;
    }
}

public class MemoryRateLimitService : IRateLimitService
{
    private class Entry { public int Count; public DateTimeOffset Exp; }
    private readonly ConcurrentDictionary<string, Entry> _map = new();

    public Task<bool> HitAsync(string key, int limit, TimeSpan window)
    {
        var now = DateTimeOffset.UtcNow;
        var e = _map.AddOrUpdate(key,
            _ => new Entry{ Count = 1, Exp = now.Add(window)},
            (_, cur) => { if (cur.Exp <= now) { cur.Count = 0; cur.Exp = now.Add(window); } cur.Count += 1; return cur; });
        var ok = e.Count <= limit;
        if (e.Exp <= now) _map.TryRemove(key, out _);
        return Task.FromResult(ok);
    }
}
