using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utility.Extensions.Redis.Internal;

namespace Utility.Extensions.Redis.Worker;

internal class MemorySubscribeService<TRedisClient> : BackgroundService where TRedisClient : IRedisClientBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// 内存缓存
    /// </summary>
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// 当前redis连接配置
    /// </summary>
    private string _redisConfig = null;

    /// <summary>
    /// 当前订阅信息
    /// </summary>
    private Subscription _subscription = null;

    public MemorySubscribeService(IServiceScopeFactory scopeFactory, ILogger<MemorySubscribeService<TRedisClient>> logger, IMemoryCache memoryCache)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        using var scope = _scopeFactory.CreateScope();
        var redis = scope.ServiceProvider.GetRequiredService<TRedisClient>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var redisCofig = redis.ConnectionString;

                //如果配置变更
                if (_redisConfig != redisCofig)
                {
                    _subscription?.Dispose();

                    _subscription = redis.Subscribe(RedisKeys.MemoryMessageChannel);
                    _subscription.Start(OnMessage, onStart: () => OnStart(redis));
                    _redisConfig = redisCofig;
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, "内存缓存订阅异常");
            }
            await Task.Delay(1000);
        }
    }

    private void OnStart(TRedisClient redis)
    {
        var memKeys = redis.ZRangeByScore<string>(RedisKeys.MemoryDeleteHistory, DateTime.Now.AddMinutes(-1).ValueOf(), long.MaxValue);
        foreach (var memKey in memKeys)
        {
            _memoryCache.Remove(memKey);
        }
    }

    private void OnMessage(string channel, string message)
    {
        foreach (var memKey in message.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            _memoryCache.Remove(memKey);
        }
        _logger.LogDebug("订阅删除内存key:{Keys}", message);
    }
}