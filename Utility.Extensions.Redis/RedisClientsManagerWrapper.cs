using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extensions.Redis
{
    internal class RedisClientsManagerWrapper
    {
        public IRedisClientsManager ClientsManager { get; set; }

        public string ConnectionString { get; set; }
    }
}