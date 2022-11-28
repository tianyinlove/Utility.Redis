using ServiceStack;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Redis
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RedisClient
    {
        //哈希

        /// <summary>
        /// 有序集合总字节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SizeOfHashSet(string key)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                ulong cursor = 0;
                int pagesize = 10000;
                long totalSize = 0L;
                while (true)
                {
                    var scanresult = client.HScan(key, cursor, count: pagesize);
                    totalSize += scanresult.Results?.Sum(d => d.LongLength) ?? 0L;
                    cursor = scanresult.Cursor;
                    if (cursor == 0)
                    {
                        break;
                    }
                }
                return totalSize;
            }
        }

        /// <summary>
        /// 从 key 指定的哈希集中移除指定的域,在哈希集中不存在的域将被忽略
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns>从哈希集中成功移除的域的数量</returns>
        public long HDel(string key, params string[] fields)
        {
            if (string.IsNullOrWhiteSpace(key) || fields.Length == 0)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HDel(key, fields.Select(d => d.ToUtf8Bytes()).ToArray());
            }
        }

        /// <summary>
        /// 返回hash里面key是否存在的标志
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns>1 哈希集中含有该字段,0 哈希集中不含有该存在字段或者key不存在</returns>
        public bool HExists(string key, string field)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            {
                return false;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HExists(key, field.ToUtf8Bytes()) == 1;
            }
        }

        /// <summary>
        /// 返回 key 指定的哈希集中该字段所关联的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public T HGet<T>(string key, string field, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            {
                return default;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return Deserialize<T>(client.HGet(key, field.ToUtf8Bytes()), serializerType);
            }
        }

        /// <summary>
        /// 返回 key 指定的哈希集中所有的字段和值
        /// 时间复杂度：O(N) where N is the size of the hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public Dictionary<string, T> HGetAll<T>(string key, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.HGetAll(key);
                var result = new Dictionary<string, T>();
                for (int i = 0; i < data.Length; i += 2)
                {
                    if (data[i] != null)
                    {
                        result[data[i].FromUtf8Bytes()] = Deserialize<T>(data[i + 1], serializerType);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 增加 key 指定的哈希集中指定字段的数值。
        /// 如果 key 不存在，会创建一个新的哈希集并与 key 关联
        /// 如果字段不存在，则字段的值在该操作执行前被设置为 0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public long HIncrby(string key, string field, long increment = 1)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HIncrby(key, field.ToUtf8Bytes(), increment);
            }
        }

        //todo:HINCRBYFLOAT

        /// <summary>
        /// 返回 key 指定的哈希集中所有字段的名字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> HKeys(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HKeys(key).Select(d => d.FromUtf8Bytes()).ToList();
            }
        }

        /// <summary>
        /// 哈希集中字段的数量，当 key 指定的哈希集不存在时返回 0
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long HLen(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HLen(key);
            }
        }

        /// <summary>
        /// 返回 key 指定的哈希集中指定字段的值
        /// 对于哈希集中不存在的每个字段，返回 nil 值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <param name="removeEmptyFields">是否排除不存在的字段</param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public Dictionary<string, T> HMGet<T>(string key, List<string> fields, bool removeEmptyFields = true, SerializerType serializerType = 0)
        {
            Dictionary<string, T> result = new Dictionary<string, T>();
            if (string.IsNullOrWhiteSpace(key) || fields == null || fields.Count == 0)
            {
                return result;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.HMGet(key, fields.Select(d => d.ToUtf8Bytes()).ToArray());
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] != null)
                    {
                        result[fields[i]] = Deserialize<T>(data[i], serializerType);
                    }
                    else if (!removeEmptyFields)
                    {
                        result[fields[i]] = default(T);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 设置 key 指定的哈希集中指定字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="serializerType"></param>
        public void HMSet<T>(string key, Dictionary<string, T> data, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null || data.Count == 0)
            {
                return;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var fields = data.Keys;
                client.HMSet(hashId: key,
                    keys: fields.Select(d => d.ToUtf8Bytes()).ToArray(),
                    values: fields.Select(d => Serialize<T>(data[d], serializerType)).ToArray());
            }
        }

        //todo:HSCAN

        /// <summary>
        /// 设置 key 指定的哈希集中指定字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="data"></param>
        /// <param name="serializerType"></param>
        /// <returns>1如果field是一个新的字段,0如果field原来在map里面已经存在</returns>
        public long HSet<T>(string key, string field, T data, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            {
                return -1;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HSet(key, field.ToUtf8Bytes(), Serialize<T>(data, serializerType));
            }
        }

        /// <summary>
        /// 不序列化，直接保存字符串
        /// 设置 key 指定的哈希集中指定字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>1如果field是一个新的字段,0如果field原来在map里面已经存在</returns>
        public long RawHSet(string key, string field, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            {
                return -1;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HSet(key, field.ToUtf8Bytes(), value.ToUtf8Bytes());
            }
        }

        /// <summary>
        /// 只在 key 指定的哈希集中不存在指定的字段时，设置字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="data"></param>
        /// <param name="serializerType"></param>
        /// <returns>1如果字段是个新的字段，并成功赋值,0如果哈希集中已存在该字段，没有操作被执行</returns>
        public bool HSetNX<T>(string key, string field, T data, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            {
                return false;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.HSetNX(key, field.ToUtf8Bytes(), Serialize<T>(data, serializerType)) > 0;
            }
        }
    }
}
