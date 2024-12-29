using igLibrary.Core;
using igLibrary.Gfx;
using igLibrary.Graphics;
using igLibrary.Math;
using igLibrary.Render;
using igLibrary.Sg;
using Assimp;

namespace igLibrary.AssetConversion.Models
{
	/// <summary>
	/// Handles model import/export for SuperChargers/Imaginators
	/// </summary>
	public class SuperChargersModel : ModelImportExport<igModelInfo>
	{
		public override ModelData ExportModel(igModelInfo gameAsset)
		{
			ModelData modelData = new ModelData(gameAsset._name);

			for(int i = 0; i < gameAsset._modelData._drawCalls._count; i++)
			{
				modelData.Meshes.Add(ExportMesh(gameAsset._modelData._drawCalls[i]));
			}

			return modelData;
		}

		private MeshData ExportMesh(igModelDrawCallData drawCall)
		{
			if(drawCall._platformData != null)
			{
				if(drawCall._platformData is igPS3EdgeGeometry ps3Geom) PS3GeometryExporter.ExportPS3Mesh(ps3Geom);
				throw new NotSupportedException();
			}
			else
			{
				MeshData mesh = new MeshData();
				return mesh;
			}
		}

		public override igModelInfo ImportModel(Scene model)
		{
			igModelData modelData = igMetaObject.ConstructInstance<igModelData>();
			modelData._name = null;
			modelData._min = new igVec4f( 1,  1,  1, 0);
			modelData._max = new igVec4f(-1, -1, -1, 1);
			modelData._transforms = new igVector<igAnimatedTransform>();
			modelData._transformHierarchy = new igVector<int>();
			modelData._drawCalls = new igVector<igModelDrawCallData>();
			modelData._drawCallTransformIndices = new igVector<int>();
			modelData._morphWeightTransforms = new igVector<igAnimatedMorphWeightsTransform>();
			modelData._blendMatrixIndices = new igVector<int>();

			for(int i = 0; i < model.Meshes.Count; i++)
			{
				modelData._drawCalls.Append(ImportMesh(model.Meshes[i]));
				modelData._drawCallTransformIndices.Append(0);
			}

			igModelInfo modelInfo = igMetaObject.ConstructInstance<igModelInfo>();
			modelInfo._name = "igSceneInfo0";
			modelInfo._directory = null;
			modelInfo._resolveState = true;
			modelInfo._modelData = modelData;

			return modelInfo;
		}

		private igModelDrawCallData ImportMesh(Mesh mesh)
		{
			List<Face> faces = mesh.Faces;
			igIndexBuffer indexBuffer = igMetaObject.ConstructInstance<igIndexBuffer>();
			indexBuffer._indexCount = (uint)faces.Count * 3;
			indexBuffer._indexCountArray = new igMemory<uint>();
			indexBuffer._format = new igHandle("indexformats." + igIndexFormat.GetFormatName(IG_INDEX_TYPE.IG_INDEX_TYPE_INT16, igRegistry.GetRegistry()._gfxPlatform, false)).GetObjectAlias<igIndexFormat>()!;
			indexBuffer._primitiveType = IG_GFX_DRAW.IG_GFX_DRAW_TRIANGLES;
			indexBuffer._vertexFormat = null;
			indexBuffer._indexArray = null;
			indexBuffer._indexArrayRefCount = 0;

			byte[] byteIndices = new byte[faces.Count * 6];

			for(int i = 0; i < faces.Count; i++)
			{
				Array.Copy(BitConverter.GetBytes((ushort)mesh.Faces[i].Indices[0]), 0x00, byteIndices, i * 0x06 + 0x00, sizeof(ushort));
				Array.Copy(BitConverter.GetBytes((ushort)mesh.Faces[i].Indices[1]), 0x00, byteIndices, i * 0x06 + 0x02, sizeof(ushort));
				Array.Copy(BitConverter.GetBytes((ushort)mesh.Faces[i].Indices[2]), 0x00, byteIndices, i * 0x06 + 0x04, sizeof(ushort));
			}

			if(igAlchemyCore.isPlatformBigEndian(igRegistry.GetRegistry()._platform))
			{
				for(int i = 0; i < byteIndices.Length; i += sizeof(ushort))
				{
					Array.Reverse(byteIndices, i, sizeof(ushort));
				}
			}

			indexBuffer._data = new igMemory<byte>(igMemoryContext.Default, byteIndices);


			igVertexFormat vertexFormat = igMetaObject.ConstructInstance<igVertexFormat>();
			vertexFormat._platform = IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT;
			vertexFormat._softwareBlendedFormat = null;
			vertexFormat._blender = null;
			vertexFormat._dynamic = false;
			vertexFormat._platformFormat = new igHandle("vertexformat.igvertexformatps3").GetObjectAlias<igVertexFormatPlatform>()!;
			vertexFormat._streams = new igMemory<igVertexStream>();
			vertexFormat._softwareBlendedMultistreamFormat = null;
			vertexFormat._enableSoftwareBlending = false;
			vertexFormat._cachedUsage = 0;

			byte offset = 0;
			int index = 0;
			vertexFormat._elements = new igMemory<igVertexElement>(igMemoryContext.Vertex, 6);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_POSITION, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_TEXCOORD, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT2, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_NORMAL, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_TANGENT, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_COLOR, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT4, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_UNUSED_0, IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNUSED, true, ref offset, ref index);
			vertexFormat._vertexSize = offset;
			vertexFormat._platformData = igVertexFormatPS3.GeneratePlatformData(vertexFormat._elements);

			indexBuffer._vertexFormat = vertexFormat;

			igVertexBuffer vertexBuffer = igMetaObject.ConstructInstance<igVertexBuffer>();
			vertexBuffer._vertexCount = (uint)mesh.Vertices.Count;
			vertexBuffer._vertexCountArray = new igMemory<uint>(igMemoryContext.Default, new uint[]{ vertexBuffer._vertexCount });
			vertexBuffer._format = vertexFormat;
			vertexBuffer._primitiveType = IG_GFX_DRAW.IG_GFX_DRAW_TRIANGLES;
			vertexBuffer._packData = new igMemory<byte>();
			vertexBuffer._vertexArray = null;
			vertexBuffer._vertexArrayRefCount = 0;

			byte[] vertexData = new byte[mesh.Vertices.Count * vertexFormat._vertexSize];
			for(int i = 0; i < mesh.Vertices.Count; i++)
			{
				Array.Copy(BitConverter.GetBytes(mesh.Vertices[i].X),                     0, vertexData, i * vertexFormat._vertexSize + 0x00, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Vertices[i].Y),                     0, vertexData, i * vertexFormat._vertexSize + 0x04, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Vertices[i].Z),                     0, vertexData, i * vertexFormat._vertexSize + 0x08, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.TextureCoordinateChannels[0][i].X), 0, vertexData, i * vertexFormat._vertexSize + 0x0C, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.TextureCoordinateChannels[0][i].Y), 0, vertexData, i * vertexFormat._vertexSize + 0x10, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Normals[i].X),                      0, vertexData, i * vertexFormat._vertexSize + 0x14, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Normals[i].Y),                      0, vertexData, i * vertexFormat._vertexSize + 0x18, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Normals[i].Z),                      0, vertexData, i * vertexFormat._vertexSize + 0x1C, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Tangents[i].X),                     0, vertexData, i * vertexFormat._vertexSize + 0x20, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Tangents[i].Y),                     0, vertexData, i * vertexFormat._vertexSize + 0x24, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Tangents[i].Z),                     0, vertexData, i * vertexFormat._vertexSize + 0x28, sizeof(float));
				Array.Copy(BitConverter.GetBytes(1.0f),                                   0, vertexData, i * vertexFormat._vertexSize + 0x2C, sizeof(float));
				Array.Copy(BitConverter.GetBytes(1.0f),                                   0, vertexData, i * vertexFormat._vertexSize + 0x30, sizeof(float));
				Array.Copy(BitConverter.GetBytes(1.0f),                                   0, vertexData, i * vertexFormat._vertexSize + 0x34, sizeof(float));
				Array.Copy(BitConverter.GetBytes(1.0f),                                   0, vertexData, i * vertexFormat._vertexSize + 0x38, sizeof(float));
			}

			if(igAlchemyCore.isPlatformBigEndian(igRegistry.GetRegistry()._platform))
			{
				for(int i = 0; i < vertexData.Length; i += 4)
				{
					Array.Reverse(vertexData, i, 4);
				}
			}

			vertexBuffer._data = new igMemory<byte>(igMemoryContext.Vertex, vertexData);
			vertexFormat._hashCode = igHash.Hash(vertexBuffer._data.Buffer);
			

			igModelDrawCallData drawCall = igMetaObject.ConstructInstance<igModelDrawCallData>();

			drawCall._name = null;
			drawCall._min = new igVec4f(mesh.BoundingBox.Min.X, mesh.BoundingBox.Min.Y, mesh.BoundingBox.Min.Z, 0);
			drawCall._max = new igVec4f(mesh.BoundingBox.Max.X, mesh.BoundingBox.Max.Y, mesh.BoundingBox.Max.Z, 0);
			drawCall._materialHandle = new igHandle("Persephone_materials,Persephone,3e.main");
			drawCall._graphicsIndexBuffer = igMetaObject.ConstructInstance<igGraphicsIndexBuffer>();
			drawCall._graphicsVertexBuffer = igMetaObject.ConstructInstance<igGraphicsVertexBuffer>();
			drawCall._platformData = null;
			drawCall._blendVectorOffset = 0;
			drawCall._blendVectorCount = 0;
			drawCall._morphWeightTransformIndex = 0;
			drawCall._primitiveCount = mesh.FaceCount;
			drawCall._shaderConstantBundles = null;
			drawCall._bakedBufferOffset = -1;
			drawCall._hash = unchecked((uint)mesh.GetHashCode());
			drawCall._vertexBufferResource = 0;
			drawCall._vertexBufferFormatResource = 0;
			drawCall._indexBufferResource = 0;
			drawCall._indexBufferType = IG_INDEX_TYPE.IG_INDEX_TYPE_INT32;
			drawCall._primitiveType = IG_GFX_DRAW.IG_GFX_DRAW_TRIANGLES;
			drawCall._lod = 1;
			drawCall._enabled = true;
			drawCall._instanceShaderConstants = (byte)igInstanceShaderConstants.kHasVertexColor;

			drawCall._graphicsIndexBuffer._usage = igResourceUsage.kUsageStatic;
			drawCall._graphicsIndexBuffer._indexBuffer = indexBuffer;
			drawCall._graphicsIndexBuffer._resource = 0;

			drawCall._graphicsVertexBuffer._usage = igResourceUsage.kUsageStatic;
			drawCall._graphicsVertexBuffer._vertexBuffer = vertexBuffer;
			drawCall._graphicsVertexBuffer._bufferResource = 0;
			drawCall._graphicsVertexBuffer._formatResource = 0;

			return drawCall;
		}
		private igVertexElement AllocateElement(IG_VERTEX_USAGE usage, IG_VERTEX_TYPE type, bool shouldUse, ref byte offset, ref int index)
		{
			if (!shouldUse)
			{
				return default;
			}

			igVertexElement element = new igVertexElement();
			element._type = (byte)type;
			element._usage = (byte)usage;

			if(type == IG_VERTEX_TYPE.IG_VERTEX_TYPE_UNUSED)
			{
				return element;
			}

			element._offset = offset;
			element._count = 0;
			element._freq = 0;
			element._packTypeAndFracHint = 0;
			element._mapToElement = 0;
			element._usageIndex = 0;
			element._packDataOffset = 0;
			element._stream = 0;

			offset += type.GetComponentSize();
			index++;

			return element;
		}
	}
}