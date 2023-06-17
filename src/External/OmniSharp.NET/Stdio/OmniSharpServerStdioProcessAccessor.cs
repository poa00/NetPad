using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OmniSharp.Utilities;

namespace OmniSharp.Stdio
{
    internal class OmniSharpServerStdioProcessAccessor : IOmniSharpServerProcessAccessor<ProcessIO>, IDisposable
    {
        private readonly OmniSharpStdioServerConfiguration _configuration;
        private ProcessHandler? _processHandler;
        private ProcessIO? _processIoHandler;

        public OmniSharpServerStdioProcessAccessor(OmniSharpStdioServerConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<ProcessIO> GetEntryPointAsync()
        {
            if (_configuration.ExternallyManagedProcess)
            {
                var process = _configuration.ProcessGetter!();

                if (!process.IsProcessRunning())
                    throw new Exception("Externally managed OmniSharpServer process is not running.");

                _processIoHandler = new ProcessIO(process);
            }
            else
            {
                if (_processHandler != null)
                    throw new Exception("OmniSharp server is already initialized.");

                var exePath = _configuration.ExecutablePath!;
                var exeArgs = _configuration.ExecutableArgs!;

                // Copy current env variables to new process
                var environmentVariables = new Dictionary<string, string?>();
                var currentEnvVars = Environment.GetEnvironmentVariables();
                foreach (string key in currentEnvVars.Keys)
                {
                    environmentVariables.Add(key, currentEnvVars[key]?.ToString());
                }

                var dotNetSdkRootDirPath = _configuration.DotNetSdkRootDirectoryPath;
                if (!string.IsNullOrWhiteSpace(dotNetSdkRootDirPath) && Directory.Exists(dotNetSdkRootDirPath))
                {
                    environmentVariables["DOTNET_ROOT"] = dotNetSdkRootDirPath;
                    environmentVariables["PATH"] = $"{dotNetSdkRootDirPath}:{environmentVariables["PATH"]}".Trim(':');
                }

                _processHandler = new ProcessHandler(exePath, exeArgs, environmentVariables);

                var startResult = _processHandler.StartProcess();

                if (!startResult.Success || _processHandler.IO == null)
                    throw new Exception($"Could not start process at: {exePath}. Args: {exeArgs}");

                _processIoHandler = _processHandler.IO;
            }

            return Task.FromResult(_processIoHandler);
        }

        public Task StopProcessAsync()
        {
            if (_configuration.ExternallyManagedProcess)
            {
                // Do nothing. Process is managed externally.
            }
            else
            {
                _processIoHandler?.Dispose();
                _processHandler?.StopProcess();
                _processHandler?.Dispose();
            }

            _processHandler = null;
            _processIoHandler = null;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            AsyncHelpers.RunSync(StopProcessAsync);
        }
    }
}
