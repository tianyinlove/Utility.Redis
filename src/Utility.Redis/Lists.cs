using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Redis
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RedisClient
    {
        #region Lists/队列

        /// <summary>
        /// 队列总字节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SizeOfList(string key)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                int pagesize = 10000;
                long totalSize = 0L;
                var fieldCount = client.LLen(key);
                for (int start = 0; start < fieldCount; start += pagesize)
                {
                    var data = client.LRange(key, start, start + pagesize);
                    totalSize += data?.Sum(d => d.LongLength) ?? 0L;
                }
                return totalSize;
            }
        }

        /// <summary>
        /// 返回存储在 key 的列表里指定范围内的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> LRange<T>(string key, int start, int end, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.LRange(key, start, end);
                return data.Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
        }


        /// <summary>
        /// 从存于 key 的列表里移除前 count 次出现的值为 value 的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count">0: 移除所有值为 value 的元素;正数: 从头往尾移除值为 value 的元素；负数: 从尾往头移除值为 value 的元素</param>
        /// <param name="value"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public long LRem<T>(string key, int count, T value, SerializerType serializerType = 0)
        {
            if (value == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.LRem(key, count, Serialize<T>(value, serializerType));
            }
        }

        /// <summary>
        /// 移除并且返回 key 对应的 list 的第一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T LPop<T>(string key, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return Deserialize<T>(client.LPop(key), serializerType);
            }
        }

        /// <summary>
        /// 是命令 LPOP 的阻塞版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T BLPop<T>(string key, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BLPop(key, timeoutSeconds);
                if (data == null || data.Length < 2)
                {
                    return default(T);
                }

                return Deserialize<T>(data[1], serializerType);
            }
        }

#if NETFRAMEWORK
        /// <summary>
        /// 是命令 LPOP 的阻塞版本,按参数 key 的先后顺序依次检查各个列表，弹出第一个非空列表的头元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public Tuple<string, T> BLPop<T>(List<string> keys, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BLPop(keys.ToArray(), timeoutSeconds);
                if (data == null || data.Length < 2)
                {
                    return new Tuple<string, T>(null, default);
                }

                return new Tuple<string, T>(data[0].FromUtf8Bytes(), Deserialize<T>(data[1], serializerType));
            }
        }
#else
        /// <summary>
        /// 是命令 LPOP 的阻塞版本,按参数 key 的先后顺序依次检查各个列表，弹出第一个非空列表的头元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public (string key, T value) BLPop<T>(List<string> keys, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BLPop(keys.ToArray(), timeoutSeconds);
                if (data == null || data.Length < 2)
                {
                    return (null, default);
                }

                return (data[0].FromUtf8Bytes(), Deserialize<T>(data[1], serializerType));
            }
        }
#endif

        /// <summary>
        /// 是命令 RPop 的阻塞版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T BRPop<T>(string key, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BRPop(key, timeoutSeconds);
                if (data == null || data.Length < 2)
                {
                    return default;
                }

                return Deserialize<T>(data[1], serializerType);
            }
        }
#if NETFRAMEWORK
        /// <summary>
        /// 是命令 RPOP 的阻塞版本,按照给出的 key 顺序查看 list，并在找到的第一个非空 list 的尾部弹出一个元素。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public Tuple<string, T> BRPop<T>(List<string> keys, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BRPop(keys.ToArray(), timeoutSeconds);
                if (data == null || data.Length < 2)
                {
                    return new Tuple<string, T>(null, default);
                }

                return new Tuple<string, T>(data[0].FromUtf8Bytes(), Deserialize<T>(data[1], serializerType));
            }
        }
#else
        /// <summary>
        /// 是命令 RPOP 的阻塞版本,按照给出的 key 顺序查看 list，并在找到的第一个非空 list 的尾部弹出一个元素。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public (string key, T value) BRPop<T>(List<string> keys, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BRPop(keys.ToArray(), timeoutSeconds);
                if (data == null || data.Length < 2)
                {
                    return (null, default);
                }

                return (data[0].FromUtf8Bytes(), Deserialize<T>(data[1], serializerType));
            }
        }
#endif

        /// <summary>
        /// 是 RPOPLPUSH 的阻塞版本。 当 source 包含元素的时候，这个命令表现得跟 RPOPLPUSH 一模一样。 当 source 是空的时候，Redis将会阻塞这个连接，直到另一个客户端 push 元素进入或者达到 timeout 时限
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromList"></param>
        /// <param name="toList"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T BRPopLPush<T>(string fromList, string toList, int timeoutSeconds, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.BRPopLPush(fromList, toList, timeoutSeconds);
                if (data == null)
                {
                    return default(T);
                }

                return Deserialize<T>(data, serializerType);
            }
        }

        /// <summary>
        /// 原子性地返回并移除存储在 source 的列表的最后一个元素（列表尾部元素）， 并把该元素放入存储在 destination 的列表的第一个元素位置（列表头部）.如果 source 不存在，那么会返回 nil 值，并且不会执行任何操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromList"></param>
        /// <param name="toList"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T RPopLPush<T>(string fromList, string toList, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.RPopLPush(fromList, toList);
                if (data == null)
                {
                    return default(T);
                }

                return Deserialize<T>(data, serializerType);
            }
        }


        /// <summary>
        /// 移除并且返回 key 对应的 list 的最后一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T RPop<T>(string key, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return Deserialize<T>(client.RPop(key), serializerType);
            }
        }

        /// <summary>
        /// 返回列表里的元素,下标是从0开始索引的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index">0 是表示第一个元素， 1 表示第二个元素,-1 表示最后一个元素，-2 表示倒数第二个元素</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T LIndex<T>(string key, int index, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return Deserialize<T>(client.LIndex(key, index), serializerType);
            }
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
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public void LInsert<T>(string key, bool before, T pivot, T value, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                client.LInsert(key, before, Serialize<T>(pivot, serializerType), Serialize<T>(value, serializerType));
            }
        }

        /// <summary>
        /// 返回存储在 key 里的list的长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long LLen(string key)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.LLen(key);
            }
        }

        /// <summary>
        /// 将所有指定的值插入到存于 key 的列表的尾部
        /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="datas"></param>
        /// <param name="serializerType"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        public long LPush<T>(string key, T[] datas, SerializerType serializerType = 0)
        {
            datas = datas.Where(d => d != null).ToArray();
            if (datas.Length == 0)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.LPush(key, datas.Select(d => Serialize<T>(d, serializerType)).ToArray());
            }
        }

        /// <summary>
        /// 只有当 key 已经存在并且存着一个 list 的时候，在这个 key 下面的 list 的头部插入 value。 与 LPUSH 相反，当 key 不存在的时候不会进行任何操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="serializerType"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        public long LPushX<T>(string key, T value, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.LPushX(key, Serialize<T>(value, serializerType));
            }
        }

        /// <summary>
        /// 将所有指定的值插入到存于 key 的列表的尾部
        /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="datas"></param>
        /// <param name="serializerType"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        public long RPush<T>(string key, T[] datas, SerializerType serializerType = 0)
        {
            datas = datas.Where(d => d != null).ToArray();
            if (datas.Length == 0)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.RPush(key, datas.Select(d => Serialize<T>(d, serializerType)).ToArray());
            }
        }

        /// <summary>
        /// 只有当 key 已经存在并且存着一个 list 的时候，在这个 key 下面的 list 的尾部插入 value。 与 LPUSH 相反，当 key 不存在的时候不会进行任何操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="serializerType"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        public long RPushX<T>(string key, T value, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.RPushX(key, Serialize<T>(value, serializerType));
            }
        }

        /// <summary>
        /// 将所有指定的值插入到存于 key 的列表的尾部
        /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="serializerType"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        public long RPush<T>(string key, T value, SerializerType serializerType = 0)
        {
            if (value == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.RPush(key, Serialize<T>(value, serializerType));
            }
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
        public void LTrim(string key, int start, int stop)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                client.LTrim(key, start, stop);
            }
        }
        #endregion
    }
}
