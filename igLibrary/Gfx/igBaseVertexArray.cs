namespace igLibrary.Gfx
{
	public class igBaseVertexArray : igObject
	{
		public uint _vertexCount;
		public ulong _vertexCountArray;
		public uint _vertexCounts;
		public igVertexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public igMemory<byte> _packData;
		public uint _size;
		public igVertexBuffer _buffer;
		public ulong _platformBuffer;
		public igVertexArray _softwareBlendedArray;
		public uint _softwareBlendedSequenceId;
		public bool _skinned;
		public bool _transient;
		public uint _cacheFlushSequenceId;
		public bool _discardGeometry;
	}
}