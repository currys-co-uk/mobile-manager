using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Logging.Logger;

namespace MobileManager
{
    /// <summary>
    /// Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            var logger = new ManagerLogger();
            using (var host = PrepareHost(args))
            {
                try
                {
                    await host.RunAsync();
                }
                catch (IOException e)
                {
                    logger.Error(e.Message, e);

                    logger.Info("Stopping main application.");
                    await host.StopAsync(TimeSpan.FromSeconds(5));
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                }
            }

            logger.Info("Application stopped.");
        }

        private static IWebHost PrepareHost(string[] args)
        {
            return new WebHostBuilder()
                .UseKestrel(
                    options =>
                    {
                        var config = AppConfigurationProvider.Get<ManagerConfiguration>();
                        options.Listen(IPAddress.Parse(config.LocalIpAddress), config.ListeningPort,
                            listenOptions => { });
                        options.Listen(IPAddress.Parse("127.0.0.1"), config.ListeningPort,
                            listenOptions => { });
                    })
                .UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
        }
    }
}