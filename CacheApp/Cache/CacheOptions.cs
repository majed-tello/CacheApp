namespace CacheApp.Cache;

public class CacheOptions
{
    public bool Enabled { get; set; }
    public int DefaultExpirationInMinutes { get; set; }
    public string RedisConnection { get; set; } = default!;
}