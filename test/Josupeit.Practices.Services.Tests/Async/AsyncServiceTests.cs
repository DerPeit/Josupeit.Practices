using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(AsyncService))]
public class AsyncServiceTests : AsyncServiceTests<AsyncService>
{
}

public abstract class AsyncServiceTests<TService> where TService : class, IAsyncService
{
    [Test]
    public void Creation_creates_service_stopped()
    {
        var subject = Substitute.ForPartsOf<TService>();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Stopped));
    }

    [Test]
    public void Stopping_stopped_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        Assert.That(subject.StopAsync, Throws.Nothing);
    }

    [Test]
    public async Task Stopping_stopped_service_does_not_call_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StopAsync();

        subject.DidNotReceive().InvokeMember("OnStopAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public void Pausing_stopped_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        Assert.That(subject.PauseAsync, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Pausing_stopped_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        Assert.That(subject.PauseAsync, Throws.InvalidOperationException);
    }

    [Test]
    public void Resuming_stopped_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        Assert.That(subject.ResumeAsync, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resuming_stopped_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        Assert.That(subject.ResumeAsync, Throws.InvalidOperationException);
    }

    [Test]
    public async Task Starting_stopped_service_invokes_OnStart_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.Received().InvokeMember("OnStartAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Starting_stopped_service_sets_starting_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Starting));

                assertionEvaluated = true;
            });

        await subject.StartAsync();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public async Task Starting_stopped_service_sets_running_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public void Starting_stopped_service_lets_exceptions_during_OnStart_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        Assert.That(subject.StartAsync, Throws.InstanceOf<TestException>());
    }

    [Test]
    public async Task Starting_stopped_service_reverts_to_stopped_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        try
        {
            await subject.StartAsync();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Stopped));
    }

    [Test]
    public void Stopped_service_throws_when_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.InvalidOperationException);
    }

    [Test]
    public void Stopped_service_throws_when_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
    }

    [Test]
    public void Stopped_service_does_not_throw_when_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
    }

    [Test]
    public void Stopped_service_does_not_throw_when_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
    }

    [Test]
    public async Task Starting_running_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.StartAsync, Throws.Nothing);
    }

    [Test]
    public async Task Starting_running_service_does_not_call_OnStart_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.StartAsync();

        subject.DidNotReceive().InvokeMember("OnStartAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Pausing_running_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.PauseAsync, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public async Task Pausing_running_service_calls_OnPause_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.PauseAsync();

        subject.Received().InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Pausing_running_service_sets_pausing_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Pausing));

                assertionEvaluated = true;
            });

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.PauseAsync();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public async Task Pausing_running_service_sets_paused_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.PauseAsync();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Paused));
    }

    [Test]
    public async Task Pausing_running_service_lets_exceptions_during_OnPause_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.PauseAsync, Throws.InstanceOf<TestException>());
    }

    [Test]
    public async Task Pause_running_service_reverts_to_running_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        try
        {
            await subject.PauseAsync();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public async Task Resuming_running_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.ResumeAsync, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public async Task Resuming_running_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.ResumeAsync, Throws.InvalidOperationException);
    }

    [Test]
    public async Task Stopping_running_service_invokes_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.StopAsync();

        subject.Received().InvokeMember("OnStopAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Stopping_running_service_sets_stopping_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Stopping));

                assertionEvaluated = true;
            });

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.StopAsync();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public async Task Stopping_running_service_sets_stopped_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        await subject.StopAsync();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Stopped));
    }

    [Test]
    public async Task Stopping_running_service_lets_exceptions_during_OnStop_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.StopAsync, Throws.InstanceOf<TestException>());
    }

    [Test]
    public async Task Stopping_running_service_reverts_to_running_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        try
        {
            await subject.StopAsync();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public async Task Running_service_does_not_throw_when_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
    }

    [Test]
    public async Task Running_service_does_not_throw_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.Nothing);
    }

    [Test]
    public async Task Running_service_throws_when_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.InvalidOperationException);
    }

    [Test]
    public async Task Running_service_does_not_throw_when_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        await subject.StartAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
    }

    [Test]
    public async Task Stopping_paused_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.StopAsync, Throws.Nothing);
    }

    [Test]
    public async Task Stopping_paused_service_calls_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.StopAsync();

        subject.Received().InvokeMember("OnStopAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Stopping_paused_service_sets_stopping_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Stopping));

                assertionEvaluated = true;
            });

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.StopAsync();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public async Task Stopping_paused_service_sets_stopped_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.StopAsync();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Stopped));
    }

    [Test]
    public async Task Stopping_paused_service_lets_exceptions_during_OnStop_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.StopAsync, Throws.InstanceOf<TestException>());
    }

    [Test]
    public async Task Stopping_paused_service_reverts_to_paused_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        try
        {
            await subject.StopAsync();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Paused));
    }

    [Test]
    public async Task Pausing_paused_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.PauseAsync, Throws.Nothing);
    }

    [Test]
    public async Task Pausing_paused_service_does_not_call_OnPause_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.PauseAsync();

        subject.DidNotReceive().InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Resuming_paused_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.ResumeAsync, Throws.Nothing);
    }

    [Test]
    public async Task Resuming_paused_service_calls_OnResume_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.ResumeAsync();

        subject.Received().InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Resuming_paused_service_sets_resuming_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Resuming));

                assertionEvaluated = true;
            });

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.ResumeAsync();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public async Task Resuming_paused_service_sets_running_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        await subject.ResumeAsync();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public async Task Resuming_paused_service_lets_exceptions_during_OnResume_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.ResumeAsync, Throws.InstanceOf<TestException>());
    }

    [Test]
    public async Task Resuming_paused_service_reverts_to_paused_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Throw<TestException>();

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        try
        {
            await subject.ResumeAsync();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Paused));
    }

    [Test]
    public async Task Starting_paused_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(subject.StartAsync, Throws.InvalidOperationException);
    }

    [Test]
    public async Task Paused_service_does_not_throw_when_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
    }

    [Test]
    public async Task Paused_service_throws_when_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
    }

    [Test]
    public async Task Paused_service_does_not_throw_when_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
    }

    [Test]
    public async Task Paused_service_throws_when_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        await subject.StartAsync();
        await subject.PauseAsync();

        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.InvalidOperationException);
    }

    [Test]
    public async Task Stopping_stopping_from_running_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StopAsync, Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_stopping_from_running_service_does_not_call_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .InvokeMember("OnStopAsync", Arg.Any<CancellationToken>())
            .Returns(OnStopAsync);

        await subject.StartAsync();
        await subject.StopAsync();

        return;

        async Task OnStopAsync(CallInfo _)
        {
            subject.ClearReceivedCalls();

            await subject.StopAsync();

            subject.DidNotReceive().InvokeMember("OnStopAsync", Arg.Any<CancellationToken>());
        }
    }

    [Test]
    public async Task Pausing_stopping_from_running_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.PauseAsync, Throws.InstanceOf<NotSupportedException>());
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Pausing_stopping_from_running_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.PauseAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Resuming_stopping_from_running_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.ResumeAsync, Throws.InstanceOf<NotSupportedException>());
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Resuming_stopping_from_running_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.ResumeAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Starting_stopping_from_running_service_cancels_stop_attempt()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .InvokeMember("OnStopAsync", Arg.Any<CancellationToken>())
            .Returns(OnStopAsync);

        await subject.StartAsync();
        await subject.StopAsync();

        return;

        async Task OnStopAsync(CallInfo ci)
        {
            subject.ClearReceivedCalls();

            var cancellationToken = ci.Arg<CancellationToken>();

            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await subject.StartAsync(CancellationToken.None);

            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }

    [Test]
    public async Task Stopping_from_running_service_throws_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_running_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_running_service_does_not_throw_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_running_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_stopping_from_pausing_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StopAsync, Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_stopping_from_pausing_service_does_not_call_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .InvokeMember("OnStopAsync", Arg.Any<CancellationToken>())
            .Returns(OnStopAsync);

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();

        return;

        async Task OnStopAsync(CallInfo _)
        {
            subject.ClearReceivedCalls();

            await subject.StopAsync();

            subject.DidNotReceive().InvokeMember("OnStopAsync", Arg.Any<CancellationToken>());
        }
    }

    [Test]
    public async Task Pausing_stopping_from_pausing_service_cancels_stop_attempt()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .InvokeMember("OnStopAsync", Arg.Any<CancellationToken>())
            .Returns(OnStopAsync);

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();

        return;

        async Task OnStopAsync(CallInfo ci)
        {
            subject.ClearReceivedCalls();

            var cancellationToken = ci.Arg<CancellationToken>();

            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await subject.PauseAsync(CancellationToken.None);

            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }

    [Test]
    public async Task Resuming_stopping_from_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.ResumeAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Starting_stopping_from_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StartAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_pausing_service_throws_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_pausing_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_pausing_service_does_not_throw_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_from_pausing_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStopAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.StopAsync();
    }

    [Test]
    public async Task Stopping_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StopAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Pausing_pausing_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.PauseAsync, Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Pausing_pausing_service_does_not_call_OnPause_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>())
            .Returns(OnPauseAsync);

        await subject.StartAsync();
        await subject.PauseAsync();

        return;

        async Task OnPauseAsync(CallInfo _)
        {
            subject.ClearReceivedCalls();

            await subject.PauseAsync();

            subject.DidNotReceive().InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>());
        }
    }

    [Test]
    public async Task Resuming_pausing_service_cancels_pause_attempt()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>())
            .Returns(OnPauseAsync);

        await subject.StartAsync();
        await subject.PauseAsync();

        return;

        async Task OnPauseAsync(CallInfo ci)
        {
            subject.ClearReceivedCalls();

            var cancellationToken = ci.Arg<CancellationToken>();

            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await subject.ResumeAsync(CancellationToken.None);

            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }

    [Test]
    public async Task Starting_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StartAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Pausing_service_does_not_throw_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Pausing_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Pausing_service_does_not_throw_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Pausing_service_throws_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPauseAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
    }

    [Test]
    public async Task Stopping_resuming_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StopAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Pausing_resuming_service_cancels_resume_attempt()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>())
            .Returns(OnResumeAsync);

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();

        return;

        async Task OnResumeAsync(CallInfo ci)
        {
            subject.ClearReceivedCalls();

            var cancellationToken = ci.Arg<CancellationToken>();

            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await subject.PauseAsync(CancellationToken.None);

            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }

    [Test]
    public async Task Resuming_resuming_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.ResumeAsync, Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Resuming_resuming_service_does_not_call_OnResume_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>())
            .Returns(OnResumeAsync);

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();

        return;

        async Task OnResumeAsync(CallInfo _)
        {
            subject.ClearReceivedCalls();

            await subject.ResumeAsync();

            subject.DidNotReceive().InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>());
        }
    }

    [Test]
    public async Task Starting_resuming_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StartAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Resuming_service_does_not_throw_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Resuming_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Resuming_service_throws_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Resuming_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResumeAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        await subject.StartAsync();
        await subject.PauseAsync();
        await subject.ResumeAsync();
    }

    [Test]
    public async Task Stopping_starting_service_cancels_start_attempt()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .InvokeMember("OnStartAsync", Arg.Any<CancellationToken>())
            .Returns(OnStartAsync);

        await subject.StartAsync();

        return;

        async Task OnStartAsync(CallInfo ci)
        {
            subject.ClearReceivedCalls();

            var cancellationToken = ci.Arg<CancellationToken>();

            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await subject.StopAsync(CancellationToken.None);

            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }

    [Test]
    public async Task Pausing_starting_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.PauseAsync, Throws.InstanceOf<NotSupportedException>());
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Pausing_starting_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.PauseAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Resuming_starting_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.ResumeAsync, Throws.InstanceOf<NotSupportedException>());
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Resuming_starting_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.ResumeAsync, Throws.InvalidOperationException);
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Starting_starting_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.StartAsync, Throws.Nothing);
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Starting_starting_service_does_not_call_OnStart_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .InvokeMember("OnStartAsync", Arg.Any<CancellationToken>())
            .Returns(OnStartAsync);

        await subject.StartAsync();

        return;

        async Task OnStartAsync(CallInfo _)
        {
            subject.ClearReceivedCalls();

            await subject.StartAsync();

            subject.DidNotReceive().InvokeMember("OnStartAsync", Arg.Any<CancellationToken>());
        }
    }

    [Test]
    public async Task Starting_service_does_not_throw_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Starting_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Starting_service_throws_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.InvalidOperationException);
            });

        await subject.StartAsync();
    }

    [Test]
    public async Task Starting_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStartAsync", Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        await subject.StartAsync();
    }
}