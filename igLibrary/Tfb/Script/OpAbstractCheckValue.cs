using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpAbstractCheckValue : OpBranch
    {
        public ValueStack _LHS;
        public RelOp _relOperator;
        public ValueRHSVariant _RHS;
    }
}
