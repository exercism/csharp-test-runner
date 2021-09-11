using System;
using Xunit;

public class FakeTests
{
    private class DisposableResource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public void DisposableObjectsAreDisposedWhenThrowingAnException()
    {
        var disposableResource = new DisposableResource();

        Assert.Throws<Exception>(() => Fake.DisposableResourcesAreDisposedWhenExceptionIsThrown(disposableResource));
        Assert.True(disposableResource.IsDisposed);
    }
}