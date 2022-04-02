using Mv.Ui.TransferService.WpfInteractions;

namespace Mv.Ui.Converters
{
    public class NumberToDisplayDataSizeConverter : ValueConverterBase<double, DisplayDataSize>
    {
        protected override DisplayDataSize ConvertNonNullValue(double value) => value;
    }
}
