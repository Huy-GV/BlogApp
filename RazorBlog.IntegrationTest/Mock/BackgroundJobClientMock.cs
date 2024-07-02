using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.States;

namespace RazorBlog.IntegrationTest.Mock;
internal class BackgroundJobClientMock : IBackgroundJobClient
{
    public bool ChangeState([NotNull] string jobId, [NotNull] IState state, [CanBeNull] string expectedState)
    {
        return true;
    }

    public string Create([NotNull] Job job, [NotNull] IState state)
    {
        return Guid.NewGuid().ToString();
    }
}
