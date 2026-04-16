using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace igLibrary.Tfb.Script
{
    public class OpForEach : OpLoop
    {
        public SetStack _LHS;
        public SetDirection _dir;
        public ValueRHSVariant _RHS;
        public ScriptObjectList? _cachedSet;
        public tfbScriptObject? _cachedObject;
    }
}
