/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igPlatformMetaImage : igMetaImage
	{
		public IG_GFX_PLATFORM _platform;
		public override uint GetPadding(uint width)
		{
			switch(_platform)
			{
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII:
					if(_bitsPerPixel == 0x20) return 0x40;
					else                      return 0x20;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
					if(_isTile) return 1;
					else        return width;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ASPEN:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_OSX:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_RASPI:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ANDROID:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_METAL:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LGTV:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LINUX:
					if(_hasPalette && _isCompressed) return 4;
					else                             return 1;
				default:
					return 1;
			}
		}
		public override ushort GetAlignment()
		{
			switch(_platform)
			{
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII:
					return 0x20;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_XENON:
					return 0x1000;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
					return 0x80;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE:
					return 0x2000;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DURANGO:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ASPEN:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_OSX:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX11:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_RASPI:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_NULL:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ANDROID:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_METAL:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WGL:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LGTV:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LINUX:
					return 1;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS4:
					return 0x100;
			}
			return 1;
		}
		public override uint GetTextureLevelOffset(int width, int height, int depth, int levelCount, int imageCount, int targetLevel, int imageIndex)
		{
			if(targetLevel == 0 && imageIndex == 0) return 0;
			int corDepth = depth < 0 ? 1 : depth;
			int corHeight = height < 0 ? 1 : height;
			uint offset = 0;
			uint blockHeight = GetBlockHeight();
			for(int i = 0; i < imageCount; i++)
			{
				uint curHeight = (uint)corHeight;
				uint curDepth = (uint)corDepth;
				uint curWidth = (uint)width;
				for(int j = 0; j < levelCount; j++)
				{
					if(i == imageIndex && j == targetLevel) return offset;

					uint pitch = GetPitch(curWidth);
					curHeight = Align(curHeight, blockHeight);
					uint numBlocksHeight = curHeight / blockHeight;
					if(numBlocksHeight == 0) numBlocksHeight = 1;
					offset += pitch * curDepth * numBlocksHeight;

					curWidth = (curWidth + 1) >> 1;
					if(curWidth == 0) curWidth = 1;

					curHeight = (curHeight + 1) >> 1;
					if(curHeight == 0) curHeight = 1;

					curDepth = (curDepth + 1) >> 1;
					if(curDepth == 0) curDepth = 1;
				}
			}
			return offset;
		}
		public override uint GetPitch(uint width)
		{
			uint blockWidth = GetBlockWidth();
			uint alignedWidth = Align(width, blockWidth);
			byte bpp = 0;
			if(_isCompressed)
			{
				bpp = GetBitsCompressed();
			}
			else
			{
				bpp = _bitsPerPixel;
			}
			uint blockHeight = GetBlockHeight();
			uint numBlocks = alignedWidth / blockWidth;
			if(numBlocks < 1) numBlocks = 1;

			if(_platform == IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII)
			{
				bpp = _bitsPerPixel;
				uint tileWidth = GetTileWidth();
				if(_isCompressed)
				{
					bpp <<= 4;
					tileWidth >>= 2;
				}
				return Align(numBlocks, tileWidth) * bpp >> 3;
			}

			uint pitch = blockHeight * bpp * blockWidth * numBlocks + 7;
			pitch >>= 3;
			uint padding = GetPadding(alignedWidth);
			return Align(pitch, padding);
		}
		public uint PlatformGetTextureSize(IG_GFX_PLATFORM platform, uint width, uint height)
		{
			uint padding = GetPadding(width);
			if(platform == IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII && _isTile)
			{
				uint tileWidth = GetTileWidth();
				uint tileHeight = GetTileHeight();
				return Align(width, tileWidth) * Align(height, tileHeight) * padding;
			}
			uint blockWidth = GetBlockWidth();
			uint blockHeight = GetBlockHeight();
			uint numBlocksY = Align(height, blockHeight) / blockHeight;
			if(numBlocksY == 0) numBlocksY = 1;
			return Align(numBlocksY * padding, GetAlignment());
		}
	}
}