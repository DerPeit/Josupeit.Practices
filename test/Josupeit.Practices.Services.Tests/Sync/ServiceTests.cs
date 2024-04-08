using System;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Services.Tests;

[TestOf(typeof(Service))]
public class ServiceTests : ServiceTests<Service>
{
}

public abstract class ServiceTests<TService> where TService : class, IService
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

        Assert.That(subject.Stop, Throws.Nothing);
    }

    [Test]
    public void Stopping_stopped_service_does_not_call_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Stop();

        subject.DidNotReceive().InvokeMember("OnStop");
    }

    [Test]
    public void Pausing_stopped_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        Assert.That(subject.Pause, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Pausing_stopped_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        Assert.That(subject.Pause, Throws.InvalidOperationException);
    }

    [Test]
    public void Resuming_stopped_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        Assert.That(subject.Resume, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resuming_stopped_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        Assert.That(subject.Resume, Throws.InvalidOperationException);
    }

    [Test]
    public void Starting_stopped_service_invokes_OnStart_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();

        subject.Received().InvokeMember("OnStart");
    }

    [Test]
    public void Starting_stopped_service_sets_starting_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Starting));

                assertionEvaluated = true;
            });

        subject.Start();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public void Starting_stopped_service_sets_running_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public void Starting_stopped_service_lets_exceptions_during_OnStart_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Throw<TestException>();

        Assert.That(subject.Start, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Starting_stopped_service_reverts_to_stopped_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Throw<TestException>();

        try
        {
            subject.Start();
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
    public void Starting_running_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(subject.Start, Throws.Nothing);
    }

    [Test]
    public void Starting_running_service_does_not_call_OnStart_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        subject.Start();

        subject.DidNotReceive().InvokeMember("OnStart");
    }

    [Test]
    public void Pausing_running_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);
        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(subject.Pause, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Pausing_running_service_calls_OnPause_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.ClearReceivedCalls();

        subject.Pause();

        subject.Received().InvokeMember("OnPause");
    }

    [Test]
    public void Pausing_running_service_sets_pausing_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Pausing));

                assertionEvaluated = true;
            });

        subject.Start();
        subject.ClearReceivedCalls();

        subject.Pause();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public void Pausing_running_service_sets_paused_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.ClearReceivedCalls();

        subject.Pause();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Paused));
    }

    [Test]
    public void Pausing_running_service_lets_exceptions_during_OnPause_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Throw<TestException>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(subject.Pause, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Pause_running_service_reverts_to_running_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Throw<TestException>();

        subject.Start();
        subject.ClearReceivedCalls();

        try
        {
            subject.Pause();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public void Resuming_running_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);
        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(subject.Resume, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public void Resuming_running_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(subject.Resume, Throws.InvalidOperationException);
    }

    [Test]
    public void Stopping_running_service_invokes_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        subject.Stop();

        subject.Received().InvokeMember("OnStop");
    }

    [Test]
    public void Stopping_running_service_sets_stopping_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Stopping));

                assertionEvaluated = true;
            });

        subject.Start();
        subject.ClearReceivedCalls();

        subject.Stop();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public void Stopping_running_service_sets_stopped_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        subject.Stop();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Stopped));
    }

    [Test]
    public void Stopping_running_service_lets_exceptions_during_OnStop_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Throw<TestException>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(subject.Stop, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Stopping_running_service_reverts_to_running_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Throw<TestException>();

        subject.Start();
        subject.ClearReceivedCalls();

        try
        {
            subject.Stop();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public void Running_service_does_not_throw_when_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
    }

    [Test]
    public void Running_service_does_not_throw_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.Nothing);
    }

    [Test]
    public void Running_service_throws_when_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.InvalidOperationException);
    }

    [Test]
    public void Running_service_does_not_throw_when_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.Start();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
    }

    [Test]
    public void Stopping_paused_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(subject.Stop, Throws.Nothing);
    }

    [Test]
    public void Stopping_paused_service_calls_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Stop();

        subject.Received().InvokeMember("OnStop");
    }

    [Test]
    public void Stopping_paused_service_sets_stopping_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Stopping));

                assertionEvaluated = true;
            });

        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Stop();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public void Stopping_paused_service_sets_stopped_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Stop();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Stopped));
    }

    [Test]
    public void Stopping_paused_service_lets_exceptions_during_OnStop_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Throw<TestException>();

        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(subject.Stop, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Stopping_paused_service_reverts_to_paused_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Throw<TestException>();

        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        try
        {
            subject.Stop();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Paused));
    }

    [Test]
    public void Pausing_paused_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(subject.Pause, Throws.Nothing);
    }

    [Test]
    public void Pausing_paused_service_does_not_call_OnPause_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Pause();

        subject.DidNotReceive().InvokeMember("OnPause");
    }

    [Test]
    public void Resuming_paused_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(subject.Resume, Throws.Nothing);
    }

    [Test]
    public void Resuming_paused_service_calls_OnResume_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Resume();

        subject.Received().InvokeMember("OnResume");
    }

    [Test]
    public void Resuming_paused_service_sets_resuming_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();
        var assertionEvaluated = false;

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                Assert.That(subject.State, Is.EqualTo(ServiceState.Resuming));

                assertionEvaluated = true;
            });

        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Resume();

        Assert.That(assertionEvaluated, Is.True);
    }

    [Test]
    public void Resuming_paused_service_sets_running_state_correctly()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        subject.Resume();

        Assert.That(subject.State, Is.EqualTo(ServiceState.Running));
    }

    [Test]
    public void Resuming_paused_service_lets_exceptions_during_OnResume_method_bubble_up()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Throw<TestException>();

        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(subject.Resume, Throws.InstanceOf<TestException>());
    }

    [Test]
    public void Resuming_paused_service_reverts_to_paused_state_on_exceptions()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Throw<TestException>();

        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        try
        {
            subject.Resume();
        }
        catch (TestException)
        {
        }

        Assert.That(subject.State, Is.EqualTo(ServiceState.Paused));
    }

    [Test]
    public void Starting_paused_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(subject.Start, Throws.InvalidOperationException);
    }

    [Test]
    public void Paused_service_does_not_throw_when_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
    }

    [Test]
    public void Paused_service_throws_when_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
    }

    [Test]
    public void Paused_service_does_not_throw_when_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
    }

    [Test]
    public void Paused_service_throws_when_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);
        subject.Start();
        subject.Pause();
        subject.ClearReceivedCalls();

        Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.InvalidOperationException);
    }

    [Test]
    public void Stopping_stopping_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Stop, Throws.Nothing);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Stopping_stopping_service_does_not_call_OnStop_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                subject.Stop();

                subject.DidNotReceive().InvokeMember("OnStop");
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Pausing_stopping_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Pause, Throws.InstanceOf<NotSupportedException>());
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Pausing_stopping_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Pause, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Resuming_stopping_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Resume, Throws.InstanceOf<NotSupportedException>());
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Resuming_stopping_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Resume, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Starting_stopping_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Start, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Stopping_service_throws_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Stopping_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Stopping_service_does_not_throw_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Stopping_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStop"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        subject.Start();
        subject.Stop();
    }

    [Test]
    public void Stopping_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Stop, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Pausing_pausing_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Pause, Throws.Nothing);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Pausing_pausing_service_does_not_call_OnPause_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                subject.Pause();

                subject.DidNotReceive().InvokeMember("OnPause");
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Resuming_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Resume, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Starting_pausing_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Start, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Pausing_service_does_not_throw_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Pausing_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Pausing_service_does_not_throw_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.Nothing);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Pausing_service_throws_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnPause"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
    }

    [Test]
    public void Stopping_resuming_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Stop, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Pausing_resuming_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Pause, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Resuming_resuming_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Resume, Throws.Nothing);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Resuming_resuming_service_does_not_call_OnResume_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                subject.Resume();

                subject.DidNotReceive().InvokeMember("OnResume");
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Starting_resuming_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Start, Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Resuming_service_does_not_throw_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Resuming_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Resuming_service_throws_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.InvalidOperationException);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Resuming_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnResume"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        subject.Start();
        subject.Pause();
        subject.Resume();
    }

    [Test]
    public void Stopping_starting_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Stop, Throws.InvalidOperationException);
            });

        subject.Start();
    }

    [Test]
    public void Pausing_starting_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Pause, Throws.InstanceOf<NotSupportedException>());
            });

        subject.Start();
    }

    [Test]
    public void Pausing_starting_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Pause, Throws.InvalidOperationException);
            });

        subject.Start();
    }

    [Test]
    public void Resuming_starting_service_throws_if_service_cannot_pause()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(false);

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Resume, Throws.InstanceOf<NotSupportedException>());
            });

        subject.Start();
    }

    [Test]
    public void Resuming_starting_service_throws()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject.InvokeMember(nameof(Service.CanPause)).Returns(true);

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Resume, Throws.InvalidOperationException);
            });

        subject.Start();
    }

    [Test]
    public void Starting_starting_service_does_not_throw()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(subject.Start, Throws.Nothing);
            });

        subject.Start();
    }

    [Test]
    public void Starting_starting_service_does_not_call_OnStart_method()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                subject.Start();

                subject.DidNotReceive().InvokeMember("OnStart");
            });

        subject.Start();
    }

    [Test]
    public void Starting_service_does_not_throw_if_ThrowIfStopped_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfStopped"), Throws.Nothing);
            });

        subject.Start();
    }

    [Test]
    public void Starting_service_throws_if_ThrowIfNotRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfNotRunning"), Throws.InvalidOperationException);
            });

        subject.Start();
    }

    [Test]
    public void Starting_service_throws_if_ThrowIfRunning_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfRunning"), Throws.InvalidOperationException);
            });

        subject.Start();
    }

    [Test]
    public void Starting_service_does_not_throw_if_ThrowIfPaused_is_called()
    {
        var subject = Substitute.ForPartsOf<TService>();

        subject
            .When(x => x.InvokeMember("OnStart"))
            .Do(_ =>
            {
                subject.ClearReceivedCalls();

                Assert.That(() => subject.InvokeMember("ThrowIfPaused"), Throws.Nothing);
            });

        subject.Start();
    }
}