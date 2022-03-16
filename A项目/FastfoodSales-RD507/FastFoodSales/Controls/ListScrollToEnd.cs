using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace DAQ.Controls
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
}
