using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpAbstractCreateVariable : OpCode
    {
        public igMetaObject _varContainerType;
        public igObject _varContentsType;
        public string _varName;
        public int _varIndex;
        public static ScriptVariantList? _localVarList; //  public static ScriptVariantList? _localVarList;
    }
}
