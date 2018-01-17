using System;

namespace Pixelbyte.JsonUnity
{
    ///If present on a float, double, or decimal field then it restricts the max decimal places to the number given
    public class DecimalPlaces : Attribute
    {
        string formatter;
        public DecimalPlaces(int maxDecimalPlaces) { formatter = string.Format("N{0}", maxDecimalPlaces); }
        public string Convert(float value) { return value.ToString(formatter); }
        public string Convert(double value) { return value.ToString(formatter); }
        public string Convert(decimal value) { return value.ToString(formatter); }
    }
}
