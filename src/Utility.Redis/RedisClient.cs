using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SSRedisClient = ServiceStack.Redis.RedisClient;

namespace Utility.Redis
{
    /// <summary>
    /// Redis客户端（调用ServiceStack.Redis）
    /// </summary>
    public sealed partial class RedisClient : IDisposable
    {
        #region 字段和初始化

        /// <summary>
        /// redis连接管理
        /// </summary>
        private IRedisClientsManager ClientsManager
        {
            get
            {
                //__accessTime[_configuration] = DateTime.Now;
                return _redisClientsManager.Value;
            }
        }

        /// <summary>
        /// redis连接管理，延迟哨兵连接
        /// </summary>
        private readonly Lazy<IRedisClientsManager> _redisClientsManager;

        /// <summary>
        /// 哨兵连接
        /// </summary>
        private readonly RedisSentinel _redisSentinel;

        /// <summary>
        /// 连接配置
        /// </summary>
        private readonly string _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration">连接配置，兼容stackexchange配置</param>
        /// <param name="serializerType"></param>
        private RedisClient(string configuration, SerializerType serializerType)
        {
            _configuration = configuration;
            _serializerType = serializerType;
            if (configuration.StartsWith("sentinel://", StringComparison.OrdinalIgnoreCase)) //连接哨兵服务器sentinel://[mastername#]sentinel1,sentinel2,sentinel3
            {
                var str = configuration.Substring("sentinel://".Length);
                string masterName = null;
                string query = null;
                if (str.Contains("#"))
                {
                    masterName = str.Substring(0, str.IndexOf("#"));
                    str = str.Substring(str.IndexOf("#") + 1);
                }

                if (str.Contains('?'))
                {
                    query = str.Substring(str.IndexOf("?") + 1);
                    str = str.Substring(0, str.IndexOf("?"));
                }

                _redisSentinel = new RedisSentinel(str.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Trim()).ToArray(), masterName)
                {
                    ScanForOtherSentinels = false
                };
                if (!string.IsNullOrWhiteSpace(query))
                {
                    _redisSentinel.HostFilter = host => $"{host}?{query}";
                }
                _redisClientsManager = new Lazy<IRedisClientsManager>(() => _redisSentinel.Start());
            }
            else if (configuration.Contains(",")) //兼容历史stackexchange配置
            {
                RedisConfig.VerifyMasterConnections = false; //阿里云的 redis 服务目前不支持 redis 的 role 命令
                Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);  //连接配置
                string hostAndPort = null;  //redis主机
                foreach (var item in configuration.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Where(d => !string.IsNullOrWhiteSpace(d)))
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
                string password = null;
                string clientName = null;
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
                            password = option.Value;
                            break;
                        case "name":
                            clientName = option.Value;
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
                string connectionString;
                if (password != null)
                {
                    if (clientName != null)
                    {
                        connectionString = $"redis://{clientName}:{password}@{hostAndPort}";
                    }
                    else
                    {
                        connectionString = $"redis://{password}@{hostAndPort}";
                    }
                }
                else
                {
                    connectionString = $"redis://{hostAndPort}";
                }
                if (querys.Any())
                {
                    connectionString += "?" + string.Join("&", querys.ToArray());
                }
                _redisClientsManager = new Lazy<IRedisClientsManager>(() => new RedisManagerPool(connectionString));
            }
            else
            {
                RedisConfig.VerifyMasterConnections = false; //阿里云的 redis 服务目前不支持 redis 的 role 命令
                _redisClientsManager = new Lazy<IRedisClientsManager>(() => new RedisManagerPool(configuration));
            }


            //_connection = new PooledRedisClientManager( ConnectionMultiplexer.Connect(configuration);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            ClientsManager?.Dispose();
            _redisSentinel?.Dispose();
        }

        ConcurrentQueue<SSRedisClient> _redisClients =new ConcurrentQueue<SSRedisClient>();
        RedisClientWrapper GetClient()
        {
            if (_redisClients.TryDequeue(out SSRedisClient client))
            {
                return new RedisClientWrapper(client, _redisClients);
            }
            return new RedisClientWrapper((SSRedisClient)ClientsManager.GetClient(), _redisClients);
        }

        #endregion

        #region 客户端管理


        /// <summary>
        /// 所有共享实例
        /// </summary>
        private static readonly ConcurrentDictionary<string, RedisClient> __allInstance = new ConcurrentDictionary<string, RedisClient>(StringComparer.OrdinalIgnoreCase);

        ///// <summary>
        ///// 共享实例的最后获取时间
        ///// </summary>
        //private static readonly ConcurrentDictionary<string, DateTime> __accessTime = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// 共享实例创建锁
        /// </summary>
        private static readonly object __instanceLocker = new object();

        static RedisClient()
        {
            //new Thread(Cleaner) { IsBackground = true }.Start();
        }

        /// <summary>
        /// 清理长期不用的连接
        /// </summary>
        private static void Cleaner()
        {
            //while (true)
            //{
            //    Thread.Sleep(30 * 1000);
            //    try
            //    {
            //        var keys = __allInstance.Keys.ToList();
            //        foreach (var configuration in keys)
            //        {
            //            if (!__accessTime.TryGetValue(configuration, out DateTime lastAccessTime) || lastAccessTime < DateTime.Now.AddMinutes(-1))
            //            {
            //                if (__allInstance.TryRemove(configuration, out RedisClient client) && client != null)
            //                {
            //                    client.Dispose();
            //                    __accessTime.TryRemove(configuration, out DateTime _);
            //                }
            //            }
            //        }
            //    }
            //    catch
            //    {

            //    }
            //}
        }

        /// <summary>
        /// 创建或者Get共享实例
        /// </summary>
        /// <param name="configuration">连接配置</param>
        /// <param name="serializerType">默认的序列化方式</param>
        /// <returns></returns>
        public static RedisClient GetInstance(string configuration, SerializerType serializerType = SerializerType.Json)
        {
            if (!__allInstance.TryGetValue(configuration, out RedisClient client))
            {
                lock (__instanceLocker)
                {
                    if (!__allInstance.TryGetValue(configuration, out client))
                    {
                        //__accessTime[configuration] = DateTime.Now;
                        client = new RedisClient(configuration, serializerType);
                        __allInstance[configuration] = client;
                    }
                }
            }
            return client;
        }


        /// <summary>
        /// 创建非共享新实例
        /// </summary>
        /// <param name="configuration">连接配置</param>
        /// <param name="serializerType">默认的序列化方式</param>
        /// <returns></returns>
        public static RedisClient Create(string configuration, SerializerType serializerType = SerializerType.Json)
        {
            return new RedisClient(configuration, serializerType);
        }
        #endregion

        #region 序列化


        /// <summary>
        /// 默认的序列化方式
        /// </summary>
        private readonly SerializerType _serializerType = SerializerType.Json;

        private static readonly JsonSerializerSettings __jsonSetting = new JsonSerializerSettings
        {
            //忽略空值
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="serializerType">指定序列化类型</param>
        /// <returns></returns>
        private byte[] Serialize<T>(T data, SerializerType serializerType)
        {
            if (data == null)
            {
                return null;
            }
            if (serializerType == 0)
            {
                serializerType = _serializerType;
            }
            switch (serializerType)
            {
                //case SerializerType.Protobuf:
                //	using (MemoryStream ms = new MemoryStream())
                //	{
                //		ProtoBuf.Serializer.Serialize<T>(ms, data);
                //		return ms.ToArray();
                //	}
                case SerializerType.Json:
                default:
                    if (typeof(T) == typeof(string))
                    {
                        return ((string)(object)data).ToUtf8Bytes();
                    }
                    return JsonConvert.SerializeObject(data, Formatting.None, __jsonSetting).ToUtf8Bytes();
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="serializerType">指定序列化类型</param>
        /// <returns></returns>
        private byte[][] Serialize<T>(List<T> data, SerializerType serializerType)
        {
            if (data == null)
            {
                return null;
            }
            return data.Select(d => Serialize<T>(d, serializerType)).ToArray();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="serializerType">指定序列化类型</param>
        /// <returns></returns>
        private T Deserialize<T>(byte[] value, SerializerType serializerType)
        {
            if (value == null)
            {
                return default;
            }
            if (serializerType == 0)
            {
                serializerType = _serializerType;
            }
            switch (serializerType)
            {
                //case SerializerType.Protobuf:
                //	using (MemoryStream ms = new MemoryStream())
                //	{
                //		byte[] arr = value;
                //		ms.Write(arr, 0, arr.Length);
                //		ms.Position = 0;
                //		return ProtoBuf.Serializer.Deserialize<T>(ms);
                //	}
                case SerializerType.Json:
                default:
                    string json = value.FromUtf8Bytes();
                    if (typeof(T) == typeof(string) && !json.StartsWith("\""))
                    {
                        return (T)(object)json;
                    }
                    return JsonConvert.DeserializeObject<T>(json, __jsonSetting);
            }
        }
        #endregion

    }
}
