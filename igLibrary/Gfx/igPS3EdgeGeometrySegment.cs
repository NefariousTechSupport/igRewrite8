namespace igLibrary.Gfx
{
	public class igPS3EdgeGeometrySegment : igObject
	{
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
	}
	public class igPS3EdgeGeometrySegmentList : igTObjectList<igPS3EdgeGeometrySegment>{}
}