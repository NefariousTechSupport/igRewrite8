using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpFindVariable : OpBranch
    {

        public igMetaObject _varContainerType;
        public igObject _varContentsType;
        public string _varName;
        public findFilter _filter;
        public ValueRHSVariant _skipRHS;
        public ScriptGroupStack _owner;
        public ValueRHSVariant _indexRHS;
        public AbstractScriptVariant _cachedObject;
    }
}
