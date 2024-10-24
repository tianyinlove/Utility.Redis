namespace Utility.Extensions.Redis.Internal;

/// <summary>
/// 一些特殊key
/// </summary>
internal static class RedisKeys
{
    /// <summary>
    /// 内存删除消息
    /// </summary>
    public const string MemoryMessageChannel = "memory_sub:message";

    /// <summary>
    /// 最近的内存删除历史
    /// </summary>
    public const string MemoryDeleteHistory = "memory_sub:history";
}