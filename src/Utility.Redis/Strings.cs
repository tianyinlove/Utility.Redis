using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Redis
{
    public partial class RedisClient
    {
        #region Strings

        /// <summary>
        /// 不序列化，直接保存字符串
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresAt"></param>
        /// <returns></returns>
        public bool RawSet(string key, string value, TimeSpan? expiresAt = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = value.ToUtf8Bytes();
                if (expiresAt == null)
                {
                    return client.Set(key, data);
                }
                else
                {
                    client.Set(key, data, expirySeconds: (int)(expiresAt.Value.TotalSeconds));
                    return true;
                }
            }
        }

        /// <summary>
        /// 将键key设定为指定的val值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="expiresAt">EX seconds – 设置键key的过期时间，单位时秒PX milliseconds – 设置键key的过期时间，单位时毫秒</param>
        /// <param name="exists">false:NX – 只有键key不存在的时候才会设置key的值, true:XX – 只有键key存在的时候才会设置key的值, null:不指定</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public bool Set<T>(string key, T val, TimeSpan? expiresAt = null, bool? exists = null, SerializerType serializerType = 0)
        {
            if (val == null || string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = Serialize<T>(val, serializerType);
                if (exists == null)
                {
                    if (expiresAt == null)
                    {
                        return client.Set(key, data);
                    }
                    else
                    {
                        client.Set(key, data, expirySeconds: (int)(expiresAt.Value.TotalSeconds));
                        return true;
                    }
                }
                else
                {
                    if (expiresAt == null)
                    {
                        return client.Set(key, data, exists: exists.Value);
                    }
                    else
                    {
                        return client.Set(key, data, exists: exists.Value, expirySeconds: (int)(expiresAt.Value.TotalSeconds));
                    }
                }
            }
        }

        /// <summary>
        /// 将key设置值为value，如果key不存在，这种情况下等同SET命令。 当key存在时，什么也不做
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public bool SetNx<T>(string key, T val, SerializerType serializerType = 0)
        {
            if (val == null || string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = Serialize<T>(val, serializerType);
                return client.SetNX(key, data) == 1;
            }
        }

        /// <summary>
        /// 设置key对应字符串value，并且设置key在给定的seconds时间之后超时过期。等效于原子SET key value + EXPIRE key seconds
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <param name="val"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public void SetEx<T>(string key, int seconds, T val, SerializerType serializerType = 0)
        {
            if (val == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = Serialize<T>(val, serializerType);
                client.SetEx(key, seconds, data);
            }
        }

        /// <summary>
        /// 字符串总字节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SizeOfString(string key)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.Get(key)?.Length ?? 0L;
            }
        }

        /// <summary>
        /// 返回key的value,如果key不存在返回default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T Get<T>(string key, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return Deserialize<T>(client.Get(key), serializerType);
            }
        }

        /// <summary>
        /// 对存储在指定key的数值执行原子的加1操作,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <returns>增加之后的value值</returns>
        public long Incr(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.Incr(key);
            }
        }

        /// <summary>
        /// 将key对应的数字加decrement,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns>增加之后的value值</returns>
        public long IncrBy(string key, int increment)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.IncrBy(key, increment);
            }
        }

        /// <summary>
        /// 将key对应的数字加decrement,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns>增加之后的value值</returns>
        public double IncrByFloat(string key, double increment)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.IncrByFloat(key, increment);
            }
        }

        /// <summary>
        /// 对存储在指定key的数值执行原子的减1操作,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <returns>减小之后的value</returns>
        public long Decr(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.Decr(key);
            }
        }

        /// <summary>
        /// 将key对应的数字减decrement,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns>减小之后的value</returns>
        public long DecrBy(string key, int count)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.DecrBy(key, count);
            }
        }

        /// <summary>
        /// 返回指定多个字段的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="ignoreEmptyValue">是否排除哈希集中不存在的字段</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public Dictionary<string, T> MGet<T>(List<string> keys, bool ignoreEmptyValue = true, SerializerType serializerType = 0)
        {
            if (keys == null || keys.Count == 0)
            {
                return new Dictionary<string, T>();
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.MGet(keys.ToArray());
                Dictionary<string, T> result = new Dictionary<string, T>();
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] != null)
                    {
                        result[keys[i]] = Deserialize<T>(data[i], serializerType);
                    }
                    else if (!ignoreEmptyValue)
                    {
                        result[keys[i]] = default(T);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 对应给定的keys到他们相应的values上,MSET是原子的且不会失败
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="serializerType"></param>
        public void MSet<T>(Dictionary<string, T> data, SerializerType serializerType = 0)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var keys = data.Keys.ToArray();
                client.MSet(keys, keys.Select(key => Serialize<T>(data[key], serializerType)).ToArray());
            }
        }

        /// <summary>
        /// 把 value 追加到原来值（value）的结尾,返回append后字符串值（value）的长度
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Append(string key, string value)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.Append(key, value.ToUtf8Bytes());
            }
        }
        //TODO:APPEND,BITCOUNT,BITPOS,GETBIT,GETRANGE,GETSET,INCRBYFLOAT,MSETNX,PSETEX,SETBIT,SETRANGE
        #endregion
    }
}
