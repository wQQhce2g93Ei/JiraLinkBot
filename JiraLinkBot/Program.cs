using System;
using System.IO;
using System.Reflection;
using JiraLinkBot.Configuration;
using Noobot.Core.Configuration;
using Topshelf;

namespace JiraLinkBot
{
    class Program
    {
        private static readonly IConfigReader ConfigReader = new ConfigReader();

        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine($"Noobot assembly version: {Assembly.GetExecutingAssembly().GetName().Version}");

            HostFactory.Run(x =>
            {
                x.Service<JiraLinkBotHost>(s =>
                {
                    s.ConstructUsing(name => new JiraLinkBotHost(ConfigReader));

                    s.WhenStarted(n =>
                    {
                        n.Start();
                    });

                    s.WhenStopped(n => n.Stop());
                });

                x.RunAsNetworkService();
                x.SetDisplayName("JiraLinkBot");
                x.SetServiceName("JiraLinkBot");
                x.SetDescription("An extensible Slackbot built in C#");
            });
        }
    }
}
