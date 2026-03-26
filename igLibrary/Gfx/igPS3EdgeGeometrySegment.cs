/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.InteropServices;
using igLibrary.PS3Edge;

namespace igLibrary.Gfx
{
	public class igPS3EdgeGeometrySegment : igObject
	{
		public byte SpuVertexes0Stride => _spuInputStreamDescs0[1];
		public byte SpuVertexes1Stride => _spuInputStreamDescs1[1];
		public byte SpuVertexesOutStride => _spuOutputStreamDesc[1];
		public byte RsxVertexesStride  => _rsxOnlyStreamDesc[1];

		public igMemory<byte> _spuConfigInfo;
		public igMemory<byte> _indexes;
		public ushort[] _indexesSizes;
		public igMemory<byte> _spuVertexes0;
		public igMemory<byte> _spuVertexes1;
		public ushort[] _spuVertexesSizes;
		public igMemory<byte> _rsxOnlyVertexes;
		public uint _rsxOnlyVertexesSize;
		public ushort[] _skinMatricesByteOffsets;
		public ushort[] _skinMatricesSizes;
		public ushort[] _skinIndexesAndWeightsSizes;
		public igMemory<byte> _skinIndexesAndWeights;
		public uint _ioBufferSize;
		public uint _scratchSize;
		public igMemory<byte> _spuInputStreamDescs0;
		public igMemory<byte> _spuInputStreamDescs1;
		public igMemory<byte> _spuOutputStreamDesc;
		public igMemory<byte> _rsxOnlyStreamDesc;
		public ushort[] _spuInputStreamDescSizes;
		public ushort _spuOutputStreamDescSize;
		public ushort _rsxOnlyStreamDescSize;
		public uint _numBlendShapes;
		public igVector<ushort> _blendShapeSizes;
		public igMemory<byte> _blendShapeData;
		public igVector<ulong> _blendShapes;
		public int _speedTreeType;

		public unsafe void SetStreamDesc(EPS3StreamDesc streamDesc, EdgeGeomAttributeBlock[] attributes)
		{
			byte kStreamDescSize = (byte)Marshal.SizeOf<EdgeGeomVertexStreamDescription>();
			byte kAttributeSize  = (byte)Marshal.SizeOf<EdgeGeomAttributeBlock>();

			// Set up stream desc
			EdgeGeomVertexStreamDescription desc = new EdgeGeomVertexStreamDescription();
			desc.numAttributes = (byte)attributes.Length;
			desc.numBlocks     = (byte)attributes.Length;

			// Determine stride
			for (int i = 0; i < attributes.Length; i++)
			{
				desc.stride = (byte)System.Math.Max(desc.stride, attributes[i].offset + attributes[i].size);
			}

			// Select destination
			ref igMemory<byte> streamDescMemory = ref _spuInputStreamDescs0;
			switch (streamDesc)
			{
				case EPS3StreamDesc.Spu0:
					streamDescMemory = ref _spuInputStreamDescs0;
					break;
				case EPS3StreamDesc.Spu1:
					streamDescMemory = ref _spuInputStreamDescs1;
					break;
				case EPS3StreamDesc.SpuOut:
					streamDescMemory = ref _spuOutputStreamDesc;
					break;
				case EPS3StreamDesc.RsxOnly:
					streamDescMemory = ref _rsxOnlyStreamDesc;
					break;
			}

			streamDescMemory.Alloc(kStreamDescSize + desc.stride * desc.numAttributes);

			// Copy data to destination, it's all single byte fields so no need to
			// endian swap
			Marshal.Copy(new IntPtr(&desc), streamDescMemory.Buffer, 0, kStreamDescSize);

			fixed (EdgeGeomAttributeBlock* attributePtr = attributes)
			{
				Marshal.Copy(new IntPtr(attributePtr), streamDescMemory.Buffer, kStreamDescSize, kAttributeSize * attributes.Length);
			}
		}
	}
	public class igPS3EdgeGeometrySegmentList : igTObjectList<igPS3EdgeGeometrySegment>{}

	public enum EPS3StreamDesc
	{
		Spu0,
		Spu1,
		SpuOut,
		RsxOnly
	}
}