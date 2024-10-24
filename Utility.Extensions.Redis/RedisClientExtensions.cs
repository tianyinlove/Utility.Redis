using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utility.Extensions.Redis.Internal;

namespace Utility.Extensions.Redis
{
    public partial class RedisClientBase
    {
        private const int WaitRetryInterval = 64;
        private const int MaxMessagePerCommand = 30;

        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        public virtual async Task<RedisLocker> WaitOneAsync(string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000)
        {
            var stopwatch = Stopwatch.StartNew();
            var lockValue = Guid.NewGuid().ToString("n");

            while (timeoutMilliSecond == 0 || stopwatch.ElapsedMilliseconds + WaitRetryInterval < timeoutMilliSecond)
            {
                if (Set(lockKey, lockValue,
                    expiresAt: TimeSpan.FromMilliseconds(RedisLocker.KEY_EXPIRE_MILLISECOND),
                    exists: false))
                {
                    stopwatch.Stop();
                    return new RedisLocker(this, lockKey, lockValue, maxLockTimeMilliSecond);
                }
                await Task.Delay(WaitRetryInterval);
            }
            stopwatch.Stop();
            return new RedisLocker { Success = false };
        }

        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        public virtual RedisLocker WaitOne(string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000)
        {
            var stopwatch = Stopwatch.StartNew();
            var lockValue = Guid.NewGuid().ToString("n");

            while (timeoutMilliSecond == 0 || stopwatch.ElapsedMilliseconds + WaitRetryInterval < timeoutMilliSecond)
            {
                if (Set(lockKey, lockValue,
                    expiresAt: TimeSpan.FromMilliseconds(RedisLocker.KEY_EXPIRE_MILLISECOND),
                    exists: false))
                {
                    stopwatch.Stop();
                    return new RedisLocker(this, lockKey, lockValue, maxLockTimeMilliSecond);
                }
                Thread.Sleep(WaitRetryInterval);
            }
            stopwatch.Stop();
            return new RedisLocker { Success = false };
        }

        /// <summary>
        /// 删除内存缓存
        /// </summary>
        /// <param name="keys"></param>
        public void DeleteMemoryCache(params string[] keys)
        {
            for (int offset = 0; offset < keys.Length; offset += MaxMessagePerCommand)
            {
                var subKeys = keys.Skip(offset).Take(MaxMessagePerCommand).ToArray();

                // 记录历史，订阅重连以后删除相关内存缓存
                List<KeyValuePair<string, long>> memberValues = subKeys.Select(d => new KeyValuePair<string, long>(d, DateTime.Now.ValueOf())).ToList();
                ZAdd<string>(RedisKeys.MemoryDeleteHistory, memberValues);

                // pub消息
                using var client = (RedisClient)GetRedisClientsManager().GetClient();
                var channel = $"{Prefix}{RedisKeys.MemoryMessageChannel}";
                var message = string.Join(' ', subKeys);
                client.PublishMessage(channel, message);
            }

            Expire(RedisKeys.MemoryDeleteHistory, 60);
            ZRemRangeByScore(RedisKeys.MemoryDeleteHistory, 0, DateTime.Now.AddMinutes(-1).ValueOf()); //清理历史
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lockKey"></param>
        /// <param name="getCache"></param>
        /// <param name="isSuccess"></param>
        /// <param name="setCache"></param>
        /// <param name="getData"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> GetTAsync<T>(string lockKey, Func<T> getCache, Func<T, bool> isSuccess, Action<T> setCache, Func<Task<T>> getData)
        {
            var value = getCache();
            if (isSuccess(value))
            {
                return value;
            }

            using var locker = await WaitOneAsync(lockKey, 10000);
            if (!locker.Success)
            {
                throw new Exception("获取锁失败");
            }

            value = getCache();
            if (isSuccess(value))
            {
                return value;
            }

            value = await getData();
            if (isSuccess(value))
            {
                setCache(value);
                return value;
            }
            return default;
        }
    }
}