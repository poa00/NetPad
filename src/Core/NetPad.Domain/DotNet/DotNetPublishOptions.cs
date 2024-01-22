using Microsoft.CodeAnalysis;

namespace NetPad.DotNet;

public class DotNetPublishOptions
{
        public DotNetPublishOptions(
        string assemblyName,
        string directoryPath,
        OutputKind outputKind,
        OptimizationLevel optimizationLevel,
        string? runtimeId,
        bool selfContained,
        bool publishReadyToRun,
        bool publishSingleFile,
        bool publishTrimmed,
        bool embedNativeLibraries,
        bool embedPdbs,
        bool deleteExistingFiles)
    {
        AssemblyName = assemblyName;
        DirectoryPath = directoryPath;
        OutputKind = outputKind;
        OptimizationLevel = optimizationLevel;
        RuntimeId = runtimeId;
        SelfContained = selfContained;
        PublishReadyToRun = publishReadyToRun;
        PublishSingleFile = publishSingleFile;
        PublishTrimmed = publishTrimmed;
        EmbedNativeLibraries = embedNativeLibraries;
        EmbedPdbs = embedPdbs;
        DeleteExistingFiles = deleteExistingFiles;
    }

    public string AssemblyName { get; }
    public string DirectoryPath { get; }
    public OutputKind OutputKind { get; }
    public OptimizationLevel OptimizationLevel { get; }
    public string? RuntimeId { get; }
    public bool SelfContained { get; }
    public bool PublishReadyToRun { get; }
    public bool PublishSingleFile { get; }
    public bool PublishTrimmed { get; }
    public bool EmbedNativeLibraries { get; }
    public bool EmbedPdbs { get; }
    public bool DeleteExistingFiles { get; }

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(AssemblyName))
        {

        }

        if (string.IsNullOrWhiteSpace(DirectoryPath))
        {

        }



        //...


        if (DeleteExistingFiles)
        {
            // Maybe check we're not mistakenly deleting somewhere dangerous
        }

        return false;
    }
}
