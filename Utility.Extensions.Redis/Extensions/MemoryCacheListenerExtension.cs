using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using Utility.Extensions.Redis;
using Utility.Extensions.Redis.Worker;

namespace Utility.Extensions
{
    /// <summary>
    /// 内存缓存扩展
    /// </summary>
    public static class MemoryCacheListenerExtension
    {
        private const string AddEmappMemoryCacheCountKey = "AddMemorySubscribe";

        /// <summary>
        /// 添加云端缓存更新客户端,通过订阅redis列表来实现内存缓存的即时删除
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMemorySubscribe<TRedisClient>(this IServiceCollection services)
            where TRedisClient : IRedisClientBase
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var key = $"{AddEmappMemoryCacheCountKey}:{typeof(TRedisClient).FullName}";

            if (Counter.IncreaseOnce(services, key))
            {
                services.AddHostedService<MemorySubscribeService<TRedisClient>>();
            }
            return services;
        }
    }

    /// <summary>
    /// 访问计数器
    /// </summary>
    internal static class Counter
    {
        private static readonly ConcurrentDictionary<object, ConcurrentDictionary<string, int>> _counts = new ConcurrentDictionary<object, ConcurrentDictionary<string, int>>();

        /// <summary>
        /// 获取计数
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetCount(object obj, string key)
        {
            ConcurrentDictionary<string, int> orAdd = _counts.GetOrAdd(obj, new ConcurrentDictionary<string, int>());
            return orAdd.GetOrAdd(key, 0);
        }

        /// <summary>
        /// 添加计数
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int IncreaseCount(object obj, string key)
        {
            ConcurrentDictionary<string, int> orAdd = _counts.GetOrAdd(obj, new ConcurrentDictionary<string, int>());
            return orAdd.AddOrUpdate(key, 1, (string k, int v) => v + 1);
        }

        /// <summary>
        /// 确保只加一次
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IncreaseOnce(object obj, string key)
        {
            return IncreaseCount(obj, key) == 1;
        }
    }
}