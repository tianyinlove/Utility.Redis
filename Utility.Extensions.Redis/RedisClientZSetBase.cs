using ServiceStack;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Extensions.Redis;
public partial class RedisClientBase
{
    /// <summary>
    /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="score"></param>
    /// <param name="member"></param>
    /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
    public virtual long ZAdd<T>(string key, double score, T member)
    {
        if (string.IsNullOrWhiteSpace(key) || member == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZAdd(Prefix + key, score, Serialize<T>(member));
    }

    /// <summary>
    /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="memberValues"></param>
    public virtual long ZAdd<T>(string key, List<KeyValuePair<T, long>> memberValues)
    {
        if (string.IsNullOrWhiteSpace(key) || memberValues == null || memberValues.Count == 0)
        {
            return 0;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var pairs = memberValues.Select(d => new KeyValuePair<byte[], long>(Serialize<T>(d.Key), d.Value)).ToList();
        return client.ZAdd(Prefix + key, pairs);
    }

    /// <summary>
    /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="memberValues"></param>
    public virtual long ZAdd<T>(string key, List<KeyValuePair<T, double>> memberValues)
    {
        if (string.IsNullOrWhiteSpace(key) || memberValues == null || memberValues.Count == 0)
        {
            return 0;
        }

        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var pairs = memberValues.Select(d => new KeyValuePair<byte[], double>(Serialize<T>(d.Key), d.Value)).ToList();
        return client.ZAdd(Prefix + key, pairs);
    }

    /// <summary>
    /// 将所有指定成员添加到键为key有序集合（sorted set）里面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="score"></param>
    /// <param name="member"></param>
    /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
    public virtual long ZAdd<T>(string key, long score, T member)
    {
        if (string.IsNullOrWhiteSpace(key) || member == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZAdd(Prefix + key, score, Serialize<T>(member));
    }

    /// <summary>
    /// 删除成员，返回的是从有序集合中删除的成员个数，不包括不存在的成员
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    public virtual long ZRem<T>(string key, T member)
    {
        if (string.IsNullOrWhiteSpace(key) || member == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRem(Prefix + key, Serialize<T>(member));
    }

    /// <summary>
    /// 删除成员，返回的是从有序集合中删除的成员个数，不包括不存在的成员
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    public virtual long ZRem<T>(string key, List<T> members)
    {
        if (string.IsNullOrWhiteSpace(key) || members == null || members.Count == 0)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRem(Prefix + key, members.Select(Serialize<T>).ToArray());
    }

    /// <summary>
    /// 为有序集key的成员member的score值加上增量increment
    /// 如果key中不存在member，就在key中添加一个member，score是increment（就好像它之前的score是0.0）。如果key不存在，就创建一个只含有指定member成员的有序集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="increment"></param>
    /// <param name="member"></param>
    /// <returns>member成员的新score值，以字符串形式表示</returns>
    public virtual double ZIncrBy<T>(string key, long increment, T member)
    {
        if (string.IsNullOrWhiteSpace(key) || member == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZIncrBy(Prefix + key, increment, Serialize<T>(member));
    }

    /// <summary>
    /// 为有序集key的成员member的score值加上增量increment
    /// 如果key中不存在member，就在key中添加一个member，score是increment（就好像它之前的score是0.0）。如果key不存在，就创建一个只含有指定member成员的有序集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="increment"></param>
    /// <param name="member"></param>
    /// <returns>member成员的新score值，以字符串形式表示</returns>
    public virtual double ZIncrBy<T>(string key, double increment, T member)
    {
        if (string.IsNullOrWhiteSpace(key) || member == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZIncrBy(Prefix + key, increment, Serialize<T>(member));
    }

    /// <summary>
    /// 返回key的有序集元素个数
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual long ZCard(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZCard(Prefix + key);
    }

    /// <summary>
    /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员
    /// </summary>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public virtual long ZCount(string key, double min, double max)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZCount(Prefix + key, min, max);
    }

    /// <summary>
    /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员
    /// </summary>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public virtual long ZCount(string key, long min, long max)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZCount(Prefix + key, min, max);
    }

    /// <summary>
    /// Returns the specified range of elements in the sorted set stored at key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public virtual List<T> ZRange<T>(string key, int start, int stop)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRange(Prefix + key, start, stop).Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public virtual List<T> ZRangeByScore<T>(string key, long min, long max, int? skip = null, int? take = null)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRangeByScore(Prefix + key, min, max, skip, take).Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 指定分数范围的元素列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public virtual List<T> ZRangeByScore<T>(string key, double min, double max, int? skip = null, int? take = null)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRangeByScore(Prefix + key, min, max, skip, take).Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public virtual List<KeyValuePair<T, long>> ZRangeByScoreWithScores<T>(string key, long min, long max, int? skip = null, int? take = null)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.ZRangeByScoreWithScores(Prefix + key, min, max, skip, take);
        var result = new List<KeyValuePair<T, long>>();
        for (int i = 0; i < data.Length; i += 2)
        {
            result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i]), long.Parse(data[i + 1].FromUtf8Bytes())));
        }
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public virtual List<KeyValuePair<T, long>> ZRangeWithScores<T>(string key, int start, int stop)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.ZRangeWithScores(Prefix + key, start, stop);
        var result = new List<KeyValuePair<T, long>>();
        for (int i = 0; i < data.Length; i += 2)
        {
            result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i]), long.Parse(data[i + 1].FromUtf8Bytes())));
        }
        return result;
    }

    /// <summary>
    /// Returns the specified range of elements in the sorted set stored at key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public virtual List<T> ZRevRange<T>(string key, int start, int stop)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRevRange(Prefix + key, start, stop).Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// Returns the specified range of elements in the sorted set stored at key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public virtual List<KeyValuePair<T, long>> ZRevRangeWithScores<T>(string key, int start, int stop)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.ZRevRangeWithScores(Prefix + key, start, stop);
        var result = new List<KeyValuePair<T, long>>();
        for (int i = 0; i < data.Length; i += 2)
        {
            result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i]), long.Parse(data[i + 1].FromUtf8Bytes())));
        }
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public virtual List<T> ZRevRangeByScore<T>(string key, long min, long max, int? skip, int? take)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRevRangeByScore(Prefix + key, min, max, skip, take).Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public virtual List<T> ZRevRangeByScore<T>(string key, double min, double max, int? skip = null, int? take = null)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRevRangeByScore(Prefix + key, min, max, skip, take).Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public virtual List<KeyValuePair<T, long>> ZRevRangeByScoreWithScores<T>(string key, long min, long max, int? skip = null, int? take = null)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.ZRevRangeByScoreWithScores(Prefix + key, min, max, skip, take);
        var result = new List<KeyValuePair<T, long>>();
        for (int i = 0; i < data.Length; i += 2)
        {
            result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i]), long.Parse(data[i + 1].FromUtf8Bytes())));
        }
        return result;
    }

    /// <summary>
    /// 返回有序集key中，成员member的score值,如果member元素不是有序集key的成员，或key不存在，返回double.NaN
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    public virtual double ZScore<T>(string key, T member)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZScore(Prefix + key, Serialize<T>(member));
    }

    /// <summary>
    /// 移除有序集key中，指定排名(rank)区间内的所有成员。下标参数start和stop都以0为底，0处是分数最小的那个元素。这些索引也可是负数，表示位移从最高分处开始数。例如，-1是分数最高的元素，-2是分数第二高的，依次类推。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <returns>被移除成员的数量</returns>
    public virtual long ZRemRangeByRank(string key, int start, int stop)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRemRangeByRank(Prefix + key, start, stop);
    }

    /// <summary>
    /// 移除有序集key中，所有score值介于min和max之间(包括等于min或max)的成员 O(log(N)+M)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns>被移除成员的数量</returns>
    public virtual long ZRemRangeByScore(string key, long min, long max)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.ZRemRangeByScore(Prefix + key, min, max);
    }
}
