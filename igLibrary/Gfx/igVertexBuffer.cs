namespace igLibrary.Gfx
{
	public class igVertexBuffer : igObject
	{
		public uint _vertexCount;
		public igMemory<uint> _vertexCountArray;
		public igMemory<byte> _data;
		public igVertexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public igMemory<byte> _packData;
		public igVertexArray? _vertexArray;
		public int _vertexArrayRefCount;
	}
}