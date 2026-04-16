using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpStartSequence : OpBranch
    {
        public ScriptGroupStack _LHS;
        public ValueRHSVariant _indexRHS;
        public igObject? _cachedObject; // ClonedSequence
    }
}
