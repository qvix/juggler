namespace JugglerTests
{
    using System;
    using Juggler.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScopeTests
    {
        [TestMethod]
        public void ScopeShouldKeepValue()
        {
            var testValue = "TestValue";
            var testValue2 = "SecondValue";

            using (Scope<string>.Create(testValue))
            {
                Assert.AreEqual(testValue, Scope<string>.Current);
                using (Scope<string>.Create(testValue2))
                {
                    Assert.AreEqual(testValue2, Scope<string>.Current);
                }
                Assert.IsTrue(Scope<string>.InScope);
                Assert.AreEqual(testValue, Scope<string>.Current);
            }

            Assert.IsFalse(Scope<string>.InScope);
        }

        [TestMethod]
        public void TransparentScopeShouldNotDisposeValue()
        {
            var value = new FragileValue();
            using (Scope<FragileValue>.CreateTransparent(value))
            {
                Assert.IsTrue(Scope<FragileValue>.InScope);
            }

            ((IDisposable)value).Dispose();

            Assert.IsFalse(Scope<FragileValue>.InScope);
        }

        private class FragileValue : IDisposable
        {
            private bool isDisposed;

            public void Dispose()
            {
                if (this.isDisposed)
                {
                    throw new InvalidOperationException("Disposed");
                }

                this.isDisposed = true;
            }
        }

    }
}