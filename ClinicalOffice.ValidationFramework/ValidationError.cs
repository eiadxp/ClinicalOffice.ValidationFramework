using System;

namespace ClinicalOffice.ValidationFramework
{
    public class ValidationError
    {
        internal static ValidationError[] EmptyArray = Array.Empty<ValidationError>();
        private string _errorMessage;

        public string PropertyName { get; private set; }
        public string ErrorMessage { get => _errorMessage; internal set => _errorMessage = value?.Trim(); }
        public bool HasError { get => !string.IsNullOrWhiteSpace(ErrorMessage); }

        public ValidationError(string propertyName, string errorMessage = null)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(ErrorMessage) ? string.Empty : PropertyName + ": " + ErrorMessage;
        }
    }
}
