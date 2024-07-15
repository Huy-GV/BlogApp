using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging;

namespace RazorBlog.Core.WriteServices;
internal class FakeBackgroundJobClient : IBackgroundJobClient
{
    private readonly ILogger<FakeBackgroundJobClient> _logger;

    public FakeBackgroundJobClient(ILogger<FakeBackgroundJobClient> logger)
    {
        _logger = logger;
    }

    public bool ChangeState([NotNull] string jobId, [NotNull] IState state, [CanBeNull] string expectedState)
    {
        _logger.LogWarning("Fake background job client is used, do not rely on returned value");
        return false;
    }

    public string Create([NotNull] Job job, [NotNull] IState state)
    {
        _logger.LogWarning("Fake background job client is used, no background job was created");
        return string.Empty;
    }
}
