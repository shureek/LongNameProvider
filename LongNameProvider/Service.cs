using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LongNameProvider
{
    class Service
    {
        public static void SetPrivateProperty(object obj, string name, object value)
        {
            var propInfo = obj.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
            propInfo.SetValue(obj, value);
        }
    }
}
