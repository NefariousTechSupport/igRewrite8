/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;
using igLibrary.Core;
using igLibrary.Math;

namespace igLibrary.Gfx
{
	public class igImage2 : igNamedObject
	{
		public ushort _width;
		public ushort _height;
		public ushort _depth;
		public ushort _levelCount;
		public ushort _imageCount;
		public igMetaImage _format;
		public int _usageDeprecated;
		public ushort _paddingDeprecated;
		public igMemory<byte> _data;
		public int _lockCount;
		public int _texHandle = -1;
		public ulong _lockedMemory;
		public bool _oglDiscardOriginalImage = true;
		public static bool _makeAbstract;
		public static bool _makeConcrete;
		public igVec4f _colorScale = new igVec4f(1, 1, 1, 1);
		public igVec4f _colorBias;
		public igObject? _graphicsHelper = null;

		public uint GetTextureLevelOffset(int targetLevel, int targetImage)
		{
			if(_format == null) return 0xFFFFFFFF;
			return _format.GetTextureLevelOffset(_width, _height, _depth, _levelCount, _imageCount, targetLevel, targetImage);
		}
		public unsafe int ConvertClone(igMetaImage target, igMemoryPool pool, out igImage2? other)
		{
			other = null;
			if(_format == null) return -1;

			if(!_format.TryGetConvertFunction(target, out Action<igImageLevel, igImageLevel>? convertFunction)) return -2;
			if(convertFunction == null) return -2;

			other = new igImage2();
			other._width = _width;
			other._height = _height;
			other._depth = _depth;
			other._levelCount = _levelCount;
			other._imageCount = _imageCount;
			other._format = target;
			other._usageDeprecated = _usageDeprecated;
			other._paddingDeprecated = _paddingDeprecated;
			other._data = new igMemory<byte>(pool, target.GetTextureSize(_width, _height, _depth, _levelCount, _imageCount));
			other._lockCount = _lockCount;
			other._lockedMemory = _lockedMemory;
			other._oglDiscardOriginalImage = _oglDiscardOriginalImage;
			other._colorScale = _colorScale;
			other._colorBias = _colorBias;

#if DEBUG
			File.WriteAllBytes("dxt1.dat", _data.Buffer);
#endif // DEBUG

            byte[] imageData = _data.Buffer;
            if (_format._name.EndsWith("_tile_cafe"))
                imageData = igWiiUSwizzle.Deswizzle(imageData, _width, _height, _format._name, _levelCount);

            fixed (byte* sourcePtr = imageData, targetPtr = other._data.Buffer)
			{
				for(int i = 0; i < _levelCount; i++)
				{
					igImageLevel sourceLevel = new igImageLevel();
					sourceLevel._width = (ushort)(_width >> i);
					sourceLevel._height = (ushort)(_height >> i);
					sourceLevel._targetMeta = _format;
					sourceLevel._imageCount = _imageCount;
					sourceLevel._imageData = sourcePtr + GetTextureLevelOffset(i, 0);
					sourceLevel._levelsImageProduct = (uint)_levelCount * _imageCount;
					if(sourceLevel._width < _format.GetBlockWidth()) sourceLevel._width = _format.GetBlockWidth();
					if(sourceLevel._height < _format.GetBlockHeight()) sourceLevel._height = _format.GetBlockHeight();
					sourceLevel._imageSize = _format.GetTextureSize((int)sourceLevel._width, (int)sourceLevel._height, _depth, 1, 1);

					igImageLevel targetLevel = new igImageLevel();
					targetLevel._width = (ushort)(other._width >> i);
					targetLevel._height = (ushort)(other._height >> i);
					targetLevel._targetMeta = other._format;
					targetLevel._imageCount = other._imageCount;
					targetLevel._imageData = targetPtr + other.GetTextureLevelOffset(i, 0);
					targetLevel._levelsImageProduct = (uint)other._levelCount * other._imageCount;
					if(targetLevel._width < target.GetBlockWidth()) targetLevel._width = target.GetBlockWidth();
					if(targetLevel._height < target.GetBlockHeight()) targetLevel._height = target.GetBlockHeight();
					targetLevel._imageSize = target.GetTextureSize((int)targetLevel._width, (int)targetLevel._height, _depth, 1, 1);

					convertFunction.Invoke(sourceLevel, targetLevel);
					break;
				}
			}
			return 0;
		}
	}
}
