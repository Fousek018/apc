﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LABPOWER_APC.Utilities
{
    public class EnumHelper<T>
    {
        //Constructor with no parameters
        public EnumHelper()
        {
            // Nastavení výchozích hodnot
            Value = default(T);
            Description = string.Empty;
        }
        public EnumHelper(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public T Value { get; }
        public string Description { get; }
    }
}
