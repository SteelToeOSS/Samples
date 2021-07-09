﻿using ConsoleGenericHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Config;
using Steeltoe.Messaging.RabbitMQ.Extensions;
using Steeltoe.Messaging.RabbitMQ.Host;

namespace ConsoleSendReceive
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            RabbitMQHost.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Add queue to be declared
                    services.AddRabbitQueue(new Queue("myqueue"));

                    services.AddRabbitDirecListenerContainer("manualContainer", (p, container) =>
                    {
                        var logFactory = p.GetRequiredService<ILoggerFactory>();
                        container.ApplicationContext = p.GetApplicationContext();
                        container.SetQueueNames("myqueue");
                        container.MessageListener = new MyMessageListener(logFactory.CreateLogger<MyMessageListener>());
                    });

                    // Add a message sender
                    services.AddSingleton<IHostedService, MyRabbitSender>();
                });
    }
}
