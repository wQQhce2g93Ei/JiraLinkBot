using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Noobot.Core.Configuration;

namespace JiraLinkBot.Configuration
{
    public class ConfigReader : IConfigReader
    {
        private JObject _currentJObject;
        private string _baseJiraAddress;

        public string SlackApiKey()
        {
            var jObject = GetJObject();
            return jObject.Value<string>("slack:apiToken");
        }

        public bool HelpEnabled()
        {
            return true;
        }

        public T GetConfigEntry<T>(string entryName)
        {
            var jObject = GetJObject();
            return jObject.Value<T>(entryName);
        }

        private JObject GetJObject()
        {
            if (_currentJObject == null)
            {
                var assemblyLocation = AssemblyLocation();
                var fileName = Path.Combine(assemblyLocation, @"configuration\config.json");
                var json = File.ReadAllText(fileName);
                _currentJObject = JObject.Parse(json);
            }

            return _currentJObject;
        }

        private string AssemblyLocation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var codebase = new Uri(assembly.CodeBase);
            var path = Path.GetDirectoryName(codebase.LocalPath);
            return path;
        }
    }
}