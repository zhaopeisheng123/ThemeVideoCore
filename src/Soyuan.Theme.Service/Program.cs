using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soyuan.Theme.Business;
using Soyuan.Theme.Core.Helper;

namespace Soyuan.Theme.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var ip = configuration["Service:IP"];
            var port = configuration["Service:Port"];
            return WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseKestrel(options =>
            {
                //所有controller都不限制post的body大小
                options.Limits.MaxRequestBodySize = null;
            })
            .UseUrls("http://" + ip + ":" + port + "")
            .Build();
        }
    }
}
