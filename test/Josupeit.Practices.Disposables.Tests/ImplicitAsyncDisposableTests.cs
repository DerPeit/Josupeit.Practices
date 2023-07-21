using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Josupeit.Practices.Disposables.Tests;

[TestOf(typeof(ImplicitAsyncDisposable))]
public class ImplicitAsyncDisposableTests
{
    [Test]
    public void Dispose_calls_DisposeManagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public async Task DisposeAsync_calls_DisposeManagedResourcesAsync()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        await subject.DisposeAsync();

        subject.Received().Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Dispose_calls_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Dispose();

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public async Task DisposeAsync_calls_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        await subject.DisposeAsync();

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_fires_Disposing_event()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();
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
    public async Task DisposeAsync_fires_Disposing_event()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();
        var disposingFired = 0;

        subject.Disposing += (sender, e) =>
        {
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
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

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
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

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
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        Assert.That(subject.IsDisposed, Is.False);

        subject.Dispose();

        Assert.That(subject.IsDisposed);
    }

    [Test]
    public async Task DisposeAsync_sets_IsDisposed_to_true()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        Assert.That(subject.IsDisposed, Is.False);

        await subject.DisposeAsync();

        Assert.That(subject.IsDisposed);
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposedManagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeManagedResources");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposedManagedResourcesAsync()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();
        
        Assert.That(new Func<Task>(async () => await subject.DisposeAsync()), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposeUnmanagedResources_during_Dispose()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Action(subject.Dispose), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_in_Disposing_event_handler_dont_prevent_from_calling_DisposeUnmanagedResources_during_DisposeAsync()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Disposing += (_, _) => throw new TestException();

        Assert.That(new Func<Task>(async () => await subject.DisposeAsync()), Throws.InstanceOf<TestException>());

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Dispose_calls_DisposeManagedResources_before_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Do(_ =>
        {
            subject.DidNotReceive().Protected("DisposeUnmanagedResources");
        });

        subject.Dispose();

        subject.Received().Protected("DisposeManagedResources");
        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public async Task DisposeAsync_calls_DisposeManagedResourcesAsync_before_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResourcesAsync")).Do(_ =>
        {
            subject.DidNotReceive().Protected("DisposeUnmanagedResources");
        });

        await subject.DisposeAsync();

        subject.Received().Protected("DisposeManagedResourcesAsync");
        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_during_DisposeManagedResources_prevent_from_calling_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResources")).Throw<TestException>();

        Assert.That(subject.Dispose, Throws.InstanceOf<TestException>());

        subject.DidNotReceive().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Exceptions_during_DisposeManagedResourcesAsync_prevent_from_calling_DisposeUnmanagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.When(x => x.Protected("DisposeManagedResourcesAsync")).Throw<TestException>();

        Assert.That(new Func<Task>(async () => await subject.DisposeAsync()), Throws.InstanceOf<TestException>());

        subject.DidNotReceive().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeManagedResources_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeManagedResources");
    }

    [Test]
    public async Task Subsequent_dispose_calls_dont_call_DisposeManagedResourcesAsync_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        await subject.DisposeAsync();
        await subject.DisposeAsync();

        subject.Received(1).Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Subsequent_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Dispose();
        subject.Dispose();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    public async Task Subsequent_async_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        await subject.DisposeAsync();
        await subject.DisposeAsync();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload", Justification = "This is intended.")]
    public async Task Subsequent_sync_and_async_dispose_calls_dont_call_DisposeUnmanagedResources_twice()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Dispose();
        await subject.DisposeAsync();

        subject.Received(1).Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void ThrowIfDispose_throws_if_disposed()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        subject.Dispose();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task ThrowIfDispose_throws_if_disposed_asynchronously()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        await subject.DisposeAsync();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void ThrowIfDispose_does_not_throw_if_not_disposed()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

        Assert.That(() => subject.Protected("ThrowIfDisposed"), Throws.Nothing);
    }

    [Test]
    public void Dispose_suppresses_finalization()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

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
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();

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
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.Received().Protected("DisposeUnmanagedResources");
    }

    [Test]
    public void Finalization_does_not_call_DisposeManagedResources()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.DidNotReceive().Protected("DisposeManagedResources");
    }

    [Test]
    public void Finalization_does_not_call_DisposeManagedResourcesAsync()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        finalizer!.Invoke(subject, null);

        subject.DidNotReceive().Protected("DisposeManagedResourcesAsync");
    }

    [Test]
    public void Finalization_does_not_fire_Disposing_event()
    {
        var subject = Substitute.ForPartsOf<ImplicitAsyncDisposable>();
        var finalizer = typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic);

        subject.Disposing += (_, _) => Assert.Fail();

        finalizer!.Invoke(subject, null);
    }
}