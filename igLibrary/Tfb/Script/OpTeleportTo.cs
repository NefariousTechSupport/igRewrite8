using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpTeleportTo : OpCode
    {
        public ScriptGroupStack _LHS;
        public SetDirection _dir;
        public ValueRHSVariant _facingRHS;
    }
}
