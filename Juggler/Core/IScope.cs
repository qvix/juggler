namespace Juggler.Core
{
    using System;

    public interface IScope<out TValue> : IDisposable
    {
        TValue Value { get; }
    }
}