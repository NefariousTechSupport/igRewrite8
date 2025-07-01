namespace igLibrary.Gfx.GX2Utils
{
    public class TextureRegisters
    {
        public static uint Register0(uint width, uint pitch, uint tileType, uint tileMode, uint dim)
        {
            return (width & 0x1FFF) << 19 |
                          (pitch & 0x07FF) << 8 |
                          (tileType & 0x0001) << 7 |
                          (tileMode & 0x000F) << 3 |
                          dim & 0x0007;
        }

        public static uint Register1(uint format, uint depth, uint height)
        {
            return (format & 0x003F) << 26 |
                          (depth & 0x1FFF) << 13 |
                          height & 0x1FFF;
        }

        public static uint Register2(uint baseLevel, uint dstSelW, uint dstSelZ, uint dstSelY, uint dstSelX, uint requestSize, uint endian, uint forceDegamma, uint surfMode, uint numFormat, uint formatComp)
        {
            return (baseLevel & 7) << 28 |
                          (dstSelW & 7) << 25 |
                          (dstSelZ & 7) << 22 |
                          (dstSelY & 7) << 19 |
                          (dstSelX & 7) << 16 |
                          (requestSize & 3) << 14 |
                          (endian & 3) << 12 |
                          (forceDegamma & 1) << 11 |
                          (surfMode & 1) << 10 |
                          (numFormat & 3) << 8 |
                          (formatComp & 3) << 6 |
                          (formatComp & 3) << 4 |
                          (formatComp & 3) << 2 |
                          formatComp & 3;
        }

        public static uint Register3(uint yuvConv, uint lastArray, uint baseArray, uint lastLevel)
        {
            return (yuvConv & 0x0003) << 30 |
                          (lastArray & 0x1FFF) << 17 |
                          (baseArray & 0x1FFF) << 4 |
                          lastLevel & 0x000F;
        }

        public static uint Register4(uint type, uint advisClampLOD, uint advisFaultLOD, uint interlaced, uint perfModulation, uint maxAnisoRatio, uint MPEGClamp)
        {
            return (type & 0x03) << 30 |
                          (advisClampLOD & 0x3F) << 13 |
                          (advisFaultLOD & 0x0F) << 9 |
                          (interlaced & 0x01) << 8 |
                          (perfModulation & 0x07) << 5 |
                          (maxAnisoRatio & 0x07) << 2 |
                          MPEGClamp & 0x03;
        }

        public static uint[] CalcRegs(uint width, uint height, uint numMips, uint format, uint tileMode, uint pitch, uint[] compSel, uint surfMode, uint perfModulation)
        {
            pitch = System.Math.Max(pitch, 8);
            uint register0 = Register0(width - 1, pitch / 8 - 1, 0, tileMode, 1);

            uint register1 = Register1(format, 0, height - 1);

            uint formatComp = 0;
            uint numFormat = 0;
            uint forceDegamma = 0;

            if ((format & 0x200) != 0)
            {
                formatComp = 1;
            }

            if ((format & 0x800) != 0)
            {
                numFormat = 2;
            }
            else if ((format & 0x100) != 0)
            {
                numFormat = 1;
            }

            if ((format & 0x400) != 0)
            {
                forceDegamma = 1;
            }

            uint register2 = Register2(0, compSel[3], compSel[2], compSel[1], compSel[0], 2, 0, forceDegamma, surfMode, numFormat, formatComp);

            uint register3 = Register3(0, 0, 0, numMips - 1);

            uint register4 = Register4(2, 0, 0, 0, perfModulation, 4, 0);

            return new uint[] { register0, register1, register2, register3, register4 };
        }
    }

}
