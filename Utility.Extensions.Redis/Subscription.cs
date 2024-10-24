using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Extensions.Redis
{
    /// <summary>
    /// 订阅信息
    /// </summary>
    public class Subscription : IDisposable
    {
        /// <summary>
        /// 订阅信息
        /// </summary>
        /// <param name="pubSubServer"></param>
        public Subscription(RedisPubSubServer pubSubServer)
        {
            PubSubServer = pubSubServer ?? throw new ArgumentNullException(nameof(pubSubServer));
        }

        /// <summary>
        ///
        /// </summary>
        public RedisPubSubServer PubSubServer { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            PubSubServer.Stop();
            PubSubServer.Dispose();
        }

        /// <summary>
        /// 启动订阅
        /// </summary>
        /// <param name="onMessage">channel,message</param>
        /// <param name="onFailover">Called before attempting to Failover to a new redis master</param>
        /// <param name="onStart">Called each time a new Connection is Started</param>
        public void Start(Action<string, string> onMessage, Action onStart = null, Action onFailover = null)
        {
            PubSubServer.OnMessage = onMessage;
            if (onFailover != null)
            {
                PubSubServer.OnFailover = s => onFailover();
            }
            if (onStart != null)
            {
                PubSubServer.OnStart = onStart;
            }
            PubSubServer.AutoRestart = true;
            PubSubServer.Start();
        }

        /// <summary>
        /// 结束订阅
        /// </summary>
        public void Stop()
        {
            PubSubServer.Stop();
        }
    }
}