namespace igLibrary.Gfx
{
	[igStruct]
	public struct igVertexElement
	{
		public byte _type;
		public byte _stream;
		public byte _mapToElement;
		public byte _count;
		public byte _usage;
		public byte _usageIndex;
		public byte _packDataOffset;
		public byte _packTypeAndFracHint;
		public ushort _offset;
		public ushort _freq;
	}
}