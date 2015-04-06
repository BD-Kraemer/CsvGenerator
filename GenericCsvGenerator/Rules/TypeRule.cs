using System;

namespace GenericCsvGenerator.Rules
{
    public class TypeRule
    {
        public TypeRule(Type type, string stringFormatter, IFormatProvider formatProvider, bool displayColumn)
        {

            Type addedType = type;

            if (addedType == null)
            {
                throw new ArgumentNullException("Type cannot be null");
            }

            if (addedType.IsGenericType && addedType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                addedType = addedType.GetGenericArguments()[0];
            }

            //Insure that the passed Type can use a string formatter.
            if (addedType.GetMethod("ToString", new Type[1] { typeof(string) }) == null && stringFormatter != null)
            {
                throw new ArgumentException(
                    string.Format(
                        "The type {0} cannot use a string as a string formatter.  Please call the constructor without a string formatter.",
                        addedType.Name));
            }

            Type = addedType;
            FormatProvider = formatProvider;
            StringFormatter = stringFormatter;
            DisplayColumn = displayColumn;
        }

        public TypeRule(Type type, IFormatProvider formatProvider)
            : this(type, null, formatProvider, true)
        {
            if (formatProvider == null)
            {
                throw new ArgumentNullException("IFormatProvider cannot be null.");
            }
        }

        public TypeRule(Type type, String stringFormatter)
            : this(type, stringFormatter, null, true)
        {
            if (stringFormatter == null)
            {
                throw new ArgumentNullException("StringFormatter cannot be null.");
            }
        }

        public TypeRule(Type type, bool suppressColumn) : this(type, null, null, suppressColumn)
        {
        }

        public Type Type { get; private set; }
        public IFormatProvider FormatProvider { get; private set; }
        public string StringFormatter { get; private set; }
        public bool DisplayColumn { get; private set; }
    }
}