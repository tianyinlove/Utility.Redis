using ServiceStack.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Extensions.Redis;
public partial class RedisClientBase
{
    /// <summary>
    /// 集合中添加元素
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual bool SAdd<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SAdd(Prefix + key, Serialize<T>(value)) > 0;
    }

    /// <summary>
    /// 集合中添加元素
    /// </summary>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public virtual bool SAdd<T>(string key, List<T> values)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SAdd(Prefix + key, Serialize<T>(values)) > 0;
    }

    /// <summary>
    /// 获取集合所有元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">集合KEY</param>
    /// <returns></returns>
    public virtual List<T> SMembers<T>(string key)
    {
        List<T> result = new List<T>();

        if (string.IsNullOrWhiteSpace(key))
        {
            return result;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.SMembers(Prefix + key);
        return data.Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 返回成员 member 是否是存储的集合 key的成员.
    /// 如果member元素是集合key的成员，则返回1
    /// 如果member元素不是key的成员，或者集合key不存在，则返回0
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">集合KEY</param>
    /// <param name="member"></param>
    /// <returns></returns>
    public virtual long SIsMember<T>(string key, T member)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SIsMember(Prefix + key, Serialize<T>(member));
    }

    /// <summary>
    /// 返回集合存储的key的基数 (集合元素的数量).如果key不存在,则返回 0.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual long SCard(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SCard(Prefix + key);
    }

    /// <summary>
    /// Remove the specified members from the set stored at key. Specified members that are not a member of this set are ignored. If key does not exist, it is treated as an empty set and this command returns 0.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public virtual long SRem<T>(string key, List<T> values)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SRem(Prefix + key, Serialize<T>(values));
    }

    /// <summary>
    /// Remove the specified members from the set stored at key. Specified members that are not a member of this set are ignored. If key does not exist, it is treated as an empty set and this command returns 0.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    public virtual long SRem<T>(string key, T member)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.SRem(Prefix + key, Serialize(member));
    }
}
