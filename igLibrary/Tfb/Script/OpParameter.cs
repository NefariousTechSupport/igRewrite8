using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpParameter : OpAbstractCreateVariable
    {
        public Combiner _combineOp;
        public igObject _RHS;
        public ValueRHSVariant _indexRHS;
    }
}
