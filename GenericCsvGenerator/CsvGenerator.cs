using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using GenericCsvGenerator.Rules;
using GenericCsvGenerator.Writers;

namespace GenericCsvGenerator
{
    /// <summary>
    ///     Generates a CSV string by calling ToString on all the accessible properties of the class.
    ///     Rules can be added that will
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CsvGenerator<T> where T : class
    {
        private readonly List<PropertyInfo> _propertyList;
        private readonly HashSet<Type> _types;
        private bool _dataIsLoaded;
        private IList<T> _dataList;
        //We maintain both a list and a dictionary of properties.  The property Dictionary
        //provides fast lookup of existing properties while the List maintains
        //the order in which the properites are inserted.
        //This causes the final CSV to output the properties in the order in which they appear
        //in the class that is being exported.
        private Dictionary<string, PropertyInfo> _properties;

        public CsvGenerator()
        {
            TypeRules = new Dictionary<Type, TypeRule>();
            PropertyRules = new Dictionary<string, PropertyRule>();
            _dataIsLoaded = false;
            _types = new HashSet<Type>();
            _propertyList = new List<PropertyInfo>();
        }

        public Dictionary<Type, TypeRule> TypeRules { get; private set; }
        public Dictionary<string, PropertyRule> PropertyRules { get; private set; }

        public void LoadData(IEnumerable<T> dataEnumerable)
        {
            if (dataEnumerable == null)
            {
                throw new ArgumentNullException();
            }
            _dataList = dataEnumerable as IList<T>;
            if (_dataList.Count < 0)
            {
                throw new ArgumentException("The enumerable cannot be empty.");
            }

            var tempData = _dataList.FirstOrDefault();
            var properties = tempData.GetType().GetProperties();

            if (properties.Count() < 1)
            {
                throw new ArgumentException("The generic object must have at least one accessible property");
            }

            //We set a dictionary of the PropertyInfo so we use less reflection when it is time to generate the CSV.
            _properties = new Dictionary<string, PropertyInfo>();
            foreach (var property in properties)
            {
                

                if (!_properties.ContainsKey(property.Name))
                {
                    _properties.Add(property.Name, property);
                }
                else
                {
                    _properties[property.Name] = property;
                }

                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
                {
                    Type type = property.PropertyType.GetGenericArguments()[0];
                    _types.Add(type);
                }
                else
                {
                    _types.Add(property.PropertyType);
                }
                _propertyList.Add(property);
            }
            _dataIsLoaded = true;
        }

        /// <summary>
        ///     Adds a new type rule.  Will overwrite any previous type rule of the same type.
        /// </summary>
        /// <param name="rule">The type the rule will modify.</param>
        public void AddRule(TypeRule rule)
        {
            

            if (!_types.Contains(rule.Type))
            {
                throw new ArgumentException(
                    "Type rule cannot be added for a type that is not property in the generic type.");
            }
            if (TypeRules.ContainsKey(rule.Type))
            {
                TypeRules[rule.Type] = rule;
            }
            else
            {
                TypeRules.Add(rule.Type, rule);
            }
        }

        /// <summary>
        ///     Adds a new property rule.  Will overwrite any previous rule of the same property.
        /// </summary>
        /// <param name="rule"></param>
        public void AddRule(PropertyRule rule)
        {
            //If the property exists AND the rule has a string formatter in it
            //we need to verify that the Type can use a string formatter.
            PropertyInfo propInfo;
            if (!_properties.TryGetValue(rule.PropertyName, out propInfo))
            {
                throw new ArgumentException("Cannot add property rule for a property that does not exist.");
            }

            var type = propInfo.PropertyType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            //Checks to see if the ToString method of the provided type does not take a string formatter
            //Throws an exception if a string formatter was provided.
            if (type.GetMethod("ToString", new[] {typeof (string)}) == null && rule.StringFormatter != null)
            {
                throw new ArgumentException(string.Format("PropertyRule for property {0} of type {1} contains " +
                                                          "a string formatter, however type {1} cannot use a string formatter.",
                    rule.PropertyName, type.Name));
            }

            if (PropertyRules.ContainsKey(rule.PropertyName))
            {
                PropertyRules[rule.PropertyName] = rule;
            }
            else
            {
                PropertyRules.Add(rule.PropertyName, rule);
            }
        }

        public string GenerateCsv()
        {
            if (!_dataIsLoaded)
            {
                throw new InvalidOperationException(
                    "Cannot generate CSV when data has not been loaded.  Please call LoadData() first and try again.");
            }
            var writer = new Writer<T>();

            var output = writer.WriteRows(_dataList, PropertyRules, TypeRules, _propertyList);
            return output;
        }
    }
}


//foreach (T data in dataList)
//{
//    var properties = data.GetType().GetProperties();
//    //Set first row of CSV with name of properties

//    if (firstPass)
//    {
//        foreach (var p in properties)
//        {
//            sb.Append(p.Name);
//            sb.Append(",");
//        }

//        firstPass = false;
//        sb.AppendLine();
//    }
//    foreach (var p in properties)
//    {
//        //If the value of any of the dataList is null, print nothing and continue
//        if (p.GetValue(data, null) == null)
//        {
//            sb.Append("");
//            sb.Append(",");
//            continue;
//        }

//        Type propType = p.PropertyType;
//        //This will grab the base type of any of our nullable value types (int?, decimal?, bool?, etc)
//        //The base type of the Nullable will replace the current type in the propType variable
//        //So if we have an int? in the as the property type, we'll get a plain int instead.
//        //THis won't be a problem since we already checked for properties will null values above.

//        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
//        {

//            propType = propType.GetGenericArguments()[0];
//        }

//        //Process the possible results.

//        if (propType == typeof(string))
//        {
//            string value = (string)p.GetValue(data, null);
//            //Any strings with a comma will be surrounded with a quotes so the comma does not cause delimination
//            sb.Append(value == null ? "" : "\"" + value.ToString() + "\"");
//            sb.Append(",");

//        }
//        else if (propType == typeof(DateTime))
//        {
//            DateTime value = (DateTime)p.GetValue(data, null);
//            sb.Append(value.ToString("d"));
//            sb.Append(",");
//        }

//        else if (propType == typeof(int))
//        {
//            int value = (int)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }

//        else if (propType == typeof(decimal))
//        {
//            decimal value = (decimal)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }

//        else if (propType == typeof(double))
//        {
//            decimal value = (decimal)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }

//        else if (propType == typeof(long))
//        {
//            long value = (long)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }

//        else if (propType == typeof(short))
//        {
//            short value = (short)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }

//        else if (propType == typeof(bool))
//        {
//            bool value = (bool)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }

//        else if (propType == typeof(Guid))
//        {
//            Guid value = (Guid)p.GetValue(data, null);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }
//        //Add new explicit type checks here
//        else
//        {
//            //Attempt to call .ToString() on an unknown type.
//            dynamic value = p.GetValue(data, null);
//            value = Convert.ChangeType(value, propType);
//            sb.Append(value.ToString());
//            sb.Append(",");
//        }
//    }
//    sb.AppendLine();
//}