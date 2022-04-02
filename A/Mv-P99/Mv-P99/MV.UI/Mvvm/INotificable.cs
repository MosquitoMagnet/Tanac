using MaterialDesignThemes.Wpf;

namespace Mv.Ui.Mvvm
{
    public interface INotificable
    {
        ISnackbarMessageQueue GlobalMessageQueue { get; set; }
    }
}
