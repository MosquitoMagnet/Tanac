using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mv.Controls.Controls
{
    class WpfTreeHelper
    {
        public static T FindChild<T>(DependencyObject node)
            where T : DependencyObject
        {
            if (node == null)
                return null;

            T found = null;
            var childlen = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < childlen; i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);
                var target = child as T;
                if (target == null)
                {
                    found = FindChild<T>(child);
                    if (found != null)
                        break;
                }
                else
                {
                    found = (T)child;
                    break;
                }
            }

            return found;
        }
    }
    public class ListScrollToEnd
    {
        #region IsScrollToEnd

        public static readonly DependencyProperty IsScrollToEndProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ListScrollToEnd),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsEnabledChanged)));

        public static bool GetIsScrollToEnd(ItemsControl d)
        {
            return (bool)d.GetValue(IsScrollToEndProperty);
        }

        public static void SetIsScrollToEnd(ItemsControl d, bool value)
        {
            d.SetValue(IsScrollToEndProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldIsEnabled = (bool)e.OldValue;
            bool newIsEnabled = (bool)d.GetValue(IsScrollToEndProperty);

            var itemsControl = d as ItemsControl;
            if (itemsControl == null)
                return;

            if (newIsEnabled)
            {
                itemsControl.Loaded += (ss, ee) =>
                {
                    ScrollViewer scrollviewer = WpfTreeHelper.FindChild<ScrollViewer>(itemsControl);
                    if (scrollviewer != null)
                    {
                        ((ICollectionView)itemsControl.Items).CollectionChanged += (sss, eee) =>
                        {
                            scrollviewer.ScrollToEnd();
                        };
                    }
                };
            }
        }

        #endregion


    }
    public partial class Notification : Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(ImageSource), typeof(Notification), new PropertyMetadata(default(ImageSource)));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(Notification), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(Notification), new PropertyMetadata(default(string)));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }


        private ICommand _closeCommand;

        public ICommand CloseCommand
        {
            get => _closeCommand;
            set => SetProperty(ref _closeCommand, value);
        }

        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Notification), new FrameworkPropertyMetadata(typeof(Notification)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }
    }
}
