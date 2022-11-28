using ServiceStack;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Redis
{
    public partial class RedisClient
    {
        #region ZSets/有序集合

        /// <summary>
        /// 有序集合总字节
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SizeOfSortedSet(string key)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                ulong cursor = 0;
                int pagesize = 10000;
                long totalSize = 0L;
                while (true)
                {
                    var scanresult = client.ZScan(key, cursor, count: pagesize);
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
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="score"></param>
        /// <param name="member"></param>
        /// <param name="serializerType"></param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        public long ZAdd<T>(string key, double score, T member, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || member == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZAdd(key, score, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="memberValues"></param>
        /// <param name="serializerType"></param>
        public long ZAdd<T>(string key, List<KeyValuePair<T, long>> memberValues, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || memberValues == null || memberValues.Count == 0)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZAdd(key, memberValues.Select(d => new KeyValuePair<byte[], long>(Serialize<T>(d.Key, serializerType), d.Value)).ToList());
            }
        }

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="memberValues"></param>
        /// <param name="serializerType"></param>
        public long ZAdd<T>(string key, List<KeyValuePair<T, double>> memberValues, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || memberValues == null || memberValues.Count == 0)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZAdd(key, memberValues.Select(d => new KeyValuePair<byte[], double>(Serialize<T>(d.Key, serializerType), d.Value)).ToList());
            }
        }

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="score"></param>
        /// <param name="member"></param>
        /// <param name="serializerType"></param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        public long ZAdd<T>(string key, long score, T member, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || member == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZAdd(key, score, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 删除成员，返回的是从有序集合中删除的成员个数，不包括不存在的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public long ZRem<T>(string key, T member, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || member == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRem(key, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 删除成员，返回的是从有序集合中删除的成员个数，不包括不存在的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public long ZRem<T>(string key, List<T> members, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || members == null || members.Count == 0)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRem(key, members.Select(d => Serialize<T>(d, serializerType)).ToArray());
            }
        }

        /// <summary>
        /// 为有序集key的成员member的score值加上增量increment
        /// 如果key中不存在member，就在key中添加一个member，score是increment（就好像它之前的score是0.0）。如果key不存在，就创建一个只含有指定member成员的有序集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <param name="member"></param>
        /// <param name="serializerType"></param>
        /// <returns>member成员的新score值，以字符串形式表示</returns>
        public double ZIncrBy<T>(string key, long increment, T member, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || member == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZIncrBy(key, increment, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 为有序集key的成员member的score值加上增量increment
        /// 如果key中不存在member，就在key中添加一个member，score是increment（就好像它之前的score是0.0）。如果key不存在，就创建一个只含有指定member成员的有序集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <param name="member"></param>
        /// <param name="serializerType"></param>
        /// <returns>member成员的新score值，以字符串形式表示</returns>
        public double ZIncrBy<T>(string key, double increment, T member, SerializerType serializerType = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || member == null)
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZIncrBy(key, increment, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 返回key的有序集元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ZCard(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZCard(key);
            }
        }

        /// <summary>
        /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public long ZCount(string key, double min, double max)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZCount(key, min, max);
            }
        }

        /// <summary>
        /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public long ZCount(string key, long min, long max)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZCount(key, min, max);
            }
        }

        ///// <summary>
        ///// 计算给定的numkeys个有序集合的交集，并且把结果放到destination中.如果destination存在，就把它覆盖
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="keys"></param>
        ///// <returns>结果有序集合destination中元素个数</returns>
        //public long ZInterStore(string destination, params string[] keys)
        //{
        //    using (var mg = (RedisClient)Manager.GetClient())
        //    {
        //        return mg.ZInterStore(destination, keys);
        //    }
        //}

        ///// <summary>
        ///// 计算给定的numkeys个有序集合的交集，并且把结果放到destination中.如果destination存在，就把它覆盖
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="keysAndWeight"></param>
        ///// <param name="aggregate">SUM|MIN|MAX</param>
        ///// <returns>结果有序集合destination中元素个数</returns>
        //public long ZInterStore(string destination, Dictionary<string, double> keysAndWeight, string aggregate = "sum")
        //{
        //    var list = keysAndWeight.ToList();
        //    Aggregate agg = Aggregate.Sum;
        //    if (aggregate.ToLower() == "min")
        //    {
        //        agg = Aggregate.Min;
        //    }
        //    else if (aggregate.ToLower() == "max")
        //    {
        //        agg = Aggregate.Max;
        //    }
        //    return Database.SortedSetCombineAndStore(SetOperation.Intersect, destination,
        //        list.Select(d => (RedisKey)d.Key).ToArray(),
        //        weights: list.Select(d => d.Value).ToArray(),
        //        aggregate: agg);
        //}



        ///// <summary>
        ///// 计算给定的numkeys个有序集合的并集，并且把结果放到destination中.如果destination存在，就把它覆盖
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="keys"></param>
        ///// <returns>结果有序集合destination中元素个数</returns>
        //public long ZUnionStore(string destination, params string[] keys)
        //{
        //    using (var mg = (RedisClient)Manager.GetClient())
        //    {
        //        return mg.ZUnionStore(destination, keys);
        //    }
        //}

        ///// <summary>
        ///// 计算给定的numkeys个有序集合的并集，并且把结果放到destination中.如果destination存在，就把它覆盖
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="keysAndWeight"></param>
        ///// <param name="aggregate">SUM|MIN|MAX</param>
        ///// <returns>结果有序集合destination中元素个数</returns>
        //public long ZUnionStore(string destination, Dictionary<string, double> keysAndWeight, string aggregate = "sum")
        //{
        //    var list = keysAndWeight.ToList();
        //    Aggregate agg = Aggregate.Sum;
        //    if (aggregate.ToLower() == "min")
        //    {
        //        agg = Aggregate.Min;
        //    }
        //    else if (aggregate.ToLower() == "max")
        //    {
        //        agg = Aggregate.Max;
        //    }
        //    return Database.SortedSetCombineAndStore(SetOperation.Union, destination,
        //        list.Select(d => (RedisKey)d.Key).ToArray(),
        //        weights: list.Select(d => d.Value).ToArray(),
        //        aggregate: agg);
        //}

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> ZRange<T>(string key, int start, int stop, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRange(key, start, stop).Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
        }

        ///// <summary>
        ///// Returns the specified range of elements in the sorted set stored at key
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="start"></param>
        ///// <param name="stop"></param>
        ///// <returns></returns>
        //public List<KeyValuePair<T, double>> ZRangeWithScores<T>(string key, int start, int stop, SerializerType serializerType = 0)
        //{
        //    using (var mg = (RedisClient)Manager.GetClient())
        //    {
        //        var arrs = mg.ZRangeWithScores(key, start, stop);
        //        List<KeyValuePair<T, double>> result = new List<KeyValuePair<T, double>>();
        //        for (int i = 0; i < arrs.Length; i += 2)
        //        {
        //            result.Add(new KeyValuePair<T, double>(Deserialize<T>(arrs[i]), double.Parse(arrs[i + 1].FromUtf8Bytes())));
        //        }
        //        return result;

        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> ZRangeByScore<T>(string key, long min, long max, int? skip = null, int? take = null, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRangeByScore(key, min, max, skip, take).Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
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
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> ZRangeByScore<T>(string key, double min, double max, int? skip = null, int? take = null, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRangeByScore(key, min, max, skip, take).Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
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
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<KeyValuePair<T, long>> ZRangeByScoreWithScores<T>(string key, long min, long max, int? skip = null, int? take = null, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.ZRangeByScoreWithScores(key, min, max, skip, take);
                var result = new List<KeyValuePair<T, long>>();
                for (int i = 0; i < data.Length; i += 2)
                {
                    result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i], serializerType), long.Parse(data[i + 1].FromUtf8Bytes())));
                }
                return result;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<KeyValuePair<T, long>> ZRangeWithScores<T>(string key, int start, int stop, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.ZRangeWithScores(key, start, stop);
                var result = new List<KeyValuePair<T, long>>();
                for (int i = 0; i < data.Length; i += 2)
                {
                    result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i], serializerType), long.Parse(data[i + 1].FromUtf8Bytes())));
                }
                return result;
            }
        }

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> ZRevRange<T>(string key, int start, int stop, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRevRange(key, start, stop).Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
        }

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<KeyValuePair<T, long>> ZRevRangeWithScores<T>(string key, int start, int stop, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.ZRevRangeWithScores(key, start, stop);
                var result = new List<KeyValuePair<T, long>>();
                for (int i = 0; i < data.Length; i += 2)
                {
                    result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i], serializerType), long.Parse(data[i + 1].FromUtf8Bytes())));
                }
                return result;
            }
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
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> ZRevRangeByScore<T>(string key, long min, long max, int? skip, int? take, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRevRangeByScore(key, min, max, skip, take).Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
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
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<T> ZRevRangeByScore<T>(string key, double min, double max, int? skip = null, int? take = null, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRevRangeByScore(key, min, max, skip, take).Select(d => Deserialize<T>(d, serializerType)).ToList();
            }
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
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public List<KeyValuePair<T, long>> ZRevRangeByScoreWithScores<T>(string key, long min, long max, int? skip = null, int? take = null, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                var data = client.ZRevRangeByScoreWithScores(key, min, max, skip, take);
                var result = new List<KeyValuePair<T, long>>();
                for (int i = 0; i < data.Length; i += 2)
                {
                    result.Add(new KeyValuePair<T, long>(Deserialize<T>(data[i], serializerType), long.Parse(data[i + 1].FromUtf8Bytes())));
                }
                return result;
            }
        }

        /// <summary>
        /// 返回有序集key中，成员member的score值,如果member元素不是有序集key的成员，或key不存在，返回double.NaN
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="serializerType"></param>
        /// <returns></returns>
        public double ZScore<T>(string key, T member, SerializerType serializerType = 0)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZScore(key, Serialize<T>(member, serializerType));
            }
        }

        /// <summary>
        /// 移除有序集key中，指定排名(rank)区间内的所有成员。下标参数start和stop都以0为底，0处是分数最小的那个元素。这些索引也可是负数，表示位移从最高分处开始数。例如，-1是分数最高的元素，-2是分数第二高的，依次类推。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>被移除成员的数量</returns>
        public long ZRemRangeByRank(string key, int start, int stop)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRemRangeByRank(key, start, stop);
            }
        }


        /// <summary>
        /// 移除有序集key中，所有score值介于min和max之间(包括等于min或max)的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>被移除成员的数量</returns>
        public long ZRemRangeByScore(string key, long min, long max)
        {
            using (var client = (ServiceStack.Redis.RedisClient)ClientsManager.GetClient())
            {
                return client.ZRemRangeByScore(key, min, max);
            }
        }
        #endregion
    }
}
