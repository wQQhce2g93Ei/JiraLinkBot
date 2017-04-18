using JiraLinkBot.Middleware;
using Noobot.Core.Configuration;
using Noobot.Toolbox.Plugins;

namespace JiraLinkBot.Configuration
{
    internal class JiraLinkBotConfiguration : ConfigurationBase
    {
        public JiraLinkBotConfiguration()
        {
            UseMiddleware<JiraLinkMiddleware>();

            UsePlugin<JsonStoragePlugin>();
        }
    }
}
