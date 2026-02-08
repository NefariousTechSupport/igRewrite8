using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpAbstractControl : OpBranch
    {
        public ScriptObjectList? _LHS;
        public tfbScriptObject? _cachedObject;
        public AbstractPlacement _lastReceiver;
        
    }
}
