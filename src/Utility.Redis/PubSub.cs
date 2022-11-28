using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Redis
{
    public partial class RedisClient
    {
        //发布订阅

        /// <summary>
        /// 将信息 message 发送到指定的频道 channel,返回收到消息的客户端数量
        /// </summary>
        /// <returns></returns>
        public long Publish<T>(string channel, T message, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.Publish(channel, Serialize<T>(message, serializerType));
            }
        }

        /// <summary>
        /// 将信息 message 发送到指定的频道 channel,返回收到消息的客户端数量
        /// </summary>
        /// <returns></returns>
        public long RawPublish(string channel, string message)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.PublishMessage(channel, message);
            }
        }

        Dictionary<string, RedisPubSubServer> subscriptions = new Dictionary<string, RedisPubSubServer>();

        /// <summary>
        /// 订阅给指定频道的信息
        /// </summary>
        /// <returns></returns>
        public Subscription Subscribe(params string[] channels)
        {
            return new Subscription(new RedisPubSubServer(ClientsManager, channels));
        }
    }
}
