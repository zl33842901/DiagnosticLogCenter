using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage
{
    public class CacheProvider : ICacheProvider
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
        public T Get<T>(string key)
        {
            var o = (T)Get(key);
            return o;
        }
    }

    public interface ICacheProvider
    {
        void Set(string key, object value, TimeSpan timeSpan);
        T Get<T>(string key);
    }
}
