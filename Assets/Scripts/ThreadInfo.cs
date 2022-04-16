using System;

public readonly struct ThreadInfo
{
    public readonly Action<object> Callback;
    public readonly object Parameter;

    public ThreadInfo(Action<object> callback, object parameter)
    {
        Callback = callback;
        Parameter = parameter;
    }
}