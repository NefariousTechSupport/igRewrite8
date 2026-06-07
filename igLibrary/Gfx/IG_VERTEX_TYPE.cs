namespace igLibrary.Gfx
{
	public enum IG_VERTEX_TYPE
	{
		IG_VERTEX_TYPE_FLOAT1 = 0,
		IG_VERTEX_TYPE_FLOAT2 = 1,
		IG_VERTEX_TYPE_FLOAT3 = 2,
		IG_VERTEX_TYPE_FLOAT4 = 3,
		IG_VERTEX_TYPE_UBYTE4N_COLOR = 4,
		IG_VERTEX_TYPE_UBYTE4N_COLOR_ARGB = 5,
		IG_VERTEX_TYPE_UBYTE4N_COLOR_RGBA = 6,
		IG_VERTEX_TYPE_UNDEFINED_0 = 7,
		IG_VERTEX_TYPE_UBYTE2N_COLOR_5650 = 8,
		IG_VERTEX_TYPE_UBYTE2N_COLOR_5551 = 9,
		IG_VERTEX_TYPE_UBYTE2N_COLOR_4444 = 10,
		IG_VERTEX_TYPE_INT1 = 11,
		IG_VERTEX_TYPE_INT2 = 12,
		IG_VERTEX_TYPE_INT4 = 13,
		IG_VERTEX_TYPE_UINT1 = 14,
		IG_VERTEX_TYPE_UINT2 = 15,
		IG_VERTEX_TYPE_UINT4 = 16,
		IG_VERTEX_TYPE_INT1N = 17,
		IG_VERTEX_TYPE_INT2N = 18,
		IG_VERTEX_TYPE_INT4N = 19,
		IG_VERTEX_TYPE_UINT1N = 20,
		IG_VERTEX_TYPE_UINT2N = 21,
		IG_VERTEX_TYPE_UINT4N = 22,
		IG_VERTEX_TYPE_UBYTE4 = 23,
		IG_VERTEX_TYPE_UBYTE4_X4 = 24,
		IG_VERTEX_TYPE_BYTE4 = 25,
		IG_VERTEX_TYPE_UBYTE4N = 26,
		IG_VERTEX_TYPE_UNDEFINED_1 = 27,
		IG_VERTEX_TYPE_BYTE4N = 28,
		IG_VERTEX_TYPE_SHORT2 = 29,
		IG_VERTEX_TYPE_SHORT4 = 30,
		IG_VERTEX_TYPE_USHORT2 = 31,
		IG_VERTEX_TYPE_USHORT4 = 32,
		IG_VERTEX_TYPE_SHORT2N = 33,
		IG_VERTEX_TYPE_SHORT3N = 34,
		IG_VERTEX_TYPE_SHORT4N = 35,
		IG_VERTEX_TYPE_USHORT2N = 36,
		IG_VERTEX_TYPE_USHORT3N = 37,
		IG_VERTEX_TYPE_USHORT4N = 38,
		IG_VERTEX_TYPE_UDEC3 = 39,
		IG_VERTEX_TYPE_DEC3N = 40,
		IG_VERTEX_TYPE_DEC3N_S11_11_10 = 41,
		IG_VERTEX_TYPE_HALF2 = 42,
		IG_VERTEX_TYPE_HALF4 = 43,
		IG_VERTEX_TYPE_UNUSED = 44,
		IG_VERTEX_TYPE_BYTE3N = 45,
		IG_VERTEX_TYPE_SHORT3 = 46,
		IG_VERTEX_TYPE_USHORT3 = 47,
		IG_VERTEX_TYPE_UBYTE4_ENDIAN = 48,
		IG_VERTEX_TYPE_UBYTE4_COLOR = 49,
		IG_VERTEX_TYPE_BYTE3 = 50,
		IG_VERTEX_TYPE_UBYTE2N_COLOR_5650_RGB = 51,
		IG_VERTEX_TYPE_UDEC3_OES = 52,
		IG_VERTEX_TYPE_DEC3N_OES = 53,
		IG_VERTEX_TYPE_SHORT4N_EDGE = 54,
		IG_VERTEX_TYPE_MAX = 55
	}

	public static class __IG_VERTEX_TYPE_Methods
	{
		private struct __IG_VERTEX_TYPE_Properties
		{
			public byte _componentSize;
			public byte _componentCount;
			public bool _signed;

			public __IG_VERTEX_TYPE_Properties(byte componentSize, byte componentCount, bool signed)
			{
				this._componentSize = componentSize;
				this._componentCount = componentCount;
				this._signed = signed;
			}
		}
		private static readonly __IG_VERTEX_TYPE_Properties[] properties =
		{
				new (0x04, 0x01,  true), // IG_VERTEX_TYPE_FLOAT1
				new (0x08, 0x02,  true), // IG_VERTEX_TYPE_FLOAT2
				new (0x0C, 0x03,  true), // IG_VERTEX_TYPE_FLOAT3
				new (0x10, 0x04,  true), // IG_VERTEX_TYPE_FLOAT4
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4N_COLOR
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4N_COLOR_ARGB
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4N_COLOR_RGBA
				new (0x00, 0x00, false), // IG_VERTEX_TYPE_UNDEFINED_0                (NOT A REAL TYPE)
				new (0x04, 0xFF, false), // IG_VERTEX_TYPE_UBYTE2N_COLOR_5650
				new (0x04, 0xFF, false), // IG_VERTEX_TYPE_UBYTE2N_COLOR_5551
				new (0x04, 0xFF, false), // IG_VERTEX_TYPE_UBYTE2N_COLOR_4444
				new (0x04, 0x01,  true), // IG_VERTEX_TYPE_INT1
				new (0x08, 0x02,  true), // IG_VERTEX_TYPE_INT2
				new (0x10, 0x04,  true), // IG_VERTEX_TYPE_INT4
				new (0x04, 0x01, false), // IG_VERTEX_TYPE_UINT1
				new (0x08, 0x02, false), // IG_VERTEX_TYPE_UINT2
				new (0x10, 0x04, false), // IG_VERTEX_TYPE_UINT4
				new (0x04, 0x01,  true), // IG_VERTEX_TYPE_INT1N
				new (0x08, 0x02,  true), // IG_VERTEX_TYPE_INT2N
				new (0x10, 0x04,  true), // IG_VERTEX_TYPE_INT4N
				new (0x04, 0x01, false), // IG_VERTEX_TYPE_UINT1N
				new (0x08, 0x02, false), // IG_VERTEX_TYPE_UINT2N
				new (0x10, 0x04, false), // IG_VERTEX_TYPE_UINT4N
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4_X4
				new (0x04, 0x04,  true), // IG_VERTEX_TYPE_BYTE4
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4N
				new (0x00, 0x00, false), // IG_VERTEX_TYPE_UNDEFINED_1                (NOT A REAL TYPE)
				new (0x04, 0x04,  true), // IG_VERTEX_TYPE_BYTE4N
				new (0x04, 0x02,  true), // IG_VERTEX_TYPE_SHORT2
				new (0x08, 0x04,  true), // IG_VERTEX_TYPE_SHORT4
				new (0x04, 0x02, false), // IG_VERTEX_TYPE_USHORT2
				new (0x08, 0x04, false), // IG_VERTEX_TYPE_USHORT4
				new (0x04, 0x02,  true), // IG_VERTEX_TYPE_SHORT2N
				new (0x06, 0x03,  true), // IG_VERTEX_TYPE_SHORT3N
				new (0x08, 0x04,  true), // IG_VERTEX_TYPE_SHORT4N
				new (0x04, 0x02, false), // IG_VERTEX_TYPE_USHORT2N
				new (0x06, 0x03, false), // IG_VERTEX_TYPE_USHORT3N
				new (0x08, 0x04, false), // IG_VERTEX_TYPE_USHORT4N
				new (0x04, 0x01, false), // IG_VERTEX_TYPE_UDEC3
				new (0x04, 0x01,  true), // IG_VERTEX_TYPE_DEC3N
				new (0x04, 0x01,  true), // IG_VERTEX_TYPE_DEC3N_S11_11_10
				new (0x04, 0x02,  true), // IG_VERTEX_TYPE_HALF2
				new (0x08, 0x04,  true), // IG_VERTEX_TYPE_HALF4
				new (0x00, 0x00, false), // IG_VERTEX_TYPE_UNUSED                     (NOT A REAL TYPE)
				new (0x03, 0x03,  true), // IG_VERTEX_TYPE_BYTE3N
				new (0x06, 0x03,  true), // IG_VERTEX_TYPE_SHORT3
				new (0x06, 0x03, false), // IG_VERTEX_TYPE_USHORT3
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4_ENDIAN
				new (0x04, 0x04, false), // IG_VERTEX_TYPE_UBYTE4_COLOR
				new (0x03, 0x03,  true), // IG_VERTEX_TYPE_BYTE3
				new (0x02, 0xFF, false), // IG_VERTEX_TYPE_UBYTE2N_COLOR_5650_RGB
				new (0x04, 0x01, false), // IG_VERTEX_TYPE_UDEC3_OES
				new (0x04, 0x01,  true), // IG_VERTEX_TYPE_DEC3N_OES
				new (0x08, 0x04,  true), // IG_VERTEX_TYPE_SHORT4N_EDGE
				new (0x00, 0x00, false)  // IG_VERTEX_TYPE_MAX                        (NOT A REAL TYPE)
		};
		public static byte GetComponentSize(this IG_VERTEX_TYPE type) => properties[(int)type]._componentSize;
		public static byte GetComponentCount(this IG_VERTEX_TYPE type) => properties[(int)type]._componentCount;
		public static bool IsSigned(this IG_VERTEX_TYPE type) => properties[(int)type]._signed;
	}
}