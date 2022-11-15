using System;

public static class Fake
{
    public static void DisposableResourcesAreDisposedWhenExceptionIsThrown(IDisposable disposableObject)
    {
        try
        {
            throw new Exception();
        }
        catch(Exception)
        {
            disposableObject.Dispose();
            throw;
        }
    }
}