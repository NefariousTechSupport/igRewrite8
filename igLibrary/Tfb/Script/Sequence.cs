using igLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class Sequence : tfbScriptObject
    {
        public int _flags;
        public IG_UTILS_PLAY_MODE _playbackMode;
        public float _playbackPercent;
        public igObject? _activator; // tfbActorInfo
        public static object? _interface; // InterfaceResolver
    }
}
