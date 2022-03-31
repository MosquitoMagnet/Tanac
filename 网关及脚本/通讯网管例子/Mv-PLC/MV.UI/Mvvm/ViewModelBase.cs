using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Mv.Core.Interfaces;
using Prism.Common;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Unity;

namespace Mv.Ui.Mvvm
{
    public abstract class ViewModelBase : BindableBase
    {
        private IMvUser _mvUser;

        protected ViewModelBase(IUnityContainer container)
        {
            Container = container;
   
            Logger = container.Resolve<ILoggerFacade>();
            EventAggregator = container.Resolve<IEventAggregator>();
            SnackbarMessageQueue = container.Resolve<ISnackbarMessageQueue>();
        }

        public Dispatcher Dispatcher { get; set; } = Application.Current.Dispatcher;

        protected IUnityContainer Container { get; }

        protected IEventAggregator EventAggregator { get; }

        protected ILoggerFacade Logger { get; }
        public ISnackbarMessageQueue SnackbarMessageQueue { get; private set; }

        public IMvUser MvUser => _mvUser ??= Container.Resolve<IMvUser>();

        protected virtual void Invoke(Action action) => OnUIThread(action);

        private void OnUIThread(Action action)
        {
            try
            {
                Application.Current?.Dispatcher.BeginInvoke(action);
            }
            catch (Exception ex)
            {
                ;
        //        throw;
            }
        }
    }

    public abstract class ViewModelValidateBase : ValidateableBase
    {
        private IMvUser _mvUser;

        protected ViewModelValidateBase(IUnityContainer container)
        {
            Container = container;
            EventAggregator = container.Resolve<IEventAggregator>();
            Logger = container.Resolve<ILoggerFacade>();
            SnackbarMessageQueue = container.Resolve<ISnackbarMessageQueue>();
        }

        public Dispatcher Dispatcher { get; set; }

        protected IUnityContainer Container { get; }

        protected IEventAggregator EventAggregator { get; }

        protected ILoggerFacade Logger { get; }
        public ISnackbarMessageQueue SnackbarMessageQueue { get; private set; }

        public IMvUser MvUser => _mvUser ??= Container.Resolve<IMvUser>();

        protected virtual void Invoke(Action action) => Dispatcher.Invoke(action);
    }



    public abstract class ValidateableBase : BindableBase, INotifyDataErrorInfo
    {
        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            var isChanged = base.SetProperty(ref storage, value, propertyName);
            if (isChanged)
                this.ValidateProperty(value, propertyName);

            return isChanged;
        }

        protected void ValidateProperty(object value, [CallerMemberName]string propertyName = null)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var validationErrors = new List<ValidationResult>();
            if (!Validator.TryValidateProperty(value, context, validationErrors))
            {
                var errors = validationErrors.Select(error => error.ErrorMessage);
                SetErrors(propertyName, errors);
            }
            else
            {
                ClearErrors(propertyName);
            }
        }


        readonly Dictionary<string, List<string>> _currentErrors = new Dictionary<string, List<string>>();


        protected void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var hasCurrentError = _currentErrors.ContainsKey(propertyName);
            var hasNewError = errors != null && errors.Count() > 0;

            if (!hasCurrentError && !hasNewError)
                return;

            if (hasNewError)
            {
                _currentErrors[propertyName] = new List<string>(errors);
            }
            else
            {
                _currentErrors.Remove(propertyName);
            }
            OnErrorsChanged(propertyName);
        }


        protected void ClearErrors(string propertyName)
        {
            if (_currentErrors.ContainsKey(propertyName))
            {
                _currentErrors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }


        private void OnErrorsChanged(string propertyName)
        {
            var h = this.ErrorsChanged;
            if (h != null)
            {
                h(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }


        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !_currentErrors.ContainsKey(propertyName))
                return null;

            return _currentErrors[propertyName];
        }

        public bool HasErrors
        {
            get { return _currentErrors.Count > 0; }
        }

    }
}
