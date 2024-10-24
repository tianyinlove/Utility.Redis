using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace Utility.Extensions.Redis
{
    /// <summary>
    /// redis客户端基础实现
    /// </summary>
    public abstract partial class RedisClientBase : IRedisClientBase
    {
        /// <summary>
        /// 设置大key预警值
        /// </summary>
        public int WarningSize = 100 << 10;

        /// <summary>
        /// key前缀
        /// </summary>
        public abstract string KeyPrefix { get; }

        /// <summary>
        /// key前缀
        /// </summary>
        public string Prefix { get => KeyPrefix ?? ""; }

        /// <summary>
        /// 随便取个名，用来区分多个redis
        /// </summary>
        protected virtual string ClientName { get; }

        /// <summary>
        /// <para>redis连接格式文档 https://docs.servicestack.net/redis/client-managers#redis-connection-strings</para>
        /// <para>localhost</para>
        /// <para>127.0.0.1:6379</para>
        /// <para>redis://localhost:6379</para>
        /// <para>password@localhost:6379</para>
        /// <para>clientid:password@localhost:6379</para>
        /// <para>redis://clientid:password@localhost:6380?ssl=true&amp;db=1</para>
        /// <para>哨兵 sentinel://[mastername#]sentinel1,sentinel2,sentinel3</para>
        /// </summary>
        public abstract string ConnectionString { get; }

        /// <summary>
        ///
        /// </summary>
        protected abstract ILogger Logger { get; }

        private bool _disposed = false;

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing">是否确定情况下调用</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        #region 连接

        /// <summary>
        /// 获取servicestack的redis客户端
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RedisClient GetClient()
        {
            return (RedisClient)GetRedisClientsManager().GetClient();
        }

        /// <summary>
        /// 全局缓存
        /// </summary>
        internal static ConcurrentDictionary<string, RedisClientsManagerWrapper> __clients = new();

        /// <summary>
        ///
        /// </summary>
        private static readonly object __clientManagerLock = new object();

        private string _clientKey = null;

        private IRedisClientsManager GetRedisClientsManager()
        {
            _clientKey ??= !string.IsNullOrWhiteSpace(ClientName) ? ClientName : GetType().FullName;
            if (__clients.TryGetValue(_clientKey, out var wrapper) && wrapper.ConnectionString == ConnectionString)
            {
                return wrapper.ClientsManager;
            }

            lock (__clientManagerLock)
            {
                if (__clients.TryGetValue(_clientKey, out wrapper) && wrapper.ConnectionString == ConnectionString)
                {
                    return wrapper.ClientsManager;
                }

                wrapper?.ClientsManager?.Dispose();
                __clients[_clientKey] = wrapper = new() { ClientsManager = BuildClientsManager(ConnectionString), ConnectionString = ConnectionString };
                return wrapper.ClientsManager;
            }
        }

        private IRedisClientsManager BuildClientsManager(string configuration)
        {
            // 连接自建的哨兵服务器sentinel://[mastername#]sentinel1,sentinel2,sentinel3?query
            if (configuration.StartsWith("sentinel://", StringComparison.OrdinalIgnoreCase))
            {
                return BuildSentinelClientsManager(configuration);
            }

            if (configuration.Contains(",")) //兼容历史stackexchange配置
            {
                configuration = TranslateStackExchangeConnectionString(configuration);
            }
            RedisConfig.VerifyMasterConnections = false; //阿里云的 redis 服务目前不支持 redis 的 role 命令
            return new RedisManagerPool(configuration);
        }

        private static string TranslateStackExchangeConnectionString(string configuration)
        {
            Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);  //连接配置
            string hostAndPort = null;  //redis主机
            foreach (var item in configuration.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                if (item.Contains("="))
                {
                    var arr = item.Split('=');
                    if (arr.Length == 2 && !string.IsNullOrWhiteSpace(arr[0]) && !string.IsNullOrWhiteSpace(arr[1]))
                    {
                        options[arr[0].Trim()] = arr[1].Trim();
                    }
                }
                else
                {
                    hostAndPort = item;
                }
            }
            var querys = new List<string>();
            foreach (var option in options)
            {
                switch (option.Key.ToLower())
                {
                    case "defaultdatabase":
                        if (int.TryParse(option.Value, out int db) && db >= 0)
                        {
                            querys.Add($"db={db}");
                        }
                        break;

                    case "ssl":
                        querys.Add($"ssl={option.Value}");
                        break;

                    case "password":
                        querys.Add($"password={HttpUtility.UrlEncode(option.Value)}");
                        break;

                    case "name":
                        querys.Add($"username={option.Value}");
                        break;

                    case "connecttimeout":
                        if (int.TryParse(option.Value, out int connectTimeout))
                        {
                            querys.Add($"connecttimeout={connectTimeout}");
                        }
                        break;

                    case "synctimeout":
                        if (int.TryParse(option.Value, out int syncTimeout))
                        {
                            querys.Add($"SendTimeout={syncTimeout}&ReceiveTimeout={syncTimeout}");
                        }
                        break;

                    default:
                        break;
                }
            }
            string connectionString = $"redis://{hostAndPort}";
            if (querys.Any())
            {
                connectionString += "?" + string.Join("&", querys.ToArray());
            }
            return connectionString;
        }

        private static IRedisClientsManager BuildSentinelClientsManager(string configuration)
        {
            var str = configuration["sentinel://".Length..];
            string masterName = null;
            string query = null;
            if (str.Contains("#"))
            {
                masterName = str[..str.IndexOf("#")];
                str = str[(str.IndexOf("#") + 1)..];
            }

            if (str.Contains('?'))
            {
                query = str[(str.IndexOf("?") + 1)..];
                str = str[..str.IndexOf("?")];
            }

            var hosts = str.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Trim());
            var redisSentinel = new RedisSentinel(hosts, masterName) { ScanForOtherSentinels = false };
            if (!string.IsNullOrWhiteSpace(query))
            {
                redisSentinel.HostFilter = host => $"{host}?{query}";
            }
            return redisSentinel.Start();
        }

        #endregion 连接

        #region 序列化

        private static readonly JsonSerializerSettings __jsonSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore, // 忽略空值
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
        };

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public virtual byte[] Serialize<T>(T data)
        {
            if (data == null)
            {
                return null;
            }
            if (data is byte[] rawVal)
            {
                return rawVal;
            }
            if (data is string str)
            {
                return str.ToUtf8Bytes();
            }
            try
            {
                return JsonConvert.SerializeObject(data, Formatting.None, __jsonSetting).ToUtf8Bytes();
            }
            catch (Exception err)
            {
                Logger?.LogWarning(err, "redis serialize error");
                throw;
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <returns></returns>
        private byte[][] Serialize<T>(List<T> data)
        {
            if (data == null)
            {
                return null;
            }
            return data.Select(Serialize<T>).ToArray();
        }

        private static readonly Type BytesType = typeof(byte[]);
        private static readonly Type StringType = typeof(string);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual T Deserialize<T>(byte[] value)
        {
            if (value == null)
            {
                return default;
            }
            var valueType = typeof(T);
            if (valueType == BytesType)
            {
                return (T)(object)value;
            }

            string json = value.FromUtf8Bytes();
            if (valueType == StringType
                && !(json.StartsWith("\"") && json.EndsWith("\"")))
            {
                return (T)(object)json;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json, __jsonSetting);
            }
            catch (Exception err)
            {
                Logger?.LogWarning(err, "redis deserialize error {json}", json);
                throw;
            }
        }

        #endregion 序列化
    }
}