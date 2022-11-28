using ServiceStack;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Redis
{
    public partial class RedisClient
    {
        /// <summary>
        /// 集合总字节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SizeOfSet(string key)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                ulong cursor = 0;
                int pagesize = 10000;
                long totalSize = 0L;
                while (true)
                {
                    var scanresult = client.SScan(key, cursor, count: pagesize);
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
        /// 集合中添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="serializerType">
        /// 序列化类型，默认JSON
        /// <para>1:JSON</para>
        /// <para>2:protobuf</para>
        /// </param>
        /// <returns></returns>
        public bool SAdd<T>(string key, T value, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.SAdd(key, Serialize<T>(value, serializerType)) > 0;
            }
        }

        /// <summary>
        /// 集合中添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="serializerType">
        /// 序列化类型，默认JSON
        /// <para>1:JSON</para>
        /// <para>2:protobuf</para>
        /// </param>
        /// <returns></returns>
        public bool SAdd<T>(string key, List<T> values, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.SAdd(key, Serialize<T>(values, serializerType)) > 0;
            }
        }

        /// <summary>
        /// 获取集合所有元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">集合KEY</param>
        /// <param name="serializerType">      
        /// 序列化类型，默认JSON
        /// <para>1:JSON</para>
        /// <para>2:protobuf</para></param>
        /// <returns></returns>
        public List<T> SMembers<T>(string key, SerializerType serializerType = 0)
        {
            List<T> result = new List<T>();

            if (string.IsNullOrWhiteSpace(key))
            {
                return result;
            }

            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.SMembers(key);
                return data.Select(o => Deserialize<T>(o, serializerType)).ToList();
            }
        }

        /// <summary>
        /// 返回成员 member 是否是存储的集合 key的成员.
        /// 如果member元素是集合key的成员，则返回1
        /// 如果member元素不是key的成员，或者集合key不存在，则返回0
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">集合KEY</param>
        /// <param name="member"></param>
        /// <param name="serializerType">      
        /// 序列化类型，默认JSON
        /// <para>1:JSON</para>
        /// <para>2:protobuf</para></param>
        /// <returns></returns>
        public long SIsMember<T>(string key, T member, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.SIsMember(key, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 返回集合存储的key的基数 (集合元素的数量).如果key不存在,则返回 0.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SCard(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.SCard(key);
            }
        }
    }
}
