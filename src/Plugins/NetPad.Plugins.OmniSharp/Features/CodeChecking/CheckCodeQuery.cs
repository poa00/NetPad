using MediatR;

namespace NetPad.Plugins.OmniSharp.Features.CodeChecking;

public class CheckCodeQuery : OmniSharpScriptQuery<OmniSharpCodeCheckRequest, OmniSharpQuickFixResponse?>
{
    public CheckCodeQuery(Guid scriptId, OmniSharpCodeCheckRequest input) : base(scriptId, input)
    {
    }

    public class Handler : IRequestHandler<CheckCodeQuery, OmniSharpQuickFixResponse?>
    {
        private readonly AppOmniSharpServer _server;

        public Handler(AppOmniSharpServer server)
        {
            _server = server;
        }

        public async Task<OmniSharpQuickFixResponse?> Handle(CheckCodeQuery request, CancellationToken cancellationToken)
        {
            var omniSharpRequest = request.Input;
            int userCodeStartsOnLine = _server.Project.UserCodeStartsOnLine;

            omniSharpRequest.FileName = _server.Project.ProgramFilePath;

            var response = await _server.OmniSharpServer.SendAsync<OmniSharpQuickFixResponse>(omniSharpRequest);

            if (response?.QuickFixes == null)
            {
                return response;
            }

            foreach (var quickFix in response.QuickFixes)
            {
                LineCorrecter.AdjustForResponse(userCodeStartsOnLine, quickFix);
            }

            return response;
        }
    }
}
