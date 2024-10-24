using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Extensions.Redis;

/// <summary>
/// 
/// </summary>
public partial class RedisClientBase
{
    /// <summary>
    /// 将键key设定为指定的val值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <param name="expiresAt">EX seconds – 设置键key的过期时间，单位时秒PX milliseconds – 设置键key的过期时间，单位时毫秒</param>
    /// <param name="exists">false:NX – 只有键key不存在的时候才会设置key的值, true:XX – 只有键key存在的时候才会设置key的值, null:不指定</param>
    /// <returns></returns>
    public virtual bool Set<T>(string key, T val, TimeSpan? expiresAt = null, bool? exists = null)
    {
        if (val == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var rawValue = Serialize<T>(val);
        if (rawValue.Length > WarningSize)
        {
            Logger?.LogWarning($"reids内容超过最大值 type:set, key:{key}");
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        switch (exists, expiresAt)
        {
            case (not null, null):
                return client.Set(Prefix + key, rawValue, exists: exists.Value);
            case (not null, not null):
                return client.Set(Prefix + key, rawValue, exists: exists.Value, expiryMs: (long)expiresAt.Value.TotalMilliseconds);
            case (null, not null):
                client.Set(Prefix + key, rawValue, expirySeconds: 0, expiryMs: (long)expiresAt.Value.TotalMilliseconds);
                return true;
            case (null, null):
            default:
                return client.Set(Prefix + key, rawValue);
        }
    }

    /// <summary>
    /// 将key设置值为value，如果key不存在，这种情况下等同SET命令。 当key存在时，什么也不做
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public virtual bool SetNx<T>(string key, T val)
    {
        if (val == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }
        var rawValue = Serialize<T>(val);
        if (rawValue.Length > WarningSize)
        {
            Logger?.LogWarning($"reids内容超过最大值 type:set, key:{key}");
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SetNX(Prefix + key, rawValue) == 1;
    }

    /// <summary>
    /// 设置key对应字符串value，并且设置key在给定的seconds时间之后超时过期。等效于原子SET key value + EXPIRE key seconds
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="seconds"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public virtual void SetEx<T>(string key, int seconds, T val)
    {
        if (val == null || string.IsNullOrWhiteSpace(key))
        {
            return;
        }
        var rawValue = Serialize<T>(val);
        if (rawValue.Length > WarningSize)
        {
            Logger?.LogWarning($"reids内容超过最大值 type:set, key:{key}");
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        client.SetEx(Prefix + key, seconds, rawValue);
    }

    /// <summary>
    /// 返回key的value,如果key不存在返回default(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual T Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return Deserialize<T>(client.Get(Prefix + key));
    }

    /// <summary>
    /// 对存储在指定key的数值执行原子的加1操作,如果指定的key不存在，会先将它的值设定为0
    /// </summary>
    /// <param name="key"></param>
    /// <returns>增加之后的value值</returns>
    public virtual long Incr(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.Incr(Prefix + key);
    }

    /// <summary>
    /// 将key对应的数字加decrement,如果指定的key不存在，会先将它的值设定为0
    /// </summary>
    /// <param name="key"></param>
    /// <param name="increment"></param>
    /// <returns>增加之后的value值</returns>
    public virtual long IncrBy(string key, int increment)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.IncrBy(Prefix + key, increment);
    }

    /// <summary>
    /// 将key对应的数字加decrement,如果指定的key不存在，会先将它的值设定为0
    /// </summary>
    /// <param name="key"></param>
    /// <param name="increment"></param>
    /// <returns>增加之后的value值</returns>
    public virtual double IncrByFloat(string key, double increment)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.IncrByFloat(Prefix + key, increment);
    }

    /// <summary>
    /// 对存储在指定key的数值执行原子的减1操作,如果指定的key不存在，会先将它的值设定为0
    /// </summary>
    /// <param name="key"></param>
    /// <returns>减小之后的value</returns>
    public virtual long Decr(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.Decr(Prefix + key);
    }

    /// <summary>
    /// 将key对应的数字减decrement,如果指定的key不存在，会先将它的值设定为0
    /// </summary>
    /// <param name="key"></param>
    /// <param name="count"></param>
    /// <returns>减小之后的value</returns>
    public virtual long DecrBy(string key, int count)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.DecrBy(Prefix + key, count);
    }

    /// <summary>
    /// 返回指定多个字段的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <param name="ignoreEmptyValue">是否排除哈希集中不存在的字段</param>
    /// <returns></returns>
    public virtual Dictionary<string, T> MGet<T>(List<string> keys, bool ignoreEmptyValue = true)
    {
        if (keys == null || keys.Count == 0)
        {
            return new Dictionary<string, T>();
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.MGet(keys.Select(key => Prefix + key).ToArray());
        Dictionary<string, T> result = new Dictionary<string, T>();
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != null)
            {
                result[keys[i]] = Deserialize<T>(data[i]);
            }
            else if (!ignoreEmptyValue)
            {
                result[keys[i]] = default(T);
            }
        }
        return result;
    }

    /// <summary>
    /// 对应给定的keys到他们相应的values上,MSET是原子的且不会失败
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public virtual void MSet<T>(Dictionary<string, T> data)
    {
        if (data == null || data.Count == 0)
        {
            return;
        }
        var keys = data.Keys.ToArray();
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        client.MSet(keys.Select(key => Prefix + key).ToArray(), keys.Select(key => Serialize<T>(data[key])).ToArray());
    }

    /// <summary>
    /// 把 value 追加到原来值（value）的结尾,返回append后字符串值（value）的长度
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual long Append(string key, string value)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.Append(Prefix + key, value.ToUtf8Bytes());
    }
}
