﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConfigurationService.Client;
using ConfigurationService.Client.Subscribers;

namespace ConfigurationService.Samples.Client
{
    class Program
    {
        static async Task Main()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddRemoteSource(s => 
                {
                    s.ConfigurationName = configuration["ConfigurationName"];
                    s.ConfigurationServiceUri = configuration["ConfigurationServiceUri"];
                    s.Subscriber = () => new RedisSubscriber(configuration["SubscriberConfiguration"]);
                    s.Optional = false;
                    s.ReloadOnChange = true;
                    s.LoggerFactory = loggerFactory;
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<ConfigWriter>();
            services.Configure<TestConfig>(configuration.GetSection("Config"));

            var serviceProvider = services.BuildServiceProvider();

            var configWriter = serviceProvider.GetService<ConfigWriter>();

            var cts = new CancellationTokenSource();
            await configWriter.Write(cts.Token);
            cts.Dispose();
        }
    }
}