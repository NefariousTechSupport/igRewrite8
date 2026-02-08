using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpCode : AbstractScriptVariant
    {
        public ushort _internalFlagsStorage;
        public ushort _branchPC;
        public static int _executionMode;
        public static igNamedObject _singleStepper;
        public static uint _PC;
        public static tfbScriptInfo? _executingScript;
        public static object _breakpointMsg; // technically a BreakpointMessage
        public static AbstractPlacement _opCodeReceiver;
    }
}
