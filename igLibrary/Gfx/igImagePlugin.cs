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
		public static igCanonicalMetaImage? a8                        { get; private set; }
		public static igCanonicalMetaImage? atitc                     { get; private set; }
		public static igCanonicalMetaImage? atitc_alpha               { get; private set; }
		public static igCanonicalMetaImage? b5g5r5a1                  { get; private set; }
		public static igCanonicalMetaImage? b5g6r5                    { get; private set; }
		public static igCanonicalMetaImage? b8g8r8                    { get; private set; }
		public static igCanonicalMetaImage? b8g8r8a8                  { get; private set; }
		public static igCanonicalMetaImage? b8g8r8x8                  { get; private set; }
		public static igCanonicalMetaImage? d15s1                     { get; private set; }
		public static igCanonicalMetaImage? d16                       { get; private set; }
		public static igCanonicalMetaImage? d24                       { get; private set; }
		public static igCanonicalMetaImage? d24fs8                    { get; private set; }
		public static igCanonicalMetaImage? d24s4x4                   { get; private set; }
		public static igCanonicalMetaImage? d24s8                     { get; private set; }
		public static igCanonicalMetaImage? d24x8                     { get; private set; }
		public static igCanonicalMetaImage? d32                       { get; private set; }
		public static igCanonicalMetaImage? d32f                      { get; private set; }
		public static igCanonicalMetaImage? d32fs8                    { get; private set; }
		public static igCanonicalMetaImage? d8                        { get; private set; }
		public static igCanonicalMetaImage? dxn                       { get; private set; }
		public static igCanonicalMetaImage? dxt1                      { get; private set; }
		public static igCanonicalMetaImage? dxt1_srgb                 { get; private set; }
		public static igCanonicalMetaImage? dxt3                      { get; private set; }
		public static igCanonicalMetaImage? dxt3_srgb                 { get; private set; }
		public static igCanonicalMetaImage? dxt5                      { get; private set; }
		public static igCanonicalMetaImage? dxt5_srgb                 { get; private set; }
		public static igCanonicalMetaImage? etc1                      { get; private set; }
		public static igCanonicalMetaImage? etc2                      { get; private set; }
		public static igCanonicalMetaImage? etc2_alpha                { get; private set; }
		public static igCanonicalMetaImage? g8b8                      { get; private set; }
		public static igCanonicalMetaImage? gas                       { get; private set; }
		public static igCanonicalMetaImage? l16                       { get; private set; }
		public static igCanonicalMetaImage? l4                        { get; private set; }
		public static igCanonicalMetaImage? l4a4                      { get; private set; }
		public static igCanonicalMetaImage? l8                        { get; private set; }
		public static igCanonicalMetaImage? l8a8                      { get; private set; }
		public static igCanonicalMetaImage? p4_r4g4b4a3x1             { get; private set; }
		public static igCanonicalMetaImage? p4_r8g8b8a8               { get; private set; }
		public static igCanonicalMetaImage? p8_r4g4b4a3x1             { get; private set; }
		public static igCanonicalMetaImage? p8_r8g8b8a8               { get; private set; }
		public static igCanonicalMetaImage? pvrtc2                    { get; private set; }
		public static igCanonicalMetaImage? pvrtc2_alpha              { get; private set; }
		public static igCanonicalMetaImage? pvrtc2_alpha_srgb         { get; private set; }
		public static igCanonicalMetaImage? pvrtc2_srgb               { get; private set; }
		public static igCanonicalMetaImage? pvrtc4                    { get; private set; }
		public static igCanonicalMetaImage? pvrtc4_alpha              { get; private set; }
		public static igCanonicalMetaImage? pvrtc4_alpha_srgb         { get; private set; }
		public static igCanonicalMetaImage? pvrtc4_srgb               { get; private set; }
		public static igCanonicalMetaImage? r16_float                 { get; private set; }
		public static igCanonicalMetaImage? r16g16                    { get; private set; }
		public static igCanonicalMetaImage? r16g16_float              { get; private set; }
		public static igCanonicalMetaImage? r16g16_signed             { get; private set; }
		public static igCanonicalMetaImage? r16g16b16                 { get; private set; }
		public static igCanonicalMetaImage? r16g16b16a16              { get; private set; }
		public static igCanonicalMetaImage? r16g16b16a16_expand_float { get; private set; }
		public static igCanonicalMetaImage? r16g16b16a16_float        { get; private set; }
		public static igCanonicalMetaImage? r16g16b16x16              { get; private set; }
		public static igCanonicalMetaImage? r32_float                 { get; private set; }
		public static igCanonicalMetaImage? r32g32_float              { get; private set; }
		public static igCanonicalMetaImage? r32g32b32a32_float        { get; private set; }
		public static igCanonicalMetaImage? r4g4b4a3x1                { get; private set; }
		public static igCanonicalMetaImage? r4g4b4a4                  { get; private set; }
		public static igCanonicalMetaImage? r5g5b5a1                  { get; private set; }
		public static igCanonicalMetaImage? r5g6b5                    { get; private set; }
		public static igCanonicalMetaImage? r6g6b6a6                  { get; private set; }
		public static igCanonicalMetaImage? r8g8                      { get; private set; }
		public static igCanonicalMetaImage? r8g8b8                    { get; private set; }
		public static igCanonicalMetaImage? r8g8b8_framebuffer        { get; private set; }
		public static igCanonicalMetaImage? r8g8b8_srgb               { get; private set; }
		public static igCanonicalMetaImage? r8g8b8a8                  { get; private set; }
		public static igCanonicalMetaImage? r8g8b8a8_srgb             { get; private set; }
		public static igCanonicalMetaImage? r8g8b8x8                  { get; private set; }
		public static igCanonicalMetaImage? r8g8b8x8_srgb             { get; private set; }
		public static igCanonicalMetaImage? shadow                    { get; private set; }

		public static igImagePluginList _pluginTypes;
		public static void RegisterPlugin()
		{
			a8                        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("a8")!;                         //Registered r8g8b8a8 func
			atitc                     = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("atitc")!;
			atitc_alpha               = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("atitc_alpha")!;
			b5g5r5a1                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("b5g5r5a1")!;
			b5g6r5                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("b5g6r5")!;
			b8g8r8                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("b8g8r8")!;
			b8g8r8a8                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("b8g8r8a8")!;                   //Registered r8g8b8a8 func
			b8g8r8x8                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("b8g8r8x8")!;
			d15s1                     = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d15s1")!;
			d16                       = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d16")!;
			d24                       = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d24")!;
			d24fs8                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d24fs8")!;
			d24s4x4                   = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d24s4x4")!;
			d24s8                     = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d24s8")!;
			d24x8                     = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d24x8")!;
			d32                       = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d32")!;
			d32f                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d32f")!;
			d32fs8                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d32fs8")!;
			d8                        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("d8")!;
			dxn                       = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxn")!;
			dxt1                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxt1")!;
			dxt1_srgb                 = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxt1_srgb")!;
			dxt3                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxt3")!;
			dxt3_srgb                 = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxt3_srgb")!;
			dxt5                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxt5")!;
			dxt5_srgb                 = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("dxt5_srgb")!;
			etc1                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("etc1")!;
			etc2                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("etc2")!;
			etc2_alpha                = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("etc2_alpha")!;
			g8b8                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("g8b8")!;
			gas                       = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("gas")!;
			l16                       = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("l16")!;
			l4                        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("l4")!;
			l4a4                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("l4a4")!;
			l8                        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("l8")!;
			l8a8                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("l8a8")!;
			p4_r4g4b4a3x1             = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("p4_r4g4b4a3x1")!;
			p4_r8g8b8a8               = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("p4_r8g8b8a8")!;
			p8_r4g4b4a3x1             = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("p8_r4g4b4a3x1")!;
			p8_r8g8b8a8               = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("p8_r8g8b8a8")!;
			pvrtc2                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc2")!;
			pvrtc2_alpha              = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc2_alpha")!;
			pvrtc2_alpha_srgb         = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc2_alpha_srgb")!;
			pvrtc2_srgb               = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc2_srgb")!;
			pvrtc4                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc4")!;
			pvrtc4_alpha              = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc4_alpha")!;
			pvrtc4_alpha_srgb         = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc4_alpha_srgb")!;
			pvrtc4_srgb               = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("pvrtc4_srgb")!;
			r16_float                 = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16_float")!;
			r16g16                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16")!;
			r16g16_float              = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16_float")!;
			r16g16_signed             = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16_signed")!;
			r16g16b16                 = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16b16")!;
			r16g16b16a16              = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16b16a16")!;
			r16g16b16a16_expand_float = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16b16a16_expand_float")!;
			r16g16b16a16_float        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16b16a16_float")!;
			r16g16b16x16              = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r16g16b16x16")!;
			r32_float                 = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r32_float")!;
			r32g32_float              = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r32g32_float")!;
			r32g32b32a32_float        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r32g32b32a32_float")!;
			r4g4b4a3x1                = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r4g4b4a3x1")!;
			r4g4b4a4                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r4g4b4a4")!;
			r5g5b5a1                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r5g5b5a1")!;                   //Registered r8g8b8a8 func
			r5g6b5                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r5g6b5")!;                     //Registered r8g8b8a8 func
			r6g6b6a6                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r6g6b6a6")!;
			r8g8                      = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8")!;
			r8g8b8                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8")!;
			r8g8b8_framebuffer        = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8_framebuffer")!;
			r8g8b8_srgb               = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8_srgb")!;
			r8g8b8a8                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8a8")!;
			r8g8b8a8_srgb             = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8a8_srgb")!;
			r8g8b8x8                  = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8x8")!;
			r8g8b8x8_srgb             = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("r8g8b8x8_srgb")!;
			shadow                    = (igCanonicalMetaImage?)igMetaImageInfo.FindFormat("shadow")!;

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