using igLibrary.Graphics;
using igLibrary.Gfx;

namespace igLibrary.Render
{
	public class igModelDrawCallData : igNamedObject
	{
		public igVec4f _min;
		public igVec4f _max;
		public igHandle _materialHandle;
		public igGraphicsVertexBuffer _graphicsVertexBuffer;
		public igGraphicsIndexBuffer _graphicsIndexBuffer;
		public igObject _platformData;
		public ushort _blendVectorOffset;
		public ushort _blendVectorCount;
		public int _morphWeightTransformIndex;
		public int _primitiveCount;
		public uint _propertiesBitField;
		public igShaderConstantBundleList _shaderConstantBundles;
		public int _bakedBufferOffset;
		public uint _hash;
		public ulong _vertexBufferResource;
		public ulong _vertexBufferFormatResource;
		public ulong _indexBufferResource;
		public IG_INDEX_TYPE _indexBufferType;
		public IG_GFX_DRAW _primitiveType;
		public byte _lod;
		public bool _enabled;
		public byte _instanceShaderConstants;
	}
}