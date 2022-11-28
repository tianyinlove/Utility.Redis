using System;
using System.Collections.Concurrent;
using SSRedisClient = ServiceStack.Redis.RedisClient;

namespace Utility.Redis
{
    internal class RedisClientWrapper : IDisposable
    {
        private readonly SSRedisClient _redisClient;
        private readonly ConcurrentQueue<SSRedisClient> _queue;

        public RedisClientWrapper(SSRedisClient redisClient, ConcurrentQueue<SSRedisClient> queue)
        {
            _redisClient = redisClient;
            _queue = queue;
        }

        public SSRedisClient RedisClient => _redisClient;

        public void Dispose()
        {
            _queue.Enqueue(RedisClient);
        }
    }
}
