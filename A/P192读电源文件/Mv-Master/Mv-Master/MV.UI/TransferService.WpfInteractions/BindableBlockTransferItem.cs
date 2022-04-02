// ReSharper disable once CheckNamespace
namespace Mv.Ui.TransferService.WpfInteractions
{
    public class BindableBlockTransferItem
    {
        public long Offset { get; }

        public BindableProgress Progress { get; }

        public BindableBlockTransferItem(long offset)
        {
            Offset = offset;
            Progress = new BindableProgress();
        }
    }
}
