/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igImagePlugin : igObject
	{
		public static igImagePluginList _pluginTypes;
		public static void RegisterPlugin()
		{
			igMetaImage a8                        = igMetaImageInfo.FindFormat("a8")!;                         //Registered r8g8b8a8 func
			igMetaImage atitc                     = igMetaImageInfo.FindFormat("atitc")!;
			igMetaImage atitc_alpha               = igMetaImageInfo.FindFormat("atitc_alpha")!;
			igMetaImage b5g5r5a1                  = igMetaImageInfo.FindFormat("b5g5r5a1")!;
			igMetaImage b5g6r5                    = igMetaImageInfo.FindFormat("b5g6r5")!;
			igMetaImage b8g8r8                    = igMetaImageInfo.FindFormat("b8g8r8")!;
			igMetaImage b8g8r8a8                  = igMetaImageInfo.FindFormat("b8g8r8a8")!;                   //Registered r8g8b8a8 func
			igMetaImage b8g8r8x8                  = igMetaImageInfo.FindFormat("b8g8r8x8")!;
			igMetaImage d15s1                     = igMetaImageInfo.FindFormat("d15s1")!;
			igMetaImage d16                       = igMetaImageInfo.FindFormat("d16")!;
			igMetaImage d24                       = igMetaImageInfo.FindFormat("d24")!;
			igMetaImage d24fs8                    = igMetaImageInfo.FindFormat("d24fs8")!;
			igMetaImage d24s4x4                   = igMetaImageInfo.FindFormat("d24s4x4")!;
			igMetaImage d24s8                     = igMetaImageInfo.FindFormat("d24s8")!;
			igMetaImage d24x8                     = igMetaImageInfo.FindFormat("d24x8")!;
			igMetaImage d32                       = igMetaImageInfo.FindFormat("d32")!;
			igMetaImage d32f                      = igMetaImageInfo.FindFormat("d32f")!;
			igMetaImage d32fs8                    = igMetaImageInfo.FindFormat("d32fs8")!;
			igMetaImage d8                        = igMetaImageInfo.FindFormat("d8")!;
			igMetaImage dxn                       = igMetaImageInfo.FindFormat("dxn")!;
			igMetaImage dxt1                      = igMetaImageInfo.FindFormat("dxt1")!;
			igMetaImage dxt1_srgb                 = igMetaImageInfo.FindFormat("dxt1_srgb")!;
			igMetaImage dxt3                      = igMetaImageInfo.FindFormat("dxt3")!;
			igMetaImage dxt3_srgb                 = igMetaImageInfo.FindFormat("dxt3_srgb")!;
			igMetaImage dxt5                      = igMetaImageInfo.FindFormat("dxt5")!;
			igMetaImage dxt5_srgb                 = igMetaImageInfo.FindFormat("dxt5_srgb")!;
			igMetaImage etc1                      = igMetaImageInfo.FindFormat("etc1")!;
			igMetaImage etc2                      = igMetaImageInfo.FindFormat("etc2")!;
			igMetaImage etc2_alpha                = igMetaImageInfo.FindFormat("etc2_alpha")!;
			igMetaImage g8b8                      = igMetaImageInfo.FindFormat("g8b8")!;
			igMetaImage gas                       = igMetaImageInfo.FindFormat("gas")!;
			igMetaImage l16                       = igMetaImageInfo.FindFormat("l16")!;
			igMetaImage l4                        = igMetaImageInfo.FindFormat("l4")!;
			igMetaImage l4a4                      = igMetaImageInfo.FindFormat("l4a4")!;
			igMetaImage l8                        = igMetaImageInfo.FindFormat("l8")!;
			igMetaImage l8a8                      = igMetaImageInfo.FindFormat("l8a8")!;
			igMetaImage p4_r4g4b4a3x1             = igMetaImageInfo.FindFormat("p4_r4g4b4a3x1")!;
			igMetaImage p4_r8g8b8a8               = igMetaImageInfo.FindFormat("p4_r8g8b8a8")!;
			igMetaImage p8_r4g4b4a3x1             = igMetaImageInfo.FindFormat("p8_r4g4b4a3x1")!;
			igMetaImage p8_r8g8b8a8               = igMetaImageInfo.FindFormat("p8_r8g8b8a8")!;
			igMetaImage pvrtc2                    = igMetaImageInfo.FindFormat("pvrtc2")!;
			igMetaImage pvrtc2_alpha              = igMetaImageInfo.FindFormat("pvrtc2_alpha")!;
			igMetaImage pvrtc2_alpha_srgb         = igMetaImageInfo.FindFormat("pvrtc2_alpha_srgb")!;
			igMetaImage pvrtc2_srgb               = igMetaImageInfo.FindFormat("pvrtc2_srgb")!;
			igMetaImage pvrtc4                    = igMetaImageInfo.FindFormat("pvrtc4")!;
			igMetaImage pvrtc4_alpha              = igMetaImageInfo.FindFormat("pvrtc4_alpha")!;
			igMetaImage pvrtc4_alpha_srgb         = igMetaImageInfo.FindFormat("pvrtc4_alpha_srgb")!;
			igMetaImage pvrtc4_srgb               = igMetaImageInfo.FindFormat("pvrtc4_srgb")!;
			igMetaImage r16_float                 = igMetaImageInfo.FindFormat("r16_float")!;
			igMetaImage r16g16                    = igMetaImageInfo.FindFormat("r16g16")!;
			igMetaImage r16g16_float              = igMetaImageInfo.FindFormat("r16g16_float")!;
			igMetaImage r16g16_signed             = igMetaImageInfo.FindFormat("r16g16_signed")!;
			igMetaImage r16g16b16                 = igMetaImageInfo.FindFormat("r16g16b16")!;
			igMetaImage r16g16b16a16              = igMetaImageInfo.FindFormat("r16g16b16a16")!;
			igMetaImage r16g16b16a16_expand_float = igMetaImageInfo.FindFormat("r16g16b16a16_expand_float")!;
			igMetaImage r16g16b16a16_float        = igMetaImageInfo.FindFormat("r16g16b16a16_float")!;
			igMetaImage r16g16b16x16              = igMetaImageInfo.FindFormat("r16g16b16x16")!;
			igMetaImage r32_float                 = igMetaImageInfo.FindFormat("r32_float")!;
			igMetaImage r32g32_float              = igMetaImageInfo.FindFormat("r32g32_float")!;
			igMetaImage r32g32b32a32_float        = igMetaImageInfo.FindFormat("r32g32b32a32_float")!;
			igMetaImage r4g4b4a3x1                = igMetaImageInfo.FindFormat("r4g4b4a3x1")!;
			igMetaImage r4g4b4a4                  = igMetaImageInfo.FindFormat("r4g4b4a4")!;
			igMetaImage r5g5b5a1                  = igMetaImageInfo.FindFormat("r5g5b5a1")!;                   //Registered r8g8b8a8 func
			igMetaImage r5g6b5                    = igMetaImageInfo.FindFormat("r5g6b5")!;                     //Registered r8g8b8a8 func
			igMetaImage r6g6b6a6                  = igMetaImageInfo.FindFormat("r6g6b6a6")!;
			igMetaImage r8g8                      = igMetaImageInfo.FindFormat("r8g8")!;
			igMetaImage r8g8b8                    = igMetaImageInfo.FindFormat("r8g8b8")!;
			igMetaImage r8g8b8_framebuffer        = igMetaImageInfo.FindFormat("r8g8b8_framebuffer")!;
			igMetaImage r8g8b8_srgb               = igMetaImageInfo.FindFormat("r8g8b8_srgb")!;
			igMetaImage r8g8b8a8                  = igMetaImageInfo.FindFormat("r8g8b8a8")!;
			igMetaImage r8g8b8a8_srgb             = igMetaImageInfo.FindFormat("r8g8b8a8_srgb")!;
			igMetaImage r8g8b8x8                  = igMetaImageInfo.FindFormat("r8g8b8x8")!;
			igMetaImage r8g8b8x8_srgb             = igMetaImageInfo.FindFormat("r8g8b8x8_srgb")!;
			igMetaImage shadow                    = igMetaImageInfo.FindFormat("shadow")!;

			     r8g8b8a8.AppendConvertFunction(b8g8r8a8, igGfx.Convert_r8g8b8a8_to_b8g8r8a8);
			r8g8b8a8_srgb.AppendConvertFunction(b8g8r8a8, igGfx.Convert_r8g8b8a8_to_b8g8r8a8);
			     b8g8r8a8.AppendConvertFunction(r8g8b8a8, igGfx.Convert_b8g8r8a8_to_r8g8b8a8);
			     r5g5b5a1.AppendConvertFunction(r8g8b8a8, igGfx.Convert_r5g5b5a1_to_r8g8b8a8);
			       r5g6b5.AppendConvertFunction(r8g8b8a8, igGfx.Convert_r5g6b5_to_r8g8b8a8);
			           a8.AppendConvertFunction(r8g8b8a8, igGfx.Convert_a8_to_r8g8b8a8);
			         dxt1.AppendConvertFunction(r8g8b8a8, igGfx.Convert_dxt1_to_r8g8b8a8);
			         dxt5.AppendConvertFunction(r8g8b8a8, igGfx.Convert_dxt5_to_r8g8b8a8);
		}
	}
	public class igImagePluginList : igTObjectList<igImagePlugin>{}
}