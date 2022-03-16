namespace Mv.Ui.Converters
{
    public class NotOperator : ValueConverterBase<bool, bool>
    {
        protected override bool Convert(bool value) => !value;

        protected override bool ConvertBack(bool value) => Convert(value);
    }

}
