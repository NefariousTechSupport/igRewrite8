using System.Runtime.InteropServices;
using igLibrary.PS3Edge;

namespace igLibrary.Gfx
{
	/// <summary>
	/// PS3 Implementation of vertex formats
	/// </summary>
	public class igVertexFormatPS3 : igVertexFormatPlatform
	{
		/// <summary>
		/// A structure representing an item in the _platformData field of igVertexFormat
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 0x08)]
		public struct VertexAttribute
		{
			[FieldOffset(0x00)] public byte unk00;
			[FieldOffset(0x01)] public byte attributeSize;
			[FieldOffset(0x02)] public byte componentCount;
			[FieldOffset(0x03)] public EDGE_GEOM_ATTRIBUTE_FORMAT format;
			[FieldOffset(0x04)] public byte unk04;
			[FieldOffset(0x05)] public byte unk05;
			[FieldOffset(0x06)] public byte usageIndex;
			[FieldOffset(0x07)] public byte offset;
		}


		/// <summary>
		/// Create a platform data array
		/// </summary>
		/// <param name="elements">The igVertexElements to create the array with</param>
		/// <returns></returns>
		public static unsafe igMemory<byte> GeneratePlatformData(igMemory<igVertexElement> elements)
		{
			igMemory<byte> platformData = new igMemory<byte>(igMemoryContext.Vertex, ((uint)elements.Length + 1u) * 0x08u);

			// Pointers are fun, easier to write to, cry if you don't like this
			fixed(byte* pPlatformData = platformData.Buffer)
			{
				VertexAttribute* attrib = (VertexAttribute*)pPlatformData;

				for(int i = 0; i < elements.Length; i++, attrib++)
				{
					attrib->unk00 = 0;
					attrib->attributeSize = ((IG_VERTEX_TYPE)elements[i]._type).GetComponentSize();
					attrib->componentCount = ((IG_VERTEX_TYPE)elements[i]._type).GetComponentCount();
					attrib->format = GetFormat((IG_VERTEX_TYPE)elements[i]._type);
					attrib->unk04 = 0;
					attrib->unk05 = 0;
					attrib->usageIndex = GetUsageIndex(elements[i]);
					attrib->offset = (byte)elements[i]._offset;
				}
			}

			return platformData;
		}


		/// <summary>
		/// Returns the ps3 equivalent of an IG_VERTEX_TYPE
		/// </summary>
		/// <param name="type">The IG_VERTEX_TYPE in question</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">The type is not supported on PS3</exception>
		/// <exception cref="ArgumentException">The type passed in is not valid</exception>
		private static EDGE_GEOM_ATTRIBUTE_FORMAT GetFormat(IG_VERTEX_TYPE type)
		{
			switch(type)
			{
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT1:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT2:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT4:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.F32;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4N_COLOR:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4N_COLOR_ARGB:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4N_COLOR_RGBA:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE2N_COLOR_5650:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE2N_COLOR_5551:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE2N_COLOR_4444:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE2N_COLOR_5650_RGB:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4N:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.U8N;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_INT1:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_INT2:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_INT4:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UINT1:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UINT2:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UINT4:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_INT1N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_INT2N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_INT4N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UINT1N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UINT2N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UINT4N:
					throw new NotSupportedException("shockingly 32 bit integers for vertices aren't supported on ps3");
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4_X4:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4_COLOR:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4_ENDIAN:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.U8;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_BYTE3:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_BYTE4:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_BYTE3N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_BYTE4N:
					throw new NotSupportedException("shockingly signed 8 bit integers for vertices aren't supported on ps3");
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT2:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT3:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT4:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.I16;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_USHORT2:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_USHORT3:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_USHORT4:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_USHORT2N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_USHORT3N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_USHORT4N:
					throw new NotSupportedException("shockingly unsigned 16 bit integers for vertices aren't supported on ps3");
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT2N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT3N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT4N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_SHORT4N_EDGE:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.I16N;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_DEC3N:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_DEC3N_S11_11_10:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_DEC3N_OES:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.X11Y11Z10N;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UDEC3:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UDEC3_OES:
					throw new NotSupportedException("unshockingly, unnormalised X11Y11Z10 aren't supported on ps3");
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_HALF2:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_HALF4:
					return EDGE_GEOM_ATTRIBUTE_FORMAT.F16;
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNDEFINED_0:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNDEFINED_1:
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_MAX:
				default:
					throw new ArgumentException("Invalid IG_VERTEX_TYPE passed");
				case IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNUSED:
					return (EDGE_GEOM_ATTRIBUTE_FORMAT)0;
			}
		}


		/// <summary>
		/// The usage index for the model
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private static byte GetUsageIndex(igVertexElement element)
		{
			// I believe this varies per game but am not sure
			switch(igArkCore.Game)
			{
				case igArkCore.EGame.EV_SkylandersSuperchargers:
					switch ((IG_VERTEX_USAGE)element._usage)
					{
						case IG_VERTEX_USAGE.IG_VERTEX_USAGE_UNUSED_0:     return 0x00;
						case IG_VERTEX_USAGE.IG_VERTEX_USAGE_POSITION:     return 0x00;
						case IG_VERTEX_USAGE.IG_VERTEX_USAGE_NORMAL:       return 0x02;
						case IG_VERTEX_USAGE.IG_VERTEX_USAGE_COLOR:        return 0x03;
						case IG_VERTEX_USAGE.IG_VERTEX_USAGE_TEXCOORD:     return (byte)(0x08 + element._usageIndex);
						case IG_VERTEX_USAGE.IG_VERTEX_USAGE_TANGENT:      return 0x0E;
						default: throw new NotImplementedException($"Vertex usage {element._usage} not implemented for PS3 model imports");
					}
				default:
					throw new NotImplementedException("Current game doesn't support model imports on PS3");
			}
		}
	}
}