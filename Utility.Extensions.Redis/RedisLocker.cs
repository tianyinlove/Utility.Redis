using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Utility.Extensions.Redis
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
        internal readonly string _lockValue;

        /// <summary>
        /// 锁键值
        /// </summary>
        internal readonly string _lockKey;

        /// <summary>
        /// redis客户端实例
        /// </summary>
        internal readonly IRedisClientBase _redis;

        /// <summary>
        ///
        /// </summary>
        private bool _disposed = false;

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
        public bool Success { get; internal set; } = false;

        /// <summary>
        /// 申请成功
        /// </summary>
        /// <param name="redis"></param>
        /// <param name="lockKey"></param>
        /// <param name="lockValue"></param>
        /// <param name="maxLockTimeMilliSecond"></param>
        internal RedisLocker(IRedisClientBase redis, string lockKey, string lockValue, int maxLockTimeMilliSecond)
        {
            Success = true;
            _lockTime = DateTime.Now;
            _lockValue = lockValue;
            _lockKey = lockKey;
            _redis = redis;
            _maxLockTimeMilliSecond = maxLockTimeMilliSecond;

            _timer = new Timer(RenewExpire, null, RENEW_TIME, Timeout.Infinite);
        }

        internal RedisLocker()
        {
        }

        /// <summary>
        /// 如果这个线程还活着，就去redis做个延期操作
        /// </summary>
        /// <param name="state"></param>
        internal void RenewExpire(object state)
        {
            try
            {
                if (_redis?.Get<string>(_lockKey) == _lockValue)
                {
                    _redis?.PExpire(_lockKey, KEY_EXPIRE_MILLISECOND);

                    if (!_disposed && (DateTime.Now - _lockTime).TotalMilliseconds + KEY_EXPIRE_MILLISECOND < _maxLockTimeMilliSecond)
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
            if (_disposed)
            {
                return;
            }

            _timer?.Dispose();
            if (Success && _redis?.Get<string>(_lockKey) == _lockValue)
            {
                _redis?.Del(_lockKey);
            }

            _disposed = true;
        }
    }
}