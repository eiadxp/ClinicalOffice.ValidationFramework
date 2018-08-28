using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    class BaseObject
    {
        protected virtual bool Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertName = null)
        {
            if (Equals(oldValue, newValue)) return false;
            oldValue = newValue;
            return true;
        }

        string _Name;
        public string Name { get => _Name; set => Set(ref _Name, value); }

        int _Age;
        public int Age { get => _Age; set => Set(ref _Age, value); }

    }
}
