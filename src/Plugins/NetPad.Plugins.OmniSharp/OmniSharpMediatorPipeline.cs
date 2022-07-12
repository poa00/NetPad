using MediatR;
using NetPad.Plugins.OmniSharp.Features;

namespace NetPad.Plugins.OmniSharp;

public class OmniSharpMediatorPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse?>
    where TRequest : class, IRequest<TResponse>
{
    private readonly OmniSharpServerCatalog _serverCatalog;
    private readonly AppOmniSharpServerAccessor _appOmniSharpServerAccessor;
    private readonly ILogger<OmniSharpMediatorPipeline<TRequest, TResponse>> _logger;

    public OmniSharpMediatorPipeline(
        OmniSharpServerCatalog serverCatalog,
        AppOmniSharpServerAccessor appOmniSharpServerAccessor,
        ILogger<OmniSharpMediatorPipeline<TRequest, TResponse>> logger)
    {
        _serverCatalog = serverCatalog;
        _appOmniSharpServerAccessor = appOmniSharpServerAccessor;
        _logger = logger;
    }

    public async Task<TResponse?> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse?> next)
    {
        // This pipeline should only process requests from current assembly
        if (typeof(Plugin).Assembly != typeof(TRequest).Assembly)
        {
            return await next();
        }

        if (request is ITargetSpecificOmniSharpServer targetsSpecificOmniSharp)
        {
            var server = await _serverCatalog.GetOmniSharpServerAsync(targetsSpecificOmniSharp.ScriptId);
            if (server == null)
            {
                bool isResponseNullable = Nullable.GetUnderlyingType(typeof(TResponse)) != null;
                if (isResponseNullable)
                {
                    return default;
                }

                throw new Exception($"Could not find an {nameof(AppOmniSharpServer)} for script ID '{targetsSpecificOmniSharp.ScriptId}'");
            }

            _appOmniSharpServerAccessor.Set(server);
        }

        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred processing request of type '{RequestType}' with an expected response of type '{ResponseType}'",
                typeof(TRequest),
                typeof(TResponse));
            throw;
        }
    }
}
