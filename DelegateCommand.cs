using System;
using System.Windows.Input;

namespace SNSelfOrder
{
    /*
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;
        bool canExecuteCache;

        // 建構子(多型)
        public DelegateCommand(Action<object> execute, object canExecute) : this(execute, null)
        {
        }
        // 建構子(傳入參數)
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            // 簡化寫法 if(execute == null) throw new ArgumentNullException("execute");
            this._execute = execute ?? throw new ArgumentNullException("execute");
            this._canExecute = canExecute;
        }

        #region -- ICommand Members --
        // 在XAML使用Interaction繫結這個事件
        public event EventHandler CanExecuteChanged;

        // 下面兩個方法是提供給 View 使用的
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            bool temp = _canExecute(parameter);

            if (canExecuteCache != temp)
            {
                canExecuteCache = temp;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }
            return canExecuteCache;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion
    }*/

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action executeMethod)
            : base(o => executeMethod())
        { 
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecuteMethod;
        private readonly Action<T> executeMethod;
        private bool isExecuting;

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if ((executeMethod == null) && (canExecuteMethod == null))
            {
                throw new ArgumentNullException("executeMethod", @"Execute Method cannot be null");
            }
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return !isExecuting && CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            isExecuting = true;
            try
            {
                RaiseCanExecuteChanged();
                Execute((T)parameter);
            }
            finally
            {
                isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public bool CanExecute(T parameter)
        {
            if (canExecuteMethod == null)
                return true;

            return canExecuteMethod(parameter);
        }

        public void Execute(T parameter)
        {
            executeMethod(parameter);
        }
    }
}
