/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;
using igLibrary.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace igCauldron3.Graphics
{
	/// <summary>
	/// OpenGL graphics stuff
	/// </summary>
	public class igOpenGLGraphicsDevice : igBaseGraphicsDevice
	{
		/// <summary>
		/// information about a pixel texture format
		/// </summary>
		private struct pixelMetaimage
		{
			public string _name;
			public PixelInternalFormat _internalFormat;
			public PixelFormat _format;
			public PixelType _pixelType;


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="name">The canonical name of the metaimage</param>
			/// <param name="internalFormat">The opengl internal pixel format enum</param>
			/// <param name="format">The opengl pixel format enum</param>
			/// <param name="pixelType">The opengl pixel type</param>
			public pixelMetaimage(string name, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType)
			{
				_name = name;
				_internalFormat = internalFormat;
				_format = format;
				_pixelType = pixelType;
			}
		}


		/// <summary>
		/// information about a compressed texture format
		/// </summary>
		private struct compressedMetaimage
		{
			public string _name;
			public InternalFormat _format;


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="name">The name of the</param>
			/// <param name="format">The opengl internal format enum</param>
			public compressedMetaimage(string name, InternalFormat format)
			{
				_name = name;
				_format = format;
			}
		}


		/// <summary>
		/// Array of pixel image formats
		/// </summary>
		private static readonly pixelMetaimage[] _pixelPlatformEnums = new pixelMetaimage[]
		{
			//new pixelMetaimage(                       "a8", 
			new pixelMetaimage(                 "b5g5r5a1", PixelInternalFormat.Rgb5A1, PixelFormat.Bgra, PixelType.UnsignedShort5551),
			new pixelMetaimage(                   "b5g6r5", PixelInternalFormat.R5G6B5IccSgix, PixelFormat.Bgr, PixelType.UnsignedShort565),
			new pixelMetaimage(                   "b8g8r8", PixelInternalFormat.Rgb8, PixelFormat.Bgr, PixelType.UnsignedInt248),
			new pixelMetaimage(                 "b8g8r8a8", PixelInternalFormat.Rgba8, PixelFormat.Bgra, PixelType.UnsignedInt8888),
			//new pixelMetaimage(                 "b8g8r8x8", PixelInternalFormat.Rgba8, PixelFormat.Bgra						),	//unsure as to what x is
			//new pixelMetaimage(                    "d15s1", PixelInternalFormat.DepthComponent16, PixelFormat.DepthStencil	),	//I give up
			//new pixelMetaimage(                      "d16", PixelInternalFormat.DepthComponent16, PixelFormat.DepthComponent, PixelType.UnsignedShort),
			//new pixelMetaimage(                      "d24", PixelInternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.),
			//new pixelMetaimage(                   "d24fs8", PixelInternalFormat.depth											),	//wth is a 24 bit float
			//new pixelMetaimage(                  "d24s4x4",),
			new pixelMetaimage(                    "d24s8", PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthStencil, PixelType.UnsignedInt248),
			//new pixelMetaimage(                    "d24x8", ),
			new pixelMetaimage(                      "d32", PixelInternalFormat.DepthComponent32, PixelFormat.DepthComponent, PixelType.UnsignedInt),
			new pixelMetaimage(                     "d32f", PixelInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float),
			//new pixelMetaimage(                   "d32fs8", PixelInternalFormat.Depth32fStencil8, PixelFormat.DepthStencil, PixelType.Float),
			new pixelMetaimage(                       "d8", PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, PixelType.UnsignedByte),
			//new pixelMetaimage(                     "g8b8", PixelInternalFormat.),
			//new pixelMetaimage(                      "l16",),
			//new pixelMetaimage(                       "l4",),
			//new pixelMetaimage(                     "l4a4",),
			//new pixelMetaimage(                       "l8",),
			//new pixelMetaimage(                     "l8a8",),
			//new pixelMetaimage(          "p4_r4g4b4a3x1",),
			//new pixelMetaimage(            "p4_r8g8b8a8",),
			//new pixelMetaimage(          "p8_r4g4b4a3x1",),
			//new pixelMetaimage(            "p8_r8g8b8a8",),
			new pixelMetaimage(                "r16_float", PixelInternalFormat.R16f, PixelFormat.Red, PixelType.HalfFloat),
			new pixelMetaimage(                   "r16g16", PixelInternalFormat.Rg16, PixelFormat.Rg, PixelType.UnsignedShort),
			new pixelMetaimage(             "r16g16_float", PixelInternalFormat.Rg16f, PixelFormat.Rg, PixelType.HalfFloat),
			new pixelMetaimage(            "r16g16_signed", PixelInternalFormat.Rg16i, PixelFormat.Rg, PixelType.Short),
			new pixelMetaimage(                "r16g16b16", PixelInternalFormat.Rgb16, PixelFormat.Rgb, PixelType.UnsignedShort),
			new pixelMetaimage(             "r16g16b16a16", PixelInternalFormat.Rgba16, PixelFormat.Rgba, PixelType.UnsignedShort),
			new pixelMetaimage("r16g16b16a16_expand_float", PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.HalfFloat),
			new pixelMetaimage(       "r16g16b16a16_float", PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.HalfFloat),
			//new pixelMetaimage(             "r16g16b16x16", ),
			new pixelMetaimage(                "r32_float", PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float),
			new pixelMetaimage(             "r32g32_float", PixelInternalFormat.Rg32f, PixelFormat.Rg, PixelType.Float),
			new pixelMetaimage(       "r32g32b32a32_float", PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float),
			//new pixelMetaimage(               "r4g4b4a3x1",),
			new pixelMetaimage(                 "r4g4b4a4", PixelInternalFormat.Rgba4, PixelFormat.Rgba, PixelType.UnsignedShort4444),
			new pixelMetaimage(                 "r5g5b5a1", PixelInternalFormat.Rgb5A1, PixelFormat.Rgba, PixelType.UnsignedShort5551),
			new pixelMetaimage(                   "r5g6b5", PixelInternalFormat.R5G6B5IccSgix, PixelFormat.Rgb, PixelType.UnsignedShort565),
			//new pixelMetaimage(                 "r6g6b6a6", PixelInternalFormat.rgba6),
			new pixelMetaimage(                     "r8g8", PixelInternalFormat.Rg8, PixelFormat.Rg, PixelType.UnsignedByte),
			new pixelMetaimage(                   "r8g8b8", PixelInternalFormat.Rgb8, PixelFormat.Rgb, PixelType.UnsignedByte),
			new pixelMetaimage(       "r8g8b8_framebuffer", PixelInternalFormat.Rgb8, PixelFormat.Rgb, PixelType.UnsignedByte),
			new pixelMetaimage(              "r8g8b8_srgb", PixelInternalFormat.Srgb8, PixelFormat.Rgb, PixelType.UnsignedByte),
			new pixelMetaimage(                 "r8g8b8a8", PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte),
			new pixelMetaimage(            "r8g8b8a8_srgb", PixelInternalFormat.Srgb8Alpha8, PixelFormat.Rgba, PixelType.UnsignedByte),
			//new pixelMetaimage(                 "r8g8b8x8",),
			//new pixelMetaimage(            "r8g8b8x8_srgb",),
			//new pixelMetaimage(                 "shadow",),
		};


		/// <summary>
		/// Array of compressed image formats 
		/// </summary>
		private static readonly compressedMetaimage[] _compressedPlatformEnums = new compressedMetaimage[]
		{
			//new compressedMetaimage(                    "atitc", InternalFormat.),
			// /new compressedMetaimage(              "atitc_alpha",),
			//new compressedMetaimage(                      "dxn", InternalFormat.dxn),
			new compressedMetaimage(                     "dxt1", InternalFormat.CompressedRgbS3tcDxt1Ext),
			new compressedMetaimage(                "dxt1_srgb", InternalFormat.CompressedSrgbS3tcDxt1Ext),
			new compressedMetaimage(                     "dxt3", InternalFormat.CompressedRgbaS3tcDxt3Ext),
			new compressedMetaimage(                "dxt3_srgb", InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext),
			new compressedMetaimage(                     "dxt5", InternalFormat.CompressedRgbaS3tcDxt5Ext),
			new compressedMetaimage(                "dxt5_srgb", InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext),
			//new compressedMetaimage(                     "etc1", ),
			//new compressedMetaimage(                     "etc2",),
			//new compressedMetaimage(               "etc2_alpha",),
			//new compressedMetaimage(                      "gas",),
			//new compressedMetaimage(                   "pvrtc2", InternalFormat),
			//new compressedMetaimage(             "pvrtc2_alpha",),
			//new compressedMetaimage(        "pvrtc2_alpha_srgb",),
			//new compressedMetaimage(              "pvrtc2_srgb",),
			//new compressedMetaimage(                   "pvrtc4",),
			//new compressedMetaimage(             "pvrtc4_alpha",),
			//new compressedMetaimage(        "pvrtc4_alpha_srgb",),
			//new compressedMetaimage(              "pvrtc4_srgb",)
		};


		/// <summary>
		/// Constructor
		/// </summary>
		public igOpenGLGraphicsDevice() : base(){}


		/// <summary>
		/// Creates a texture based on an igImage2
		/// </summary>
		/// <param name="usage">How the texture should be used</param>
		/// <param name="image">The igImage2 to create the texture with</param>
		/// <returns>Returns an opengl texture handle</returns>
		/// <exception cref="ArgumentNullException">thrown when the image is null</exception>
		/// <exception cref="NotImplementedException">Thrown when a texture format isn't specified</exception>
		public override unsafe int CreateTexture(igResourceUsage usage, igImage2 image)
		{
			if(image == null) throw new ArgumentNullException("image is null!");
			if(image._texHandle != -1) return image._texHandle;
			if(image._format == null) throw new ArgumentNullException($"image._format for {image._name} is null! this shouldn't happen!");

			image._texHandle = GL.GenTexture();
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, image._texHandle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, image._levelCount - 1);

			fixed(byte* data = image._data.Buffer)
			{
				if(image._format._isCompressed)
				{
					int formatIndex = Array.FindIndex<compressedMetaimage>(_compressedPlatformEnums, x => x._name == image._format._canonical._name);
					if(formatIndex < 0)
					{
						GL.DeleteTexture(image._texHandle);
						throw new NotImplementedException($"format {image._format._name} is not implemented!");
					}
					uint mipWidth = (image._width + 3u) & ~3u;
					uint mipHeight = (image._height + 3u) & ~3u;
					uint blockWidth = image._format.GetBlockWidth();
					uint blockHeight = image._format.GetBlockHeight();
					for(int i = 0; i < image._levelCount; i++)
					{
						GL.CompressedTexImage2D(TextureTarget.Texture2D, i, _compressedPlatformEnums[formatIndex]._format,
							(int)mipWidth, (int)mipHeight, 0, (int)image._format.GetTextureSize((int)mipWidth, (int)mipHeight, image._depth, 1, image._imageCount),
							(IntPtr)(data + image._format.GetTextureLevelOffset(image._width, image._height, image._depth, image._levelCount, image._imageCount, i, 0))
						);
						mipWidth >>= 1;     if(mipWidth < blockWidth)   mipWidth = blockWidth;
						mipHeight >>= 1;    if(mipHeight < blockHeight) mipHeight = blockHeight;
					}
				}
				else
				{
					int formatIndex = Array.FindIndex<pixelMetaimage>(_pixelPlatformEnums, x => x._name == image._format._canonical._name);
					if(formatIndex < 0)
					{
						GL.DeleteTexture(image._texHandle);
						throw new NotImplementedException($"format {image._format._name} is not implemented!");
					}
					for(int i = 0; i < image._levelCount; i++)
					{
						GL.TexImage2D(TextureTarget.Texture2D, i, _pixelPlatformEnums[formatIndex]._internalFormat,
							image._width, image._height, 0, _pixelPlatformEnums[formatIndex]._format, _pixelPlatformEnums[formatIndex]._pixelType,
							(IntPtr)(data + image.GetTextureLevelOffset(i, 0))
						);
					}
				}
			}
			return image._texHandle;
		}


		/// <summary>
		/// Free the opengl texture
		/// </summary>
		/// <param name="texture">the opengl texture handle</param>
		public override void FreeTexture(int texture)
		{
			GL.DeleteTexture(texture);
		}
	}
}