using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using xLiAd.DiagnosticLogCenter.SampleFulluseAspNetCore.Services;

namespace xLiAd.DiagnosticLogCenter.SampleFulluseAspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ISampleService sampleService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ISampleService sampleService)
        {
            _logger = logger;
            this.sampleService = sampleService;
        }

        [HttpGet]
        public object Get()
        {
            var dbresult = sampleService.QueryDb(2);
            var httpResult = sampleService.RequestWeb("https://www.baidu.com/");
            return new { dbresult, httpResult };
        }
    }
}
