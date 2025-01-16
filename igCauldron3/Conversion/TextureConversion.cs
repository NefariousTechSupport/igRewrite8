/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;
using igLibrary.Gfx;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace igCauldron3.Conversion
{
	/// <summary>
	/// Utilities for dealing with texture assets in the game
	/// </summary>
	public static class TextureConversion
	{
		/// <summary>
		/// Exports <c>igImage2</c> textures to more normal files 
		/// </summary>
		/// <param name="image">The image to export</param>
		/// <param name="dst">Where to output the converted texture</param>
		/// <param name="ext">The file extension to write to</param>
		public static void Export(igImage2 image, Stream dst, string ext)
        {
            int res = image.ConvertClone(igMetaImageInfo.FindFormat("r8g8b8a8"), igMemoryContext.Singleton.GetMemoryPoolByName("Image"), out igImage2? r8g8b8a8Image);
            if (res != 0 || r8g8b8a8Image == null) return;
            Image<Rgba32> output = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(r8g8b8a8Image._data.Buffer, r8g8b8a8Image._width, r8g8b8a8Image._height);
			switch(ext)
			{
				case ".png":
					output.SaveAsPng(dst);
					break;
				case ".jpeg":
				case ".jpg":
					output.SaveAsJpeg(dst);
					break;
				case ".bmp":
					output.SaveAsBmp(dst);
					break;
				case ".gif":
					output.SaveAsGif(dst);
					break;
				case ".pbm":
					output.SaveAsPbm(dst);
					break;
				case ".qoi":
					output.SaveAsQoi(dst);
					break;
				case ".tga":
					output.SaveAsTga(dst);
					break;
				case ".tiff":
					output.SaveAsTiff(dst);
					break;
				case ".webp":
					output.SaveAsWebp(dst);
					break;
			}
		}


		/// <summary>
		/// Internal method for importing an <c>igImage2</c> texture
		/// </summary>
		/// <typeparam name="T">The pixel type</typeparam>
		/// <param name="src">The source stream to import from</param>
		/// <param name="image">The <c>igImage2</c> to export to</param>
		/// <param name="normalFormatName">The regular metaimage format to export with</param>
		/// <param name="srgbFormatName">The srgb metaimage format to export with</param>
		/// <exception cref="InvalidImageContentException">Thrown when the image cannot be stored in an igImage2</exception>
		/// <exception cref="InvalidOperationException">Thrown when some poorly written code fails</exception>
		private static void ImportInternal<T>(Stream src, igImage2 image, string normalFormatName, string srgbFormatName) where T : unmanaged, IPixel<T>
		{
			Image<T> newImage = SixLabors.ImageSharp.Image.Load<T>(src);
			if(newImage.Width > ushort.MaxValue || newImage.Height > ushort.MaxValue)
			{
				throw new InvalidImageContentException("Both the image width and height must be less than 65535.");
			}
			image._width = (ushort)newImage.Width;
			image._height = (ushort)newImage.Height;
			image._data.Alloc(image._width * image._height * newImage.PixelType.BitsPerPixel / 8);
			image._levelCount = 1;
			image._imageCount = 1;
			image._depth = 1;
			string formatName;
			     if(!image._format._isSrgb/* && !image._format._isTile*/) formatName = normalFormatName;
			else if( image._format._isSrgb/* && !image._format._isTile*/) formatName = srgbFormatName;
			//else if(!image._format._isSrgb &&  image._format._isTile) formatName = tileFormatName;
			//else if( image._format._isSrgb &&  image._format._isTile) formatName = srgbTileFormatName;
			else throw new InvalidOperationException("This is impossible");
			image._format = igMetaImageInfo.FindFormat(formatName);
            newImage.CopyPixelDataTo(image._data.Buffer);
            if (formatName.StartsWith("r8g8b8a8") && formatName.EndsWith("_tile_cafe"))
            {
                byte[] swizzled_data = igWiiUSwizzle.Swizzle_rgba32(image._data.Buffer, image._width, image._height);
                image._data.SetData(swizzled_data);
            }
        }


		/// <summary>
		/// Import a texture to an <c>igImage2</c>
		/// </summary>
		/// <param name="image">The <c>igImage2</c> to import to</param>
		/// <param name="src">The source image to import from</param>
		/// <exception cref="NotImplementedException">If a platform's not implemented</exception>
		public static void Import(igImage2 image, Stream src)
		{
			switch((image._format as igPlatformMetaImage)._platform)
			{
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DURANGO:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ASPEN:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_XENON:
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
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_MAX:
					throw new NotImplementedException($"Texture conversion for platform {(image._format as igPlatformMetaImage)._platform} is not supported");
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
					//read as Argb32 and write as Bgra32 because funky endianness
					ImportInternal<Argb32>(src, image, "b8g8r8a8_big_ps3", "b8g8r8a8_srgb_big_ps3"/*, "b8g8r8a8_tile_big_ps3", "b8g8r8a8_srgb_tile_big_ps3"*/);
					break;
                case IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE:
                    ImportInternal<Rgba32>(src, image, "r8g8b8a8_tile_cafe", "r8g8b8a8_srgb_tile_cafe");
                    break;
            }
		}
	}
}