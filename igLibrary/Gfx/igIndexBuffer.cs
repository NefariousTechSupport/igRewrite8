namespace igLibrary.Gfx
{
	public class igIndexBuffer : igObject
	{
		public uint _indexCount;
		public igMemory<uint> _indexCountArray;
		public igMemory<byte> _data;
		public igIndexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public igVertexFormat? _vertexFormat;
		public igIndexArray2? _indexArray;
		public int _indexArrayRefCount;
	}
}