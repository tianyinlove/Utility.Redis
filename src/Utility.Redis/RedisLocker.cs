using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.Redis
{
    /// <summary>
    /// 基于Redis的分布式锁，请使用using
    /// </summary>
    public sealed class RedisLocker : IDisposable
    {
        /// <summary>
        /// 锁键值超时时间
        /// </summary>
        internal const int KEY_EXPIRE_MILLISECOND = 2000;

        /// <summary>
        /// 刷新锁的时间间隔
        /// </summary>

        internal const int RENEW_TIME = 200;

        /// <summary>
        /// 一个随机数值，确认这个锁是本线程创建的
        /// </summary>
        internal readonly Guid _uniqueValue;

        /// <summary>
        /// 锁键值
        /// </summary>
        internal readonly string _lockKey;

        /// <summary>
        /// redis客户端实例
        /// </summary>
        internal readonly RedisClient _redis;

        /// <summary>
        /// 
        /// </summary>
        private bool _disposing;

        /// <summary>
        /// 最大锁定时间
        /// </summary>
        internal readonly int _maxLockTimeMilliSecond;

        /// <summary>
        /// 锁延期定时器
        /// </summary>
        internal Timer _timer;

        /// <summary>
        /// 占用锁的开始时间
        /// </summary>
        internal DateTime _lockTime;

        /// <summary>
        /// 是否成功申请到锁
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        public RedisLocker(string configuration, string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000)
        {
            _uniqueValue = Guid.NewGuid();
            _lockKey = lockKey;
            _redis = RedisClient.GetInstance(configuration);
            _maxLockTimeMilliSecond = maxLockTimeMilliSecond;
            Random rnd = new Random();
            var startTime = DateTime.Now;

            while (timeoutMilliSecond == 0 || DateTime.Now < startTime.AddMilliseconds(timeoutMilliSecond))
            {
                if (_redis.Set<Guid>(key: _lockKey, val: _uniqueValue, expiresAt: TimeSpan.FromMilliseconds(KEY_EXPIRE_MILLISECOND), exists: false))
                {
                    Success = true;
                    _lockTime = DateTime.Now;
                    _timer = new Timer(RenewExpire, null, RENEW_TIME, Timeout.Infinite);
                    break;
                }
                else
                {
                    Thread.Sleep(rnd.Next(30, 100));
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="lockKey"></param>
        /// <param name="maxLockTimeMilliSecond"></param>
        private RedisLocker(string configuration, string lockKey, int maxLockTimeMilliSecond)
        {
            _uniqueValue = Guid.NewGuid();
            _lockKey = lockKey;
            _redis = RedisClient.GetInstance(configuration);
            _maxLockTimeMilliSecond = maxLockTimeMilliSecond;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redis"></param>
        /// <param name="lockKey"></param>
        /// <param name="maxLockTimeMilliSecond"></param>
        internal RedisLocker(RedisClient redis, string lockKey, int maxLockTimeMilliSecond)
        {
            _uniqueValue = Guid.NewGuid();
            _lockKey = lockKey;
            _redis = redis;
            _maxLockTimeMilliSecond = maxLockTimeMilliSecond;
        }

        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        public static async Task<RedisLocker> WaitOneAsync(string configuration, string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000)
        {
            var locker = new RedisLocker(configuration, lockKey, maxLockTimeMilliSecond);
            Random rnd = new Random();
            var startTime = DateTime.Now;

            while (timeoutMilliSecond == 0 || DateTime.Now < startTime.AddMilliseconds(timeoutMilliSecond))
            {
                if (locker._redis.Set<Guid>(
                    key: locker._lockKey,
                    val: locker._uniqueValue,
                    expiresAt: TimeSpan.FromMilliseconds(KEY_EXPIRE_MILLISECOND),
                    exists: false))
                {
                    locker.Success = true;
                    locker._lockTime = DateTime.Now;
                    locker._timer = new Timer(locker.RenewExpire, null, RENEW_TIME, Timeout.Infinite);
                    break;
                }
                else
                {
                    await Task.Delay(rnd.Next(30, 100));
                }
            }
            return locker;
        }


        /// <summary>
        /// 如果这个线程还活着，就去redis做个延期操作
        /// </summary>
        /// <param name="state"></param>
        internal void RenewExpire(object state)
        {
            try
            {
                if (_redis.Get<Guid>(_lockKey) == _uniqueValue)
                {
                    _redis.PExpire(_lockKey, KEY_EXPIRE_MILLISECOND);

                    if (!_disposing && (DateTime.Now - _lockTime).TotalMilliseconds + KEY_EXPIRE_MILLISECOND < _maxLockTimeMilliSecond)
                    {
                        _timer.Change(RENEW_TIME, Timeout.Infinite);
                    }
                }
            }
            catch //disposing
            {

            }
        }

        /// <summary>
        /// 释放锁，如果是自己创建的就删除键值
        /// </summary>
        public void Dispose()
        {
            _disposing = true;
            if (_timer != null)
            {
                _timer.Dispose();
            }
            if (Success && _redis.Get<Guid>(_lockKey) == _uniqueValue)
            {
                _redis.Del(_lockKey);
            }
        }
    }
}
