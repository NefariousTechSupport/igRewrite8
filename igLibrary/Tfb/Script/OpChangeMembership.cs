using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpChangeMembership : OpCode
    {
        public SetStack _LHS;
        public Combiner _combineOp;
        public ScriptGroupStack _RHS;
    }
}
