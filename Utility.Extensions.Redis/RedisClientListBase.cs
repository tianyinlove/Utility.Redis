using ServiceStack;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Extensions.Redis;
public partial class RedisClientBase
{
    /// <summary>
    /// 返回存储在 key 的列表里指定范围内的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public virtual List<T> LRange<T>(string key, int start, int end)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.LRange(Prefix + key, start, end);
        return data.Select(Deserialize<T>).ToList();
    }

    /// <summary>
    /// 从存于 key 的列表里移除前 count 次出现的值为 value 的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="count">0: 移除所有值为 value 的元素;正数: 从头往尾移除值为 value 的元素；负数: 从尾往头移除值为 value 的元素</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual long LRem<T>(string key, int count, T value)
    {
        if (value == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.LRem(Prefix + key, count, Serialize<T>(value));
    }

    /// <summary>
    /// 移除并且返回 key 对应的 list 的第一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual T LPop<T>(string key)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return Deserialize<T>(client.LPop(Prefix + key));
    }

    /// <summary>
    /// 是命令 LPOP 的阻塞版本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
    /// <returns></returns>
    public virtual T BLPop<T>(string key, int timeoutSeconds)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.BLPop(Prefix + key, timeoutSeconds);
        if (data == null || data.Length < 2)
        {
            return default(T);
        }

        return Deserialize<T>(data[1]);
    }

    /// <summary>
    /// 是命令 LPOP 的阻塞版本,按参数 key 的先后顺序依次检查各个列表，弹出第一个非空列表的头元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
    /// <returns></returns>
    public virtual (string key, T value) BLPop<T>(string[] keys, int timeoutSeconds)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.BLPop(keys.Select(key => Prefix + key).ToArray(), timeoutSeconds);
        if (data == null || data.Length < 2)
        {
            return (null, default);
        }

        return (data[0].FromUtf8Bytes()[Prefix.Length..], Deserialize<T>(data[1]));
    }

    /// <summary>
    /// 是命令 RPop 的阻塞版本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
    /// <returns></returns>
    public virtual T BRPop<T>(string key, int timeoutSeconds)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.BRPop(Prefix + key, timeoutSeconds);
        if (data == null || data.Length < 2)
        {
            return default;
        }

        return Deserialize<T>(data[1]);
    }

    /// <summary>
    /// 是命令 RPOP 的阻塞版本,按照给出的 key 顺序查看 list，并在找到的第一个非空 list 的尾部弹出一个元素。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
    /// <returns></returns>
    public virtual (string key, T value) BRPop<T>(string[] keys, int timeoutSeconds)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.BRPop(keys.Select(key => Prefix + key).ToArray(), timeoutSeconds);
        if (data == null || data.Length < 2)
        {
            return (null, default);
        }

        return (data[0].FromUtf8Bytes()[Prefix.Length..], Deserialize<T>(data[1]));
    }

    /// <summary>
    /// 是 RPOPLPUSH 的阻塞版本。 当 source 包含元素的时候，这个命令表现得跟 RPOPLPUSH 一模一样。 当 source 是空的时候，Redis将会阻塞这个连接，直到另一个客户端 push 元素进入或者达到 timeout 时限
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fromList"></param>
    /// <param name="toList"></param>
    /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
    /// <returns></returns>
    public virtual T BRPopLPush<T>(string fromList, string toList, int timeoutSeconds)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.BRPopLPush(Prefix + fromList, Prefix + toList, timeoutSeconds);
        if (data == null)
        {
            return default(T);
        }

        return Deserialize<T>(data);
    }

    /// <summary>
    /// 原子性地返回并移除存储在 source 的列表的最后一个元素（列表尾部元素）， 并把该元素放入存储在 destination 的列表的第一个元素位置（列表头部）.如果 source 不存在，那么会返回 nil 值，并且不会执行任何操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fromList"></param>
    /// <param name="toList"></param>
    /// <returns></returns>
    public virtual T RPopLPush<T>(string fromList, string toList)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        var data = client.RPopLPush(Prefix + fromList, Prefix + toList);
        if (data == null)
        {
            return default(T);
        }

        return Deserialize<T>(data);
    }

    /// <summary>
    /// 移除并且返回 key 对应的 list 的最后一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual T RPop<T>(string key)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return Deserialize<T>(client.RPop(Prefix + key));
    }

    /// <summary>
    /// 返回列表里的元素,下标是从0开始索引的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="index">0 是表示第一个元素， 1 表示第二个元素,-1 表示最后一个元素，-2 表示倒数第二个元素</param>
    /// <returns></returns>
    public virtual T LIndex<T>(string key, int index)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return Deserialize<T>(client.LIndex(Prefix + key, index));
    }

    /// <summary>
    /// 把 value 插入存于 key 的列表中在基准值 pivot 的前面或后面,当 key 不存在时，这个list会被看作是空list，任何操作都不会发生
    /// 返回经过插入操作后的list长度，或者当 pivot 值找不到的时候返回 -1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="before"></param>
    /// <param name="pivot"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual void LInsert<T>(string key, bool before, T pivot, T value)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        client.LInsert(Prefix + key, before, Serialize<T>(pivot), Serialize<T>(value));
    }

    /// <summary>
    /// 返回存储在 key 里的list的长度
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual long LLen(string key)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.LLen(Prefix + key);
    }

    /// <summary>
    /// 将所有指定的值插入到存于 key 的列表的尾部
    /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns>在 push 操作后的 list 长度</returns>
    public virtual long LPush<T>(string key, T[] values)
    {
        values = values.Where(d => d != null).ToArray();
        if (values.Length == 0)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.LPush(Prefix + key, values.Select(Serialize<T>).ToArray());
    }

    /// <summary>
    /// 只有当 key 已经存在并且存着一个 list 的时候，在这个 key 下面的 list 的头部插入 value。 与 LPUSH 相反，当 key 不存在的时候不会进行任何操作。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>在 push 操作后的 list 长度</returns>
    public virtual long LPushX<T>(string key, T value)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.LPushX(Prefix + key, Serialize<T>(value));
    }

    /// <summary>
    /// 将所有指定的值插入到存于 key 的列表的尾部
    /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns>在 push 操作后的 list 长度</returns>
    public virtual long RPush<T>(string key, T[] values)
    {
        values = values.Where(d => d != null).ToArray();
        if (values.Length == 0)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.RPush(Prefix + key, values.Select(Serialize<T>).ToArray());
    }

    /// <summary>
    /// 只有当 key 已经存在并且存着一个 list 的时候，在这个 key 下面的 list 的尾部插入 value。 与 LPUSH 相反，当 key 不存在的时候不会进行任何操作。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>在 push 操作后的 list 长度</returns>
    public virtual long RPushX<T>(string key, T value)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.RPushX(Prefix + key, Serialize<T>(value));
    }

    /// <summary>
    /// 将所有指定的值插入到存于 key 的列表的尾部
    /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>在 push 操作后的 list 长度</returns>
    public virtual long RPush<T>(string key, T value)
    {
        if (value == null)
        {
            return 0;
        }
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.RPush(Prefix + key, Serialize<T>(value));
    }

    /// <summary>
    /// 修剪(trim)一个已存在的 list，这样 list 就会只包含指定范围的指定元素。
    /// start 和 stop 都是由0开始计数的
    /// start 和 end 也可以用负数来表示与表尾的偏移量，比如 -1 表示列表里的最后一个元素， -2 表示倒数第二个
    /// 超过范围的下标并不会产生错误：如果 start 超过列表尾部，或者 start > end 结果会是列表变成空表（即该 key 会被移除）。
    /// 如果 end 超过列表尾部，Redis 会将其当作列表的最后一个元素。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    public virtual void LTrim(string key, int start, int stop)
    {
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        client.LTrim(Prefix + key, start, stop);
    }
}
