using System.Numerics;
using igLibrary.Core;

namespace igLibrary.AssetConversion.Models
{
	public class ModelData
	{
		public List<MeshData> Meshes => _meshes;
		public string Name
		{
			get => _name;
			set => _name = value;
		}

		private List<MeshData> _meshes;
		private string _name;


		public ModelData(string name)
		{
			_meshes = new List<MeshData>();
			_name = name;
		}
	}
	public class MeshData
	{
		public EPrimitiveType PrimitiveType
		{
			get => _primitiveType;
			set => _primitiveType = value;
		}
		public uint[]? Indices => _indices;
		public VertexData[]? Vertices => _vertices;
		public string Name
		{
			get => _name;
			set => _name = value;
		}


		private EPrimitiveType _primitiveType;
		private uint[]? _indices;
		private VertexData[]? _vertices;
		private string _name;

		public void AllocateIndices(uint indexCount)
		{
			_indices = new uint[indexCount];
		}

		public void AllocateVertices(uint vertexCount)
		{
			_vertices = new VertexData[vertexCount];
		}
		public int GetPrimitiveCount()
		{
			if(_indices == null)
			{
				return 0;
			}
			else
			{
				int primitiveLength = 0;
				switch(_primitiveType)
				{
					case EPrimitiveType.Triangle:
						primitiveLength = 3;
						break;
					default:
						throw new NotImplementedException();
				}

				return _indices.Length / primitiveLength;
			}
		}
		public byte[] GetIndexBufferBytes()
		{
			if(_indices == null)
			{
				return Array.Empty<byte>();
			}

			byte[] data = new byte[_indices.Length * sizeof(uint)];

			Buffer.BlockCopy(_indices, 0, data, 0, data.Length);

			if(igAlchemyCore.isPlatformBigEndian(igRegistry.GetRegistry()._platform))
			{
				for(int i = 0; i < data.Length; i += 4)
				{
					Array.Reverse(data, i, 4);
				}
			}

			return data;
		}
	}
	public struct VertexData
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector3 bitangent;
		public Vector2 uv;
		public Vector4 colour;

		public VertexData(float xpos, float ypos, float zpos)
		{
			position = new Vector3(xpos, ypos, zpos);
			normal = default;
			tangent = default;
			bitangent = default;
			uv = default;
			colour = default;
		}
		public VertexData(float xpos, float ypos, float zpos, float utexcoord, float vtexcoord)
		{
			position = new Vector3(xpos, ypos, zpos);
			normal = default;
			tangent = default;
			bitangent = default;
			uv = new Vector2(utexcoord, vtexcoord);
			colour = default;
		}
	}
	public enum EPrimitiveType
	{
		Triangle
	}
}