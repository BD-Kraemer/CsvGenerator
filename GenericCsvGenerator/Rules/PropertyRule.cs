using System;

namespace GenericCsvGenerator.Rules
{
    public class PropertyRule
    {
        public PropertyRule(string propertyName, string columnHeaderName, string stringFormatter, IFormatProvider formatProvider,
             bool displayColumn)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("Type cannot be null");
            }

            PropertyName = propertyName;
            FormatProvider = formatProvider;
            StringFormatter = stringFormatter;
            ColumnHeaderName = columnHeaderName;
            DisplayColumn = displayColumn;
        }

        public PropertyRule(string propertyName, string columnHeaderName, IFormatProvider formatProvider,
            string stringFormatter)
            : this(propertyName, columnHeaderName, stringFormatter, formatProvider, true)
        {
        }

        public PropertyRule(string propertyName, string columnHeaderName)
            : this(propertyName, columnHeaderName, null, null, true)
        {
        }

        public PropertyRule(string propertyName, bool displayColumn)
            : this(propertyName, null, null, null, displayColumn)
        {
        }

        public string PropertyName { get; private set; }
        public string ColumnHeaderName { get; private set; }
        public IFormatProvider FormatProvider { get; private set; }
        public string StringFormatter { get; private set; }
        public bool DisplayColumn { get; private set; }
    }
}