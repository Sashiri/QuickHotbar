using System;

internal readonly struct Defer : IDisposable {
    readonly Action _action;
    public Defer(Action action) => _action = action;
    public void Dispose() => _action();
}

internal class DefferedFlow<T> { 
    
}