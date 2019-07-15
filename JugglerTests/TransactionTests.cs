namespace JugglerTests
{
    using Juggler.Transactions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void TransactionShouldForward()
        {
            var value = 0;

            Transaction.Execute(transaction =>
            {
                value++;

                transaction.AddRollback(() => value--);

                value++;
            });

            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void TransactionShouldForwardWithInnerTransaction()
        {
            var value = 0;

            Transaction.Execute(transaction =>
            {
                value++;

                transaction.AddRollback(() => value--);

                Transaction.Execute(transaction2 =>
                {
                    value++;
                    transaction2.AddRollback(() => value--);
                });

                value++;
            });

            Assert.AreEqual(3, value);
        }

        [TestMethod]
        public void TransactionShouldRollback()
        {
            var value = 0;

            try
            {
                Transaction.Execute(transaction =>
                {
                    value++;

                    transaction.AddRollback(() => value--);

                    throw new Exception();

                    value++;
                });
            }
            catch (Exception)
            {

            }

            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void TransactionShouldRollbackOnInnerException()
        {
            var value = 0;

            try
            {
                Transaction.Execute(transaction =>
                {
                    value++;

                    transaction.AddRollback(() => value--);

                    Transaction.Execute(transaction2 =>
                    {
                        value++;
                        transaction2.AddRollback(() => value--);
                        throw new Exception();
                    });

                    value++;
                });
            }
            catch (Exception)
            {

            }

            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void TransactionShouldRollbackOnInnerExceptionWithExceptionInRollback()
        {
            var value = 0;

            try
            {
                Transaction.Execute(transaction =>
                {
                    value++;

                    transaction.AddRollback(() =>
                    {
                        value--;
                        throw new Exception("Expected exception");
                    });

                    Transaction.Execute(transaction2 =>
                    {
                        value++;
                        transaction2.AddRollback(() => value--);
                        throw new Exception();
                    });

                    value++;
                });
            }
            catch (Exception)
            {

            }

            Assert.AreEqual(0, value);
        }

    }
}
