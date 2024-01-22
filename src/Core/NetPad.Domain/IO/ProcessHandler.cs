using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NetPad.IO;

public record ProcessResult(int ExitCode, string? Output, string? Error)
{
    public bool HasError => ExitCode != 0;

    public void EnsureSuccessful()
    {
        if (HasError)
        {
            throw new Exception(!string.IsNullOrWhiteSpace(Error) ? Error : Output);
        }
    }
}

public sealed class ProcessHandler
{
    public ProcessHandler(ProcessStartInfo processStartInfo)
    {
        ProcessStartInfo = processStartInfo;
    }

    public ProcessStartInfo ProcessStartInfo { get; }
    public bool IsRunning { get; private set; }

    public ProcessHandler WithRedirectIO()
    {
        ProcessStartInfo.UseShellExecute = false;
        ProcessStartInfo.RedirectStandardOutput = true;
        ProcessStartInfo.RedirectStandardError = true;
        // We're not redirecting input since this class does not yet offer any api to consume redirected input
        return this;
    }

    public async Task<ProcessResult> RunAsync(CancellationToken cancellationToken = default)
    {
        ProcessStartInfo.CreateNoWindow = true;
        ProcessStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

        using var p = Process.Start(ProcessStartInfo);

        if (p == null)
        {
            throw new Exception($"Process not found. Could not start {ProcessStartInfo.FileName} with args: {ProcessStartInfo.Arguments}");
        }

        IsRunning = true;

        try
        {
            await p.WaitForExitAsync(cancellationToken);

            string output = ProcessStartInfo.RedirectStandardOutput ? await p.StandardOutput.ReadToEndAsync() : string.Empty;
            string errors = ProcessStartInfo.RedirectStandardError ? await p.StandardError.ReadToEndAsync() : string.Empty;
            return new ProcessResult(p.ExitCode, output, errors);
        }
        finally
        {
            IsRunning = false;
        }
    }

    public static ProcessHandler Create(ProcessStartInfo processStartInfo)
    {
        return new ProcessHandler(processStartInfo);
    }

    public static async Task<ProcessResult> RunAsync(ProcessStartInfo processStartInfo)
    {
        var handler = new ProcessHandler(processStartInfo);

        return await handler.RunAsync();
    }
}
