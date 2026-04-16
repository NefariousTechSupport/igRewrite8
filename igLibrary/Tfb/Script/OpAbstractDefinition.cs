using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpAbstractDefinition : OpBranch
    {
        public enum Publicity
        {
            unpublished = 0,
            published = 1
        }
        public Publicity _publicity;
        public ulong _creationHandle;
        public uint _thisPC;
        public tfbScriptInfo? _scriptInfo;
    }
}
