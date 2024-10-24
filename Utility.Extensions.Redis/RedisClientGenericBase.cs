using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extensions.Redis
{
    public partial class RedisClientBase
    {
        /// <summary>
        /// 将key重命名为newkey，如果key与newkey相同，将返回一个错误
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newkey"></param>
        /// <returns></returns>
        public virtual bool Rename(string key, string newkey)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(newkey) || key == newkey)
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            client.Rename(Prefix + key, Prefix + newkey);
            return true;
        }

        /// <summary>
        /// 当且仅当 newkey 不存在时，将 key 改名为 newkey
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newkey"></param>
        /// <returns></returns>
        public virtual bool RenameNx(string key, string newkey)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(newkey) || key == newkey)
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.RenameNx(Prefix + key, Prefix + newkey);
        }

        /// <summary>
        /// 删除key,如果删除的key不存在则直接忽略
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>被删除的keys的数量</returns>
        public virtual long Del(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return 0;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Del(keys.Select(d => Prefix + d).ToArray());
        }

        /// <summary>
        /// 序列化给定 key ，并返回被序列化的值，使用 RESTORE 命令可以将这个值反序列化为 Redis 键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual byte[] Dump(string key)
        {
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Dump(Prefix + key);
        }

        /// <summary>
        /// 反序列化给定的序列化值，并将它和给定的 key 关联。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttlMilliSecond">参数 ttl 以毫秒为单位为 key 设置生存时间；如果 ttl 为 0 ，那么不设置生存时间。</param>
        /// <returns>如果反序列化成功那么返回null ，否则返回一个错误。</returns>
        public virtual string Restore(string key, byte[] value, long ttlMilliSecond = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null || ttlMilliSecond < 0)
            {
                return null;
            }
            try
            {
                using var client = (RedisClient)GetRedisClientsManager().GetClient();
                return client.Restore(Prefix + key, ttlMilliSecond, value).FromUtf8Bytes();
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        /// <summary>
        /// 返回key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Exists(Prefix + key) == 1;
        }

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns>1 如果成功设置过期时间 0 如果key不存在或者不能设置过期时间</returns>
        public virtual bool Expire(string key, int seconds)
        {
            if (string.IsNullOrWhiteSpace(key) || seconds < 0)
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Expire(Prefix + key, seconds);
        }

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestamp"></param>
        /// <returns>1 如果设置了过期时间 0 如果没有设置过期时间，或者不能设置过期时间</returns>
        public virtual bool ExpireAt(string key, DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.ExpireAt(Prefix + key, GetUnixTimestamp(timestamp));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<string> Keys(string patten)
        {
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Keys(Prefix + patten).Select(d => d.FromUtf8Bytes()[Prefix.Length..]).ToList();
        }

        /// <summary>
        /// 扫描所有rediskey
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public virtual ScanResult Scan(ulong cursor, int count, string match)
        {
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            var scanResult = client.Scan(cursor: cursor, count: count, match: Prefix + match);
            return new ScanResult
            {
                Cursor = scanResult.Cursor,
                Results = scanResult.Results
                    .Select(d => d.FromUtf8Bytes()[Prefix.Length..])
                    .ToList()
            };
        }

        /// <summary>
        /// 移除给定key的生存时间，将这个 key 从『易失的』(带生存时间 key )转换成『持久的』(一个不带生存时间、永不过期的 key )
        /// </summary>
        /// <param name="key"></param>
        /// <returns>当生存时间移除成功时，返回 1 ,如果 key 不存在或 key 没有设置生存时间，返回 0 .</returns>
        public virtual bool Persist(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Persist(Prefix + key);
        }

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        /// <returns>1 如果成功设置过期时间 0 如果key不存在或者不能设置过期时间</returns>
        public virtual bool PExpire(string key, long milliseconds)
        {
            if (string.IsNullOrWhiteSpace(key) || milliseconds < 0)
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.PExpire(Prefix + key, milliseconds);
        }

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestamp"></param>
        /// <returns>1 如果设置了过期时间 0 如果没有设置过期时间，或者不能设置过期时间</returns>
        public virtual bool PExpireAt(string key, DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.PExpireAt(Prefix + key, (long)(timestamp.ToUniversalTime() - Epoch).TotalMilliseconds);
        }

        /// <summary>
        /// 返回key剩余的过期时间秒数。 这种反射能力允许Redis客户端检查指定key在数据集里面剩余的有效期。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Redis2.8开始如果key不存在或者已过期，返回 -2如果key没有设置过期时间（永久有效），返回 -1</returns>
        public virtual long TTL(string key)
        {
            if (!Exists(key))
            {
                return -2;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.Ttl(Prefix + key);
        }

        /// <summary>
        /// 返回key剩余的过期时间。 这种反射能力允许Redis客户端检查指定key在数据集里面剩余的有效期。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Redis2.8开始如果key不存在或者已过期，返回 -2如果key没有设置过期时间（永久有效），返回 -1</returns>
        public virtual long PTTL(string key)
        {
            if (!Exists(key))
            {
                return -2;
            }
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            return client.PTtl(Prefix + key);
        }

        /// <summary>
        /// 返回或存储key的list、 set 或sorted set 中的元素。默认是按照数值类型排序的，并且按照两个元素的双精度浮点数类型值进行比较。
        /// SORT key [BY pattern] [LIMIT offset count] [GET pattern [GET pattern ...]] [ASC|DESC] [ALPHA] [STORE destination]
        /// </summary>
        /// <param name="key"></param>
        /// <param name="byPattern">BY 选项带有一个模式（例 weight_* ），用于生成用于排序的 Key,BY 选项可以是一个并不存在的key，这会导致 SORT 命令跳过排序操作</param>
        /// <param name="offset">指定了跳过的元素数量</param>
        /// <param name="count">指定了从 offset 开始返回的元素数量</param>
        /// <param name="getPattern">GET 选项可多次使用，以便获取每一个原始列表、集合或有序集合中元素的key,还可以通过使用特殊 # 模式获取 GET 元素本身</param>
        /// <param name="desc">假设mylist是一个数字列表，这条命令将返回一个元素从小到大排序的相同大小列表。如果想从大到小排序，可以使用 !DESC 修饰符。</param>
        /// <param name="alpha"></param>
        /// <param name="storeDestination">使用　STORE　选项，可以将结果存储于一个特定的列表中，以代替返回到客户端</param>
        /// <returns>返回排序后的元素列表</returns>
        public virtual List<T> Sort<T>(string key,
                string byPattern = null,
                int? offset = null,
                int? count = null,
                string getPattern = null,
                bool? desc = null,
                bool alpha = false,
                string storeDestination = null)
        {
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            var data = client.Sort(Prefix + key, new SortOptions
            {
                GetPattern = getPattern,
                Skip = offset,
                Take = count,
                SortAlpha = alpha,
                SortDesc = desc == true,
                SortPattern = byPattern,
                StoreAtKey = storeDestination
            });
            return data.Select(Deserialize<T>).ToList();
        }

        /// <summary>
        /// 返回key所存储的value的数据结构类型，它可以返回string, list, set, zset 和 hash等不同的类型。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual RedisKeyType Type(string key)
        {
            using var client = (RedisClient)GetRedisClientsManager().GetClient();
            var type = client.Type(Prefix + key);
            return type == "zset" ? RedisKeyType.SortedSet
                            : Enum.TryParse<RedisKeyType>(type, true, out RedisKeyType result) ? result
                            : RedisKeyType.None;
        }

        ///// <summary>
        ///// 从当前数据库返回一个随机的key,如果数据库没有任何key，返回nil
        ///// </summary>
        ///// <returns></returns>
        //public virtual string RandomKey()
        //{
        //    return RedisClient.RandomKey();
        //}
    }
}