using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DAQ.Core.Localization
{
    public class LangResource : MarkupExtension, INotifyPropertyChanged, IWeakEventListener
    {
        private string _Value;

        private static readonly PropertyChangedEventArgs valueChangedEventArgs = new PropertyChangedEventArgs("Value");

        public string Key
        {
            get;
            set;
        }

        public string Value
        {
            get
            {
                if (Key != null)
                {
                    string result = null;
                    try
                    {
                        result = AppLocalizationService.GetLang(Key);
                        return result;
                    }
                    catch
                    {
                        return result;
                    }
                }

                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LangResource(string key)
            : this()
        {
            Key = key;
            LangEventManager.AddListener(this);
        }

        public LangResource()
        {
        }

        ~LangResource()
        {
            LangEventManager.RemoveListener(this);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if ((serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget).TargetObject is Setter)
            {
                return new Binding("Value")
                {
                    Source = this,
                    Mode = BindingMode.OneWay
                };
            }

            return new Binding("Value")
            {
                Source = this,
                Mode = BindingMode.OneWay
            }.ProvideValue(serviceProvider);
        }

        protected void NotifyValueChanged()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, valueChangedEventArgs);
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            NotifyValueChanged();
            return true;
        }
    }
}
