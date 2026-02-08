using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpCheckReference : OpBranch
    {
        public ReferenceStack _LHS;
        public enum BoolOp
        {
            beq = 1,
            bne = 5,
        }
        public BoolOp _relOperator;
        public RHSReferenceStack _RHS;
        public ValueRHSVariant _indexRHS;
        public static igMetaObject? _cachedType;
        public static tfbScriptObject? _cachedObj;
        public static tfbScriptObject? _cachedOwner;

    }
}
