using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class OpSlideValue : OpBranch
    {
        public ValueStack _LHS;
        public ValueRHSVariant _RHS;
        public ValueRHSVariant _secondsRHS;
        public ValueRHSVariant _easeOutRHS;
        public ValueRHSVariant _easeInRHS;
        public igObject? _cachedSlider; // Slider
    }
}
