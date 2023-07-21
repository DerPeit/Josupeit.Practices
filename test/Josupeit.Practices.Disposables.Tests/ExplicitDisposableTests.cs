using System;
using System.Reflection;
using System.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Disposables.Tests;

[TestOf(typeof(ExplicitDisposable))]
public class ExplicitDisposableTests
{
    [Test]
    public void ExplicitDisposable_does_not_expose_members()
    {
        Assert.That(typeof(ExplicitDisposable).GetEvent("Disposing", BindingFlags.Instance | BindingFlags.NonPublic), Is.Not.Null);
        Assert.That(typeof(ExplicitDisposable).GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.NonPublic), Is.Not.Null);
        Assert.That(typeof(ExplicitDisposable).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic, binder: null, Type.EmptyTypes, modifiers: null), Is.Not.Null);
    }

    [Test]
    public void Dispose_calls_DisposeManagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public void Dispose_calls_DisposeUnmanagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_fires_Disposing_event()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();
        var disposingFired = 0;

        subject.Disposing += (sender, e) =>
        {
            // ReSharper disable once AccessToDisposedClosure
            Assert.That(sender, Is.SameAs(subject));
            Assert.That(e, Is.SameAs(EventArgs.Empty));

            Interlocked.Increment(ref disposingFired);
        };

        subject.Dispose();

        Assert.That(disposingFired, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_does_not_fire_detached_Disposing_event_handler()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        static void OnSubjectOnDisposing(object? sender, EventArgs e)
        {
            Assert.Fail();
        }

        subject.Disposing += OnSubjectOnDisposing;
        subject.Disposing -= OnSubjectOnDisposing;

        subject.Dispose();
    }

    [Test]
    public void Dispose_sets_IsDisposed_to_true()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        Assert.That(subject.IsDisposed, Is.False);

        subject.Dispose();

        Assert.That(subject.IsDisposed);
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposedManagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposeUnmanagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_calls_DisposeManagedResources_before_DisposeUnmanagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Do(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            subject.DidNotReceive().Protected("DisposeUnmanagedResources");
        });

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_during_DisposeManagedResources_prevent_from_calling_DisposeUnmanagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Throw<TestException>();

        Assert.That(subject.Dispose, Throws.InstanceOf<TestException>());

        subject.DidNotReceive().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeManagedResources_twice()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeManagedResources");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void ThrowIfDispose_throws_if_disposed()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        subject.Dispose();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void ThrowIfDispose_does_not_throw_if_not_disposed()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.Nothing);
    }

    [Test]
    public void Dispose_suppresses_finalization()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();

        GCProvider.Current = Substitute.For<GCProvider>();

        try
        {
            subject.Dispose();

            GCProvider.Current.Received().SuppressFinalize(subject);
        }
        finally
        {
            GCProvider.Current = GCProvider.Default;
        }
    }

    [Test]
    public void Finalization_calls_DisposeUnmanagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Finalization_does_not_call_DisposeManagedResources()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.DidNotReceive().Protected("DisposeManagedResources");
    }

    [Test]
    public void Finalization_does_not_fire_Disposing_event()
    {
        INotifyingDisposable subject = Substitute.ForPartsOf<ExplicitDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        subject.Disposing += (_, _) => Assert.Fail();

        finalizer!.Invoke(subject, null);
    }
}