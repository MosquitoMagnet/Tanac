using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mv.Controls.Controls
{

    public class SimpleLed:Control
    {

        public bool State { get => (bool)GetValue(StateProperty); set => SetValue(StateProperty, value); }
        public SolidColorBrush OffColor { get=>(SolidColorBrush)GetValue(OffColorProperty); set=>SetValue(OffColorProperty,value); }
        public SolidColorBrush OnColor { get=>(SolidColorBrush)GetValue(OnColorProperty); set=>SetValue(OnColorProperty,value); }
        
        public static readonly DependencyProperty OnColorProperty = DependencyProperty.Register(
            "OnColor", typeof(SolidColorBrush), typeof(SimpleLed),new PropertyMetadata(Brushes.LimeGreen));
        public static readonly DependencyProperty OffColorProperty = DependencyProperty.Register(
            "OffColor", typeof(SolidColorBrush), typeof(SimpleLed),new  PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State", typeof(bool), typeof(SimpleLed), new PropertyMetadata(default(bool)));

        static SimpleLed()
        {
           
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleLed), new FrameworkPropertyMetadata(typeof(SimpleLed)));
        }

    }
}