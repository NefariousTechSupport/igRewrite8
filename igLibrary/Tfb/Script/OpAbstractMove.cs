using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpAbstractMove : OpBranch
    {
        public ScriptGroupStack _LHS;
        public SetDirection _dir;
        public ValueRHSVariant _RHS;
        public ScriptGroupStack _NP;
        public ValueRHSVariant _indexRHS;
    }
}
