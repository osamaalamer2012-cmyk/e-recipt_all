using System.Text.Json;
using EReceiptAllInOne.Data;
using StackExchange.Redis;

namespace EReceiptAllInOne.Repositories.Impl;

public class RedisReceiptRepository : IReceiptRepository
{
    private readonly IDatabase _db;
    private readonly string _prefix;
    public RedisReceiptRepository(IConnectionMultiplexer mux, string prefix)
    {
        _db = mux.GetDatabase();
        _prefix = prefix.TrimEnd(':') + ":";
    }

    public Task CreateAsync(ReceiptEntity r, CancellationToken ct = default)
    {
        var key = _prefix + r.ReceiptId;
        var json = JsonSerializer.Serialize(r);
        return _db.StringSetAsync(key, json);
    }

    public async Task<ReceiptEntity?> GetAsync(string id, CancellationToken ct = default)
    {
        var val = await _db.StringGetAsync(_prefix + id);
        if (val.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<ReceiptEntity>(val!);
    }

    public async Task IncrementUsesAsync(string id, CancellationToken ct = default)
    {
        var key = _prefix + id;
        var val = await _db.StringGetAsync(key);
        if (val.IsNullOrEmpty) return;
        var r = JsonSerializer.Deserialize<ReceiptEntity>(val!)!;
        r.Uses += 1;
        await _db.StringSetAsync(key, JsonSerializer.Serialize(r));
    }
}

public class RedisShortLinkRepository : IShortLinkRepository
{
    private readonly IDatabase _db;
    private readonly string _prefix;
    public RedisShortLinkRepository(IConnectionMultiplexer mux, string prefix)
    {
        _db = mux.GetDatabase();
        _prefix = prefix.TrimEnd(':') + ":";
    }

    public Task CreateAsync(ShortLinkEntity s, CancellationToken ct = default)
    {
        var key = _prefix + s.Code;
        var json = JsonSerializer.Serialize(s);
        var ttl = s.ExpiresAt - DateTimeOffset.UtcNow;
        if (ttl < TimeSpan.Zero) ttl = TimeSpan.FromMinutes(5);
        return _db.StringSetAsync(key, json, ttl);
    }

    public async Task<ShortLinkEntity?> GetAsync(string code, CancellationToken ct = default)
    {
        var val = await _db.StringGetAsync(_prefix + code);
        if (val.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<ShortLinkEntity>(val!);
    }

    public async Task IncrementUsageAsync(string code, CancellationToken ct = default)
    {
        var key = _prefix + code;
        var val = await _db.StringGetAsync(key);
        if (val.IsNullOrEmpty) return;
        var s = JsonSerializer.Deserialize<ShortLinkEntity>(val!)!;
        s.Usage += 1;
        var ttl = s.ExpiresAt - DateTimeOffset.UtcNow;
        if (ttl < TimeSpan.Zero) ttl = TimeSpan.FromMinutes(5);
        await _db.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(s), ttl);
    }
}
