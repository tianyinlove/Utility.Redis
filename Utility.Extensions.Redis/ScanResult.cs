using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Extensions.Redis
{
    /// <summary>
    /// 各种scan搜索结果
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