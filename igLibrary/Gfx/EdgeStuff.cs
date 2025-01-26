/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.InteropServices;

//Should be noted that this isn't a library of Alchemy but rather belongs to the PS3's Edge SDK 
//Also, add an example class + support for bones

namespace igLibrary.PS3Edge
{
	[StructLayout(LayoutKind.Explicit, Size = 0x10)]
	public struct EdgeGeomSpuConfigInfo
	{
		[FieldOffset(0x00)] public byte flagsAndUniformTableCount;
		[FieldOffset(0x01)] public byte commandBufferHoldSize;
		[FieldOffset(0x02)] public byte inputVertexFormatId;
		[FieldOffset(0x03)] public byte secondaryInputVertexFormatId;
		[FieldOffset(0x04)] public byte outputVertexFormatId;
		[FieldOffset(0x05)] public byte vertexDeltaFormatId;
		[FieldOffset(0x06)] public byte indexesFlavorAndSkinningFlavor;
		[FieldOffset(0x07)] public byte skinningMatrixFormat;
		[FieldOffset(0x08)] public ushort numVertexes;
		[FieldOffset(0x0A)] public ushort numIndexes;
		[FieldOffset(0x0C)] public uint indexesOffset;
	}

	[StructLayout(LayoutKind.Explicit, Size = 0x08)]
	public struct EdgeGeomVertexStreamDescription
	{
		[FieldOffset(0x00)] public byte numAttributes;
		[FieldOffset(0x01)] public byte stride;
		[FieldOffset(0x02)] public byte numBlocks;
		//5 bytes padding
		//EdgeGeomGenericBlock starts at end of structure
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x10)]
	public struct EdgeGeomGenericBlock
	{
		[FieldOffset(0x00)] public EdgeGeomAttributeBlock attributeBlock;
		[FieldOffset(0x08)] public EdgeGeomAttributeFixedBlock fixedBlock;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x08)]
	public struct EdgeGeomAttributeBlock
	{
		[FieldOffset(0x00)] public byte offset;
		[FieldOffset(0x01)] public EDGE_GEOM_ATTRIBUTE_FORMAT format;
		[FieldOffset(0x02)] public byte componentCount;
		[FieldOffset(0x03)] public EDGE_GEOM_ATTRIBUTE_ID edgeAttributeId;
		[FieldOffset(0x04)] public byte size;
		[FieldOffset(0x05)] public byte vertexProgramSlotIndex;
		[FieldOffset(0x06)] public byte fixedBlockOffset;
		[FieldOffset(0x07)] public byte padding;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x08)]
	public struct EdgeGeomAttributeFixedBlock
	{
		[FieldOffset(0x00)] public byte integer0;
		[FieldOffset(0x01)] public byte mantissa0;
		[FieldOffset(0x02)] public byte integer1;
		[FieldOffset(0x03)] public byte mantissa1;
		[FieldOffset(0x04)] public byte integer2;
		[FieldOffset(0x05)] public byte mantissa2;
		[FieldOffset(0x06)] public byte integer3;
		[FieldOffset(0x07)] public byte mantissa3;
	}
	public enum EDGE_GEOM_ATTRIBUTE_FORMAT : byte
	{
		I16N		= 0x01,
		F32			= 0x02,
		F16			= 0x03,
		U8N			= 0x04,
		I16			= 0x05,
		X11Y11Z10N	= 0x06,
		U8			= 0x07,
		FIXED_POINT	= 0x08,
		UNIT_VECTOR	= 0x09
	}
	public enum EDGE_GEOM_ATTRIBUTE_ID : byte
	{
		UNKNOWN				= 0x00,
		POSITION			= 0x01,
		NORMAL				= 0x02,
		TANGENT				= 0x03,
		BINORMAL			= 0x04,
		UV0					= 0x05,
		UV1					= 0x06,
		UV2					= 0x07,
		UV3					= 0x08,
		COLOR				= 0x09,
		INSTANCED_COLOR		= 0x10
	}
	public enum EDGE_GEOM_SKIN : byte
	{
		NONE							= 0,
		NO_SCALING						= 1,
		UNIFORM_SCALING					= 2,
		NON_UNIFORM_SCALING				= 3,
		SINGLE_BONE_NO_SCALING			= 4,
		SINGLE_BONE_UNIFORM_SCALING		= 5,
		SINGLE_BONE_NON_UNIFORM_SCALING	= 6
	}

	public class EdgeGeomVertexConversion
	{
		public static float[] edgeUnpack_UNKNOWN(StreamHelper sh) =>		throw new ArgumentException("EDGE_GEOM_ATTRIBUTE_FORMAT_UNKNOWN is invalid");
		public static float[] edgeUnpack_I16N(StreamHelper sh) =>			new float[] { (float)sh.ReadInt16() / 0x7FFF };
		public static float[] edgeUnpack_F32(StreamHelper sh) =>			new float[] { sh.ReadSingle() };
		public static float[] edgeUnpack_F16(StreamHelper sh) =>			new float[] { (float)sh.ReadHalf() };
		public static float[] edgeUnpack_U8N(StreamHelper sh) =>			new float[] { (float)sh.ReadByte() / 0xFF };
		public static float[] edgeUnpack_I16(StreamHelper sh) =>			new float[] { (float)sh.ReadInt16() };
		public static float[] edgeUnpack_X11Y11Z10N(StreamHelper sh)
		{
			uint raw = sh.ReadUInt32();
			float[] processed = new float[3];
			processed[0] = (float)((raw & 0x000007FF) >> 0x00) / 0x7FF;
			processed[1] = (float)((raw & 0x003FF800) >> 0x0B) / 0x7FF;
			processed[2] = (float)((raw & 0xFFC00000) >> 0x16) / 0x3FF;
			return processed;
		}
		public static float[] edgeUnpack_U8(StreamHelper sh) =>				new float[] {(float)sh.ReadByte()};
		public static float[] edgeUnpack_FIXED_POINT(StreamHelper sh) =>	throw new NotImplementedException("EDGE_GEOM_ATTRIBUTE_FORMAT_FIXED_POINT Not Implemented");
		public static float[] edgeUnpack_UNIT_VECTOR(StreamHelper sh) =>	throw new NotImplementedException("EDGE_GEOM_ATTRIBUTE_FORMAT_UNIT_VECTOR Not Implemented");

		private static Func<StreamHelper, float[]>[] edgeUnpackFunctions =
		{
			sh => edgeUnpack_UNKNOWN(sh),
			sh => edgeUnpack_I16N(sh),
			sh => edgeUnpack_F32(sh),
			sh => edgeUnpack_F16(sh),
			sh => edgeUnpack_U8N(sh),
			sh => edgeUnpack_I16(sh),
			sh => edgeUnpack_X11Y11Z10N(sh),
			sh => edgeUnpack_U8N(sh),
			sh => edgeUnpack_FIXED_POINT(sh),
			sh => edgeUnpack_UNIT_VECTOR(sh),
		};

		//Convert the vertex buffer for an attribute into a byte array of floats
		public static void UnpackBufferForAttribute(in byte[] inBuffer, EdgeGeomSpuConfigInfo spuConfigInfo, EdgeGeomVertexStreamDescription streamDesc, EdgeGeomAttributeBlock attributeBlock, out float[] outBuffer)
		{
			StreamHelper sh = new StreamHelper(new MemoryStream(inBuffer), StreamHelper.Endianness.Big);
			outBuffer = new float[spuConfigInfo.numVertexes * attributeBlock.componentCount];

			int currentIndex = 0;

			for(int i = 0; i < spuConfigInfo.numVertexes; i++)
			{
				sh.Seek(i * streamDesc.stride + attributeBlock.offset);
				for(int j = 0; j < attributeBlock.componentCount; j++)
				{
					float[] processed = edgeUnpackFunctions[(uint)attributeBlock.format].Invoke(sh);
					
					Array.Copy(processed, 0, outBuffer, currentIndex, processed.Length);
					
					currentIndex += processed.Length;

					if(attributeBlock.format == EDGE_GEOM_ATTRIBUTE_FORMAT.X11Y11Z10N) break;
				}
			}
		}

		//Thanks to chroxx for reverse engineering this. (https://github.com/Danilodum/noesis-plugins-official/blob/master/chrrox/import/beta/psa.py)
		public static void UnpackIndexBuffer(in byte[] inBuffer, EdgeGeomSpuConfigInfo spuConfigInfo, out uint[] outBuffer)
		{
			StreamHelper indexes = new StreamHelper(new MemoryStream(inBuffer), StreamHelper.Endianness.Big);
			indexes.Seek(0);
			uint numIndices = indexes.ReadUInt16();
			int indexBase = (int)indexes.ReadUInt16();
			uint sequenceSize = indexes.ReadUInt16();
			byte indexBitSize = indexes.ReadByte();

			indexes.Seek(0x08);

			uint sequenceCount = sequenceSize * 8;
			byte[] sequenceBytes = indexes.ReadBytes((int)sequenceSize);	//array of 1 bit values
			StreamHelper sequenceStream = new StreamHelper(new MemoryStream(sequenceBytes), StreamHelper.Endianness.Big);

			uint triangleCount = spuConfigInfo.numIndexes / 3u;
			uint triangleSize = ((triangleCount * 2) + 7) / 8;
			byte[] triangleBytes = indexes.ReadBytes((int)triangleSize);	//array of 2 bit values
			StreamHelper triangleStream = new StreamHelper(new MemoryStream(triangleBytes), StreamHelper.Endianness.Big);

			uint indicesSize = (numIndices * indexBitSize) + 7 / 8;
			byte[] indicesBytes = indexes.ReadBytes((int)indicesSize);	//array of indexBitSize bit values
			StreamHelper indicesStream = new StreamHelper(new MemoryStream(indicesBytes), StreamHelper.Endianness.Big);

			int[] deltaIndices = new int[numIndices];
			for(uint i = 0; i < numIndices; i++)
			{
				int value = (int)indicesStream.ReadUIntN(indexBitSize);
				value -= indexBase;
				if(i > 7)
				{
					value += deltaIndices[i - 8];
				}
				deltaIndices[i] = value;
			}

			//create sequence indices

			uint[] inputIndices = new uint[sequenceCount];
			uint sequencedIndex = 0;
			uint unorderedIndex = 0;
			for(int i = 0; i < sequenceCount; i++)
			{
				bool value = sequenceStream.ReadBit();
				if(!value)
				{
					inputIndices[i] = sequencedIndex;
					sequencedIndex++;
				}
				else
				{
					inputIndices[i] = (uint)deltaIndices[unorderedIndex];
					unorderedIndex++;
				}
			}

			//create triangle indices

			uint[] outputIndices = new uint[triangleCount * 3];
			uint currentIndex = 0;
			uint[] triangleIndices = new uint[3];
			for(int i = 0; i < triangleCount; i++)
			{
				uint value = (uint)triangleStream.ReadUIntN(2);

				switch(value)
				{
					case 0:
					case 1:
						triangleIndices[1 - value] = triangleIndices[2];
						triangleIndices[2] = inputIndices[currentIndex];
						currentIndex++;
						break;
					case 2:
						uint tempIndex = triangleIndices[0];
						triangleIndices[0] = triangleIndices[1];
						triangleIndices[1] = tempIndex;
						triangleIndices[2] = inputIndices[currentIndex];
						currentIndex++;
						break;
					case 3:
						triangleIndices[0] = inputIndices[currentIndex];
						currentIndex++;
						triangleIndices[1] = inputIndices[currentIndex];
						currentIndex++;
						triangleIndices[2] = inputIndices[currentIndex];
						currentIndex++;
						break;
				}

				outputIndices[i * 3 + 0] = triangleIndices[0];
				outputIndices[i * 3 + 1] = triangleIndices[1];
				outputIndices[i * 3 + 2] = triangleIndices[2];
			}
			outBuffer = outputIndices;
		}
	}
}