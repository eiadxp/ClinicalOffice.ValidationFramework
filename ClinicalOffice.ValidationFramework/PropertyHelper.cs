using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace ClinicalOffice.ValidationFramework
{
    static class PropertyHelper
    {
        #region Properties Getters
        static readonly ConcurrentDictionary<PropertyInfo, Delegate> _CachedGetters = new ConcurrentDictionary<PropertyInfo, Delegate>();
        static Delegate CreateGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return null;
            var parameter = Expression.Parameter(propertyInfo.DeclaringType);
            var propertyExpression = Expression.Property(parameter, propertyInfo);
            var lambdaExpression = Expression.Lambda(propertyExpression, parameter);
            return lambdaExpression.Compile();
        }
        public static Delegate GetPrpertyGetter(Type type, string propertyName)
        {
            Delegate getter = null;
            foreach (var item in _CachedGetters)
            {
                if (item.Key.DeclaringType == type && item.Key.Name == propertyName)
                {
                    getter = item.Value;
                    break;
                }
            }
            if (getter == null)
            {
                var pi = type.GetProperty(propertyName);
                getter = CreateGetter(pi);
                if (getter != null) _CachedGetters[pi] = getter;
            }
            return getter;
        }
        public static object GetValue(object obj, string propertyName)
        {
            var getter = GetPrpertyGetter(obj.GetType(), propertyName);
            if (getter == null)
                throw new InvalidOperationException("Can not get property getter.");
            return getter.DynamicInvoke(obj);
        }
        #endregion
        #region ValidationAttributes
        static ConcurrentDictionary<PropertyInfo, IEnumerable<ValidationAttribute>> _cachedAttributes =
            new ConcurrentDictionary<PropertyInfo, IEnumerable<ValidationAttribute>>();
        public static IEnumerable<ValidationAttribute> GetValidationAttributes(PropertyInfo propertyInfo)
        {
            IEnumerable<ValidationAttribute> result = null;
            if (!_cachedAttributes.TryGetValue(propertyInfo, out result))
            {
                result = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                _cachedAttributes.TryAdd(propertyInfo, result);
            }
            return result;
        }
        public static IEnumerable<ValidationAttribute> GetValidationAttributes(Type DeclaringType, string propertyName)
        {
            foreach (var item in _cachedAttributes)
            {
                if (item.Key.DeclaringType == DeclaringType && item.Key.Name == propertyName)
                    return item.Value;
            }
            return GetValidationAttributes(DeclaringType.GetProperty(propertyName));
        }
        #endregion
        #region Types Properties
        static ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _cachedProperties =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            IEnumerable<PropertyInfo> result = null;
            if (!_cachedProperties.TryGetValue(type, out result))
            {
                result = type.GetProperties();
                _cachedProperties.TryAdd(type, result);
            }
            return result;
        }
        #endregion
    }
}
