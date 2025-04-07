/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igCanonicalMetaImage : igMetaImage
	{
		public override ushort GetAlignment() => 1;
		public override uint GetPadding(uint width) => 1;
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
			uint pitch = blockHeight * bpp * blockWidth * numBlocks + 7;
			pitch >>= 3;
			uint padding = GetPadding(alignedWidth);
			return Align(pitch, padding);
		}
	}
}