namespace Utility.Redis
{
    /// <summary>
    /// 保存到redis时数据的序列化方式
    /// </summary>
    public enum SerializerType
    {
        /// <summary>
        /// Newtonsoft.Json
        /// </summary>
        Json = 1,

        ///// <summary>
        ///// 采用protobuf-net
        ///// </summary>
        ////Protobuf = 2
    }
}
