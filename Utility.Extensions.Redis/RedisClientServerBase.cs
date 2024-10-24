using ServiceStack;
using ServiceStack.Redis;
using System;

namespace Utility.Extensions.Redis;
public partial class RedisClientBase
{
    /// <summary>
    /// Utc1970年1月1日
    /// </summary>
    static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    long GetUnixTimestamp(DateTimeOffset time)
    {
        return (long)(time.ToUniversalTime() - Epoch).TotalSeconds;
    }

    /// <summary>
    /// 返回当前数据里面keys的数量
    /// </summary>
    /// <returns></returns>
    public virtual long DbSize()
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.DbSize;
    }

    /// <summary>
    /// 返回redis服务器时间
    /// </summary>
    /// <returns></returns>
    public virtual DateTimeOffset Time()
    {
        //返回内容包含两个元素UNIX时间戳（单位：秒）,微秒
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.Time();

        long seconds = long.Parse(data[0].FromUtf8Bytes());
        double milliseconds = long.Parse(data[1].FromUtf8Bytes()) / 1000.0;
        return Epoch.AddSeconds(seconds).AddMilliseconds(milliseconds).ToLocalTime();
    }
}
