using System;
using System.Reflection;
using System.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Disposables.Tests;

[TestOf(typeof(ImplicitDisposable))]
public class ImplicitDisposableTests
{
    [Test]
    public void Dispose_calls_DisposeManagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public void Dispose_calls_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_fires_Disposing_event()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();
        var disposingFired = 0;

        subject.Disposing += (sender, e) =>
        {
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
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

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
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        Assert.That(subject.IsDisposed, Is.False);

        subject.Dispose();

        Assert.That(subject.IsDisposed);
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposedManagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_calls_DisposeManagedResources_before_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Do(_ =>
        {
            subject.DidNotReceive().Protected("DisposeUnmanagedResources");
        });

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_during_DisposeManagedResources_prevent_from_calling_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Throw<TestException>();

        Assert.That(subject.Dispose, Throws.InstanceOf<TestException>());

        subject.DidNotReceive().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeManagedResources_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeManagedResources");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void ThrowIfDispose_throws_if_disposed()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        subject.Dispose();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void ThrowIfDispose_does_not_throw_if_not_disposed()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.Nothing);
    }

    [Test]
    public void Dispose_suppresses_finalization()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();

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
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Finalization_does_not_call_DisposeManagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.DidNotReceive().Protected("DisposeManagedResources");
    }

    [Test]
    public void Finalization_does_not_fire_Disposing_event()
    {
        var subject = Substitute.ForPartsOf<ImplicitDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        subject.Disposing += (_, _) => Assert.Fail();

        finalizer!.Invoke(subject, null);
    }
}