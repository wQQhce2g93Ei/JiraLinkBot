using System;
using Common.Logging;
using JiraLinkBot.Configuration;
using Noobot.Core;
using Noobot.Core.Configuration;
using Noobot.Core.DependencyResolution;

namespace JiraLinkBot
{
    public class JiraLinkBotHost
    {
        private readonly IConfigReader _configReader;
        private readonly ILog _logger;
        private INoobotCore _noobotCore;

        public JiraLinkBotHost(IConfigReader configReader)
        {
            _configReader = configReader;
            _logger = LogManager.GetLogger(GetType());
        }

        public void Start()
        {
            IContainerFactory containerFactory = new ContainerFactory(new JiraLinkBotConfiguration(), _configReader, _logger);
            var container = containerFactory.CreateContainer();
            _noobotCore = container.GetNoobotCore();

            Console.WriteLine("Connecting...");
            _noobotCore
                .Connect()
                .ContinueWith(task =>
                {
                    if (!task.IsCompleted || task.IsFaulted)
                    {
                        Console.WriteLine($"Error connecting to Slack: {task.Exception}");
                    }
                });
        }

        public void Stop()
        {
            Console.WriteLine("Disconnecting...");
            _noobotCore.Disconnect();
        }
    }
}
