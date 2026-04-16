using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using igLibrary.Tfb.Core;

namespace igLibrary.Tfb.Script
{
    public class AbstractPlacement : AbstractAttachNode
    {
        public uint _flagPool;
        public int _ID;
        public tfbEulerTransform _initMatrix;
        public tfbEulerTransform _currentMatrix;
        public enum AbstractPlacement_State
        {
            FIRST_PLACEMENT_STATE = 0,
            PLACEMENT_PLAYER = 0,
            PLACEMENT_ACTIVE = 1,
            PLACEMENT_INACTIVE = 2,
            NUM_ITERATED_PLACEMENT_STATES = 3,
            PLACEMENT_REMOVED = 3,
            NUM_MANAGED_PLACEMENT_STATES = 4,
            PLACEMENT_UNMANAGED = 4,
            PLACEMENT_TEMPLATE = 5

        }
        public AbstractPlacement_State _activityState;
        public ScriptGroupStack? _clones;
        public AbstractPlacement? _parent;
        public igNamedObject? _activator; // tfbActorInfo. But can I call it a namedobject? i only need the name
        public igObject? _moveObject;
        public int _moveIndex;
        public AbstractAttachNode? _attachNode;
        public tfbScriptObject? _attachNodeOwner; // AbstractPlacement
        public ScriptObjectList? _attachedObjects; // public AbstractPlacementList
        public tfbEulerTransform _attachXform;
        public SoundList? _soundList;
        public AnimationStack? _animationList;
        public AbstractPlacement? _resetTemplate;
        public uint _activeTimeLayer;
        public static tfbScriptObject? _interface;  // InterfaceResolver
        public bool _explicitAttachment; // make bitfield logic for this later
        // bitfield boolean _explicitAttachment (storage field: _flagPool) shift 0x00 bits 0x01



    }
}
