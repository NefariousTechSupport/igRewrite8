using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpBranch : OpCode
    {
        public static igVectorMetaField _contextStack; // branchContextMetaField
        public OpCode? branchTarget; // for the tfbScriptEditor
    }
}
