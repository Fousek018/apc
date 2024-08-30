using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LABPOWER_APC.Utilities
{
    public class EnumHelper<T>
    {
        public EnumHelper(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public T Value { get; }
        public string Description { get; }
    }
}
