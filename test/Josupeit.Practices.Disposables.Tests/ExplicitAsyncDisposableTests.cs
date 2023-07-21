using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Disposables.Tests;

[TestOf(typeof(ExplicitAsyncDisposable))]
public class ExplicitAsyncDisposableTests
{
    [Test]
    public void ExplicitAsyncDisposable_does_not_expose_members()
    {
        Assert.That(typeof(ExplicitAsyncDisposable).GetEvent("Disposing", BindingFlags.Instance | BindingFlags.NonPublic), Is.Not.Null);
        Assert.That(typeof(ExplicitAsyncDisposable).GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.NonPublic), Is.Not.Null);
        Assert.That(typeof(ExplicitAsyncDisposable).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic, binder: null, Type.EmptyTypes, modifiers: null), Is.Not.Null);
        Assert.That(typeof(ExplicitAsyncDisposable).GetMethod("DisposeAsync", BindingFlags.Instance | BindingFlags.NonPublic, binder: null, Type.EmptyTypes, modifiers: null), Is.Not.Null);
    }

    [Test]
    public void Dispose_calls_DisposeManagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public async Task DisposeAsync_calls_DisposeManagedResourcesAsync()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        await subject.DisposeAsync();

        subject.Received().Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Dispose_calls_DisposeUnmanagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public async Task DisposeAsync_calls_DisposeUnmanagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        await subject.DisposeAsync();

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_fires_Disposing_event()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();
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
    public async Task DisposeAsync_fires_Disposing_event()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();
        var disposingFired = 0;

        subject.Disposing += (sender, e) =>
        {
            // ReSharper disable once AccessToDisposedClosure
            Assert.That(sender, Is.SameAs(subject));
            Assert.That(e, Is.SameAs(EventArgs.Empty));

            Interlocked.Increment(ref disposingFired);
        };

        await subject.DisposeAsync();

        Assert.That(disposingFired, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_does_not_fire_detached_Disposing_event_handler()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        static void OnSubjectOnDisposing(object? sender, EventArgs e)
        {
            Assert.Fail();
        }

        subject.Disposing += OnSubjectOnDisposing;
        subject.Disposing -= OnSubjectOnDisposing;

        subject.Dispose();
    }

    [Test]
    public async Task DisposeAsync_does_not_fire_detached_Disposing_event_handler()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        static void OnSubjectOnDisposing(object? sender, EventArgs e)
        {
            Assert.Fail();
        }

        subject.Disposing += OnSubjectOnDisposing;
        subject.Disposing -= OnSubjectOnDisposing;

        await subject.DisposeAsync();
    }

    [Test]
    public void Dispose_sets_IsDisposed_to_true()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        Assert.That(subject.IsDisposed, Is.False);

        subject.Dispose();

        Assert.That(subject.IsDisposed);
    }

    [Test]
    public async Task DisposeAsync_sets_IsDisposed_to_true()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        Assert.That(subject.IsDisposed, Is.False);

        await subject.DisposeAsync();

        Assert.That(subject.IsDisposed);
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposedManagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposedManagedResourcesAsync()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Func<Task>(async () => await subject.DisposeAsync()), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposeUnmanagedResources_during_Dispose()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposeUnmanagedResources_during_DisposeAsync()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Func<Task>(async () => await subject.DisposeAsync()), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_calls_DisposeManagedResources_before_DisposeUnmanagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

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
    public async Task DisposeAsync_calls_DisposeManagedResourcesAsync_before_DisposeUnmanagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResourcesAsync")).Do(_ =>
        {
            // ReSharper disable once AccessToDisposedClosure
            subject.DidNotReceive().Protected("DisposeUnmanagedResources");
        });

        await subject.DisposeAsync();

        subject.Received().Protected("DisposeManagedResourcesAsync");
        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_during_DisposeManagedResources_prevent_from_calling_DisposeUnmanagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Throw<TestException>();

        Assert.That(subject.Dispose, Throws.InstanceOf<TestException>());

        subject.DidNotReceive().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_during_DisposeManagedResourcesAsync_prevent_from_calling_DisposeUnmanagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResourcesAsync")).Throw<TestException>();

        Assert.That(new Func<Task>(async () => await subject.DisposeAsync()), Throws.InstanceOf<TestException>());

        subject.DidNotReceive().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeManagedResources_twice()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeManagedResources");
    }

    [Test]
    public async Task Subsequent_async_dispose_calls_dont_call_DisposeManagedResourcesAsync_twice()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        await subject.DisposeAsync();
        await subject.DisposeAsync();

        subject.Received(1).Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    public async Task Subsequent_async_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        await subject.DisposeAsync();
        await subject.DisposeAsync();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload", Justification = "This is intended.")]
    public async Task Subsequent_sync_and_async_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Dispose();
        await subject.DisposeAsync();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void ThrowIfDispose_throws_if_disposed()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        subject.Dispose();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task ThrowIfDispose_throws_if_disposed_asynchronously()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        await subject.DisposeAsync();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void ThrowIfDispose_does_not_throw_if_not_disposed()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.Nothing);
    }

    [Test]
    public void Dispose_suppresses_finalization()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

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
    public async Task DisposeAsync_suppresses_finalization()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();

        GCProvider.Current = Substitute.For<GCProvider>();

        try
        {
            await subject.DisposeAsync();

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
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Finalization_does_not_call_DisposeManagedResources()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.DidNotReceive().Protected("DisposeManagedResources");
    }

    [Test]
    public void Finalization_does_not_call_DisposeManagedResourcesAsync()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.DidNotReceive().Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Finalization_does_not_fire_Disposing_event()
    {
        INotifyingAsyncDisposable subject = Substitute.ForPartsOf<ExplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        subject.Disposing += (_, _) => Assert.Fail();

        finalizer!.Invoke(subject, null);
    }
}