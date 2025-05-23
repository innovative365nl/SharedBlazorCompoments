using System;

namespace Innovative.Blazor.Components.Tests.TestBase;

public abstract class TestBaseService : IDisposable
{
    protected virtual void Dispose(bool disposing)
    {
        // Clean up managed resources
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
