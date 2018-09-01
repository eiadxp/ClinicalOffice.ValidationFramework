using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq.Expressions;

namespace ClinicalOffice.ValidationFramework
{
    public static class Validator
    {
        #region Rules
        static readonly Dictionary<PropertyInfo, ValidationRules> PropertiesRules = new Dictionary<PropertyInfo, ValidationRules>();
        static readonly Dictionary<Type, List<Func<object, string>>> ObjectsRules = new Dictionary<Type, List<Func<object, string>>>();
        static ValidationRules TryGetRules(Type type, string propertyName)
        {
            foreach (var item in PropertiesRules)
            {
                if (item.Key.ReflectedType == type && item.Key.Name == propertyName) return item.Value;
            }
            return null;
        }
        static List<Func<object, string>> TryGetObjectRules(Type type)
        {
            foreach (var item in ObjectsRules)
            {
                if (item.Key == type) return item.Value;
            }
            return null;
        }

        public static ValidationRules AddRules<T>(string propertyName)
        {
            var type = typeof(T);
            ValidationRules rules = TryGetRules(type, propertyName);
            if (rules == null)
            {
                rules = new ValidationRules();
                PropertiesRules[type.GetProperty(propertyName)] = rules;
            }
            return rules;
        }
        public static ValidationRules SetRules<T>(string propertyName)
        {
            var rules = new ValidationRules();
            PropertiesRules[typeof(T).GetProperty(propertyName)] = rules;
            return rules;
        }

        public static void AddObjectRules<T>(Func<T, string> rule)
        {
            var type = typeof(T);
            var rules = TryGetObjectRules(type);
            if (rules == null)
            {
                rules = new List<Func<object, string>>();
                ObjectsRules[type] = rules;
            }
            rules.Add((object obj) => rule((T)obj));
        }
        public static void SetObjectRules<T>(Func<T, string> rule)
        {
            var rules = new List<Func<object, string>>();
            ObjectsRules[typeof(T)] = rules;
            rules.Add((object obj) => rule((T)obj));
        }
        #endregion
        #region Validate Rules
        public static ValidationError ValidateRules(object obj, string propertyName, object value)
        {
            string result = null;
            var rules = TryGetRules(obj.GetType(), propertyName);
            if (rules != null ) result = rules.Validate(value);
            return new ValidationError(propertyName, result);
        }
        public static ValidationError ValidateRules(object obj, string propertyName)
        {
            return ValidateRules(obj, propertyName, PropertyHelper.GetValue(obj, propertyName));
        }
        public static IEnumerable<ValidationError> ValidateRules(this object obj)
        {
            if (obj == null) return ValidationError.EmptyArray;
            var result = new List<ValidationError>();
            var ObjectsRules = TryGetObjectRules(obj.GetType());
            if (ObjectsRules != null)
            {
                foreach (var item in ObjectsRules)
                {
                    var s = item(obj);
                    if (!string.IsNullOrWhiteSpace(s)) result.Add(new ValidationError("", s));
                }
            }
            foreach (var item in PropertyHelper.GetProperties(obj.GetType()))
            {
                var error = ValidateRules(obj, item.Name);
                if (error != null) result.Add(error);
            }
            return result;
        }

        public static string ValidateRulesAsString(object obj, string propertyName, object value)
        {
            return ValidateRules(obj, propertyName, value)?.ErrorMessage;
        }
        public static string ValidateRulesAsString(object obj, string propertyName)
        {
            return ValidateRules(obj, propertyName)?.ErrorMessage;
        }
        public static string ValidateRulesAsString(object obj)
        {
            return string.Concat(ValidateRulesAsStrings(obj).Select(s => s + "\n")).Trim();
        }
        public static IEnumerable<string> ValidateRulesAsStrings(object obj)
        {
            return ValidateRules(obj).Select(e=>e.ErrorMessage).ToList();
        }
        #endregion
        #region Validate Attributes
        public static ValidationError ValidateAttributes(object obj, string propertyName, object value)
        {
            IEnumerable< ValidationAttribute> attributes = PropertyHelper.GetValidationAttributes(obj.GetType(), propertyName);
            if (attributes == null) return null;
            var sb = new StringBuilder();
            foreach (var item in attributes)
            {
                if (!item.IsValid(value)) sb.AppendLine(item.FormatErrorMessage(propertyName));
            }
            var result = sb.ToString();
            return new ValidationError(propertyName, result);
        }
        public static ValidationError ValidateAttributes(object obj, string propertyName)
        {
            return ValidateAttributes(obj, propertyName, PropertyHelper.GetValue(obj, propertyName));
        }
        public static IEnumerable<ValidationError> ValidateAttributes(this object obj)
        {
            if (obj == null) return ValidationError.EmptyArray;
            var result = new List<ValidationError>();
            foreach (var item in PropertyHelper.GetProperties(obj.GetType()))
            {
                var error = ValidateAttributes(obj, item.Name);
                if (error != null) result.Add(error);
            }
            return result;
        }

        public static string ValidateAttributesAsString(object obj, string propertyName, object value)
        {
            return ValidateAttributes(obj, propertyName, value)?.ErrorMessage;
        }
        public static string ValidateAttributesAsString(object obj, string propertyName)
        {
            return ValidateAttributes(obj, propertyName)?.ErrorMessage;
        }
        public static string ValidateAttributesAsString(object obj)
        {
            return string.Concat(ValidateAttributesAsStrings(obj).Select(s => s + "\n")).Trim();
        }
        public static IEnumerable<string> ValidateAttributesAsStrings(object obj)
        {
            return ValidateAttributes(obj).Select(e => e.ErrorMessage).ToList();
        }
        #endregion
        #region Validate All
        public static ValidationError Validate(this object obj, string propertyName, object value)
        {
            var error1 = ValidateRules(obj, propertyName, value);
            var hasError1 = (error1?.HasError).GetValueOrDefault();
            var error2 = ValidateAttributes(obj, propertyName, value);
            var hasError2 = (error2?.HasError).GetValueOrDefault();
            ValidationError error;
            if (!hasError1 && !hasError2) error = null;
            else if (!hasError2) error = error1;
            else if (!hasError1) error = error2;
            else error = new ValidationError(propertyName, error1.ErrorMessage + "\n" + error2.ErrorMessage);
            return error;
        }
        public static ValidationError Validate<T, TProperty>(this T obj, Expression<Func<T, TProperty>> property)
        {
            var p = property.Body as MemberExpression;
            var pi = p?.Member as PropertyInfo;
            if (pi == null) throw new InvalidOperationException("Should use a property expression.");
            return Validate(obj, pi.Name);
        }
        public static ValidationError Validate(this object obj, string propertyName)
        {
            return Validate(obj, propertyName, PropertyHelper.GetValue(obj, propertyName));
        }
        public static IEnumerable<ValidationError> Validate(this object obj)
        {
            if (obj == null) return ValidationError.EmptyArray;
            var result = new List<ValidationError>();
            foreach (var item in PropertyHelper.GetProperties(obj.GetType()))
            {
                var error = Validate(obj, item.Name);
                if (error != null) result.Add(error);
            }
            return result;
        }

        public static string ValidateAsString(this object obj, string propertyName, object value)
        {
            return Validate(obj, propertyName, value)?.ErrorMessage;
        }
        public static string ValidateAsString(this object obj, string propertyName)
        {
            return Validate(obj, propertyName)?.ErrorMessage;
        }
        public static string ValidateAsString(this object obj)
        {
            return string.Concat(ValidateAsStrings(obj).Select(s => s + "\n")).Trim();
        }
        public static IEnumerable<string> ValidateAsStrings(this object obj)
        {
            return Validate(obj).Select(e => e.ErrorMessage).ToList();
        }
        #endregion
    }
}
