using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Services
{
    public class CacheService : ICacheService
    {
        private readonly MemoryCache mc = new MemoryCache(new MemoryCacheOptions() { });
        public void Set(string key, object value, TimeSpan timeSpan)
        {
            mc.Set(key, value, timeSpan);
        }

        public object Get(string key)
        {
            return mc.Get(key);
        }
    }

    public interface ICacheService
    {
        void Set(string key, object value, TimeSpan timeSpan);
        object Get(string key);
    }
}
