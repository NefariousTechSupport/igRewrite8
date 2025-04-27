using igLibrary.Core;
using igLibrary.Gfx;
using igLibrary.Graphics;
using igLibrary.Math;
using igLibrary.Render;
using igLibrary.Sg;
using Assimp;
using igLibrary.Anim;
using System.Numerics;
using System.Diagnostics;
using igLibrary.PS3Edge;
using System.Runtime.InteropServices;

namespace igLibrary.AssetConversion.Models
{
	/// <summary>
	/// Handles model import/export for SuperChargers/Imaginators
	/// </summary>
	public class SuperChargersModel : ModelImportExport<igModelInfo, CGraphicsSkinInfo>
	{
		private static Matrix4x4 kXYZToXZY = Matrix4x4.CreateRotationX(MathF.PI / 2);
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
				modelData._drawCalls.Append(ImportMesh(model.Meshes[i], false, null, null));
				modelData._drawCallTransformIndices.Append(0);
			}

			igModelInfo modelInfo = igMetaObject.ConstructInstance<igModelInfo>();
			modelInfo._name = "igSceneInfo0";
			modelInfo._directory = null;
			modelInfo._resolveState = true;
			modelInfo._modelData = modelData;

			return modelInfo;
		}

		public override CGraphicsSkinInfo ImportActor(Scene model)
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

			Dictionary<string, Bone> boneLookup = new Dictionary<string, Bone>();

			// Figure out the skeleton
			//Bone rootBone = model.Meshes[0].Bones[0];
			Node rootNode = model.RootNode.FindNode("Bip001");
			List<Node> bones = new List<Node>();
			PopulateBones(bones, rootNode);

			for(int i = 0; i < model.Meshes.Count; i++)
			{
				modelData._drawCalls.Append(ImportMesh(model.Meshes[i], true, modelData._blendMatrixIndices, bones));
				modelData._drawCallTransformIndices.Append(0);

				for (int b = 0; b < model.Meshes[i].BoneCount; b++)
				{
					Bone bone = model.Meshes[i].Bones[b];
					boneLookup.TryAdd(bone.Name, bone);
				}
			}

			igSkeleton2 skeleton = igMetaObject.ConstructInstance<igSkeleton2>();

			skeleton._inverseJointArray.Alloc(bones.Count);

			for (int i = 0; i < bones.Count; i++)
			{
				Node assimpNode = bones[i];
				igSkeletonBone alchemyBone = igMetaObject.ConstructInstance<igSkeletonBone>();

				Matrix4x4 transform = Matrix4x4.Transpose(GetGlobalTransform(assimpNode)) * kXYZToXZY;
				Matrix4x4.Invert(transform, out Matrix4x4 inverseOffsetMatrix);

				alchemyBone._blendMatrixIndex = i;
				alchemyBone._parentIndex      = bones.FindIndex(x => x.Name == assimpNode.Parent.Name) + 1;
				alchemyBone._translation      = transform.Translation;
				alchemyBone._name             = assimpNode.Name;

				skeleton._boneList.Append(alchemyBone);

				skeleton._inverseJointArray[i] = inverseOffsetMatrix;
			}

			CGraphicsSkinInfo skinInfo = igMetaObject.ConstructInstance<CGraphicsSkinInfo>();
			skinInfo._name = "CGraphicsSkinInfo";
			skinInfo._directory = null;
			skinInfo._resolveState = true;
			skinInfo._skin = modelData;
			skinInfo._havokSkeleton = null;
			skinInfo._skeleton = skeleton;

			return skinInfo;
		}

		private void PopulateBones(List<Node> bones, Node currentNode)
		{
			bones.Add(currentNode);
			for (int i = 0; i < currentNode.ChildCount; i++)
			{
				PopulateBones(bones, currentNode.Children[i]);
			}
		}

		private Matrix4x4 GetGlobalTransform(Node assimpNode)
		{
			if (assimpNode.Parent != null)
			{
				return GetGlobalTransform(assimpNode.Parent) * assimpNode.Transform;
			}
			return assimpNode.Transform;
		}

		private igModelDrawCallData ImportMesh(Mesh mesh, bool isActor, igVector<int> blendMatrixIndices, List<Node> bones)
		{
			igModelDrawCallData drawCall = igMetaObject.ConstructInstance<igModelDrawCallData>();

			drawCall._name = null;
			drawCall._min = new igVec4f(mesh.BoundingBox.Min.X, mesh.BoundingBox.Min.Y, mesh.BoundingBox.Min.Z, 0);
			drawCall._max = new igVec4f(mesh.BoundingBox.Max.X, mesh.BoundingBox.Max.Y, mesh.BoundingBox.Max.Z, 0);
			drawCall._materialHandle = new igHandle("Persephone_materials,Persephone,3e.main");
			drawCall._graphicsVertexBuffer = null;
			drawCall._graphicsIndexBuffer = null;
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

			if (isActor)
			{
				blendMatrixIndices.SetCapacity(blendMatrixIndices.GetCapacity() + mesh.BoneCount);
				drawCall._blendVectorOffset = (ushort)blendMatrixIndices._count;
				for (int i = 0; i < mesh.BoneCount; i++)
				{
					int blendMatrixIndex = bones.FindIndex(x => x.Name == mesh.Bones[i].Name);
					Debug.Assert(blendMatrixIndex >= 0);

					blendMatrixIndices.Append(blendMatrixIndex);
				}
				drawCall._blendVectorCount = (ushort)(blendMatrixIndices._count - drawCall._blendVectorOffset);
			}

			if (igRegistry.GetRegistry()._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3)
			{
				CreatePS3Buffers(mesh, drawCall, isActor);
			}
			else
			{
				CreateGraphicsBuffers(mesh, drawCall, isActor);
			}

			return drawCall;
		}

		private void CreateGraphicsBuffers(Mesh mesh, igModelDrawCallData drawCall, bool isActor)
		{
			bool hasBones = isActor && mesh.HasBones;

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
			vertexFormat._elements = new igMemory<igVertexElement>(igMemoryContext.Vertex, 6u + (hasBones ? 2u : 0u));
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_POSITION, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_TEXCOORD, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT2, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_NORMAL, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_TANGENT, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT3, true, ref offset, ref index);
			vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_COLOR, IG_VERTEX_TYPE.IG_VERTEX_TYPE_FLOAT4, true, ref offset, ref index);

			byte weightOffset = 0;
			byte indexOffset = 0;
			if (hasBones)
			{
				weightOffset = offset;
				vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_BLENDWEIGHTS, IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4, true, ref offset, ref index, 4);
				indexOffset = offset;
				vertexFormat._elements[index] = AllocateElement(IG_VERTEX_USAGE.IG_VERTEX_USAGE_BLENDINDICES, IG_VERTEX_TYPE.IG_VERTEX_TYPE_UBYTE4, true, ref offset, ref index, 4);
			}

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
				Array.Copy(BitConverter.GetBytes(mesh.Tangents.Count > 0 ? mesh.Tangents[i].X : 0),                     0, vertexData, i * vertexFormat._vertexSize + 0x20, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Tangents.Count > 0 ? mesh.Tangents[i].Y : 0),                     0, vertexData, i * vertexFormat._vertexSize + 0x24, sizeof(float));
				Array.Copy(BitConverter.GetBytes(mesh.Tangents.Count > 0 ? mesh.Tangents[i].Z : 0),                     0, vertexData, i * vertexFormat._vertexSize + 0x28, sizeof(float));
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

			if (hasBones)
			{
				byte[] affectedVerticies = new byte[mesh.VertexCount];
				for (int b = 0; b < mesh.Bones.Count; b++)
				{
					Bone bone = mesh.Bones[b];
					for (int w = 0; w < bone.VertexWeightCount; w++)
					{
						VertexWeight weight = bone.VertexWeights[w];

						if (affectedVerticies[weight.VertexID] == 4)
						{
							Logging.Error("Ignoring bone {0}'s influence on a vertex, as there were already a maximum of 4 assigned to that vertex");
						}
						else
						{
							vertexData[weight.VertexID * vertexFormat._vertexSize + weightOffset + affectedVerticies[weight.VertexID]] = (byte)(weight.Weight * 255);
							vertexData[weight.VertexID * vertexFormat._vertexSize + indexOffset  + affectedVerticies[weight.VertexID]] = (byte)b;

							affectedVerticies[weight.VertexID]++;
						}
					}
				}
			}

			vertexBuffer._data = new igMemory<byte>(igMemoryContext.Vertex, vertexData);
			vertexFormat._hashCode = igHash.Hash(vertexBuffer._data.Buffer);

			drawCall._graphicsIndexBuffer = igMetaObject.ConstructInstance<igGraphicsIndexBuffer>();
			drawCall._graphicsVertexBuffer = igMetaObject.ConstructInstance<igGraphicsVertexBuffer>();

			drawCall._graphicsIndexBuffer._usage = igResourceUsage.kUsageStatic;
			drawCall._graphicsIndexBuffer._indexBuffer = indexBuffer;
			drawCall._graphicsIndexBuffer._resource = 0;

			drawCall._graphicsVertexBuffer._usage = igResourceUsage.kUsageStatic;
			drawCall._graphicsVertexBuffer._vertexBuffer = vertexBuffer;
			drawCall._graphicsVertexBuffer._bufferResource = 0;
			drawCall._graphicsVertexBuffer._formatResource = 0;
		}

			return drawCall;
		private unsafe void CreatePS3Buffers(Mesh mesh, igModelDrawCallData drawCall, bool isActor)
		{
		}

		private igVertexElement AllocateElement(IG_VERTEX_USAGE usage, IG_VERTEX_TYPE type, bool shouldUse, ref byte offset, ref int index, byte count = 0)
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
			element._count = count;
			element._freq = 0;
			element._packTypeAndFracHint = 0;
			element._mapToElement = 0;
			element._usageIndex = 0;
			element._packDataOffset = 0;
			element._stream = 0;

			offset += (byte)(type.GetComponentSize() * (count == 0 ? 1u : count));
			index++;

			return element;
		}

		public override ModelData ExportActor(CGraphicsSkinInfo gameAsset)
		{
			throw new NotImplementedException();
		}
	}
}