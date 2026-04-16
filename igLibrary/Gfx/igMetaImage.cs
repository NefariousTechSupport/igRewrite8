/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.Gfx
{
	public abstract class igMetaImage : igObject
	{
		public struct igImage2ConvertFunction
		{
			public igMetaImage _targetMeta;
			public Action<igImageLevel, igImageLevel> _function;
			public igImage2ConvertFunction(igMetaImage targetMeta, Action<igImageLevel, igImageLevel> function)
			{
				_targetMeta = targetMeta;
				_function = function;
			}
		}
		public ushort _bitsRed; // obsolete
        public ushort _bitsGreen; // obsolete
        public ushort _bitsBlue; // obsolete
        public ushort _bitsAlpha; // obsolete
		public ushort _bitsLuminance;
		public ushort _bitsIndex;
		public ushort _bitsDepth;
		public ushort _bitsStencil;
		public ushort _bitsCompressed;
		public ushort _bitsOther;
		public ushort _tileWidth = 1;
		public ushort _tileHeight = 1;
		public ushort _blockWidth = 1;
		public ushort _blockHeight = 1;
		public uint _overheadSize;
        public igMetaImage _canonical;
		public bool _isTile;
		public bool _isFloatingPoint;
		public bool _isPowerOfTwo;
		public bool _isSrgb;
		public int _minDimension;
		public int _maxDimension;
		public int _remap;
        public igMetaImageList _formats = new igMetaImageList();    //Technically an igNonRefCountedMetaImageList
        public igImage2ConvertFunctionList _functions = new igImage2ConvertFunctionList();

        // the following don't exist in ssa:
        public string _name;
        public byte _bitsPerPixel;
		public byte _properties;
		public bool _isCanonical;
		public bool _isCompressed;
		public bool _hasPalette;
		// implement these bools through igBitFieldMetaField (for trap team)
		// _isCanonical
		// _isCompressed
		// _hasPalette
		// _isSrgb
		// _isFloatingPoint

		public class igImage2ConvertFunctionList : igTDataList<igImage2ConvertFunction>{}
		
		protected static uint Align(uint val, uint align) => (val + align - 1) / align * align;
		public byte GetBlockWidth()
		{
			if (!_isCompressed) return 1;
			if (!_isCanonical) return _canonical.GetBlockWidth();
			for (int i = 0; i < igGfx.compressedInfos.Length; i++)
			{
				if (igGfx.compressedInfos[i]._name != _name) continue;
				return igGfx.compressedInfos[i]._blockWidth;
			}
			return 1;
		}
		public byte GetBlockHeight()
		{
			if (!_isCompressed) return 1;
			if (!_isCanonical) return _canonical.GetBlockHeight();
			for (int i = 0; i < igGfx.compressedInfos.Length; i++)
			{
				if (igGfx.compressedInfos[i]._name != _name) continue;
				return igGfx.compressedInfos[i]._blockHeight;
			}
			return 1;
		}
		public byte GetBitsCompressed()
		{
			if (!_isCompressed) return 1;
			if (!_isCanonical) return _canonical.GetBitsCompressed();
			for (int i = 0; i < igGfx.compressedInfos.Length; i++)
			{
				if (igGfx.compressedInfos[i]._name != _name) continue;
				return igGfx.compressedInfos[i]._bpp;
			}
			return 1;
		}
        public byte GetTileWidth()
        {
            if (!_isCanonical) return _canonical.GetTileWidth();
            for (int i = 0; i < igGfx.tileInfo.Length; i++)
            {
                if (igGfx.tileInfo[i]._name != _name) continue;
                return igGfx.tileInfo[i]._tileWidth;
            }
            return 1;
        }
        public byte GetTileHeight()
		{
			if(!_isCanonical) return _canonical.GetTileHeight();
			for(int i = 0; i < igGfx.tileInfo.Length; i++)
			{
				if(igGfx.tileInfo[i]._name != _name) continue;
				return igGfx.tileInfo[i]._tileHeight;
			}
			return 1;
		}
		// these ones are used in trap team only
        public byte GetBitsRed()
        {
            if (_isCompressed) return 0;
            if (!_isCanonical) return _canonical.GetBitsRed();
            for (int i = 0; i < igGfx.pixelInfos.Length; i++)
            {
                if (igGfx.pixelInfos[i]._name != _name) continue;
                return igGfx.pixelInfos[i]._rBits;
            }
            return 0;
        }
        public byte GetBitsGreen()
        {
            if (_isCompressed) return 0;
            if (!_isCanonical) return _canonical.GetBitsGreen();
            for (int i = 0; i < igGfx.pixelInfos.Length; i++)
            {
                if (igGfx.pixelInfos[i]._name != _name) continue;
                return igGfx.pixelInfos[i]._gBits;
            }
            return 0;
        }
        public byte GetBitsBlue()
        {
            if (_isCompressed) return 0;
            if (!_isCanonical) return _canonical.GetBitsBlue();
            for (int i = 0; i < igGfx.pixelInfos.Length; i++)
            {
                if (igGfx.pixelInfos[i]._name != _name) continue;
                return igGfx.pixelInfos[i]._bBits;
            }
            return 0;
        }
        public byte GetBitsAlpha()
        {
            if (_isCompressed) return 0;
            if (!_isCanonical) return _canonical.GetBitsAlpha();
            for (int i = 0; i < igGfx.pixelInfos.Length; i++)
            {
                if (igGfx.pixelInfos[i]._name != _name) continue;
                return igGfx.pixelInfos[i]._aBits;
            }
            return 0;
        }
        public abstract uint GetPadding(uint width);
		public uint GetTextureSize(int width, int height, int depth, int levelCount, int imageCount) => GetTextureLevelOffset(width, height, depth, levelCount, imageCount, -1, -1);
		public uint GetTextureSize(igImage2 image) => GetTextureLevelOffset(image._width, image._height, image._depth, image._levelCount, image._imageCount, -1, -1);
		public abstract uint GetTextureLevelOffset(int width, int height, int depth, int levelCount, int imageCount, int targetLevel, int imageIndex);
		public abstract uint GetPitch(uint width);
		public abstract ushort GetAlignment();
		public void AppendConvertFunction(igMetaImage target, Action<igImageLevel, igImageLevel> function)
		{
			igImage2ConvertFunction convertFunction = new igImage2ConvertFunction();
			convertFunction._targetMeta = target;
			convertFunction._function = function;
			_functions.Append(convertFunction);
		}
		public bool TryGetConvertFunction(igMetaImage target, out Action<igImageLevel, igImageLevel>? function)
		{
			if(!_isCanonical) return _canonical.TryGetConvertFunction(target, out function);
			for(int i = 0; i < _functions._count; i++)
			{
				if(target == _functions[i]._targetMeta)
				{
					function = _functions[i]._function;
					return true;
				}
			}
			function = null;
			return false;
		}
		public igMetaImage MakePlatformFormat(IG_GFX_PLATFORM platform)
		{
			if(!_isCanonical) return _canonical.MakePlatformFormat(platform);
			return igMetaImageInfo.FindFormat(_name + "_" + igGfx.GetPlatformString(platform));
		}


		/// <summary>
		/// Whether the metaimage derives from a provided metaimage
		/// </summary>
		/// <param name="canonical">The metaimage type to compare against</param>
		/// <returns>Whether the metaimage derives from the provided metaimage</returns>
		public bool IsOfType(igMetaImage? canonical)
		{
			return this == canonical ||
			       (canonical != null && !_isCanonical && _canonical.IsOfType(canonical));
		}


		/// <summary>
		/// Whether the metaimage is a swizzled wiiu texture
		/// </summary>
		/// <returns>Whether the metaimage is a swizzled wiiu texture</returns>
		public bool IsCafeSwizzled()
		{
			return this is igPlatformMetaImage platformFormat
			       && platformFormat._isTile
			       && platformFormat._platform == IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE;
		}
	}
	public class igMetaImageList : igTObjectList<igMetaImage>{}
}