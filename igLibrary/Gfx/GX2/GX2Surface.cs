using System.Diagnostics;

namespace igLibrary.Gfx.GX2Utils
{
    public class GX2Surface
    {
        private static string sFormat = ">" + Size() + "x10I";
        public GX2SurfaceDim Dim { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Depth { get; set; }
        public uint NumMips { get; set; }
        public GX2SurfaceFormat2 Format { get; set; }
        public GX2AAMode2 AA { get; set; }
        public GX2SurfaceUse2 Use { get; set; }
        public uint ImageSize { get; set; }
        public byte[] ImageData { get; set; }
        public uint MipSize { get; set; }
        public byte[] MipData { get; set; }
        public GX2TileMode2 TileMode { get; set; }
        public uint Swizzle { get; set; }
        public uint Alignment { get; set; }
        public uint Pitch { get; set; }
        public uint[] MipOffset { get; set; }

        public GX2Surface(byte[] data = null, int pos = 0)
        {
            Dim = GX2SurfaceDim.Dim2D;
            Width = 0;
            Height = 0;
            Depth = 1;
            NumMips = 1;
            Format = GX2SurfaceFormat2.Invalid;
            AA = GX2AAMode2.Mode1X;
            Use = GX2SurfaceUse2.Texture;
            ImageSize = 0;
            ImageData = new byte[0];
            MipSize = 0;
            MipData = new byte[0];
            TileMode = GX2TileMode2.Default;
            Swizzle = 0;
            Alignment = 0;
            Pitch = 0;
            MipOffset = new uint[13];

            if (data != null)
            {
                Load(data, pos);
            }
        }

        public void Load(byte[] data, int pos = 0)
        {
            var values = Struct.Unpack(sFormat, data, pos);
            Dim = (GX2SurfaceDim)values[0];
            Width = values[1];
            Height = values[2];
            Depth = values[3];
            NumMips = values[4];
            Format = (GX2SurfaceFormat2)values[5];
            AA = (GX2AAMode2)values[6];
            Use = (GX2SurfaceUse2)values[7];
            ImageSize = values[8];
            uint imagePtr = values[9];
            MipSize = values[10];
            uint mipPtr = values[11];
            TileMode = (GX2TileMode2)values[12];
            Swizzle = values[13];
            Alignment = values[14];
            Pitch = values[15];
            MipOffset = values.Skip(16).Select(x => x).ToArray();

            Debug.Assert(Width != 0);
            Debug.Assert(Height != 0);
            Debug.Assert(NumMips <= 14);
            Debug.Assert(ImageSize != 0);
            Debug.Assert(MipSize != 0 == NumMips > 1);
            Debug.Assert(Pitch != 0);

            // Assertion must not fail for serialized GX2Surface
            Debug.Assert(Format != GX2SurfaceFormat2.Invalid);
            Debug.Assert(imagePtr == 0);
            Debug.Assert(mipPtr == 0);
            Debug.Assert(TileMode != GX2TileMode2.Default);

            if (Depth == 0)
            {
                Depth = 1;
            }

            if (NumMips == 0)
            {
                NumMips = 1;
            }
        }

        public byte[] Save()
        {
            byte[] surface = Struct.Pack(sFormat, new uint[] { (uint)Dim, Width, Height, Depth, NumMips, (uint)Format, (uint)AA, (uint)Use, ImageSize, 0, MipSize, 0, (uint)TileMode, Swizzle, Alignment, Pitch }.Concat(MipOffset).ToArray());

            return surface;
        }

        public static int Size()
        {
            return 0x74;
        }
        public void CalcSurfaceSizeAndAlignment()
        {
            // Calculate the best tileMode if set to default
            if (TileMode == GX2TileMode2.Default)
            {
                TileMode = (GX2TileMode2)GX2.getDefaultGX2TileMode(
                    (uint)Dim, Width, Height, Depth,
                    (uint)Format, (uint)AA, (uint)Use
                );
            }

            // Calculate the surface info for the base level
            var surfInfo = GX2.getSurfaceInfo(
                (GX2.GX2SurfaceFormat)Format, Width, Height, Depth,
                (uint)Dim, (uint)TileMode, (uint)AA, 0
            );

            // Set the image size, alignment and pitch
            ImageSize = (uint)surfInfo.surfSize;
            Alignment = surfInfo.baseAlign;
            Pitch = surfInfo.pitch;

            // Ensure pipe and bank swizzle is valid
            Swizzle &= 0x0700;

            // Calculate the swizzle 1D tiling start level, mip size, mip offsets and
            uint tiling1dLevel = 0;
            bool tiling1dLevelSet = new GX2TileMode2[] {
                GX2TileMode2.Linear_Aligned, GX2TileMode2.Linear_Special,
                GX2TileMode2.Tiled_1D_Thin1, GX2TileMode2.Tiled_1D_Thick
            }.Contains((GX2TileMode2)surfInfo.tileMode);
            if (!tiling1dLevelSet)
            {
                tiling1dLevel += 1;
            }

            MipSize = 0;
            for (int mipLevel = 1; mipLevel < NumMips; mipLevel++)
            {
                // Calculate the surface info for the mip level
                surfInfo = GX2.getSurfaceInfo(
                    (GX2.GX2SurfaceFormat)Format, Width, Height, Depth,
                    (uint)Dim, (uint)TileMode, (uint)AA, mipLevel
                );

                // Make sure the level is aligned
                MipSize = RoundUp(MipSize, surfInfo.baseAlign);

                // Set the offset of the level
                //   Level 1 offset is used to place the mip data (levels 1+) after the image data (level 0)
                //   The value is the minimum size of the image data + padding to ensure the mip data is aligned
                if (mipLevel == 1)
                {
                    // Level 1 alignment should suffice to ensure all the other levels are aligned as well
                    MipOffset[0] = RoundUp(ImageSize, surfInfo.baseAlign);
                }
                else
                {
                    // Level offset should be the size of all previous levels (aligned)
                    MipOffset[mipLevel - 1] = MipSize;
                }

                // Increase the total mip size by this level's size
                MipSize += (uint)surfInfo.surfSize;

                // Calculate the swizzle 1D tiling start level for tiled surfaces
                if (!tiling1dLevelSet)
                {
                    // Check if the tiling mode switched to 1D tiling
                    var tileMode = (GX2TileMode2)surfInfo.tileMode;
                    if (tileMode == GX2TileMode2.Tiled_1D_Thin1 || tileMode == GX2TileMode2.Tiled_1D_Thick)
                    {
                        tiling1dLevelSet = true;
                    }
                    else
                    {
                        tiling1dLevel += 1;
                    }
                }
            }

            //  If the tiling mode never switched to 1D tiling, set the start level to 13 (observed from existing files)
            if (!tiling1dLevelSet)
            {
                tiling1dLevel = 13;
            }

            Swizzle |= tiling1dLevel << 16;

            // Clear the unused mip offsets
            for (uint mipLevel = NumMips; mipLevel < 14; mipLevel++)
            {
                MipOffset[mipLevel - 1] = 0;
            }
        }
        public static void CopySurface(GX2Surface src, GX2Surface dst)
        {
            // Check requirements
            Debug.Assert(dst.Dim == src.Dim);
            Debug.Assert(dst.Width == src.Width);
            Debug.Assert(dst.Height == src.Height);
            Debug.Assert(dst.Depth <= src.Depth);
            Debug.Assert(dst.NumMips <= src.NumMips);
            Debug.Assert(dst.Format == src.Format);

            // Check if the two surfaces are the same
            // (If they are, we can just copy the data over)
            if (src.TileMode == dst.TileMode &&
                (src.TileMode == GX2TileMode2.Linear_Aligned ||
                 src.TileMode == GX2TileMode2.Linear_Special ||
                 (src.Swizzle >> 8 & 7) == (dst.Swizzle >> 8 & 7)) &&
                (src.Depth == dst.Depth &&
                 (src.Depth == 1 || src.NumMips == dst.NumMips) ||
                 src.NumMips == 1))
            {
                // No need to process anything, just copy the data over
                Array.Copy(src.ImageData, dst.ImageData, dst.ImageSize);
                Array.Copy(src.MipData, dst.MipData, dst.MipSize);
                return;
            }

            // Untile the source data
            List<byte[]> levels = new List<byte[]>();

            // Calculate the surface info for the base level
            var surfInfo = GX2.getSurfaceInfo(
                (GX2.GX2SurfaceFormat)src.Format, src.Width, src.Height, src.Depth,
                (uint)src.Dim, (uint)src.TileMode, (uint)src.AA, 0
            );

            // Get the depth used for tiling
            var tileMode = (GX2TileMode2)surfInfo.tileMode;
            uint tilingDepth = surfInfo.depth;

            if (tileMode == GX2TileMode2.Tiled_1D_Thick ||
                tileMode == GX2TileMode2.Tiled_2D_Thick || tileMode == GX2TileMode2.Tiled_2B_Thick ||
                tileMode == GX2TileMode2.Tiled_3D_Thick || tileMode == GX2TileMode2.Tiled_3B_Thick)
            {
                tilingDepth = DivRoundUp(tilingDepth, 4);
            }

            // Depths higher than 1 are currently not supported
            Debug.Assert(tilingDepth == 1);

            // Block width and height for the format
            int blkWidth = src.Format.IsCompressed() ? 4 : 1;
            int blkHeight = src.Format.IsCompressed() ? 4 : 1;

            // Bytes-per-pixel
            uint bpp = DivRoundUp(surfInfo.bpp, 8);

            // Untile the base level
            byte[] result = GX2.deswizzle(
                src.Width, src.Height, 1, 0, (uint)src.Format, 0, (uint)src.Use, surfInfo.tileMode,
                src.Swizzle, surfInfo.pitch, surfInfo.bpp, 0, 0, src.ImageData
            );

            // Make sure it's the correct size
            uint size = DivRoundUp(src.Width, (uint)blkWidth) * DivRoundUp(src.Height, (uint)blkHeight) * bpp;
            Debug.Assert(result.Length >= size);
            levels.Add(result.Take((int)size).ToArray());

            // Untile the other levels (mipmaps)
            uint offset = 0;
            for (int mipLevel = 1; mipLevel < dst.NumMips; mipLevel++)
            {
                // Calculate the width and height of the mip level
                uint width = System.Math.Max(1, src.Width >> mipLevel);
                uint height = System.Math.Max(1, src.Height >> mipLevel);

                // Calculate the surface info for the mip level
                surfInfo = GX2.getSurfaceInfo(
                    (GX2.GX2SurfaceFormat)src.Format, src.Width, src.Height, src.Depth,
                    (uint)src.Dim, (uint)src.TileMode, (uint)src.AA, mipLevel
                );

                // Untile the mip level
                result = GX2.deswizzle(
                    width, height, 1, 0, (uint)src.Format, 0, (uint)src.Use, surfInfo.tileMode,
                    src.Swizzle, surfInfo.pitch, surfInfo.bpp, 0, 0, src.MipData.Skip((int)offset).Take((int)surfInfo.surfSize).ToArray()
                );

                // Make sure it's the correct size
                size = DivRoundUp(width, (uint)blkWidth) * DivRoundUp(height, (uint)blkHeight) * bpp;
                Debug.Assert(result.Length >= size);
                levels.Add(result.Take((int)size).ToArray());

                // Set the offset of the next level
                if (mipLevel < src.NumMips - 1)
                {
                    offset = src.MipOffset[mipLevel];
                }
            }
            // Tile the destination data
            // Calculate the surface info for the base level
            surfInfo = GX2.getSurfaceInfo(
                (GX2.GX2SurfaceFormat)dst.Format, dst.Width, dst.Height, dst.Depth,
                (uint)dst.Dim, (uint)dst.TileMode, (uint)dst.AA, 0
            );
            Debug.Assert(dst.ImageSize == surfInfo.surfSize);

            // Get the depth used for tiling
            tileMode = (GX2TileMode2)surfInfo.tileMode;
            tilingDepth = surfInfo.depth;

            if (tileMode == GX2TileMode2.Tiled_1D_Thick ||
                tileMode == GX2TileMode2.Tiled_2D_Thick || tileMode == GX2TileMode2.Tiled_2B_Thick ||
                tileMode == GX2TileMode2.Tiled_3D_Thick || tileMode == GX2TileMode2.Tiled_3B_Thick)
            {
                tilingDepth = DivRoundUp(tilingDepth, 4);
            }

            // Depths higher than 1 are currently not supported
            Debug.Assert(tilingDepth == 1);

            // Block width and height for the format
            blkWidth = src.Format.IsCompressed() ? 4 : 1;
            blkHeight = src.Format.IsCompressed() ? 4 : 1;

            // Bytes-per-pixel
            bpp = DivRoundUp(surfInfo.bpp, 8);

            // Tile the base level
            dst.ImageData = GX2.swizzle(
                dst.Width, dst.Height, 1, 0, (uint)dst.Format, 0, (uint)dst.Use, surfInfo.tileMode,
                dst.Swizzle, surfInfo.pitch, surfInfo.bpp, 0, 0, PadRight(levels[0], (int)surfInfo.surfSize, 0)
            ).Take((int)surfInfo.surfSize).ToArray();

            // Tile the other levels (mipmaps)
            List<byte> mipData = new List<byte>();
            for (int mipLevel = 1; mipLevel < dst.NumMips; mipLevel++)
            {
                // Calculate the width and height of the mip level
                uint width = System.Math.Max(1, dst.Width >> mipLevel);
                uint height = System.Math.Max(1, dst.Height >> mipLevel);

                // Calculate the surface info for the mip level
                surfInfo = GX2.getSurfaceInfo(
                    (GX2.GX2SurfaceFormat)dst.Format, dst.Width, dst.Height, dst.Depth,
                    (uint)dst.Dim, (uint)dst.TileMode, (uint)dst.AA, mipLevel
                );

                if (mipLevel != 1)
                {
                    mipData.AddRange(new byte[dst.MipOffset[mipLevel - 1] - mipData.Count]);
                }

                // Untile the mip level
                mipData.AddRange(GX2.swizzle(
                    width, height, 1, 0, (uint)dst.Format, 0, (uint)dst.Use, surfInfo.tileMode,
                    dst.Swizzle, surfInfo.pitch, surfInfo.bpp, 0, 0, PadRight(levels[mipLevel], (int)surfInfo.surfSize, 0)
                ).Take((int)surfInfo.surfSize));
            }

            Debug.Assert(mipData.Count == dst.MipSize);
            dst.MipData = mipData.ToArray();
        }
        public static void GX2SurfacePrintInfo(GX2Surface surface)
        {
            Console.WriteLine();
            Console.WriteLine("// ----- GX2Surface Info ----- ");
            Console.WriteLine("  dim             = " + surface.Dim);
            Console.WriteLine("  width           = " + surface.Width);
            Console.WriteLine("  height          = " + surface.Height);
            Console.WriteLine("  depth           = " + surface.Depth);
            Console.WriteLine("  numMips         = " + surface.NumMips);
            Console.WriteLine("  format          = " + surface.Format);
            Console.WriteLine("  aa              = " + surface.AA);
            Console.WriteLine("  use             = " + surface.Use);
            Console.WriteLine("  imageSize       = " + surface.ImageSize);
            Console.WriteLine("  mipSize         = " + surface.MipSize);
            Console.WriteLine("  tileMode        = " + surface.TileMode);
            Console.WriteLine("  swizzle         = " + surface.Swizzle + ", " + surface.Swizzle.ToString("X"));
            Console.WriteLine("  alignment       = " + surface.Alignment);
            Console.WriteLine("  pitch           = " + surface.Pitch);
        }
        public static uint DivRoundUp(uint n, uint d)
        {
            return (n + d - 1) / d;
        }
        public static uint RoundUp(uint x, uint y)
        {
            return (x - 1 | y - 1) + 1;
        }
        public static byte[] PadRight(byte[] original, int length, byte paddingByte)
        {
            if (original.Length >= length)
            {
                return original;
            }
            else
            {
                byte[] padded = new byte[length];
                Array.Copy(original, padded, original.Length);
                for (int i = original.Length; i < length; i++)
                {
                    padded[i] = paddingByte;
                }
                return padded;
            }
        }

    }
}
