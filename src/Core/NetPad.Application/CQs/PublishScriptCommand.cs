using MediatR;
using Microsoft.CodeAnalysis;
using NetPad.Compilation;
using NetPad.Configuration;
using NetPad.Data;
using NetPad.DotNet;
using NetPad.Scripts;

namespace NetPad.CQs;

public class PublishScriptCommand : Command
{
    public PublishScriptCommand(Script script, DotNetPublishOptions options)
    {
        Script = script;
        Options = options;
    }

    public Script Script { get; }
    public DotNetPublishOptions Options { get; }

    public class Handler : IRequestHandler<PublishScriptCommand, Unit>
    {
        private readonly IDotNetInfo _dotNetInfo;
        private readonly Settings _settings;
        private readonly IDataConnectionResourcesCache _dataConnectionResourcesCache;
        private readonly ICodeParser _codeParser;

        public Handler(IDotNetInfo dotNetInfo, Settings settings, IDataConnectionResourcesCache dataConnectionResourcesCache, ICodeParser codeParser)
        {
            _dotNetInfo = dotNetInfo;
            _settings = settings;
            _dataConnectionResourcesCache = dataConnectionResourcesCache;
            _codeParser = codeParser;
        }

        public async Task<Unit> Handle(PublishScriptCommand request, CancellationToken cancellationToken)
        {
            var script = request.Script;
            var options = request.Options;

            if (!options.IsValid())
            {
                throw new ArgumentException("Invalid options");
            }

            // Prepare project
            var project = await CreateProjectAsync(
                AppDataProvider.PublishScriptTempDirectoryPath.Combine(script.Id.ToString()).Path,
                script.Config.TargetFrameworkVersion,
                options.OptimizationLevel,
                script.Config.UseAspNet
            );

            await AddCodeAsync(script, project);
            await AddReferencesAsync(script, project);


            //await project.AddReferencesAsync(references);

            // add db con refs

            // add externalprocess user accessible assemblies

            // Publish
            await project.PublishAsync(options, cancellationToken);

            return Unit.Value;
        }

        private async Task<DotNetCSharpProject> CreateProjectAsync(
            string directoryPath,
            DotNetFrameworkVersion targetFrameworkVersion,
            OptimizationLevel optimizationLevel,
            bool useAspNet)
        {
            var project = new DotNetCSharpProject(
                _dotNetInfo,
                directoryPath,
                packageCacheDirectoryPath: Path.Combine(_settings.PackageCacheDirectoryPath, "NuGet"));

            await project.CreateAsync(
                targetFrameworkVersion,
                ProjectOutputType.Executable,
                useAspNet ? DotNetSdkPack.AspNetApp : DotNetSdkPack.NetApp,
                true);

            await project.SetAllowUnsafeBlocksAsync();
            await project.SetPreprocessorSymbolsAsync(PreprocessorSymbols.For(optimizationLevel));

            return project;
        }

        private async Task AddCodeAsync(Script script, DotNetCSharpProject project)
        {
            var source = new SourceCodeCollection();

            var parsingResult = _codeParser.Parse(script.Code, script.Config.Kind, script.Config.Namespaces, new CodeParsingOptions()
            {
                IncludeAspNetUsings = script.Config.UseAspNet,
            });

            var usings = parsingResult.GetFullProgram().GetAllUsings()
                .Select(u => u.ToCodeString(true))
                .JoinToString(Environment.NewLine);

            var bootstrapperProgramCode = $"{usings}\n\n{parsingResult.BootstrapperProgram.Code.ToCodeString()}";
            source.Add(new SourceCode());

            if (script.DataConnection != null)
            {
                var dataConnectionSource =
                    await _dataConnectionResourcesCache.GetSourceGeneratedCodeAsync(script.DataConnection, script.Config.TargetFrameworkVersion);

                if (dataConnectionSource.ApplicationCode.Any())
                {
                    //runOptions.AdditionalCode.AddRange(connectionCode.ApplicationCode);
                }
            }
        }

        private async Task AddReferencesAsync(Script script, DotNetCSharpProject project)
        {
            throw new NotImplementedException();
        }

        private async Task GetResourcesAsync(Script script)
        {
            // Add source files
            // var source = new SourceCodeCollection();
            var parsingResult = _codeParser.Parse(script.Code, script.Config.Kind, script.Config.Namespaces, new CodeParsingOptions()
            {
                IncludeAspNetUsings = script.Config.UseAspNet,
            });

            var usings = parsingResult.GetFullProgram().GetAllUsings()
                .Select(u => u.ToCodeString(true))
                .JoinToString(Environment.NewLine);

            var bootstrapperProgramCode = $"{usings}\n\n{parsingResult.BootstrapperProgram.Code.ToCodeString()}";

            if (script.DataConnection != null)
            {
                var dataConnectionSource =
                    await _dataConnectionResourcesCache.GetSourceGeneratedCodeAsync(script.DataConnection, script.Config.TargetFrameworkVersion);

                if (dataConnectionSource.ApplicationCode.Any())
                {
                    //runOptions.AdditionalCode.AddRange(connectionCode.ApplicationCode);
                }
            }





            // Add references
            var references = new List<Reference>();

            references.AddRange(script.Config.References);

            if (script.DataConnection != null)
            {
                var dataConnectionReferences =
                    await _dataConnectionResourcesCache.GetRequiredReferencesAsync(script.DataConnection, script.Config.TargetFrameworkVersion);

                references.AddRange(dataConnectionReferences);
            }
        }

        private async Task AppendDataConnectionResourcesAsync(
            DotNetFrameworkVersion targetFrameworkVersion,
            DataConnection dataConnection,
            SourceCodeCollection source,
            List<Reference> references)
        {
            var connectionCode = await _dataConnectionResourcesCache.GetSourceGeneratedCodeAsync(dataConnection, targetFrameworkVersion);
            if (connectionCode.ApplicationCode.Any())
            {
                source.AddRange(connectionCode.ApplicationCode);
            }

            var connectionAssembly = await _dataConnectionResourcesCache.GetAssemblyAsync(dataConnection, targetFrameworkVersion);
            if (connectionAssembly != null)
            {
                references.Add(new AssemblyImageReference(connectionAssembly));
            }

            var requiredReferences = await _dataConnectionResourcesCache.GetRequiredReferencesAsync(dataConnection, targetFrameworkVersion);

            if (requiredReferences.Any())
            {
                references.AddRange(requiredReferences);
            }
        }
    }
}
