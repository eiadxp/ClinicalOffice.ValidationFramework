using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace ClinicalOffice.ValidationFramework
{
    public class ErrorsCollection : ICollection<ValidationError>
    {
        public ErrorsCollection(object entity)
        {
            Entity = entity;
            ValidatableEntity = entity as IValidatable;
        }

        readonly List<ValidationError> Errors = new List<ValidationError>();
        readonly IValidatable ValidatableEntity;
        readonly object Entity;

        public bool HasErrors => Errors.Any(e => e.HasError);

        #region Get and set errors
        public ValidationError SetError(string propertyName, string errorMessage)
        {
            var error = Errors.FirstOrDefault(e => e.PropertyName == propertyName);
            if (error == null)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    error = new ValidationError(propertyName, errorMessage);
                    Errors.Add(error);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage) && string.IsNullOrEmpty(error.ErrorMessage))
                {
                    Errors.Remove(error);
                    error = null;
                }
                else
                {
                    //if (error.ErrorMessage == errorMessage) return error;
                    error.ErrorMessage = errorMessage;
                }
            }
            if (string.IsNullOrWhiteSpace(error?.ErrorMessage)) Errors.Remove(error);
            ValidatableEntity?.NotifyErrorChanged(propertyName);
            if (string.IsNullOrWhiteSpace(error?.ErrorMessage)) return null;
            return error;
        }
        public ValidationError GetError(string propertyName)
        {
            var error = Errors.FirstOrDefault(e => e.PropertyName == propertyName);
            if (string.IsNullOrWhiteSpace(error?.ErrorMessage)) return null;
            return error;
        }
        public string GetErrorString(string propertyName)
        {
            var error = GetError(propertyName);
            if (string.IsNullOrWhiteSpace(error?.ErrorMessage)) return string.Empty;
            return error.ErrorMessage;
        }

        public IEnumerable<ValidationError> GetErrors(string propertyName)
        {
            var error = GetError(propertyName);
            if (error == null) return ValidationError.EmptyArray;
            return new[] { error };
        }
        public IEnumerable<string> GetErrorsAsStrings(string propertyName)
        {
            return GetErrors(propertyName).Select(e => e.ErrorMessage);
        }
        public IEnumerable<ValidationError> GetErrors()
        {
            if (HasErrors) return Errors.ToArray();
            return ValidationError.EmptyArray;
        }
        public void ClearErrors()
        {
            var lst = Errors.Select(e => e.PropertyName).ToList();
            Errors.Clear();
            foreach (var item in lst)
            {
                ValidatableEntity?.NotifyErrorChanged(item);
            }
        }
        #endregion
        #region Validate
        public ValidationError Validate(string propertyName, object value)
        {
            var error = Entity.Validate(propertyName, value);
            return SetError(propertyName, error?.ErrorMessage);
        }
        public ValidationError Validate(string propertyName)
        {
            var error = Entity.Validate(propertyName);
            return SetError(propertyName, error?.ErrorMessage);
        }
        public IEnumerable<ValidationError> Validate()
        {
            var errors = Entity.Validate();
            var properties = errors.Select(e => e.PropertyName).Concat(Errors.Select(e => e.PropertyName)).Distinct().ToList();
            Errors.Clear();
            Errors.AddRange(errors);
            if (ValidatableEntity != null)
                foreach (var item in properties)
                {
                    ValidatableEntity.NotifyErrorChanged(item);
                }
            return Errors.ToList();
        }
        public string ValidateAsString(string propertyName, object value)
        {
            return Validate(propertyName, value).ErrorMessage;
        }
        public string ValidateAsString(string propertyName)
        {
            return Validate(propertyName).ErrorMessage;
        }
        public IEnumerable<string> ValidateAsString()
        {
            return Validate().Select(e => e.ErrorMessage).ToArray();
        }
        #endregion
        #region ICollection<Error>
        void ICollection<ValidationError>.Add(ValidationError item)
        {
            Errors.Add(item);
            ValidatableEntity?.NotifyErrorChanged(item.PropertyName);
        }
        void ICollection<ValidationError>.Clear()
        {
            var lst = Errors.Select(e => e.PropertyName).ToList();
            Errors.Clear();
            foreach (var item in lst)
            {
                ValidatableEntity?.NotifyErrorChanged(item);
            }
        }
        bool ICollection<ValidationError>.Remove(ValidationError item)
        {
            var b = Errors.Remove(item);
            ValidatableEntity?.NotifyErrorChanged(item.PropertyName);
            return b;
        }

        int ICollection<ValidationError>.Count => Errors.Count;
        bool ICollection<ValidationError>.IsReadOnly => false;
        bool ICollection<ValidationError>.Contains(ValidationError item) => Errors.Contains(item);
        void ICollection<ValidationError>.CopyTo(ValidationError[] array, int arrayIndex) => Errors.CopyTo(array, arrayIndex);
        IEnumerator<ValidationError> IEnumerable<ValidationError>.GetEnumerator() => Errors.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Errors.GetEnumerator();
        #endregion
    }
}
