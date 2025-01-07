using igLibrary.Gfx.GX2Utils;

namespace igLibrary.Gfx
{
	/// <summary>
	/// Class for handling the Wii U's Swizzle
	/// </summary>
    public class igWiiUSwizzle
    {
        /// <summary>
        /// Get tilemode for rgba32 texture
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>GX2TileMode</returns>
        private static GX2.GX2TileMode GetTileModeForR8G8B8A8_UNORM(int width, int height)
        {
            if (width < 31 && height < 15) return GX2.GX2TileMode.MODE_1D_TILED_THIN1;
            if ((width - 1) % 32 > 23 && (height - 1) % 16 > 7) return GX2.GX2TileMode.MODE_2D_TILED_THIN1;
            return GX2.GX2TileMode.MODE_1D_TILED_THIN1;
        }

        /// <summary>
        /// Deswizzles one mipmap of a texture
        /// </summary>
        /// <param name="imageData">Byte data</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">GX2SurfaceFormat</param>
        /// <param name="formatHash">FNV-1a Hash of the name of the format</param>
        /// <returns>byte[] imageData</returns>
        private static byte[] DeswizzleLayer(byte[] imageData, ushort width, ushort height, uint format, uint formatHash)
        {
            var surf = GX2.CreateGx2Texture(imageData, "", 0, 0, width, height, 1, format, 0, (uint)GX2.GX2SurfaceDimension.DIM_2D, 1);
            surf.tileMode = GX2.getDefaultGX2TileMode((uint)GX2.GX2SurfaceDimension.DIM_2D, width, height, 1, formatHash, 0, 1);
            var surfout = GX2.getSurfaceInfo((GX2.GX2SurfaceFormat)surf.format, surf.width, surf.height, surf.depth, surf.dim, surf.tileMode, surf.aa, 0);
            if (surfout.surfSize > surf.data.Length || (surf.mipSize > 0 && surfout.surfSize + surf.mipSize != surf.data.Length))
                surf.tileMode = (uint)GX2.GX2TileMode.MODE_1D_TILED_THIN1;
            surf.data = imageData;
            return GX2.Decode(surf, 0, 0);
        }

        /// <summary>
        /// Deswizzles a texture
        /// </summary>
        /// <param name="imageData">Byte data</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formatName">Name of the format</param>
        /// <param name="mipCount">Number of mipmaps</param>
        /// <returns>byte[] imageData</returns>
        public static byte[] Deswizzle(byte[] imageData, ushort width, ushort height, string formatName, uint mipCount)
        {
            try
            {
                if (formatName.StartsWith("r8g8b8a8"))
                {
                    var surf = GX2.CreateGx2Texture(imageData, "", 0, 0, width, height, 1, (uint)GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNORM, 0, (uint)GX2.GX2SurfaceDimension.DIM_2D, 0);
                    surf.tileMode = (uint)GetTileModeForR8G8B8A8_UNORM(width, height);
                    surf.data = imageData;
                    return GX2.Decode(surf, 0, 0);
                }
                else
                {
                    uint format = (uint)(formatName.StartsWith("dxt1") ? GX2.GX2SurfaceFormat.T_BC1_UNORM : GX2.GX2SurfaceFormat.T_BC5_UNORM);
                    uint formatHash = igHash.Hash(formatName);
                    var surf = GX2.CreateGx2Texture(imageData, "", 0, 0, width, height, 1, format, 0, (uint)GX2.GX2SurfaceDimension.DIM_2D, mipCount);
                    surf.tileMode = GX2.getDefaultGX2TileMode((uint)GX2.GX2SurfaceDimension.DIM_2D, width, height, 1, formatHash, 0, 1);
                    var surfout = GX2.getSurfaceInfo((GX2.GX2SurfaceFormat)surf.format, surf.width, surf.height, surf.depth, surf.dim, surf.tileMode, surf.aa, 0);
                    if (surfout.surfSize > surf.data.Length || (surf.mipSize > 0 && surfout.surfSize + surf.mipSize != surf.data.Length))
                        surf.tileMode = (uint)GX2.GX2TileMode.MODE_1D_TILED_THIN1;
                    surf.data = imageData;
                    List<byte> deswizzledData = new List<byte>();
                    uint currentOffset = 0;
                    for (var i = 0; i < mipCount; i++)
                    {
                        surfout = GX2.getSurfaceInfo((GX2.GX2SurfaceFormat)surf.format, surf.width, surf.height, surf.depth, surf.dim, surf.tileMode, surf.aa, i);
                        ushort mipWidth = (ushort)System.Math.Max(1, width >> i);
                        ushort mipHeight = (ushort)System.Math.Max(1, height >> i);
                        uint mipSize = (uint)surfout.surfSize;
                        byte[] mipData = new byte[mipSize];
                        Array.Copy(imageData, (int)currentOffset, mipData, 0, (int)mipSize);
                        deswizzledData.AddRange(DeswizzleLayer(mipData, mipWidth, mipHeight, format, formatHash));
                        currentOffset += mipSize;
                    }
                    return deswizzledData.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return imageData;
            }
        }

        /// <summary>
        /// Swizzles a rgba32 texture
        /// </summary>
        /// <param name="imageData">Byte data</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>byte[] imageData</returns>
        public static byte[] Swizzle_rgba32(byte[] imageData, ushort width, ushort height)
        {
            return GX2.CreateGx2Texture(imageData, "", (uint)GetTileModeForR8G8B8A8_UNORM(width, height), 0, width, height, 1, (uint)GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNORM, 0, (uint)GX2.GX2SurfaceDimension.DIM_2D, 0).data;
        }
    }
}