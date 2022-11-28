using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility.Redis;

namespace Test.HostWeb.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly Appsettings appConfig;

        RedisClient redisClient
        {
            get
            {
                return RedisClient.GetInstance(appConfig.RedisConnection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsMonitor"></param>
        public TestController(IOptionsMonitor<Appsettings> optionsMonitor)
        {
            appConfig = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetData(string key)
        {
            return redisClient.HGet<string>(key, "");
        }
    }
}
