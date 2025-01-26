/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.InteropServices;

namespace igLibrary.Gfx
{
	public static class igImage2Exporter
	{
		[Flags]
		private enum DdsFlags : uint
		{
			DDSD_CAPS = 0x1,
			DDSD_HEIGHT = 0x2,
			DDSD_WIDTH = 0x4,
			DDSD_PITCH = 0x8,
			DDSD_PIXELFORMAT = 0x1000,
			DDSD_MIPMAPCOUNT = 0x20000,
			DDSD_LINEARSIZE = 0x80000,
			DDSD_DEPTH = 0x800000
		}
		[Flags]
		private enum DdsCapsFlags : uint
		{
			DDSCAPS_COMPLEX = 0x8,
			DDSCAPS_MIPMAP = 0x400000,
			DDSCAPS_TEXTURE = 0x1000
		}
		[Flags]
		private enum DdsCaps2Flags : uint
		{
			DDSCAPS2_CUBEMAP = 0x200,
			DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
			DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
			DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
			DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
			DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
			DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
			DDSCAPS2_VOLUME = 0x200000
		}
		[StructLayout(LayoutKind.Explicit, Size = 0x7C)]
		private struct DdsHeader
		{
			public const uint MagicCookie = 0x20534444;
			[FieldOffset(0x00)] public uint dwSize;
			[FieldOffset(0x04)] public DdsFlags dwFlags;
			[FieldOffset(0x08)] public uint dwHeight;
			[FieldOffset(0x0C)] public uint dwWidth;
			[FieldOffset(0x10)] public uint dwPitchOrLinearSize;
			[FieldOffset(0x14)] public uint dwDepth;
			[FieldOffset(0x18)] public uint dwMipMapCount;
			[FieldOffset(0x48)] public DdsPixelFormat ddspf;
			[FieldOffset(0x68)] public DdsCapsFlags dwCaps;
			[FieldOffset(0x6C)] public DdsCaps2Flags dwCaps2;
			[FieldOffset(0x70)] public uint dwCaps3;
			[FieldOffset(0x74)] public uint dwCaps4;
			public DdsHeader()
			{
				dwSize = 0x7C;
				dwFlags = DdsFlags.DDSD_CAPS | DdsFlags.DDSD_HEIGHT | DdsFlags.DDSD_WIDTH | DdsFlags.DDSD_PIXELFORMAT;
				dwHeight = default;
				dwWidth = default;
				dwPitchOrLinearSize = default;
				dwDepth = default;
				dwMipMapCount = default;
				ddspf = new DdsPixelFormat();
				dwCaps = DdsCapsFlags.DDSCAPS_TEXTURE;
				dwCaps2 = default;
				dwCaps3 = default;
				dwCaps4 = default;
			}
		}
		[Flags]
		private enum DdsPixelFormatFlags : uint
		{
			DDPF_ALPHAPIXELS = 0x1,
			DDPF_ALPHA = 0x2,
			DDPF_FOURCC = 0x4,
			DDPF_RGB = 0x40,
			DDPF_YUV = 0x200,
			DDPF_LUMINANCE = 0x20000
		}
		[StructLayout(LayoutKind.Explicit, Size = 0x20)]
		private struct DdsPixelFormat
		{
			public const uint FourCC_DXT1 = 0x31545844;
			public const uint FourCC_DXT2 = 0x32545844;
			public const uint FourCC_DXT3 = 0x33545844;
			public const uint FourCC_DXT4 = 0x34545844;
			public const uint FourCC_DXT5 = 0x35545844;
			[FieldOffset(0x00)] public uint dwSize;
			[FieldOffset(0x04)] public DdsPixelFormatFlags dwFlags;
			[FieldOffset(0x08)] public uint dwFourCC;
			[FieldOffset(0x0C)] public uint dwRGBBitCount;
			[FieldOffset(0x10)] public uint dwRBitMask;
			[FieldOffset(0x14)] public uint dwGBitMask;
			[FieldOffset(0x18)] public uint dwBBitMask;
			[FieldOffset(0x1C)] public uint dwABitMask;
			public DdsPixelFormat()
			{
				dwSize = 0x20;
				dwFlags = default;
				dwFourCC = default;
				dwRGBBitCount = default;
				dwRBitMask = default;
				dwGBitMask = default;
				dwBBitMask = default;
				dwABitMask = default;
			}
		}
		public static void ExportToDds(igImage2 image, Stream dst)
		{
			DdsHeader header = new DdsHeader();

			header.dwFlags |= image._format._isCompressed ? DdsFlags.DDSD_LINEARSIZE : DdsFlags.DDSD_PITCH;
			header.dwFlags |= image._levelCount > 1 ? DdsFlags.DDSD_MIPMAPCOUNT : 0;

			header.dwHeight = image._height;
			header.dwWidth = image._width;

			header.dwPitchOrLinearSize = image._format.GetPitch(image._width);
			header.dwDepth = image._depth;
			header.dwMipMapCount = image._levelCount;

			if(image._format._isCompressed)
			{
				header.ddspf.dwFlags |= DdsPixelFormatFlags.DDPF_FOURCC;

				     if(image._format._name.StartsWith("dxt1")) header.ddspf.dwFourCC = DdsPixelFormat.FourCC_DXT1;
				else if(image._format._name.StartsWith("dxt3")) header.ddspf.dwFourCC = DdsPixelFormat.FourCC_DXT3;
				else if(image._format._name.StartsWith("dxt5")) header.ddspf.dwFourCC = DdsPixelFormat.FourCC_DXT5;
				else throw new NotImplementedException($"format {image._format._name} is not supported for export");
			}
			else
			{
				igVec4uc bits = new igVec4uc(image._format.GetBitsRed(), image._format.GetBitsBlue(), image._format.GetBitsGreen(), image._format.GetBitsAlpha());
				header.ddspf.dwRGBBitCount = image._format._bitsPerPixel;
				throw new NotImplementedException("pixel formats aren't supported for export right now");
			}

			header.dwCaps |= DdsCapsFlags.DDSCAPS_MIPMAP;

			StreamHelper sh = new StreamHelper(dst, StreamHelper.Endianness.Little);
			sh.WriteUInt32(DdsHeader.MagicCookie);
			sh.WriteStruct<DdsHeader>(header);
			sh.WriteBytes(image._data.Buffer);
			sh.Dispose();
		}
	}
}