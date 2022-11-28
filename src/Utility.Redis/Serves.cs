using ServiceStack;
using System;

namespace Utility.Redis
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RedisClient
    {
        /// <summary>
        /// Utc1970年1月1日
        /// </summary>
        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        long GetUnixTimestamp(DateTime time)
        {
            return (long)(time.ToUniversalTime() - Epoch).TotalSeconds;
        }
        #region Server

        /// <summary>
        /// 返回当前数据里面keys的数量
        /// </summary>
        /// <returns></returns>
        public long DbSize()
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.DbSize;
            }
        }

        /// <summary>
        /// 返回redis服务器时间
        /// </summary>
        /// <returns></returns>
        public DateTime Time()
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.Time();
                //返回内容包含两个元素UNIX时间戳（单位：秒）,微秒
                return Epoch.AddSeconds(long.Parse(data[0].FromUtf8Bytes()))
                    .AddMilliseconds(long.Parse(data[1].FromUtf8Bytes()) / 1000.0)
                    .ToLocalTime();
            }
        }
        #endregion
    }
}
