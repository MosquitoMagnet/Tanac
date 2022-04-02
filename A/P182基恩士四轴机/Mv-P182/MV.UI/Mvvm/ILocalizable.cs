using Mv.Ui.Core.I18n;

namespace Mv.Ui.Mvvm
{
    public interface ILocalizable
    {
        I18nManager I18nManager { get; set; }

        void OnCurrentUICultureChanged();
    }
}
