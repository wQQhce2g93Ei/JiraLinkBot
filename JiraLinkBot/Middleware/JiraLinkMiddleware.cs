using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JiraLinkBot.Model;
using Noobot.Core.Configuration;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Core.Plugins.StandardPlugins;
using Noobot.Toolbox.Plugins;

namespace JiraLinkBot.Middleware
{
    public class JiraLinkMiddleware : MiddlewareBase
    {
        private readonly StatsPlugin _statsPlugin;
        private readonly JsonStoragePlugin _jsonStoragePlugin;
        private readonly IConfigReader _configReader;

        private string _filename { get; } = "JiraProjects";

        private readonly Regex _jiraTicketRegex = new Regex(@"\b(?<!/)(\w{1,10})-(\d{1,10})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _jiraAddProjectRegex = new Regex(@"addproject (\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _jiraRemoveProjectRegex = new Regex(@"removeproject (\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public JiraLinkMiddleware(IMiddleware next, JsonStoragePlugin jsonStoragePlugin, StatsPlugin statsPlugin, IConfigReader configReader) : base(next)
        {
            _jsonStoragePlugin = jsonStoragePlugin;
            _statsPlugin = statsPlugin;
            _configReader = configReader;

            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = new[] {""},
                    ShouldContinueProcessing = true,
                    VisibleInHelp = false,
                    EvaluatorFunc = JiraTicketHandler
                },
                new HandlerMapping
                {
                    ValidHandles = new[] {"AddProject"},
                    Description = "Add a new Jira project to JiraLinkBot",
                    ShouldContinueProcessing = false,
                    VisibleInHelp = true,
                    EvaluatorFunc = JiraProjectAddHandler
                },
                new HandlerMapping
                {
                    ValidHandles = new[] {"RemoveProject"},
                    Description = "Remove a Jira project from JiraLinkBot",
                    ShouldContinueProcessing = false,
                    VisibleInHelp = true,
                    EvaluatorFunc = JiraProjectRemoveHandler
                },
                new HandlerMapping
                {
                    ValidHandles = new[] {"ListProjects"},
                    Description = "Print the list of Jira projects in JiraLinkBot",
                    ShouldContinueProcessing = false,
                    VisibleInHelp = true,
                    EvaluatorFunc = JiraProjectListHandler
                },
                new HandlerMapping
                {
                    ValidHandles = new[] {"ClearProjects"},
                    ShouldContinueProcessing = false,
                    VisibleInHelp = false,
                    EvaluatorFunc = JiraProjectClearListHandler
                }
            };
        }

        private IEnumerable<ResponseMessage> JiraTicketHandler(IncomingMessage message, string matchedHandle)
        {
            var matches = _jiraTicketRegex.Matches(message.FullText);
            var links = new List<string>();
            foreach (Match match in matches)
            {
                var projects = _jsonStoragePlugin.ReadFile<JiraProject>(_filename);

                if (projects.Any(o => o.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase)))
                {
                    var link = _configReader.GetConfigEntry<string>("jira:baseAddress") + match.Value;
                    if (!links.Contains(link))
                    {
                        links.Add(link);
                    }
                }
            }

            if (links.Any())
            {
                var linkCount = _statsPlugin.GetStat<int>("JiraTicketLinksCreated");
                _statsPlugin.RecordStat("JiraTicketLinksCreated", linkCount + links.Count);
                var text = links.Count > 1 ? "I found some Jira links for you! \n" : "I found a Jira link for you! ";
                yield return message.ReplyToChannel(text + string.Join("\n", links));
            }
        }

        private IEnumerable<ResponseMessage> JiraProjectAddHandler(IncomingMessage message, string matchedHandle)
        {
            var match = _jiraAddProjectRegex.Match(message.TargetedText);
            var newProject = match.Groups.Count >= 1 ? match.Groups[1].Value : string.Empty;

            if (!string.IsNullOrEmpty(newProject))
            {
                var projects = _jsonStoragePlugin.ReadFile<JiraProject>(_filename);
                if (!projects.Any(o => o.Name.Equals(newProject, StringComparison.OrdinalIgnoreCase)))
                {
                    projects = projects.Union(new[] {new JiraProject {Name = newProject } }).ToArray();
                    _jsonStoragePlugin.SaveFile(_filename, projects);
                    yield return message.ReplyToChannel($"{newProject} project added!");
                }
                else
                {
                    yield return message.ReplyToChannel($"{newProject} project already exists in JiraLinkBot's project list.");
                }
            }
            else
            {
                yield return message.ReplyToChannel("No project specified, the format of this command is '@jiralinkbot AddProject {ProjectName}'");
            }
        }

        private IEnumerable<ResponseMessage> JiraProjectRemoveHandler(IncomingMessage message, string matchedHandle)
        {
            var match = _jiraRemoveProjectRegex.Match(message.TargetedText);
            var existingProject = match.Groups.Count >= 1 ? match.Groups[1].Value : string.Empty;

            if (!string.IsNullOrEmpty(existingProject))
            {
                var projects = _jsonStoragePlugin.ReadFile<JiraProject>(_filename);
                if (projects.Any(o => o.Name.Equals(existingProject, StringComparison.OrdinalIgnoreCase)))
                {
                    projects = projects.Where(o => !o.Name.Equals(existingProject, StringComparison.OrdinalIgnoreCase)).ToArray();
                    _jsonStoragePlugin.SaveFile(_filename, projects);
                    yield return message.ReplyToChannel($"{existingProject} project removed!");
                }
                else
                {
                    yield return message.ReplyToChannel($"{existingProject} project not in JiraLinkBot's project list.");
                }
            }
            else
            {
                yield return message.ReplyToChannel("No project specified, the format of this command is '@jiralinkbot RemoveProject {ProjectName}'");
            }
        }

        private IEnumerable<ResponseMessage> JiraProjectListHandler(IncomingMessage message, string matchedHandle)
        {
            var projects = _jsonStoragePlugin.ReadFile<JiraProject>(_filename);
            yield return message.ReplyToChannel($"Here are all the projects: {string.Join(", ", projects.Select(o => o.Name.ToUpper()))}");
        }

        private IEnumerable<ResponseMessage> JiraProjectClearListHandler(IncomingMessage message, string matchedHandle)
        {
            _jsonStoragePlugin.SaveFile(_filename, new JiraProject[0]);
            yield return message.ReplyToChannel("Cleared all the Jira projects!");
        }
    }
}