namespace Utility.Redis
{
    /// <summary>
    /// 返回当前key的数据类型
    /// </summary>
    public enum RedisKeyType
    {
        /// <summary>
        /// 不存在
        /// </summary>
        None = 0,
        
        /// <summary>
        /// string
        /// </summary>
        String = 1,
        
        /// <summary>
        /// list
        /// </summary>
        List = 2,

        /// <summary>
        /// set
        /// </summary>
        Set = 3,
        
        /// <summary>
        /// zset
        /// </summary>
        SortedSet = 4,
        
        /// <summary>
        /// hash
        /// </summary>
        Hash = 5
    }
}
