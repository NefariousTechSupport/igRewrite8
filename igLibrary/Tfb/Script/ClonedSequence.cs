using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class ClonedSequence : Sequence
    {
        public ScriptGroupStack _clones;
        public ClonedSequence _parent;
        public static object? _interface; // InterfaceResolver
    }
}
