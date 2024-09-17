namespace igLibrary.Gfx
{
	public class igBaseIndexArray : igObject
	{
		public uint _indexCount;
		public ulong _indexCountArray;
		public uint _indexCounts;
		public igIndexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public uint _size;
		public igIndexBuffer _buffer;
		public ulong _platformBuffer;
		public byte[] _vertexFormat;
		public bool _discardGeometry;
	}
}