using igLibrary.Tfb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{
    public class PlacementReference : ScriptReference
    {
        public tfbEulerTransform _rememberedMatrix;
        public static object? _interface; // InterfaceResolver
        public static MatrixMeasurement? _rememberedMatrixInterface;
    }
}
