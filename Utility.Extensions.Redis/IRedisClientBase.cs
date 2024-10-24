using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extensions.Redis
{
    /// <summary>
    /// redis命令查询 https://redis.io/commands/
    /// </summary>
    public partial interface IRedisClientBase : IDisposable
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// key前缀
        /// </summary>
        string Prefix { get; }

        #region generic

        /// <summary>
        /// 将key重命名为newkey，如果key与newkey相同，将返回一个错误
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newkey"></param>
        /// <returns></returns>
        bool Rename(string key, string newkey);

        /// <summary>
        /// 当且仅当 newkey 不存在时，将 key 改名为 newkey
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newkey"></param>
        /// <returns></returns>
        bool RenameNx(string key, string newkey);

        /// <summary>
        /// 删除key,如果删除的key不存在则直接忽略
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>被删除的keys的数量</returns>
        long Del(params string[] keys);

        /// <summary>
        /// 序列化给定 key ，并返回被序列化的值，使用 RESTORE 命令可以将这个值反序列化为 Redis 键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        byte[] Dump(string key);

        /// <summary>
        /// 反序列化给定的序列化值，并将它和给定的 key 关联。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttlMilliSecond">参数 ttl 以毫秒为单位为 key 设置生存时间；如果 ttl 为 0 ，那么不设置生存时间。</param>
        /// <returns>如果反序列化成功那么返回null ，否则返回一个错误。</returns>
        string Restore(string key, byte[] value, long ttlMilliSecond = 0);

        /// <summary>
        /// 返回key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns>1 如果成功设置过期时间 0 如果key不存在或者不能设置过期时间</returns>
        bool Expire(string key, int seconds);

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestamp"></param>
        /// <returns>1 如果设置了过期时间 0 如果没有设置过期时间，或者不能设置过期时间</returns>
        bool ExpireAt(string key, DateTime timestamp);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        List<string> Keys(string patten);

        /// <summary>
        /// 扫描所有rediskey
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        ScanResult Scan(ulong cursor, int count, string match);

        /// <summary>
        /// 移除给定key的生存时间，将这个 key 从『易失的』(带生存时间 key )转换成『持久的』(一个不带生存时间、永不过期的 key )
        /// </summary>
        /// <param name="key"></param>
        /// <returns>当生存时间移除成功时，返回 1 ,如果 key 不存在或 key 没有设置生存时间，返回 0 .</returns>
        bool Persist(string key);

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        /// <returns>1 如果成功设置过期时间 0 如果key不存在或者不能设置过期时间</returns>
        bool PExpire(string key, long milliseconds);

        /// <summary>
        /// 设置key的过期时间，超过时间后，将会自动删除该key。使用PERSIST命令可以清除超时，使其变成一个永久的key。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestamp"></param>
        /// <returns>1 如果设置了过期时间 0 如果没有设置过期时间，或者不能设置过期时间</returns>
        bool PExpireAt(string key, DateTime timestamp);

        /// <summary>
        /// 返回key剩余的过期时间秒数。 这种反射能力允许Redis客户端检查指定key在数据集里面剩余的有效期。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// Redis2.8开始
        /// 如果key不存在或者已过期返回 -2
        /// 如果key没有设置过期时间（永久有效）返回 -1
        /// </returns>
        long TTL(string key);

        /// <summary>
        /// 返回key剩余的过期时间。 这种反射能力允许Redis客户端检查指定key在数据集里面剩余的有效期。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Redis2.8开始如果key不存在或者已过期返回 -2;如果key没有设置过期时间（永久有效）返回 -1</returns>
        long PTTL(string key);

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
        List<T> Sort<T>(string key,
                string byPattern = null,
                int? offset = null,
                int? count = null,
                string getPattern = null,
                bool? desc = null,
                bool alpha = false,
                string storeDestination = null);

        /// <summary>
        /// 返回key所存储的value的数据结构类型，它可以返回string, list, set, zset 和 hash等不同的类型。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        RedisKeyType Type(string key);

        ///// <summary>
        ///// 从当前数据库返回一个随机的key,如果数据库没有任何key，返回nil
        ///// </summary>
        ///// <returns></returns>
        //string RandomKey();

        #endregion generic

        #region server

        /// <summary>
        /// 返回当前数据里面keys的数量
        /// </summary>
        /// <returns></returns>
        long DbSize();

        /// <summary>
        /// 返回redis服务器时间
        /// </summary>
        /// <returns></returns>
        DateTimeOffset Time();

        #endregion server

        #region string

        /// <summary>
        /// 将键key设定为指定的val值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="expiresAt">EX seconds – 设置键key的过期时间，单位时秒PX milliseconds – 设置键key的过期时间，单位时毫秒</param>
        /// <param name="exists">false:NX – 只有键key不存在的时候才会设置key的值, true:XX – 只有键key存在的时候才会设置key的值, null:不指定</param>
        /// <returns></returns>
        bool Set<T>(string key, T val, TimeSpan? expiresAt = null, bool? exists = null);

        /// <summary>
        /// 将key设置值为value，如果key不存在，这种情况下等同SET命令。 当key存在时，什么也不做
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        bool SetNx<T>(string key, T val);

        /// <summary>
        /// 设置key对应字符串value，并且设置key在给定的seconds时间之后超时过期。等效于原子SET key value + EXPIRE key seconds
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        void SetEx<T>(string key, int seconds, T val);

        /// <summary>
        /// 返回key的value,如果key不存在返回default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// 对存储在指定key的数值执行原子的加1操作,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <returns>增加之后的value值</returns>
        long Incr(string key);

        /// <summary>
        /// 将key对应的数字加decrement,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns>增加之后的value值</returns>
        long IncrBy(string key, int increment);

        /// <summary>
        /// 将key对应的数字加decrement,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <returns>增加之后的value值</returns>
        double IncrByFloat(string key, double increment);

        /// <summary>
        /// 对存储在指定key的数值执行原子的减1操作,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <returns>减小之后的value</returns>
        long Decr(string key);

        /// <summary>
        /// 将key对应的数字减decrement,如果指定的key不存在，会先将它的值设定为0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns>减小之后的value</returns>
        long DecrBy(string key, int count);

        /// <summary>
        /// 返回指定多个字段的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="ignoreEmptyValue">是否排除哈希集中不存在的字段</param>
        /// <returns></returns>
        Dictionary<string, T> MGet<T>(List<string> keys, bool ignoreEmptyValue = true);

        /// <summary>
        /// 对应给定的keys到他们相应的values上,MSET是原子的且不会失败
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        void MSet<T>(Dictionary<string, T> data);

        /// <summary>
        /// 把 value 追加到原来值（value）的结尾,返回append后字符串值（value）的长度
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long Append(string key, string value);

        #endregion string

        #region hash

        /// <summary>
        /// 从 key 指定的哈希集中移除指定的域,在哈希集中不存在的域将被忽略
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns>从哈希集中成功移除的域的数量</returns>
        long HDel(string key, params string[] fields);

        /// <summary>
        /// 返回hash里面key是否存在的标志
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns>1 哈希集中含有该字段,0 哈希集中不含有该存在字段或者key不存在</returns>
        bool HExists(string key, string field);

        /// <summary>
        /// 返回 key 指定的哈希集中该字段所关联的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        T HGet<T>(string key, string field);

        /// <summary>
        /// 返回 key 指定的哈希集中所有的字段和值
        /// 时间复杂度：O(N) where N is the size of the hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Dictionary<string, T> HGetAll<T>(string key);

        /// <summary>
        /// 增加 key 指定的哈希集中指定字段的数值。
        /// 如果 key 不存在，会创建一个新的哈希集并与 key 关联
        /// 如果字段不存在，则字段的值在该操作执行前被设置为 0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        long HIncrby(string key, string field, long increment = 1);

        //todo:HINCRBYFLOAT

        /// <summary>
        /// 返回 key 指定的哈希集中所有字段的名字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<string> HKeys(string key);

        /// <summary>
        /// 哈希集中字段的数量，当 key 指定的哈希集不存在时返回 0
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long HLen(string key);

        /// <summary>
        /// 返回 key 指定的哈希集中指定字段的值
        /// 对于哈希集中不存在的每个字段，返回 nil 值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <param name="removeEmptyFields">是否排除不存在的字段</param>
        /// <returns></returns>
        Dictionary<string, T> HMGet<T>(string key, List<string> fields, bool removeEmptyFields = true);

        /// <summary>
        /// 设置 key 指定的哈希集中指定字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void HMSet<T>(string key, Dictionary<string, T> data);

        //todo:HSCAN

        /// <summary>
        /// 设置 key 指定的哈希集中指定字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>1如果field是一个新的字段,0如果field原来在map里面已经存在</returns>
        long HSet<T>(string key, string field, T value);

        /// <summary>
        /// 只在 key 指定的哈希集中不存在指定的字段时，设置字段的值
        /// 如果 key 指定的哈希集不存在，会创建一个新的哈希集并与 key 关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>1如果字段是个新的字段，并成功赋值,0如果哈希集中已存在该字段，没有操作被执行</returns>
        bool HSetNX<T>(string key, string field, T value);

        #endregion hash

        #region list

        /// <summary>
        /// 返回存储在 key 的列表里指定范围内的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<T> LRange<T>(string key, int start, int end);

        /// <summary>
        /// 从存于 key 的列表里移除前 count 次出现的值为 value 的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count">0: 移除所有值为 value 的元素;正数: 从头往尾移除值为 value 的元素；负数: 从尾往头移除值为 value 的元素</param>
        /// <param name="value"></param>
        /// <returns></returns>
        long LRem<T>(string key, int count, T value);

        /// <summary>
        /// 移除并且返回 key 对应的 list 的第一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T LPop<T>(string key);

        /// <summary>
        /// 是命令 LPOP 的阻塞版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <returns></returns>
        T BLPop<T>(string key, int timeoutSeconds);

        /// <summary>
        /// 是命令 LPOP 的阻塞版本,按参数 key 的先后顺序依次检查各个列表，弹出第一个非空列表的头元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <returns></returns>
        (string key, T value) BLPop<T>(string[] keys, int timeoutSeconds);

        /// <summary>
        /// 是命令 RPop 的阻塞版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <returns></returns>
        T BRPop<T>(string key, int timeoutSeconds);

        /// <summary>
        /// 是命令 RPOP 的阻塞版本,按照给出的 key 顺序查看 list，并在找到的第一个非空 list 的尾部弹出一个元素。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <returns></returns>
        (string key, T value) BRPop<T>(string[] keys, int timeoutSeconds);

        /// <summary>
        /// 是 RPOPLPUSH 的阻塞版本。 当 source 包含元素的时候，这个命令表现得跟 RPOPLPUSH 一模一样。 当 source 是空的时候，Redis将会阻塞这个连接，直到另一个客户端 push 元素进入或者达到 timeout 时限
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromList"></param>
        /// <param name="toList"></param>
        /// <param name="timeoutSeconds">当 timeout 为 0 是表示阻塞时间无限制</param>
        /// <returns></returns>
        T BRPopLPush<T>(string fromList, string toList, int timeoutSeconds);

        /// <summary>
        /// 原子性地返回并移除存储在 source 的列表的最后一个元素（列表尾部元素）， 并把该元素放入存储在 destination 的列表的第一个元素位置（列表头部）.如果 source 不存在，那么会返回 nil 值，并且不会执行任何操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromList"></param>
        /// <param name="toList"></param>
        /// <returns></returns>
        T RPopLPush<T>(string fromList, string toList);

        /// <summary>
        /// 移除并且返回 key 对应的 list 的最后一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T RPop<T>(string key);

        /// <summary>
        /// 返回列表里的元素,下标是从0开始索引的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index">0 是表示第一个元素， 1 表示第二个元素,-1 表示最后一个元素，-2 表示倒数第二个元素</param>
        /// <returns></returns>
        T LIndex<T>(string key, int index);

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
        void LInsert<T>(string key, bool before, T pivot, T value);

        /// <summary>
        /// 返回存储在 key 里的list的长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long LLen(string key);

        /// <summary>
        /// 将所有指定的值插入到存于 key 的列表的尾部
        /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        long LPush<T>(string key, T[] values);

        /// <summary>
        /// 只有当 key 已经存在并且存着一个 list 的时候，在这个 key 下面的 list 的头部插入 value。 与 LPUSH 相反，当 key 不存在的时候不会进行任何操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        long LPushX<T>(string key, T value);

        /// <summary>
        /// 将所有指定的值插入到存于 key 的列表的尾部
        /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        long RPush<T>(string key, T[] values);

        /// <summary>
        /// 只有当 key 已经存在并且存着一个 list 的时候，在这个 key 下面的 list 的尾部插入 value。 与 LPUSH 相反，当 key 不存在的时候不会进行任何操作。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        long RPushX<T>(string key, T value);

        /// <summary>
        /// 将所有指定的值插入到存于 key 的列表的尾部
        /// 如果 key 不存在，那么在进行 push 操作前会创建一个空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>在 push 操作后的 list 长度</returns>
        long RPush<T>(string key, T value);

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
        void LTrim(string key, int start, int stop);

        #endregion list

        #region set

        /// <summary>
        /// 集合中添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SAdd<T>(string key, T value);

        /// <summary>
        /// 集合中添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        bool SAdd<T>(string key, List<T> values);

        /// <summary>
        /// 获取集合所有元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">集合KEY</param>
        /// <returns></returns>
        List<T> SMembers<T>(string key);

        /// <summary>
        /// 返回成员 member 是否是存储的集合 key的成员.
        /// 如果member元素是集合key的成员，则返回1
        /// 如果member元素不是key的成员，或者集合key不存在，则返回0
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">集合KEY</param>
        /// <param name="member"></param>
        /// <returns></returns>
        long SIsMember<T>(string key, T member);

        /// <summary>
        /// 返回集合存储的key的基数 (集合元素的数量).如果key不存在,则返回 0.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long SCard(string key);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        long SRem<T>(string key, List<T> values);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        long SRem<T>(string key, T member);

        #endregion set

        #region sorted set

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="score"></param>
        /// <param name="member"></param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        long ZAdd<T>(string key, double score, T member);

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="memberValues"></param>
        long ZAdd<T>(string key, List<KeyValuePair<T, long>> memberValues);

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面，返回添加到有序集合的成员数量，不包括已经存在更新分数的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="memberValues"></param>
        long ZAdd<T>(string key, List<KeyValuePair<T, double>> memberValues);

        /// <summary>
        /// 将所有指定成员添加到键为key有序集合（sorted set）里面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="score"></param>
        /// <param name="member"></param>
        /// <returns>添加到有序集合的成员数量，不包括已经存在更新分数的成员</returns>
        long ZAdd<T>(string key, long score, T member);

        /// <summary>
        /// 删除成员，返回的是从有序集合中删除的成员个数，不包括不存在的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        long ZRem<T>(string key, T member);

        /// <summary>
        /// 删除成员，返回的是从有序集合中删除的成员个数，不包括不存在的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        long ZRem<T>(string key, List<T> members);

        /// <summary>
        /// 为有序集key的成员member的score值加上增量increment
        /// 如果key中不存在member，就在key中添加一个member，score是increment（就好像它之前的score是0.0）。如果key不存在，就创建一个只含有指定member成员的有序集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <param name="member"></param>
        /// <returns>member成员的新score值，以字符串形式表示</returns>
        double ZIncrBy<T>(string key, long increment, T member);

        /// <summary>
        /// 为有序集key的成员member的score值加上增量increment
        /// 如果key中不存在member，就在key中添加一个member，score是increment（就好像它之前的score是0.0）。如果key不存在，就创建一个只含有指定member成员的有序集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="increment"></param>
        /// <param name="member"></param>
        /// <returns>member成员的新score值，以字符串形式表示</returns>
        double ZIncrBy<T>(string key, double increment, T member);

        /// <summary>
        /// 返回key的有序集元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long ZCard(string key);

        /// <summary>
        /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        long ZCount(string key, double min, double max);

        /// <summary>
        /// 返回有序集key中，score值在min和max之间(默认包括score值等于min或max)的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        long ZCount(string key, long min, long max);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<T> ZRange<T>(string key, int start, int stop);

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
        List<T> ZRangeByScore<T>(string key, long min, long max, int? skip = null, int? take = null);

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
        List<T> ZRangeByScore<T>(string key, double min, double max, int? skip = null, int? take = null);

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
        List<KeyValuePair<T, long>> ZRangeByScoreWithScores<T>(string key, long min, long max, int? skip = null, int? take = null);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<KeyValuePair<T, long>> ZRangeWithScores<T>(string key, int start, int stop);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<T> ZRevRange<T>(string key, int start, int stop);

        /// <summary>
        /// Returns the specified range of elements in the sorted set stored at key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<KeyValuePair<T, long>> ZRevRangeWithScores<T>(string key, int start, int stop);

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
        List<T> ZRevRangeByScore<T>(string key, long min, long max, int? skip, int? take);

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
        List<T> ZRevRangeByScore<T>(string key, double min, double max, int? skip = null, int? take = null);

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
        List<KeyValuePair<T, long>> ZRevRangeByScoreWithScores<T>(string key, long min, long max, int? skip = null, int? take = null);

        /// <summary>
        /// 返回有序集key中，成员member的score值,如果member元素不是有序集key的成员，或key不存在，返回double.NaN
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        double ZScore<T>(string key, T member);

        /// <summary>
        /// 移除有序集key中，指定排名(rank)区间内的所有成员。下标参数start和stop都以0为底，0处是分数最小的那个元素。这些索引也可是负数，表示位移从最高分处开始数。例如，-1是分数最高的元素，-2是分数第二高的，依次类推。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>被移除成员的数量</returns>
        long ZRemRangeByRank(string key, int start, int stop);

        /// <summary>
        /// 移除有序集key中，所有score值介于min和max之间(包括等于min或max)的成员
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>被移除成员的数量</returns>
        long ZRemRangeByScore(string key, long min, long max);

        #endregion sorted set

        #region pubsub

        /// <summary>
        /// 将信息 message 发送到指定的频道 channel,返回收到消息的客户端数量
        /// </summary>
        /// <returns></returns>
        long Publish<T>(string channel, T message);

        /// <summary>
        /// 订阅给指定频道的信息
        /// </summary>
        /// <returns></returns>
        Subscription Subscribe(params string[] channels);

        #endregion pubsub

        #region extensions

        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        Task<RedisLocker> WaitOneAsync(string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000);

        /// <summary>
        /// 创建一个redis锁，请使用using确保调用Dispose,
        /// </summary>
        /// <param name="lockKey">竞争的redisKey</param>
        /// <param name="timeoutMilliSecond">超时时间，0表示无限等待,超时后可以通过Success字段判断是否成功</param>
        /// <param name="maxLockTimeMilliSecond">最大锁定时间，超过该时间后，即使事务未结束也将释放锁，默认是10000</param>
        RedisLocker WaitOne(string lockKey, int timeoutMilliSecond, int maxLockTimeMilliSecond = 10000);

        /// <summary>
        /// 删除内存缓存
        /// </summary>
        /// <param name="keys"></param>
        void DeleteMemoryCache(params string[] keys);

        #endregion extensions

        #region scripting

        /// <summary>
        /// 执行lua脚本,返回结果是Redis multi bulk replies的Lua数组，这是一个Redis的返回类型，您的客户端库可能会将他们转换成数组类型。
        /// </summary>
        /// <param name="luaBody"></param>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        string[] Eval(string luaBody, List<string> keys, List<string> values);

        #endregion scripting
    }
}