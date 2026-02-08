using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpSetReference : OpCode
    {
        public ReferenceStack _LHS;
        public RHSReferenceStack _RHS;
        public ValueRHSVariant _indexRHS;
        public static tfbScriptObject? _LHSowner;
    }
}
