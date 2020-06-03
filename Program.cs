using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace highwind
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{currentEnv}.json", optional: true)
                .Build();

            if(Boolean.Parse(config["Highwind:useIIS"])){
                CreateWebHostBuilderIIS(args).Build().Run();
            }
            else{
                 CreateWebHostBuilderWebListener(args, config["Highwind:nonIISBindAddress"]).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilderIIS(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();

        public static IWebHostBuilder CreateWebHostBuilderWebListener(string[] args, string listenAddress) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseHttpSys(options =>
                {
                    // The following options are set to default values.
                    options.Authentication.Schemes = AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                    options.Authentication.AllowAnonymous = false;
                    options.MaxConnections = null;
                    options.MaxRequestBodySize = 30000000;
                    options.UrlPrefixes.Add(listenAddress);
                });
    }
}
