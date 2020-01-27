using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoRCards.WorkerOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LoRCards
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    IConfiguration configuration = hostContext.Configuration;

                    var telegramWorkerOptions = configuration.GetSection("Telegram").Get<TelegramWorkerOptions>();

                    services.AddSingleton(telegramWorkerOptions);

                    services.AddHostedService<Worker>();
                });
    }
}
