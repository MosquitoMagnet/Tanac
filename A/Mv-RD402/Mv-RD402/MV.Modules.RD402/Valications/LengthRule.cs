using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace Mv.Modules.RD402.Valications
{
    public class LengthRule : ValidationRule

    {
        public int Min { get; set; } = 1;
        public int Max { get; set; } = int.MaxValue;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "value should not be null");
            if (string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, "value should not be empty");
            if (value.ToString().Length<Min)
                return new ValidationResult(false, $"vale length should > {Min}");
            if (value.ToString().Length > Max)
                return new ValidationResult(false, $"vale length should < {Max}");
            return new ValidationResult(true, null);
        }
    }
}
