namespace Juggler.Transactions
{
    using Juggler.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Transaction: IDisposable
    {
        private readonly TransactionActions actions;
        private readonly IScope<TransactionActions> scope;
        private readonly Action<Exception> exceptionHandler;
        private TransactionState state;

        private Transaction(IsolationLevel isolationLevel, Action<Exception> exceptionHandler)
        {
            this.exceptionHandler = exceptionHandler;
            switch (isolationLevel)
            {
                case IsolationLevel.Attach:
                    if (!Scope<TransactionActions>.InScope)
                    {
                        this.scope = Scope<TransactionActions>.Create(new TransactionActions());
                    }
                    this.actions = Scope<TransactionActions>.Current;
                    break;
                case IsolationLevel.Supress:
                    this.actions = new TransactionActions();
                    break;
            }
            this.state = TransactionState.Active;
        }

        public static void Execute(Action<Transaction> action)
        {
            Execute(IsolationLevel.Attach, action);
        }

        public static void Execute(IsolationLevel level, Action<Transaction> action)
        {
            Execute(level, action, _ => { });
        }

        public static void Execute(IsolationLevel level, Action<Transaction> action,
            Action<Exception> exceptionHandler)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var transaction = new Transaction(level, exceptionHandler))
            {
                action(transaction);
                transaction.Commit();
            }
        }

        public void AddRollback(Action rollbackAction)
        {
            this.actions.AddRollback(rollbackAction);
        }

        private void Commit()
        {
            if (this.state != TransactionState.Active)
            {
                return;
            }

            this.state = TransactionState.Committed;
        }

        void IDisposable.Dispose()
        {
            var currentState = this.state;
            if (currentState == TransactionState.Disposing)
            {
                return;
            }

            this.state = TransactionState.Disposing;
            if (currentState != TransactionState.Committed)
            {
                this.actions.Rollback(exceptionHandler);
            }

            if (this.scope != null)
            {
                using (scope) { }
            }
        }

        private enum TransactionState
        {
            Active,
            Disposing,
            Committed
        }

        private class TransactionActions
        {
            private readonly IList<Action> rollbackActions = new List<Action>();
            private bool rollbackExecuted;

            public void AddRollback(Action rollbackAction)
            {
                if (this.rollbackExecuted)
                {
                    throw new InvalidOperationException("Rollback already executed");
                }
                this.rollbackActions.Add(rollbackAction ?? throw new ArgumentNullException(nameof(rollbackAction)));
            }

            public void Rollback(Action<Exception> exceptionHandler)
            {
                if (this.rollbackExecuted)
                {
                    return;
                }

                foreach (var action in this.rollbackActions.Reverse())
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception exception)
                    {
                        exceptionHandler(exception);
                    }
                }
                this.rollbackExecuted = true;
            }
        }
    }
}