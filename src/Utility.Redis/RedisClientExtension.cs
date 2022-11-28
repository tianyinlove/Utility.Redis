using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.Redis
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class RedisClientExtension
    {
        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="redisClient"></param>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        public static async Task<RedisLocker> WaitOneAsync(this RedisClient redisClient, string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000)
        {
            var locker = new RedisLocker(redisClient, lockKey, maxLockTimeMilliSecond);
            Random rnd = new Random();
            var startTime = DateTime.Now;

            while (timeoutMilliSecond == 0 || DateTime.Now < startTime.AddMilliseconds(timeoutMilliSecond))
            {
                if (locker._redis.Set<Guid>(
                    key: locker._lockKey,
                    val: locker._uniqueValue,
                    expiresAt: TimeSpan.FromMilliseconds(RedisLocker.KEY_EXPIRE_MILLISECOND),
                    exists: false))
                {
                    locker.Success = true;
                    locker._lockTime = DateTime.Now;
                    locker._timer = new Timer(locker.RenewExpire, null, RedisLocker.RENEW_TIME, Timeout.Infinite);
                    break;
                }
                else
                {
                    await Task.Delay(rnd.Next(30, 100));
                }
            }
            return locker;
        }
    }
}
