using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpCheckFOV : OpBranch
    {
        public PositionStack _fromLHS;
        public ValueRHSVariant _RHSfacing;
        public ValueRHSVariant _RHSfov;
        public ScriptGroupStack _LHS;
        public RelOp _relOperator;
        public ValueRHSVariant _RHS;
        public obstructMode _mode;
        public ScriptSet _cachedObject;
    }
}
