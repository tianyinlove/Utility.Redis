using ServiceStack.Redis;
using System.Linq;

namespace Utility.Extensions.Redis;
public partial class RedisClientBase
{
    /// <summary>
    /// 将信息 message 发送到指定的频道 channel,返回收到消息的客户端数量
    /// </summary>
    /// <returns></returns>
    public virtual long Publish<T>(string channel, T message)
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.Publish(Prefix + channel, Serialize<T>(message));
    }

    /// <summary>
    /// 订阅给指定频道的信息
    /// </summary>
    /// <returns></returns>
    public virtual Subscription Subscribe(params string[] channels)
    {
        return new Subscription(new RedisPubSubServer(GetRedisClientsManager(), channels.Select(d => Prefix + d).ToArray()));
    }
}
