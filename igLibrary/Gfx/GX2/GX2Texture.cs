using System.Diagnostics;

namespace igLibrary.Gfx.GX2Utils
{
    public class GX2Texture
    {
        private static string sFormat = ">" + GX2Surface.Size() + "x10I";
        public GX2Surface Surface { get; set; }
        public uint ViewFirstMip { get; set; }
        public uint ViewNumMips { get; set; }
        public uint ViewFirstSlice { get; set; }
        public uint ViewNumSlices { get; set; }
        public uint CompSel { get; set; }
        public uint[] Regs { get; set; }

        public GX2Texture(byte[] data = null, int pos = 0)
        {
            Surface = new GX2Surface();
            ViewFirstMip = 0;
            ViewNumMips = 1;
            ViewFirstSlice = 0;
            ViewNumSlices = 1;
            CompSel = GX2CompSel.ZZZO;
            Regs = new uint[5];

            if (data != null)
            {
                Load(data, pos);
            }
        }

        public void Load(byte[] data, int pos = 0)
        {
            Surface.Load(data, pos);

            Debug.Assert(Surface.AA == GX2AAMode2.Mode1X);
            Debug.Assert((Surface.Use & GX2SurfaceUse2.Texture) != 0);

            var values = Struct.Unpack(sFormat, data, pos);
            ViewFirstMip = values[0];
            ViewNumMips = values[1];
            ViewFirstSlice = values[2];
            ViewNumSlices = values[3];
            CompSel = values[4];
            Regs = values.Skip(5).ToArray();

            if (ViewNumMips == 0)
            {
                ViewNumMips = 1;
            }

            if (ViewNumSlices == 0)
            {
                ViewNumSlices = 1;
            }

            Debug.Assert(0 <= ViewFirstMip && ViewFirstMip <= Surface.NumMips - 1);
            Debug.Assert(1 <= ViewNumMips && ViewNumMips <= Surface.NumMips - ViewFirstMip);
            Debug.Assert(0 <= ViewFirstSlice && ViewFirstSlice <= Surface.Depth - 1);
            Debug.Assert(1 <= ViewNumSlices && ViewNumSlices <= Surface.Depth - ViewFirstSlice);
        }

        public byte[] Save()
        {
            byte[] surface = Surface.Save();
            byte[] texture = Struct.Pack(sFormat, new uint[] { ViewFirstMip, ViewNumMips, ViewFirstSlice, ViewNumSlices, CompSel }.Concat(Regs).ToArray());

            Array.Copy(surface, 0, texture, 0, GX2Surface.Size());

            return texture;
        }

        public static int Size()
        {
            return GX2Surface.Size() + 0x28;
        }

        public void InitTextureRegs(uint surfMode = 0, uint perfModulation = 7)
        {
            Regs = TextureRegisters.CalcRegs(
                Surface.Width, Surface.Height, Surface.NumMips, (uint)Surface.Format,
                (uint)Surface.TileMode, Surface.Pitch * (uint)(Surface.Format.IsCompressed() ? 4 : 1),
                GX2CompSel.GetCompSelAsArray(CompSel), surfMode, perfModulation
            ).ToArray();
        }

        public static GX2Texture InitTexture(GX2SurfaceDim dim, uint width, uint height, uint depth, uint numMips, GX2SurfaceFormat2 format, uint compSel, GX2TileMode2 tileMode = GX2TileMode2.Default,
                                             uint swizzle = 0, uint surfMode = 0, uint perfModulation = 7)
        {
            GX2Texture texture = new GX2Texture();

            texture.Surface.Dim = dim;
            texture.Surface.Width = width;
            texture.Surface.Height = height;
            texture.Surface.Depth = depth;
            texture.Surface.NumMips = numMips;
            texture.Surface.Format = format;
            texture.Surface.TileMode = tileMode;
            texture.Surface.Swizzle = swizzle << 8;

            texture.Surface.CalcSurfaceSizeAndAlignment();

            texture.ViewFirstMip = 0;
            texture.ViewNumMips = numMips;
            texture.ViewFirstSlice = 0;
            texture.ViewNumSlices = depth;
            texture.CompSel = compSel;

            texture.InitTextureRegs(surfMode, perfModulation);

            return texture;
        }
    }

}
