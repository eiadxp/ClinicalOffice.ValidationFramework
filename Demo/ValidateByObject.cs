using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicalOffice.ValidationFramework;
using System.Runtime.CompilerServices;

namespace Demo
{
    class ValidateByObject : BaseObject , IValidatable
    {
        #region IValidatable
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => Errors.HasErrors;
        public IEnumerable GetErrors(string propertyName) => Errors.GetErrors(propertyName);

        public void NotifyErrorChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        #endregion
        public ErrorsCollection Errors { get; set; }

        public ValidateByObject()
        {
            Errors = new ErrorsCollection(this);
            Validator.AddRules<ValidateByObject>(nameof(Name)).StringLength(7, 3);
            Validator.AddRules<ValidateByObject>(nameof(Age)).Range(18, 200);
            this.Validate();
        }

        protected override bool Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertName = null)
        {
            var b = base.Set(ref oldValue, newValue, propertName);
            if (b) Errors.Validate();
            return b;
        }
    }
}
