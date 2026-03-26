using System.Runtime.InteropServices;

namespace igLibrary.Gfx
{
	/// <summary>
	/// Wii U Implementation of vertex formats
	/// </summary>
	public class igVertexFormatCafe : igVertexFormatPlatform
	{
		/// <summary>
		/// The _platformData field of igVertexFormat is an array of this structure
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 0x20)]
		public struct GX2AttribStream
		{
			[FieldOffset(0x00)] public uint location;
			[FieldOffset(0x04)] public uint buffer;
			[FieldOffset(0x08)] public uint offset;
			[FieldOffset(0x0C)] public GX2AttribFormat format;
			[FieldOffset(0x10)] public GX2AttribIndexType type;
			[FieldOffset(0x14)] public uint aluDivisor;
			[FieldOffset(0x18)] public uint mask;
			[FieldOffset(0x1C)] public GX2EndianSwapMode endianSwap;
		}



		/// <summary>
		/// Vertex format information
		/// </summary>
		[Flags]
		public enum GX2AttribFormat : uint
		{
			GX2_ATTRIB_TYPE_8                   = 0x00,
			GX2_ATTRIB_TYPE_4_4                 = 0x01,
			GX2_ATTRIB_TYPE_16                  = 0x02,
			GX2_ATTRIB_TYPE_16_FLOAT            = 0x03,
			GX2_ATTRIB_TYPE_8_8                 = 0x04,
			GX2_ATTRIB_TYPE_32                  = 0x05,
			GX2_ATTRIB_TYPE_32_FLOAT            = 0x06,
			GX2_ATTRIB_TYPE_16_16               = 0x07,
			GX2_ATTRIB_TYPE_16_16_FLOAT         = 0x08,
			GX2_ATTRIB_TYPE_10_11_11_FLOAT      = 0x09,
			GX2_ATTRIB_TYPE_8_8_8_8             = 0x0A,
			GX2_ATTRIB_TYPE_10_10_10_2          = 0x0B,
			GX2_ATTRIB_TYPE_32_32               = 0x0C,
			GX2_ATTRIB_TYPE_32_32_FLOAT         = 0x0D,
			GX2_ATTRIB_TYPE_16_16_16_16         = 0x0E,
			GX2_ATTRIB_TYPE_16_16_16_16_FLOAT   = 0x0F,
			GX2_ATTRIB_TYPE_32_32_32            = 0x10,
			GX2_ATTRIB_TYPE_32_32_32_FLOAT      = 0x11,
			GX2_ATTRIB_TYPE_32_32_32_32         = 0x12,
			GX2_ATTRIB_TYPE_32_32_32_32_FLOAT   = 0x13,

			// Specify this to say it's unnormalised
			GX2_ATTRIB_FLAG_INTEGER             = 0x100,
			// Specify this to say it's a signed format
			GX2_ATTRIB_FLAG_SIGNED              = 0x200,
			GX2_ATTRIB_FLAG_DEGAMMA             = 0x400,
			GX2_ATTRIB_FLAG_SCALED              = 0x800,

			GX2_ATTRIB_FORMAT_UNORM_8           = GX2_ATTRIB_TYPE_8,
			GX2_ATTRIB_FORMAT_UNORM_8_8         = GX2_ATTRIB_TYPE_8_8,
			GX2_ATTRIB_FORMAT_UNORM_8_8_8_8     = GX2_ATTRIB_TYPE_8_8_8_8,

			GX2_ATTRIB_FORMAT_UINT_8            = GX2_ATTRIB_FLAG_INTEGER | GX2_ATTRIB_TYPE_8,
			GX2_ATTRIB_FORMAT_UINT_8_8          = GX2_ATTRIB_FLAG_INTEGER | GX2_ATTRIB_TYPE_8_8,
			GX2_ATTRIB_FORMAT_UINT_8_8_8_8      = GX2_ATTRIB_FLAG_INTEGER | GX2_ATTRIB_TYPE_8_8_8_8,

			GX2_ATTRIB_FORMAT_SNORM_8           = GX2_ATTRIB_FLAG_SIGNED | GX2_ATTRIB_TYPE_8,
			GX2_ATTRIB_FORMAT_SNORM_8_8         = GX2_ATTRIB_FLAG_SIGNED | GX2_ATTRIB_TYPE_8_8,
			GX2_ATTRIB_FORMAT_SNORM_8_8_8_8     = GX2_ATTRIB_FLAG_SIGNED | GX2_ATTRIB_TYPE_8_8_8_8,

			GX2_ATTRIB_FORMAT_SINT_8            = GX2_ATTRIB_FLAG_SIGNED | GX2_ATTRIB_FLAG_INTEGER | GX2_ATTRIB_TYPE_8,
			GX2_ATTRIB_FORMAT_SINT_8_8          = GX2_ATTRIB_FLAG_SIGNED | GX2_ATTRIB_FLAG_INTEGER | GX2_ATTRIB_TYPE_8_8,
			GX2_ATTRIB_FORMAT_SINT_8_8_8_8      = GX2_ATTRIB_FLAG_SIGNED | GX2_ATTRIB_FLAG_INTEGER | GX2_ATTRIB_TYPE_8_8_8_8,

			GX2_ATTRIB_FORMAT_FLOAT_32          = GX2_ATTRIB_FLAG_SCALED | GX2_ATTRIB_TYPE_32_FLOAT,
			GX2_ATTRIB_FORMAT_FLOAT_32_32       = GX2_ATTRIB_FLAG_SCALED | GX2_ATTRIB_TYPE_32_32_FLOAT,
			GX2_ATTRIB_FORMAT_FLOAT_32_32_32    = GX2_ATTRIB_FLAG_SCALED | GX2_ATTRIB_TYPE_32_32_32_FLOAT,
			GX2_ATTRIB_FORMAT_FLOAT_32_32_32_32 = GX2_ATTRIB_FLAG_SCALED | GX2_ATTRIB_TYPE_32_32_32_32_FLOAT,
		};



		/// <summary>
		/// Index type information
		/// </summary>
		public enum GX2AttribIndexType : uint
		{
			GX2_ATTRIB_INDEX_PER_VERTEX   = 0,
			GX2_ATTRIB_INDEX_PER_INSTANCE = 1,
		}



		/// <summary>
		/// Endian swap information
		/// </summary>
		public enum GX2EndianSwapMode : uint
		{
			GX2_ENDIAN_SWAP_NONE    = 0,
			GX2_ENDIAN_SWAP_8_IN_16 = 1,
			GX2_ENDIAN_SWAP_8_IN_32 = 2,
			GX2_ENDIAN_SWAP_DEFAULT = 3,
		}



		/// <summary>
		/// A different way of storing GX2AttribFormat and the associated mask
		/// </summary>
		private struct AlchemyGX2FormatInfo
		{
			public bool isSupported;
			public bool isSigned;
			public bool isNormalised;
			public bool isScaled;
			public uint mask;
			public GX2AttribFormat type;

			public AlchemyGX2FormatInfo(bool isSigned, bool isNormalised, bool isScaled, uint mask, GX2AttribFormat type)
			{
				this.isSupported  = true;
				this.isSigned     = isSigned;
				this.isNormalised = isNormalised;
				this.isScaled     = isScaled;
				this.mask         = mask;
				this.type         = type;
			}
			public AlchemyGX2FormatInfo()
			{
				this.isSupported  = false;
				this.isSigned     = false;
				this.isNormalised = false;
				this.isScaled     = false;
				this.mask         = 0x00_00_00_00;
				this.type         = GX2AttribFormat.GX2_ATTRIB_TYPE_8;
			}
		}



		/// <summary>
		/// Lookup table from alchemy IG_VERTEX_TYPE to format info for GX2 stuff
		/// </summary>
		private static AlchemyGX2FormatInfo[] kAlchemyGX2Formats =
		{
			/* Alchemy Type                                | sign | norm | scal | mask         | type */
			/*=============================================|======|======|======|==============|======*/
			/* IG_VERTEX_TYPE_FLOAT1                 */ new( false, false,  true, 0x00_04_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_FLOAT ),
			/* IG_VERTEX_TYPE_FLOAT2                 */ new( false, false,  true, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_FLOAT ),
			/* IG_VERTEX_TYPE_FLOAT3                 */ new( false, false,  true, 0x00_01_02_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_32_FLOAT ),
			/* IG_VERTEX_TYPE_FLOAT4                 */ new( false, false,  true, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_32_32_FLOAT ),
			/* IG_VERTEX_TYPE_UBYTE4N_COLOR          */ new( false, false, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UBYTE4N_COLOR_ARGB     */ new( false, false, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UBYTE4N_COLOR_RGBA     */ new( false, false, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UNDEFINED_0            */ new(),
			/* IG_VERTEX_TYPE_UBYTE2N_COLOR_5650     */ new(),
			/* IG_VERTEX_TYPE_UBYTE2N_COLOR_5551     */ new(),
			/* IG_VERTEX_TYPE_UBYTE2N_COLOR_4444     */ new(),
			/* IG_VERTEX_TYPE_INT1                   */ new(  true, false, false, 0x00_04_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32 ),
			/* IG_VERTEX_TYPE_INT2                   */ new(  true, false, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32 ),
			/* IG_VERTEX_TYPE_INT4                   */ new(  true, false, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_32 ),
			/* IG_VERTEX_TYPE_UINT1                  */ new( false, false, false, 0x00_04_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32 ),
			/* IG_VERTEX_TYPE_UINT2                  */ new( false, false, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32 ),
			/* IG_VERTEX_TYPE_UINT4                  */ new( false, false, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_32 ),
			/* IG_VERTEX_TYPE_INT1N                  */ new(  true,  true, false, 0x00_04_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32 ),
			/* IG_VERTEX_TYPE_INT2N                  */ new(  true,  true, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32 ),
			/* IG_VERTEX_TYPE_INT4N                  */ new(  true,  true, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_32 ),
			/* IG_VERTEX_TYPE_UINT1N                 */ new( false,  true, false, 0x00_04_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32 ),
			/* IG_VERTEX_TYPE_UINT2N                 */ new( false,  true, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32 ),
			/* IG_VERTEX_TYPE_UINT4N                 */ new( false,  true, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_32_32_32 ),
			/* IG_VERTEX_TYPE_UBYTE4                 */ new( false, false, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UBYTE4_X4              */ new( false, false, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_BYTE4                  */ new(  true, false, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UBYTE4N                */ new( false,  true, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UNDEFINED_1            */ new(),
			/* IG_VERTEX_TYPE_BYTE4N                 */ new(  true,  true, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_SHORT2                 */ new(  true, false, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16 ),
			/* IG_VERTEX_TYPE_SHORT4                 */ new(  true, false, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16_16_16 ),
			/* IG_VERTEX_TYPE_USHORT2                */ new( false, false, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16 ),
			/* IG_VERTEX_TYPE_USHORT4                */ new( false, false, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16_16_16 ),
			/* IG_VERTEX_TYPE_SHORT2N                */ new(  true,  true, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16 ),
			/* IG_VERTEX_TYPE_SHORT3N                */ new(),
			/* IG_VERTEX_TYPE_SHORT4N                */ new(  true,  true, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16_16_16 ),
			/* IG_VERTEX_TYPE_USHORT2N               */ new( false,  true, false, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16 ),
			/* IG_VERTEX_TYPE_USHORT3N               */ new(),
			/* IG_VERTEX_TYPE_USHORT4N               */ new( false,  true, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16_16_16 ),
			/* IG_VERTEX_TYPE_UDEC3                  */ new( false, false, false, 0x00_01_02_05, GX2AttribFormat.GX2_ATTRIB_TYPE_10_10_10_2 ),
			/* IG_VERTEX_TYPE_DEC3N                  */ new(  true,  true, false, 0x00_01_02_05, GX2AttribFormat.GX2_ATTRIB_TYPE_10_10_10_2 ),
			/* IG_VERTEX_TYPE_DEC3N_S11_11_10        */ new(), // Uncertain
			/* IG_VERTEX_TYPE_HALF2                  */ new( false, false,  true, 0x00_01_04_05, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16_FLOAT ),
			/* IG_VERTEX_TYPE_HALF4                  */ new( false, false,  true, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_16_16_16_16_FLOAT ),
			/* IG_VERTEX_TYPE_UNUSED                 */ new(),
			/* IG_VERTEX_TYPE_BYTE3N                 */ new(),
			/* IG_VERTEX_TYPE_SHORT3                 */ new(),
			/* IG_VERTEX_TYPE_USHORT3                */ new(),
			/* IG_VERTEX_TYPE_UBYTE4_ENDIAN          */ new( false, false, false, 0x00_01_02_03, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_UBYTE4_COLOR           */ new( false, false, false, 0x03_02_01_00, GX2AttribFormat.GX2_ATTRIB_TYPE_8_8_8_8 ),
			/* IG_VERTEX_TYPE_BYTE3                  */ new(),
			/* IG_VERTEX_TYPE_UBYTE2N_COLOR_5650_RGB */ new(), // Uncertain
			/* IG_VERTEX_TYPE_UDEC3_OES              */ new( false, false, false, 0x00_01_02_05, GX2AttribFormat.GX2_ATTRIB_TYPE_10_10_10_2 ),
			/* IG_VERTEX_TYPE_DEC3N_OES              */ new(  true,  true, false, 0x00_01_02_05, GX2AttribFormat.GX2_ATTRIB_TYPE_10_10_10_2 ),
			/* IG_VERTEX_TYPE_SHORT4N_EDGE           */ new(),
		};



		/// <summary>
		/// Utility Method to swap endianness
		/// </summary>
		/// <param name="data">The pointer to the data to endian swap</param>
		/// <param name="size">The length of the data to endian swap</param>
		private static unsafe void EndianSwap(byte* data, int size)
		{
			for (int i = 0; i < size / 2; i++)
			{
				byte temp = data[i];
				data[i] = data[size - i - 1];
				data[size - i - 1] = temp;
			}
		}



		/// <summary>
		/// Create a platform data array
		/// </summary>
		/// <param name="elements">The igVertexElements to create the array with</param>
		/// <returns></returns>
		public static unsafe igMemory<byte> GeneratePlatformData(igMemory<igVertexElement> elements)
		{
			int elementCount = elements.Count(x => x._type != (byte)IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNUSED);

			igMemory<byte> platformData = new igMemory<byte>(igMemoryContext.Vertex, (uint)elementCount * (uint)Marshal.SizeOf<GX2AttribStream>());

			// Pointers are fun, easier to write to, cry if you don't like this
			fixed(byte* pPlatformData = platformData.Buffer)
			{
				GX2AttribStream* attrib = (GX2AttribStream*)pPlatformData;

				for(int i = 0; i < elements.Length; i++, attrib++)
				{
					if (elements[i]._type == (byte)IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNUSED)
					{
						continue;
					}

					attrib->location    = 0;
					attrib->buffer      = 0;
					attrib->offset      = elements[i]._offset;
					attrib->format      = GetFormat((IG_VERTEX_TYPE)elements[i]._type);
					attrib->type        = GX2AttribIndexType.GX2_ATTRIB_INDEX_PER_VERTEX;
					attrib->aluDivisor  = 0;
					attrib->mask        = GetMask((IG_VERTEX_TYPE)elements[i]._type);
					attrib->endianSwap  = GX2EndianSwapMode.GX2_ENDIAN_SWAP_DEFAULT;

					// wiiu is big endian
					if (BitConverter.IsLittleEndian)
					{
						EndianSwap((byte*)&attrib->location,   sizeof(uint));
						EndianSwap((byte*)&attrib->buffer,     sizeof(uint));
						EndianSwap((byte*)&attrib->offset,     sizeof(uint));
						EndianSwap((byte*)&attrib->format,     sizeof(GX2AttribFormat));
						EndianSwap((byte*)&attrib->type,       sizeof(GX2AttribIndexType));
						EndianSwap((byte*)&attrib->aluDivisor, sizeof(uint));
						EndianSwap((byte*)&attrib->mask,       sizeof(uint));
						EndianSwap((byte*)&attrib->endianSwap, sizeof(GX2EndianSwapMode));
					}
				}
			}

			return platformData;
		}



		/// <summary>
		/// Returns the Wii U equivalent of an IG_VERTEX_TYPE
		/// </summary>
		/// <param name="type">The IG_VERTEX_TYPE in question</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">The type is not supported on Wii U</exception>
		/// <exception cref="ArgumentOutOfRangeException">The type passed in is not valid</exception>
		private static GX2AttribFormat GetFormat(IG_VERTEX_TYPE type)
		{
			if (type >= IG_VERTEX_TYPE.IG_VERTEX_TYPE_MAX
			 || (int)type >= kAlchemyGX2Formats.Length)
			{
				throw new ArgumentOutOfRangeException($"Vertex type {(uint)type} is out of range");
			}

			AlchemyGX2FormatInfo formatInfo = kAlchemyGX2Formats[(int)type];
			if (!formatInfo.isSupported)
			{
				throw new NotSupportedException($"Vertex type {(int)type} is not supported on Wii U");
			}

			GX2AttribFormat format = formatInfo.type;
			format |= formatInfo.isSigned     ? GX2AttribFormat.GX2_ATTRIB_FLAG_SIGNED : 0;
			format |= formatInfo.isScaled     ? GX2AttribFormat.GX2_ATTRIB_FLAG_SCALED : 0;
			format |= formatInfo.isNormalised ? 0 : GX2AttribFormat.GX2_ATTRIB_FLAG_INTEGER;

			return format;
		}



		/// <summary>
		/// Gets the mask for the vertex type
		/// The mask is essentially how the different xyzw is remapped when passed to the shader
		/// it also supports passing in 0 or 1 as values
		/// </summary>
		/// <param name="type">The IG_VERTEX_TYPE in question</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">The type is not supported on Wii U</exception>
		/// <exception cref="ArgumentOutOfRangeException">The type passed in is not valid</exception>
		private static uint GetMask(IG_VERTEX_TYPE type)
		{
			if (type >= IG_VERTEX_TYPE.IG_VERTEX_TYPE_MAX
			 || (int)type >= kAlchemyGX2Formats.Length)
			{
				throw new ArgumentOutOfRangeException($"Vertex type {(uint)type} is out of range");
			}

			AlchemyGX2FormatInfo formatInfo = kAlchemyGX2Formats[(int)type];
			if (!formatInfo.isSupported)
			{
				throw new NotSupportedException($"Vertex type {(int)type} is not supported on Wii U");
			}

			return formatInfo.mask;
		}
	}
}
