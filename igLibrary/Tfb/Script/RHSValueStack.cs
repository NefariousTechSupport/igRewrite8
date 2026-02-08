using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using igLibrary.Core;

namespace igLibrary.Tfb.Script
{
    public class RHSValueStack : ValueStack
    {
        public igMetaObject _type;

        public int _value;
        public uint _properties;
        public enum enumRHSfunc
        {
            kRHSfuncNone = 0,
            kRHSfuncRandom = 1,
            __separator0__ = 1,
            kRHSfuncInteger = 2,
            kRHSfuncRound = 3,
            kRHSfuncFloor = 4,
            kRHSfuncCeiling = 5,
            __separator1__ = 5,
            kRHSfuncAbsolute_value = 6,
            kRHSfuncSign = 7,
            kRHSfuncSquare = 8,
            kRHSfuncSquare_root = 9,
            __separator2__ = 9,
            kRHSfuncNormalize_angle = 10,
            kRHSfuncSine = 11,
            kRHSfuncCosine = 12,
            kRHSfuncTangent = 13,
            kRHSfuncArc_sine = 14,
            kRHSfuncArc_cosine = 15,
            kRHSfuncArc_tangent = 16,

        }
        public enumRHSfunc _funcRHS;
        public bool _typeIsNumber;
        public bool _isRandom;
        //{
        //    get => (_properties & 0x00) != 0;
        //    set => _properties = (uint)((_properties & -0x01u) | (value ? 0x01u : 0u));
        //}

        // todo: igBitMetaFields
    }
}
