using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpCheckMembership : OpBranch
    {
        public SetStack _LHS;
        public membershipTest _membershipOp;
        public ScriptGroupStack _RHS;
        public igMetaObject _cachedType;
        public ScriptObjectList _cachedObj;
    }
}
