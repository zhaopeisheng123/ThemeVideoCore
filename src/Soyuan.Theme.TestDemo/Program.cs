using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Soyuan.Theme.TestDemo
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
                  .UseUrls("http://" + ip + ":" + port + "")
                 .Build();
        }

    }
}
