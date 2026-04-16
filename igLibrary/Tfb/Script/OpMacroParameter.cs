using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpMacroParameter : OpParameter
    {
        public igObject? _macroDef; // OpDefineMacro
        public ulong _parameterID;
    }
}
