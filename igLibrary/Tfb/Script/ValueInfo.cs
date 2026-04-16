using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class ValueInfo : AbstractNumberMeasurement
    {
        public static igMetaObject? _interface; // InterfaceResolver
        public igMetaObject? _type; // exnm ref
        public int _value;
        public int _resetValue;
    }
}
