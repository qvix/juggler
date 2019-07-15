namespace Juggler.Core
{
    using System;

    public static class Scope<TValue> 
    {
        [ThreadStatic] private static ScopeBase Top;

        public static IScope<TValue> Create(TValue value)
        {
            return new ScopeInstance(value);
        }

        public static IScope<TValue> CreateTransparent(TValue value)
        {
            return new TransparentScopeInstance(value);
        }

        public static bool InScope
        {
            get
            {
                var currentScope = Top;
                return currentScope != null && !currentScope.IsDisposed;
            }
        }

        public static TValue Current
        {
            get
            {
                var currentScope = Top;
                if (currentScope == null || currentScope.IsDisposed)
                {
                    throw new InvalidOperationException($"Out of scope ({typeof(TValue)})");
                }

                return currentScope.Value;
            }
        }

        private abstract class ScopeBase : IScope<TValue>
        {
            protected readonly ScopeBase parent;
            public TValue Value { get; }
            public bool IsDisposed { get; protected set; }

            protected ScopeBase(TValue value)
            {
                this.parent = Top;
                this.Value = value;
                Top = this;
            }

            void IDisposable.Dispose() {}
        }

        private class TransparentScopeInstance : ScopeBase, IDisposable
        {
            public TransparentScopeInstance(TValue value) : base(value) { }

            public void Dispose()
            {
                if (this.IsDisposed)
                {
                    return;
                }

                if (!ReferenceEquals(Top, this))
                {
                    return;
                }

                Top = this.parent;

                this.IsDisposed = true;
            }
        }

        private class ScopeInstance : ScopeBase, IDisposable
        {
            public ScopeInstance(TValue value) : base(value) { }

            void IDisposable.Dispose()
            {
                if (this.IsDisposed)
                {
                    return;
                }

                if (!ReferenceEquals(Top, this))
                {
                    return;
                }

                Top = this.parent;

                using (this.Value as IDisposable) { }

                this.IsDisposed = true;
            }
        }
    }
}