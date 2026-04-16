using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class ScriptStructure : AbstractScriptVariant
    {
        public static object? _interface; //InterfaceResolver
        public ScriptVariantList? _fieldList;
        public OpDefineStructure? _varContentsType;
        public static object? _unTyped; //OpDefineStructure

    }
}
