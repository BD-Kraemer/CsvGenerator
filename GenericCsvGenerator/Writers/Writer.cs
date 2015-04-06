using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GenericCsvGenerator.Rules;

namespace GenericCsvGenerator.Writers
{
    internal class Writer<T> where T : class
    {
        private List<PropertyInfo> _cleanPropertyList;
        private IList<T> _dataList;
        private bool _firstPass;
        private List<PropertyInfo> _properties;
        private Dictionary<string, PropertyRule> _propertyRules;
        private Dictionary<Type, TypeRule> _typeRules;
        private StringBuilder sb;

        public string WriteRows(IEnumerable<T> dataList, Dictionary<string, PropertyRule> propertyRules,
            Dictionary<Type, TypeRule> typeRules, List<PropertyInfo> properties)
        {

           
            var enumerable = dataList as IList<T> ?? new List<T>();

            _dataList = enumerable;
            _propertyRules = propertyRules;
            _typeRules = typeRules;
            _properties = properties;
            _cleanPropertyList = new List<PropertyInfo>();
            sb = new StringBuilder();
            _firstPass = true;


            //Clear out suppressed columns
            foreach (var property in _properties)
            {
                var type = property.PropertyType;
                var name = property.Name;
                TypeRule typeRule;
                PropertyRule propRule;

                if (_typeRules.TryGetValue(type, out typeRule) && !typeRule.DisplayColumn)
                {
                    continue;
                }
                if (_propertyRules.TryGetValue(name, out propRule) && !propRule.DisplayColumn)
                {
                    continue;
                }
                _cleanPropertyList.Add(property);
            }

            //First we check for property rules which take a higher precedence over type rules.
            //If a property rule exists, we don't check for a rule for the associated type.
            foreach (var data in _dataList)
            {
                //Surround in quotation marks to escape commas.
                
                if (_firstPass)
                {
                    
                    //Create column headers on first pass
                    foreach (var property in _cleanPropertyList)
                    {
                        sb.Append("\"");
                        PropertyRule rule;
                        if (_propertyRules.TryGetValue(property.Name, out rule) && rule.ColumnHeaderName != null)
                        {
                            sb.Append(rule.ColumnHeaderName.Replace("\"", "\"\"") + "\",");
                        }
                        else
                        {
                            sb.Append(property.Name + "\",");
                        }
                       
                    }
                    
                    sb.AppendLine();
                    _firstPass = false;
                }

                foreach (var property in _cleanPropertyList)
                {
                    sb.Append("\"");
                    var propertyType = property.PropertyType;
                    dynamic value = property.GetValue(data, null);
                    if (value == null)
                    {
                        sb.Append("\"");
                        sb.Append(",");
                        
                        continue;
                    }

                    //Get non nullable value of nullable types. Set type to the non-nullable version
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                    }

                    value = Convert.ChangeType(value, propertyType);

                    if (_propertyRules.ContainsKey(property.Name))
                    {
                        var rule = _propertyRules[property.Name];

                        if (rule.FormatProvider != null && rule.StringFormatter != null)
                        {
                            sb.Append(value.ToString(rule.StringFormatter, rule.FormatProvider).Replace("\"", "\"\""));
                        }
                        else if (rule.FormatProvider != null)
                        {
                            sb.Append(value.ToString(rule.FormatProvider).Replace("\"", "\"\""));
                        }
                        else if (rule.StringFormatter != null)
                        {
                            sb.Append(value.ToString(rule.StringFormatter).Replace("\"", "\"\""));
                        }
                        else
                        {
                            sb.Append(value.ToString().Replace("\"", "\"\""));
                        }
                    }
                    else if (_typeRules.ContainsKey(propertyType))
                    {
                        var rule = _typeRules[propertyType];

                        if (rule.FormatProvider != null && rule.StringFormatter != null)
                        {
                            sb.Append(value.ToString(rule.StringFormatter, rule.FormatProvider).Replace("\"", "\"\""));
                        }
                        else if (rule.FormatProvider != null)
                        {
                            sb.Append(value.ToString(rule.FormatProvider).Replace("\"", "\"\""));
                        }
                        else if (rule.StringFormatter != null)
                        {
                            sb.Append(value.ToString(rule.StringFormatter).Replace("\"", "\"\""));
                        }
                        else
                        {
                            sb.Append(value.ToString().Replace("\"", "\"\""));
                        }
                    }
                    else
                    {
                        sb.Append(value.ToString().Replace("\"", "\"\""));
                    }
                    sb.Append("\"");
                    sb.Append(",");
                    
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}