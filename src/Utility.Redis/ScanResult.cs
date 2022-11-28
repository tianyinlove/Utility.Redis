using System.Collections.Generic;

namespace Utility.Redis
{
    /// <summary>
    /// scan搜索结果
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// 
        /// </summary>
        public ulong Cursor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Results { get; set; }
    }
}
