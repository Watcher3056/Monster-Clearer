using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public class FieldInfoContainer
    {
        public FieldInfo fieldInfo;
        public object owner;

        public object Value => fieldInfo.GetValue(owner);
    }
}
