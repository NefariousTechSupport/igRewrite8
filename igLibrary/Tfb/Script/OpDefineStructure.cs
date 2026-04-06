using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpDefineStructure : OpAbstractDefinition
    {
        public Dictionary<string, OpAbstractCreateVariable> structVars = new Dictionary<string, OpAbstractCreateVariable>(); // for the tfbScriptEditor
    }
}
