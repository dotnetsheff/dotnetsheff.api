using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace dotnetsheff.Api.FunctionalTests
{
    public class AzureFunctionsStartup
    {
        private readonly int _port;
        private readonly string _meetupApiBaseUri;
        private readonly string _meetupApiKey;
        private readonly string _alexaSkillId;
        private Process _process;

        public AzureFunctionsStartup(int port, string meetupApiBaseUri, string meetupApiKey, string alexaSkillId)
        {
            _port = port;
            _meetupApiBaseUri = meetupApiBaseUri;
            _meetupApiKey = meetupApiKey;
            _alexaSkillId = alexaSkillId;
        }

        public void Start()
        {
            var currentDir = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var index = currentDir.LastIndexOf("\\bin\\");
            var projectDir = currentDir.Remove(index);
            var solutionDir = Directory.GetParent(projectDir).Parent.FullName;
            var mode = "Debug";

#if (!DEBUG)
            mode = "Release";
#endif
            var funcCliExe = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), @"npm\func.cmd");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = funcCliExe,
                Arguments = $"host start -p {_port}",
                WorkingDirectory = $@"{solutionDir}\src\dotnetsheff.Api\bin\{mode}\netcoreapp2.1\",
                UseShellExecute = false,
            };

            processStartInfo.EnvironmentVariables["AzureWebJobsStorage"] = "UseDevelopmentStorage=true";
            processStartInfo.EnvironmentVariables["AzureWebJobsDashboard"] = "UseDevelopmentStorage=true";
            processStartInfo.EnvironmentVariables["MeetupApiBaseUri"] = _meetupApiBaseUri;
            processStartInfo.EnvironmentVariables["MeetupApiKey"] = _meetupApiKey;
            processStartInfo.EnvironmentVariables["AcceptInvalidAlexaSignature"] = bool.TrueString;
            processStartInfo.EnvironmentVariables["AlexaSkillId"] = _alexaSkillId;

            _process = Process.Start(processStartInfo);

            Thread.Sleep(2000);
        }

        public void Stop()
        {
            var processesByName = Process.GetProcessesByName("func");
            foreach (var process in processesByName)
            {
                process.Kill();
            }
        }
    }
}