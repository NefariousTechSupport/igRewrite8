using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpDisplace : OpCode
    {

        public VectorStack _NP;
        public combineMode _combineMode;
        public ValueRHSVariant _lenNP;
        public ValueRHSVariant _headNP;
        public ValueRHSVariant _pitchNP;
    }
}
