using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class ValueRHSVariant : igObject
    {
        public RHSValueStack _varOp1;
        public enum ArithOp
        {
            dotdotdot = -1,
            plus = 0,
            minus = 1,
            times = 2,
            divide = 3,
        }
        public ArithOp _arithOperator;
        public RHSValueStack _varOp2;
        public object? _resultFunc;
        public igObject _conditioner;
        
        

    }
}
