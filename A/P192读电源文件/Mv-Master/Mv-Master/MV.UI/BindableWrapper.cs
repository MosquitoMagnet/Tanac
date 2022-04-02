using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Mv.Ui.Core
{
    public sealed class BindableWrapper<T> : INotifyPropertyChanged
    {
        private T value;
        public T Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, this.value))
                {
                    this.value = value;
                    OnPropertyChanged();
                }
            }
        }

        public static implicit operator BindableWrapper<T>(T value)
        {
            return new BindableWrapper<T> { value = value };
        }
        public static implicit operator T(BindableWrapper<T> wrapper)
        {
            return wrapper.value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}