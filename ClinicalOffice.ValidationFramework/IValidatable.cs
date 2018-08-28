using System.Collections.Generic;
using System.ComponentModel;

namespace ClinicalOffice.ValidationFramework
{
    public interface IValidatable : INotifyDataErrorInfo
    {
        void NotifyErrorChanged(string propertyName);
    }
}
