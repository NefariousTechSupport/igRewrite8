using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class AbstractAttachNode : MatrixMeasurement
    {
        public ScriptVariantList? _tagList;
        public static tfbScriptObject? _interface; // public static InterfaceResolver
    }
}
