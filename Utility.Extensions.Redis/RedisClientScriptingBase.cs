using ServiceStack;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Extensions.Redis;
public partial class RedisClientBase
{
    /// <summary>
    /// 执行lua脚本,返回结果是Redis multi bulk replies的Lua数组，这是一个Redis的返回类型，您的客户端库可能会将他们转换成数组类型。
    /// </summary>
    /// <param name="luaBody"></param>
    /// <param name="keys"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public virtual string[] Eval(string luaBody, List<string> keys, List<string> values)
    {
        var data = keys.Select(d => d.ToUtf8Bytes()).ToList();
        data.AddRange(values.Select(d => d.ToUtf8Bytes()));
        using var client = (RedisClient)GetRedisClientsManager().GetClient();
        return client.Eval(luaBody, keys.Count, data.ToArray()).Select(d => d.FromUtf8Bytes()).ToArray();
    }
}
