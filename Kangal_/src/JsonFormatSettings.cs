using System;

namespace Kangal
{
    /// <summary>
    /// Json Format
    /// </summary>
    public enum JsonFormat
    {
        Simple = 1,
        Showy =2
    }
    /// <summary>
    /// Json Data Types Format Settings
    /// </summary>
    public class JsonFormatSettings
    {
        public string DateTimeFormat { get; set; }
        public string DoubleFormat { get; set; }
        public string DecimalFormat { get; set; }

        public JsonFormatSettings() { }
        public JsonFormatSettings(string dateTimeFormat)
        {
            if (string.IsNullOrEmpty(dateTimeFormat)) throw new ArgumentNullException(nameof(dateTimeFormat));
            this.DateTimeFormat = dateTimeFormat;
        }
        public JsonFormatSettings(string dateTimeFormat,string doubleFormat)
        {
            if (string.IsNullOrEmpty(dateTimeFormat)) throw new ArgumentNullException(nameof(dateTimeFormat));
            if (string.IsNullOrEmpty(doubleFormat)) throw new ArgumentNullException(nameof(doubleFormat));

            this.DateTimeFormat = dateTimeFormat;
            this.DoubleFormat = doubleFormat;
        }
        public JsonFormatSettings(string dateTimeFormat, string doubleFormat,string decimalFormat)
        {
            if (string.IsNullOrEmpty(dateTimeFormat)) throw new ArgumentNullException(nameof(dateTimeFormat));
            if (string.IsNullOrEmpty(doubleFormat)) throw new ArgumentNullException(nameof(doubleFormat));
            if (string.IsNullOrEmpty(decimalFormat)) throw new ArgumentNullException(nameof(decimalFormat));

            this.DateTimeFormat = dateTimeFormat;
            this.DoubleFormat = doubleFormat;
            this.DecimalFormat = decimalFormat;
        }
    }
}