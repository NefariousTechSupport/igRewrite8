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
	/// <summary>
	/// static class with graphics utilities
	/// </summary>
	public static class igGfx
	{
		/// <summary>
		/// Metadata for different compressed texture formats
		/// </summary>
		public struct igMetaImageCompressedInfo
		{
			public string _name;
			public byte _bpp;
			public byte _blockWidth;
			public byte _blockHeight;


			public igMetaImageCompressedInfo(string name, byte bpp, byte blockWidth, byte blockHeight)
			{
				_name = name;
				_bpp = bpp;
				_blockWidth = blockWidth;
				_blockHeight = blockHeight;
			}
		}


		/// <summary>
		/// Metadata for different pixel texture formats
		/// </summary>
		public struct igMetaImagePixelInfo
		{
			public string _name;
			public byte _rBits;
			public byte _gBits;
			public byte _bBits;
			public byte _aBits;


			public igMetaImagePixelInfo(string name, byte rBits, byte gBits, byte bBits, byte aBits)
			{
				_name = name;
				_rBits = rBits;
				_gBits = gBits;
				_bBits = bBits;
				_aBits = aBits;
			}
		}


		/// <summary>
		/// Metadata for uh some kind of texture format
		/// </summary>
		public struct igMetaImageLInfo
		{
			public string _name;
			public byte _lBits;


			public igMetaImageLInfo(string name, byte lBits)
			{
				_name = name;
				_lBits = lBits;
			}
		}


		/// <summary>
		/// Metadata for depth stencil texture formats
		/// </summary>
		public struct igMetaImageDepthStencilInfo
		{
			public string _name;
			public byte _depthBits;
			public byte _stencilBits;


			public igMetaImageDepthStencilInfo(string name, byte depthBits, byte stencilBits)
			{
				_name = name;
				_depthBits = depthBits;
				_stencilBits = stencilBits;
			}
		}


		/// <summary>
		/// Metadata for uh some kind of texture format
		/// </summary>
		public struct igMetaImageXInfo
		{
			public string _name;
			public byte _xBits;


			public igMetaImageXInfo(string name, byte xBits)
			{
				_name = name;
				_xBits = xBits;
			}
		}


		/// <summary>
		/// Metadata for pallette texture formats
		/// </summary>
		public struct igMetaImagePalletteInfo
		{
			public string _name;
			public ushort _unk1;
			public byte _unk2;
			public igMetaImagePalletteInfo(string name, ushort unk1, byte unk2)
			{
				_name = name;
				_unk1 = unk1;
				_unk2 = unk2;
			}
		}


		/// <summary>
		/// Metadata for uh some kind of texture format
		/// </summary>
		public struct igMetaImageTileInfo
		{
			public string _name;
			public byte _tileWidth;
			public byte _tileHeight;


			public igMetaImageTileInfo(string name, byte tileWidth, byte tileHeight)
			{
				_name = name;
				_tileWidth = tileWidth;
				_tileHeight = tileHeight;
			}
		}


		/// <summary>
		/// The list of canonical metaimages by name
		/// </summary>
		public static readonly string[] canonicalMetaImages = new string[]
		{
			"a8",
			"atitc",
			"atitc_alpha",
			"b5g5r5a1",
			"b5g6r5",
			"b8g8r8",
			"b8g8r8a8",
			"b8g8r8x8",
			"d15s1",
			"d16",
			"d24",
			"d24fs8",
			"d24s4x4",
			"d24s8",
			"d24x8",
			"d32",
			"d32f",
			"d32fs8",
			"d8",
			"dxn",
			"dxt1",
			"dxt1_srgb",
			"dxt3",
			"dxt3_srgb",
			"dxt5",
			"dxt5_srgb",
			"etc1",
			"etc2",
			"etc2_alpha",
			"g8b8",
			"gas",
			"l16",
			"l4",
			"l4a4",
			"l8",
			"l8a8",
			"p4_r4g4b4a3x1",
			"p4_r8g8b8a8",
			"p8_r4g4b4a3x1",
			"p8_r8g8b8a8",
			"pvrtc2",
			"pvrtc2_alpha",
			"pvrtc2_alpha_srgb",
			"pvrtc2_srgb",
			"pvrtc4",
			"pvrtc4_alpha",
			"pvrtc4_alpha_srgb",
			"pvrtc4_srgb",
			"r16_float",
			"r16g16",
			"r16g16_float",
			"r16g16_signed",
			"r16g16b16",
			"r16g16b16a16",
			"r16g16b16a16_expand_float",
			"r16g16b16a16_float",
			"r16g16b16x16",
			"r32_float",
			"r32g32_float",
			"r32g32b32a32_float",
			"r4g4b4a3x1",
			"r4g4b4a4",
			"r5g5b5a1",
			"r5g6b5",
			"r6g6b6a6",
			"r8g8",
			"r8g8b8",
			"r8g8b8_framebuffer",
			"r8g8b8_srgb",
			"r8g8b8a8",
			"r8g8b8a8_srgb",
			"r8g8b8x8",
			"r8g8b8x8_srgb",
			"shadow"
		};


		/// <summary>
		/// The list of compressed texture formats
		/// </summary>
		public static readonly igMetaImageCompressedInfo[] compressedInfos = new igMetaImageCompressedInfo[]
		{
			new igMetaImageCompressedInfo(            "atitc", 0x04, 0x04, 0x04),
			new igMetaImageCompressedInfo(      "atitc_alpha", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(              "dxn", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(             "dxt1", 0x04, 0x04, 0x04),
			new igMetaImageCompressedInfo(        "dxt1_srgb", 0x04, 0x04, 0x04),
			new igMetaImageCompressedInfo(             "dxt3", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(        "dxt3_srgb", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(             "dxt5", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(        "dxt5_srgb", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(             "etc1", 0x04, 0x04, 0x04),
			new igMetaImageCompressedInfo(             "etc2", 0x04, 0x04, 0x04),
			new igMetaImageCompressedInfo(       "etc2_alpha", 0x08, 0x04, 0x04),
			new igMetaImageCompressedInfo(           "pvrtc2", 0x02, 0x10, 0x08),
			new igMetaImageCompressedInfo(     "pvrtc2_alpha", 0x02, 0x10, 0x08),
			new igMetaImageCompressedInfo("pvrtc2_alpha_srgb", 0x02, 0x10, 0x08),
			new igMetaImageCompressedInfo(      "pvrtc2_srgb", 0x02, 0x10, 0x08),
			new igMetaImageCompressedInfo(           "pvrtc4", 0x04, 0x08, 0x08),
			new igMetaImageCompressedInfo(     "pvrtc4_alpha", 0x04, 0x08, 0x08),
			new igMetaImageCompressedInfo("pvrtc4_alpha_srgb", 0x04, 0x08, 0x08),
			new igMetaImageCompressedInfo(      "pvrtc4_srgb", 0x04, 0x08, 0x08)
		};


		/// <summary>
		/// The list of pixel texture formats
		/// </summary>
		public static readonly igMetaImagePixelInfo[] pixelInfos = new igMetaImagePixelInfo[]
		{
			new igMetaImagePixelInfo(                       "a8", 0x00, 0x00, 0x00, 0x08),
			new igMetaImagePixelInfo(                 "b5g5r5a1", 0x05, 0x05, 0x05, 0x01),
			new igMetaImagePixelInfo(                   "b5g6r5", 0x05, 0x06, 0x05, 0x00),
			new igMetaImagePixelInfo(                   "b8g8r8", 0x08, 0x08, 0x08, 0x00),
			new igMetaImagePixelInfo(                 "b8g8r8a8", 0x08, 0x08, 0x08, 0x08),
			new igMetaImagePixelInfo(                 "b8g8r8x8", 0x08, 0x08, 0x08, 0x00),
			new igMetaImagePixelInfo(                      "dxn", 0x08, 0x08, 0x00, 0x00),
			new igMetaImagePixelInfo(                     "dxt1", 0x05, 0x06, 0x05, 0x01),
			new igMetaImagePixelInfo(                "dxt1_srgb", 0x05, 0x06, 0x05, 0x01),
			new igMetaImagePixelInfo(                     "dxt3", 0x05, 0x06, 0x05, 0x04),
			new igMetaImagePixelInfo(                "dxt3_srgb", 0x05, 0x06, 0x05, 0x04),
			new igMetaImagePixelInfo(                     "dxt5", 0x05, 0x06, 0x05, 0x04),
			new igMetaImagePixelInfo(                "dxt5_srgb", 0x05, 0x06, 0x05, 0x04),
			new igMetaImagePixelInfo(                     "g8b8", 0x00, 0x08, 0x08, 0x00),
			new igMetaImagePixelInfo(                      "gas", 0x08, 0x08, 0x08, 0x08),
			new igMetaImagePixelInfo(                     "l4a4", 0x00, 0x00, 0x00, 0x04),
			new igMetaImagePixelInfo(                     "l8a8", 0x00, 0x00, 0x00, 0x08),
			new igMetaImagePixelInfo(            "p4_r4g4b4a3x1", 0x04, 0x04, 0x04, 0x03),
			new igMetaImagePixelInfo(              "p4_r8g8b8a8", 0x08, 0x08, 0x08, 0x08),
			new igMetaImagePixelInfo(            "p8_r4g4b4a3x1", 0x04, 0x04, 0x04, 0x03),
			new igMetaImagePixelInfo(              "p8_r8g8b8a8", 0x08, 0x08, 0x08, 0x08),
			new igMetaImagePixelInfo(                "r16_float", 0x10, 0x00, 0x00, 0x00),
			new igMetaImagePixelInfo(                   "r16g16", 0x10, 0x10, 0x00, 0x00),
			new igMetaImagePixelInfo(             "r16g16_float", 0x10, 0x10, 0x00, 0x00),
			new igMetaImagePixelInfo(            "r16g16_signed", 0x10, 0x10, 0x00, 0x00),
			new igMetaImagePixelInfo(                "r16g16b16", 0x10, 0x10, 0x10, 0x00),
			new igMetaImagePixelInfo(             "r16g16b16a16", 0x10, 0x10, 0x10, 0x10),
			new igMetaImagePixelInfo("r16g16b16a16_expand_float", 0x10, 0x10, 0x10, 0x10),
			new igMetaImagePixelInfo(       "r16g16b16a16_float", 0x10, 0x10, 0x10, 0x10),
			new igMetaImagePixelInfo(       "r16g16b16x16_float", 0x10, 0x10, 0x10, 0x00),
			new igMetaImagePixelInfo(                "r32_float", 0x20, 0x00, 0x00, 0x00),
			new igMetaImagePixelInfo(             "r32g32_float", 0x20, 0x20, 0x00, 0x00),
			new igMetaImagePixelInfo(       "r32g32b32a32_float", 0x20, 0x20, 0x20, 0x20),
			new igMetaImagePixelInfo(               "r4g4b4a3x1", 0x04, 0x04, 0x04, 0x03),
			new igMetaImagePixelInfo(                 "r5g5b5a1", 0x05, 0x05, 0x05, 0x01),
			new igMetaImagePixelInfo(                   "r5g6b5", 0x05, 0x06, 0x05, 0x00),
			new igMetaImagePixelInfo(                 "r6g6b6a6", 0x06, 0x06, 0x06, 0x06),
			new igMetaImagePixelInfo(                     "r8g8", 0x08, 0x08, 0x00, 0x00),
			new igMetaImagePixelInfo(                   "r8g8b8", 0x08, 0x08, 0x08, 0x00),
			new igMetaImagePixelInfo(       "r8g8b8_framebuffer", 0x08, 0x08, 0x08, 0x00),
			new igMetaImagePixelInfo(                 "r8g8b8a8", 0x08, 0x08, 0x08, 0x08),
			new igMetaImagePixelInfo(            "r8g8b8a8_srgb", 0x08, 0x08, 0x08, 0x08),
			new igMetaImagePixelInfo(                 "r8g8b8x8", 0x08, 0x08, 0x08, 0x00),
			new igMetaImagePixelInfo(                   "shadow", 0x08, 0x08, 0x08, 0x08)
		};


		/// <summary>
		/// The list of some kind of texture format
		/// </summary>
		public static readonly igMetaImageLInfo[] lInfo = new igMetaImageLInfo[]
		{
			new igMetaImageLInfo( "l16", 0x10),
			new igMetaImageLInfo(  "l4", 0x04),
			new igMetaImageLInfo("l4a4", 0x04),
			new igMetaImageLInfo(  "l8", 0x08),
			new igMetaImageLInfo("l8a8", 0x08)
		};


		/// <summary>
		/// The list for depth stencil texture formats
		/// </summary>
		public static readonly igMetaImageDepthStencilInfo[] dsInfo = new igMetaImageDepthStencilInfo[]
		{
			new igMetaImageDepthStencilInfo(  "d15s1", 0x0F, 0x01),
			new igMetaImageDepthStencilInfo(    "d16", 0x10, 0x00),
			new igMetaImageDepthStencilInfo(    "d24", 0x18, 0x00),
			new igMetaImageDepthStencilInfo( "d24fs8", 0x18, 0x08),
			new igMetaImageDepthStencilInfo("d24s4x8", 0x18, 0x04),
			new igMetaImageDepthStencilInfo(  "d24s8", 0x18, 0x08),
			new igMetaImageDepthStencilInfo(  "d24x8", 0x18, 0x00),
			new igMetaImageDepthStencilInfo(    "d32", 0x20, 0x00),
			new igMetaImageDepthStencilInfo(   "d32f", 0x20, 0x00),
			new igMetaImageDepthStencilInfo( "d32fs8", 0x20, 0x08),
			new igMetaImageDepthStencilInfo(     "d8", 0x08, 0x00),
		};


		/// <summary>
		/// The list of some kind of texture format
		/// </summary>
		public static readonly igMetaImageXInfo[] xInfo = new igMetaImageXInfo[]
		{
			new igMetaImageXInfo(     "b8g8r8x8", 0x10),
			new igMetaImageXInfo(      "d24s4x4", 0x04),
			new igMetaImageXInfo(        "d24x8", 0x08),
			new igMetaImageXInfo("p4_r4g4b4a3x1", 0x01),
			new igMetaImageXInfo("p8_r4g4b4a3x1", 0x01),
			new igMetaImageXInfo( "r16g16b16x16", 0x10),
			new igMetaImageXInfo(   "r4g4b4a3x1", 0x01),
			new igMetaImageXInfo(     "r8g8b8x8", 0x08),
			new igMetaImageXInfo("r8g8b8x8_srgb", 0x08)
		};


		/// <summary>
		/// The list of pallette texture formats
		/// </summary>
		public static readonly igMetaImagePalletteInfo[] palletteInfo = new igMetaImagePalletteInfo[]
		{
			new igMetaImagePalletteInfo("p4_r4g4b4a3x1", 0x0020, 0x04),
			new igMetaImagePalletteInfo(  "p4_r8g8b8a8", 0x0040, 0x04),
			new igMetaImagePalletteInfo("p8_r4g4b4a3x1", 0x0200, 0x08),
			new igMetaImagePalletteInfo(  "p8_r8g8b8a8", 0x0400, 0x08)
		};


		/// <summary>
		/// The list of some kind of texture format
		/// </summary>
		public static readonly igMetaImageTileInfo[] tileInfo = new igMetaImageTileInfo[]
		{
			new igMetaImageTileInfo(         "a4l4_tile_big_wii", 0x08, 0x04),
			new igMetaImageTileInfo(   "b4g4r4a3x1_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo(       "b5g6r5_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo(     "b6g6r6a6_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo(       "b8g8r8_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo(     "b8g8r8a8_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo(         "dxt1_tile_big_wii", 0x08, 0x08),
			new igMetaImageTileInfo(           "l4_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo(           "l8_tile_big_wii", 0x08, 0x04),
			new igMetaImageTileInfo(         "l8a8_tile_big_wii", 0x04, 0x04),
			new igMetaImageTileInfo("p4_b4g4r4a3x1_tile_big_wii", 0x08, 0x08),
			new igMetaImageTileInfo("p8_b4g4r4a3x1_tile_big_wii", 0x08, 0x04)
		};


		/// <summary>
		/// Initialize igGfx, must be called on boot
		/// </summary>
		public static void Initialize()
		{
			InitializeIndexFormats();
			InitializeVertexFormatPlatforms();
			InitializeMetaImages();
		}


		/// <summary>
		/// Initialize all index formats
		/// </summary>
		private static void InitializeIndexFormats()
		{
			igObjectDirectory indexformats = new igObjectDirectory();
			indexformats._name = new igName(igIndexFormat._indexFormatNamespace);
			indexformats._useNameList = true;
			indexformats._nameList = new igNameList();
			igObjectStreamManager.Singleton.AddObjectDirectory(indexformats, indexformats._name._string);
			igObjectHandleManager.Singleton.AddSystemNamespace(igIndexFormat._indexFormatNamespace);

			for(IG_INDEX_TYPE indexType = IG_INDEX_TYPE.IG_INDEX_TYPE_INT8; indexType <= IG_INDEX_TYPE.IG_INDEX_TYPE_INT32; indexType++)
			{
				if(indexType == (IG_INDEX_TYPE)3) continue;

				for(IG_GFX_PLATFORM platform = IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT; platform < IG_GFX_PLATFORM.IG_GFX_PLATFORM_MAX; platform++)
				{
					igIndexFormat indexFormat = igIndexFormat.CreateIndexFormat(indexType, platform, false);
					indexformats.AddObject(indexFormat, default(igName), new igName(igIndexFormat.GetFormatName(indexType, platform, false)));
					igIndexFormat._indexFormats.Append(indexFormat);

					indexFormat = igIndexFormat.CreateIndexFormat(indexType, platform, true);
					indexformats.AddObject(indexFormat, default(igName), new igName(igIndexFormat.GetFormatName(indexType, platform, true)));
					igIndexFormat._indexFormats.Append(indexFormat);
				}
			}
		}


		/// <summary>
		/// Initialize all vertex formats
		/// </summary>
		private static void InitializeVertexFormatPlatforms()
		{
			igObjectDirectory vertexformat = new igObjectDirectory();
			vertexformat._name = new igName("vertexformat");
			vertexformat._useNameList = true;
			vertexformat._nameList = new igNameList();
			igObjectStreamManager.Singleton.AddObjectDirectory(vertexformat, vertexformat._name._string);
			igObjectHandleManager.Singleton.AddSystemNamespace("vertexformat");

			AddVertexFormatPlatform<igVertexFormatAspen>  (vertexformat);
			AddVertexFormatPlatform<igVertexFormatCafe>   (vertexformat);
			AddVertexFormatPlatform<igVertexFormatDurango>(vertexformat);
			AddVertexFormatPlatform<igVertexFormatDX>     (vertexformat);
			AddVertexFormatPlatform<igVertexFormatMetal>  (vertexformat);
			AddVertexFormatPlatform<igVertexFormatPS3>    (vertexformat);
			AddVertexFormatPlatform<igVertexFormatWii>    (vertexformat);
			AddVertexFormatPlatform<igVertexFormatXenon>  (vertexformat);
			AddVertexFormatPlatform<igVertexFormatOSX>    (vertexformat);
			AddVertexFormatPlatform<igVertexFormatDX11>   (vertexformat);
			AddVertexFormatPlatform<igVertexFormatRaspi>  (vertexformat);
			AddVertexFormatPlatform<igVertexFormatNull>   (vertexformat);
			AddVertexFormatPlatform<igVertexFormatAndroid>(vertexformat);
			AddVertexFormatPlatform<igVertexFormatWgl>    (vertexformat);
			AddVertexFormatPlatform<igVertexFormatLGTV>   (vertexformat);
			AddVertexFormatPlatform<igVertexFormatPS4>    (vertexformat);
		}


		/// <summary>
		/// Add the vertex format for a given platform
		/// </summary>
		private static void AddVertexFormatPlatform<T>(igObjectDirectory vertexformat) where T : igVertexFormatPlatform, new()
		{
			// I'm aware that it's igVertexFormatPlatform and not new T(), this is because there's a lack of metadata
			// It's easier to just lie
			vertexformat.AddObject(new igVertexFormatPlatform(), default(igName), new igName(typeof(T).Name));
		}


		/// <summary>
		/// Initialise all the vertex blenders
		/// </summary>
		public static void InitializeVertexBlenders()
		{
			igObjectDirectory vertexblender = new igObjectDirectory();
			vertexblender._name = new igName("vertexblender");
			vertexblender._useNameList = true;
			vertexblender._nameList = new igNameList();
			igObjectStreamManager.Singleton.AddObjectDirectory(vertexblender, vertexblender._name._string);
			igObjectHandleManager.Singleton.AddSystemNamespace("vertexblender");

			AddVertexBlender<igVertexBlenderDefault>(vertexblender);
		}


		/// <summary>
		/// Add a specific vertex blender
		/// </summary>
		/// <typeparam name="T">The blender type</typeparam>
		/// <param name="vertexblender">The directory</param>
		private static void AddVertexBlender<T>(igObjectDirectory vertexblender) where T : igVertexBlender, new()
		{
			// I'm aware that it's igVertexBlender and not new T(), this is because there's a lack of metadata
			// It's easier to just lie
			vertexblender.AddObject(new T(), default(igName), new igName(typeof(T).Name.Substring(15)));
		}


		/// <summary>
		/// Initialize all metaimages
		/// </summary>
		public static void InitializeMetaImages()
		{
			igObjectDirectory metaimages = new igObjectDirectory();
			metaimages._name = new igName("metaimages");
			metaimages._useNameList = true;
			metaimages._nameList = new igNameList();
			igObjectStreamManager.Singleton.AddObjectDirectory(metaimages, metaimages._name._string);
			igObjectHandleManager.Singleton.AddSystemNamespace("metaimages");

			for(int i = 0; i < canonicalMetaImages.Length; i++)
			{
				igCanonicalMetaImage canonMeta = new igCanonicalMetaImage();
				canonMeta._canonical = canonMeta;
				canonMeta._isCanonical = true;
				canonMeta._name = canonicalMetaImages[i];
				igMetaImageInfo.RegisterFormat(canonMeta);

				canonMeta._bitsPerPixel = 0;
				int pixelIndex = Array.FindIndex<igMetaImagePixelInfo>(pixelInfos, x => x._name == canonMeta._name);
				if(pixelIndex >= 0)
				{
					canonMeta._bitsPerPixel = (byte)(pixelInfos[pixelIndex]._rBits + pixelInfos[pixelIndex]._gBits + pixelInfos[pixelIndex]._bBits + pixelInfos[pixelIndex]._aBits);
				}
				int lIndex = Array.FindIndex<igMetaImageLInfo>(lInfo, x => x._name == canonMeta._name);
				if(lIndex >= 0)
				{
					canonMeta._bitsPerPixel += lInfo[lIndex]._lBits;
				}
				int dsIndex = Array.FindIndex<igMetaImageDepthStencilInfo>(dsInfo, x => x._name == canonMeta._name);
				if(dsIndex >= 0)
				{
					canonMeta._bitsPerPixel += (byte)(dsInfo[dsIndex]._depthBits + dsInfo[dsIndex]._stencilBits);
				}
				int xIndex = Array.FindIndex<igMetaImageXInfo>(xInfo, x => x._name == canonMeta._name);
				if(xIndex >= 0)
				{
					canonMeta._bitsPerPixel += xInfo[xIndex]._xBits;
				}

				int palletteIndex = Array.FindIndex<igMetaImagePalletteInfo>(palletteInfo, x => x._name == canonMeta._name);
				if(palletteIndex >= 0)
				{
					canonMeta._hasPalette = true;
				}

				int compressedIndex = Array.FindIndex<igMetaImageCompressedInfo>(compressedInfos, x => x._name == canonMeta._name);
				if(compressedIndex >= 0)
				{
					canonMeta._isCompressed = true;
				}

				if(canonMeta._name.EndsWith("_srgb")) canonMeta._isSrgb = true;
				else
				{
					//was quicker to write than a whole lot of if-elses
					switch(canonMeta._name)
					{
						case "d24fs8":
						case "d32f":
						case "d32fs8":
						case "r16_float":
						case "r16g16_float":
						case "r16g16b16a16_expand_float":
						case "r16g16b16a16_float":
						case "r32_float":
						case "r32g32_float":
						case "r32g32b32a32_float":
							canonMeta._isFloatingPoint = true;
							break;
					}
				}
				for(uint p = (uint)IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT + 1; p < (uint)IG_GFX_PLATFORM.IG_GFX_PLATFORM_MAX; p++)
				{
					string? platformString = GetPlatformString((IG_GFX_PLATFORM)p);
					if(platformString == null) continue;
					igPlatformMetaImage platformMeta = new igPlatformMetaImage();
					platformMeta._properties = canonMeta._properties;
					platformMeta._name = canonMeta._name + "_" + platformString;
					platformMeta._isCanonical = false;
					platformMeta._canonical = canonMeta;
					platformMeta._bitsPerPixel = canonMeta._bitsPerPixel;
					platformMeta._functions = canonMeta._functions;
					platformMeta._platform = (IG_GFX_PLATFORM)p;
					igPlatformMetaImage platformTileMeta = new igPlatformMetaImage();
					platformTileMeta._properties = canonMeta._properties;
					platformTileMeta._isTile = true;
					platformTileMeta._isCanonical = false;
					platformTileMeta._name = canonMeta._name + "_tile_" + platformString;
					platformTileMeta._canonical = canonMeta;
					platformTileMeta._bitsPerPixel = canonMeta._bitsPerPixel;
					platformTileMeta._functions = canonMeta._functions;
					platformTileMeta._platform = (IG_GFX_PLATFORM)p;
					canonMeta._formats.Append(platformMeta);
					canonMeta._formats.Append(platformTileMeta);
					igMetaImageInfo.RegisterFormat(platformMeta);
					igMetaImageInfo.RegisterFormat(platformTileMeta);
				}
			}

			igMetaImageInfo.Debug();
			igImagePlugin.RegisterPlugin();
		}


		/// <summary>
		/// Determine if a gfx platform is little endian or not
		/// </summary>
		/// <param name="platform">the gfx platform</param>
		/// <returns>whether the platform is little endian</returns>
		public static bool IsPlatformLittleEndian(IG_GFX_PLATFORM platform)
		{
			switch(platform)
			{
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
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS4:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LINUX:
					return true;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_XENON:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE:
				default:
					return false;
			}
		}


		/// <summary>
		/// Translates the core platform to its gfx equivalent
		/// </summary>
		/// <param name="platform">The core platform</param>
		/// <returns>The gfx platform</returns>
		public static IG_GFX_PLATFORM GetGfxPlatformFromCore(IG_CORE_PLATFORM platform)
		{
			switch(platform)
			{
				default:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_DURANGO;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_ASPEN;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_XENON;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_OSX;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX11;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_NGP:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_NULL;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_MARMALADE:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_NULL;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_RASPI;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_ANDROID;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_METAL;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_LGTV;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS4;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_NULL;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX:
					return IG_GFX_PLATFORM.IG_GFX_PLATFORM_LINUX;
			}
		}


		/// <summary>
		/// Gets the string representation of that platform
		/// </summary>
		/// <param name="platform">The gfx platform</param>
		/// <returns>The name for that platform</returns>
		public static string GetPlatformString(IG_GFX_PLATFORM platform)
		{
			switch(platform)
			{
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_NULL:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_MAX:
				default:
					return null;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX:
					return "dx";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII:
					return "big_wii";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DURANGO:
					return "durango";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ASPEN:
					return "aspen";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_XENON:
					return "big_xenon";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
					return "big_ps3";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_OSX:
					return "osx";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX11:
					return "dx11";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE:
					return "cafe";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_RASPI:
					return "raspi";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ANDROID:
					return "android";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_METAL:
					return "metal";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WGL:
					return "wgl";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LGTV:
					return "lgtv";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS4:
					return "ps4";
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_LINUX:
					return "linux";
			}
		}


		/// <summary>
		/// Should you endian swap between two different image levels
		/// </summary>
		/// <param name="source">The source image level</param>
		/// <param name="target">The target image level</param>
		/// <returns>Whether an endian swap is needed</returns>
		public static bool ShouldEndianSwap(igImageLevel source, igImageLevel target)
		{
			bool sourceEndian = false;
			bool targetEndian = false;
			if(source._targetMeta is igPlatformMetaImage platSource) sourceEndian = IsPlatformLittleEndian(platSource._platform);
			if(target._targetMeta is igPlatformMetaImage targSource) targetEndian = IsPlatformLittleEndian(targSource._platform);
			return sourceEndian ^ targetEndian;
		}

#region Image conversion

		public static unsafe void Convert_r8g8b8a8_to_b8g8r8a8(igImageLevel source, igImageLevel target)
		{
			for(uint i = 0; i < source._imageSize; i += 4)
			{
				target._imageData[i + 0] = source._imageData[i + 2];
				target._imageData[i + 1] = source._imageData[i + 1];
				target._imageData[i + 2] = source._imageData[i + 0];
				target._imageData[i + 3] = source._imageData[i + 3];
			}
		}


		public static unsafe void Convert_b8g8r8a8_to_r8g8b8a8(igImageLevel source, igImageLevel target)
		{
			for(uint i = 0; i < source._imageSize; i += 4)
			{
				target._imageData[i + 0] = source._imageData[i + 1];
				target._imageData[i + 1] = source._imageData[i + 2];
				target._imageData[i + 2] = source._imageData[i + 3];
				target._imageData[i + 3] = source._imageData[i + 0];
			}
		}


		public static unsafe void Convert_r5g5b5a1_to_r8g8b8a8(igImageLevel source, igImageLevel target)
		{
			bool sourceEndian = false;
			if(source._targetMeta is igPlatformMetaImage platSource) sourceEndian = IsPlatformLittleEndian(platSource._platform);
			bool targetEndian = false;
			if(target._targetMeta is igPlatformMetaImage targSource) targetEndian = IsPlatformLittleEndian(targSource._platform);

			for(uint i = 0, j = 0; i < source._imageSize; i += 2, j += 4)
			{
				ushort rgba;
				if(sourceEndian)
				{
					rgba = (ushort)((source._imageData[i + 1] << 8) | source._imageData[i + 0]);
				}
				else
				{
					rgba = (ushort)((source._imageData[i + 0] << 8) | source._imageData[i + 1]);
				}
				target._imageData[i + 0] = (byte)(((rgba >> 11) & 0x1F) << 3);
				target._imageData[i + 1] = (byte)(((rgba >> 06) & 0x1F) << 3);
				target._imageData[i + 2] = (byte)(((rgba >> 01) & 0x1F) << 3);
				target._imageData[i + 3] = (byte)(((rgba >> 00) & 0x01) << 7);
			}
		}


		public static unsafe void Convert_r5g6b5_to_r8g8b8a8(igImageLevel source, igImageLevel target)
		{
			bool sourceEndian = false;
			if(source._targetMeta is igPlatformMetaImage platSource) sourceEndian = IsPlatformLittleEndian(platSource._platform);
			bool targetEndian = false;
			if(target._targetMeta is igPlatformMetaImage targSource) targetEndian = IsPlatformLittleEndian(targSource._platform);

			for(uint i = 0, j = 0; i < source._imageSize; i += 2, j += 4)
			{
				ushort rgba;
				if(sourceEndian)
				{
					rgba = (ushort)((source._imageData[i + 1] << 8) | source._imageData[i + 0]);
				}
				else
				{
					rgba = (ushort)((source._imageData[i + 0] << 8) | source._imageData[i + 1]);
				}
				target._imageData[i + 0] = (byte)(((rgba >> 11) & 0x1F) << 3);
				target._imageData[i + 1] = (byte)(((rgba >> 05) & 0x1F) << 2);
				target._imageData[i + 2] = (byte)(((rgba >> 00) & 0x1F) << 3);
				target._imageData[i + 3] = 0xFF;
			}
		}


		public static unsafe void Convert_a8_to_r8g8b8a8(igImageLevel source, igImageLevel target)
		{
			for(uint i = 0; i < source._imageSize; i++)
			{
				target._imageData[(i << 2) + 0] = 0;
				target._imageData[(i << 2) + 1] = 0;
				target._imageData[(i << 2) + 2] = 0;
				target._imageData[(i << 2) + 3] = source._imageData[i];
			}
		}


		public static unsafe void Convert_dxt1_to_r8g8b8a8(igImageLevel source, igImageLevel target)
		{
			uint numBlocks = source._imageSize / 0x08;
			uint blockWidth = (source._width + 3) >> 2;
			uint blockHeight = (source._height + 3) >> 2;
			byte* sourceData = source._imageData;
			for(uint i = 0; i < numBlocks; i++, sourceData += 0x08)
			{
				ushort col0 = (ushort)((sourceData[0x01] << 8) | (sourceData[0x00]));
				ushort col1 = (ushort)((sourceData[0x03] << 8) | (sourceData[0x02]));
				uint codes = (uint)((sourceData[0x04] << 24) | (sourceData[0x05] << 16) | (sourceData[0x06] << 8) | sourceData[0x07]);
				byte r0 = (byte)(((col0 >> 11) & 0x1F) << 3);
				byte r1 = (byte)(((col1 >> 11) & 0x1F) << 3);
				byte g0 = (byte)(((col0 >>  5) & 0x3F) << 2);
				byte g1 = (byte)(((col1 >>  5) & 0x3F) << 2);
				byte b0 = (byte)((col0 & 0x1F) << 3);
				byte b1 = (byte)((col1 & 0x1F) << 3);
				uint pixelX = (i % blockWidth) << 2;	//Multiply by 4 cos block size
				uint pixelY = (i / blockWidth) << 2;
				for(uint j = 0; j < 16; j++)
				{
					uint pixelOffset = ((pixelY + 3 - (j >> 2)) * target._width + pixelX + (j & 3)) << 2;	//Multiply by 4 cos r8g8b8a8 pixel size
					byte code = (byte)((codes >> (int)(j * 2)) & 0x03);
					//Could potentially cause an IndexOutOfBoundsException when the target width or height is not a multiple of 4
					if(col0 > col1) switch(code)
					{
						case 0:
							target._imageData[pixelOffset + 0] = r0;
							target._imageData[pixelOffset + 1] = g0;
							target._imageData[pixelOffset + 2] = b0;
							break;
						case 1:
							target._imageData[pixelOffset + 0] = r1;
							target._imageData[pixelOffset + 1] = g1;
							target._imageData[pixelOffset + 2] = b1;
							break;
						case 2:
							target._imageData[pixelOffset + 0] = (byte)((2 * r0 + r1) / 3);
							target._imageData[pixelOffset + 1] = (byte)((2 * g0 + g1) / 3);
							target._imageData[pixelOffset + 2] = (byte)((2 * b0 + b1) / 3);
							break;
						case 3:
							target._imageData[pixelOffset + 0] = (byte)((r0 + 2 * r1) / 3);
							target._imageData[pixelOffset + 1] = (byte)((g0 + 2 * g1) / 3);
							target._imageData[pixelOffset + 2] = (byte)((b0 + 2 * b1) / 3);
							break;
					}
					else switch(code)
					{
						case 0:
							target._imageData[pixelOffset + 0] = r0;
							target._imageData[pixelOffset + 1] = g0;
							target._imageData[pixelOffset + 2] = b0;
							break;
						case 1:
							target._imageData[pixelOffset + 0] = r1;
							target._imageData[pixelOffset + 1] = g1;
							target._imageData[pixelOffset + 2] = b1;
							break;
						case 2:
							target._imageData[pixelOffset + 0] = (byte)((r0 + r1) / 2);
							target._imageData[pixelOffset + 1] = (byte)((g0 + g1) / 2);
							target._imageData[pixelOffset + 2] = (byte)((b0 + b1) / 2);
							break;
						case 3:
							target._imageData[pixelOffset + 0] = 0;
							target._imageData[pixelOffset + 1] = 0;
							target._imageData[pixelOffset + 2] = 0;
							break;
					}

					target._imageData[pixelOffset + 3] = 0xFF;
				}
			}
		}


		public static unsafe void Convert_dxt5_to_r8g8b8a8(igImageLevel source, igImageLevel target)
		{
			uint numBlocks = source._imageSize / 0x10;
			uint blockWidth = (source._width + 3) >> 2;
			uint blockHeight = (source._height + 3) >> 2;
			byte* sourceData = source._imageData;
			for(uint i = 0; i < numBlocks; i++, sourceData += 0x10)
			{
				ushort col0  = (ushort)((sourceData[0x09] << 8) | (sourceData[0x08]));
				ushort col1  = (ushort)((sourceData[0x0B] << 8) | (sourceData[0x0A]));
				uint   codes = (uint)((sourceData[0x0F] << 24) | (sourceData[0x0E] << 16) | (sourceData[0x0D] << 8) | sourceData[0x0C]);
				byte   r0    = (byte)(((col0 >> 11) & 0x1F) << 3);
				byte   r1    = (byte)(((col1 >> 11) & 0x1F) << 3);
				byte   g0    = (byte)(((col0 >>  5) & 0x3F) << 2);
				byte   g1    = (byte)(((col1 >>  5) & 0x3F) << 2);
				byte   b0    = (byte)((col0 & 0x1F) << 3);
				byte   b1    = (byte)((col1 & 0x1F) << 3);

				byte   a0    = sourceData[0x00];
				byte   a1    = sourceData[0x01];
				ulong  aCodes= ((ulong)sourceData[0x07] << 40) | ((ulong)sourceData[0x06] << 32) | ((uint)sourceData[0x05] << 24) | ((uint)sourceData[0x04] << 16) | ((uint)sourceData[0x03] << 8) | sourceData[0x02];

				//Multiply by 4 cos block size
				uint pixelX = (i % blockWidth) << 2;
				uint pixelY = (i / blockWidth) << 2;

				for(uint j = 0; j < 16; j++)
				{
					uint pixelOffset = ((pixelY + (j >> 2)) * target._width + pixelX + (j & 3)) << 2;	//Multiply by 4 cos r8g8b8a8 pixel size
					byte code = (byte)((codes >> (int)(j * 2)) & 0x03);
					//Could potentially cause an IndexOutOfBoundsException when the target width or height is not a multiple of 4
					if(col0 > col1) switch(code)
					{
						case 0:
							target._imageData[pixelOffset + 0] = r0;
							target._imageData[pixelOffset + 1] = g0;
							target._imageData[pixelOffset + 2] = b0;
							break;
						case 1:
							target._imageData[pixelOffset + 0] = r1;
							target._imageData[pixelOffset + 1] = g1;
							target._imageData[pixelOffset + 2] = b1;
							break;
						case 2:
							target._imageData[pixelOffset + 0] = (byte)((2 * r0 + r1) / 3);
							target._imageData[pixelOffset + 1] = (byte)((2 * g0 + g1) / 3);
							target._imageData[pixelOffset + 2] = (byte)((2 * b0 + b1) / 3);
							break;
						case 3:
							target._imageData[pixelOffset + 0] = (byte)((r0 + 2 * r1) / 3);
							target._imageData[pixelOffset + 1] = (byte)((g0 + 2 * g1) / 3);
							target._imageData[pixelOffset + 2] = (byte)((b0 + 2 * b1) / 3);
							break;
					}
					else switch(code)
					{
						case 0:
							target._imageData[pixelOffset + 0] = r0;
							target._imageData[pixelOffset + 1] = g0;
							target._imageData[pixelOffset + 2] = b0;
							break;
						case 1:
							target._imageData[pixelOffset + 0] = r1;
							target._imageData[pixelOffset + 1] = g1;
							target._imageData[pixelOffset + 2] = b1;
							break;
						case 2:
							target._imageData[pixelOffset + 0] = (byte)((r0 + r1) / 2);
							target._imageData[pixelOffset + 1] = (byte)((g0 + g1) / 2);
							target._imageData[pixelOffset + 2] = (byte)((b0 + b1) / 2);
							break;
						case 3:
							target._imageData[pixelOffset + 0] = 0;
							target._imageData[pixelOffset + 1] = 0;
							target._imageData[pixelOffset + 2] = 0;
							break;
					}
				}
				for(uint j = 0; j < 16; j++)
				{
					uint pixelOffset = ((pixelY + (j >> 2)) * target._width + pixelX + (j & 3)) << 2;	//Multiply by 4 cos r8g8b8a8 pixel size
					byte code = (byte)((aCodes >> (int)(j * 3)) & 0x07);

					//Could potentially cause an IndexOutOfBoundsException when the target width or height is not a multiple of 4
					if(a0 > a1) switch(code)
					{
						case 0:
							target._imageData[pixelOffset + 3] = a0;
							break;
						case 1:
							target._imageData[pixelOffset + 3] = a1;
							break;
						case 2:
							target._imageData[pixelOffset + 3] = unchecked((byte)((6u*a0 +    a1) / 7u));
							break;
						case 3:
							target._imageData[pixelOffset + 3] = unchecked((byte)((5u*a0 + 2u*a1) / 7u));
							break;
						case 4:
							target._imageData[pixelOffset + 3] = unchecked((byte)((4u*a0 + 3u*a1) / 7u));
							break;
						case 5:
							target._imageData[pixelOffset + 3] = unchecked((byte)((3u*a0 + 4u*a1) / 7u));
							break;
						case 6:
							target._imageData[pixelOffset + 3] = unchecked((byte)((2u*a0 + 5u*a1) / 7u));
							break;
						case 7:
							target._imageData[pixelOffset + 3] = unchecked((byte)((   a0 + 7u*a1) / 7u));
							break;
					}
					else switch(code)
					{
						case 0:
							target._imageData[pixelOffset + 3] = a0;
							break;
						case 1:
							target._imageData[pixelOffset + 3] = a1;
							break;
						case 2:
							target._imageData[pixelOffset + 3] = unchecked((byte)((4u*a0 +    a1) / 5u));
							break;
						case 3:
							target._imageData[pixelOffset + 3] = unchecked((byte)((3u*a0 + 2u*a1) / 5u));
							break;
						case 4:
							target._imageData[pixelOffset + 3] = unchecked((byte)((2u*a0 + 3u*a1) / 5u));
							break;
						case 5:
							target._imageData[pixelOffset + 3] = unchecked((byte)((1u*a0 + 4u*a1) / 5u));
							break;
						case 6:
							target._imageData[pixelOffset + 3] = 0;
							break;
						case 7:
							target._imageData[pixelOffset + 3] = 0xFF;
							break;
					}
				}
			}
		}
#endregion Image conversion
	}
}