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
			modelData._min = default;
			modelData._max = default;
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

				BoundingBox meshBoundingBox = model.Meshes[i].BoundingBox;
				modelData._min._x = MathF.Min(modelData._min._x, meshBoundingBox.Min.X);
				modelData._min._y = MathF.Min(modelData._min._y, meshBoundingBox.Min.Y);
				modelData._min._z = MathF.Min(modelData._min._z, meshBoundingBox.Min.Z);
				modelData._max._x = MathF.Max(modelData._max._x, meshBoundingBox.Max.X);
				modelData._max._y = MathF.Max(modelData._max._y, meshBoundingBox.Max.Y);
				modelData._max._z = MathF.Max(modelData._max._z, meshBoundingBox.Max.Z);
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
			modelData._min = default;
			modelData._max = default;
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

				BoundingBox meshBoundingBox = model.Meshes[i].BoundingBox;
				modelData._min._x = MathF.Min(modelData._min._x, meshBoundingBox.Min.X);
				modelData._min._y = MathF.Min(modelData._min._y, meshBoundingBox.Min.Y);
				modelData._min._z = MathF.Min(modelData._min._z, meshBoundingBox.Min.Z);
				modelData._max._x = MathF.Max(modelData._max._x, meshBoundingBox.Max.X);
				modelData._max._y = MathF.Max(modelData._max._y, meshBoundingBox.Max.Y);
				modelData._max._z = MathF.Max(modelData._max._z, meshBoundingBox.Max.Z);
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
			switch (igRegistry.GetRegistry()._gfxPlatform)
			{
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
					vertexFormat._platformData = igVertexFormatPS3.GeneratePlatformData(vertexFormat._elements);
					break;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE:
					vertexFormat._platformData = igVertexFormatCafe.GeneratePlatformData(vertexFormat._elements);
					break;
				default:
					throw new NotImplementedException($"Platform {igRegistry.GetRegistry()._gfxPlatform} unimplemented");
			}

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

		private unsafe void CreatePS3Buffers(Mesh mesh, igModelDrawCallData drawCall, bool isActor)
		{
			Debug.Assert(mesh.VertexCount <= 0xFFFF);

			bool hasBones = mesh.HasBones && isActor;
			igPS3EdgeGeometry geometry = igMetaObject.ConstructInstance<igPS3EdgeGeometry>(igMemoryContext.Geometry);
			geometry._isMorphed        = false;
			geometry._isSkinned        = hasBones;
			geometry._isSpeedTree      = false;
			geometry._isVertexAnimated = false;
			geometry._hasVertexColor   = true;

			igPS3EdgeGeometrySegment segment = igMetaObject.ConstructInstance<igPS3EdgeGeometrySegment>(igMemoryContext.Geometry);
			EdgeGeomSpuConfigInfo spuConfigInfo = new EdgeGeomSpuConfigInfo();

			// Setup indices
			uint[] indexBuffer = mesh.GetUnsignedIndices().ToArray();
			EdgeGeomVertexConversion.PackIndexBuffer(indexBuffer, ref spuConfigInfo, out byte[] compressedIndices);

			segment._indexes = new igMemory<byte>(igMemoryContext.VertexEdge, compressedIndices);
			segment._indexesSizes[0] = (ushort)compressedIndices.Length;

			// Setup vertex stream descs
			EdgeGeomAttributeBlock[] spuVertexesAttrs0 = new EdgeGeomAttributeBlock[1];
			EdgeGeomAttributeBlock[] spuVertexesAttrs1 = new EdgeGeomAttributeBlock[2];
			EdgeGeomAttributeBlock[] rsxVertexesAttrs  = new EdgeGeomAttributeBlock[2];

			spuVertexesAttrs0[0] = new EdgeGeomAttributeBlock()
			{
				offset                 = 0x00,
				format                 = EDGE_GEOM_ATTRIBUTE_FORMAT.I16N,
				componentCount         = 4,
				edgeAttributeId        = EDGE_GEOM_ATTRIBUTE_ID.POSITION,
				size                   = sizeof(short) * 4,
				vertexProgramSlotIndex = igVertexFormatPS3.GetVPSlotIndex(IG_VERTEX_USAGE.IG_VERTEX_USAGE_POSITION, 0),
				fixedBlockOffset       = 0,
				padding                = 0
			};

			spuVertexesAttrs1[0] = new EdgeGeomAttributeBlock()
			{
				offset                 = 0x00,
				format                 = EDGE_GEOM_ATTRIBUTE_FORMAT.X11Y11Z10N,
				componentCount         = 1,
				edgeAttributeId        = EDGE_GEOM_ATTRIBUTE_ID.NORMAL,
				size                   = sizeof(uint),
				vertexProgramSlotIndex = igVertexFormatPS3.GetVPSlotIndex(IG_VERTEX_USAGE.IG_VERTEX_USAGE_NORMAL, 0),
				fixedBlockOffset       = 0,
				padding                = 0
			};
			spuVertexesAttrs1[1] = new EdgeGeomAttributeBlock()
			{
				offset                 = 0x06,
				format                 = EDGE_GEOM_ATTRIBUTE_FORMAT.X11Y11Z10N,
				componentCount         = 1,
				edgeAttributeId        = EDGE_GEOM_ATTRIBUTE_ID.TANGENT,
				size                   = sizeof(uint),
				vertexProgramSlotIndex = igVertexFormatPS3.GetVPSlotIndex(IG_VERTEX_USAGE.IG_VERTEX_USAGE_TANGENT, 0),
				fixedBlockOffset       = 0,
				padding                = 0
			};

			rsxVertexesAttrs[0] = new EdgeGeomAttributeBlock()
			{
				offset                 = 0x00,
				format                 = EDGE_GEOM_ATTRIBUTE_FORMAT.F16,
				componentCount         = 2,
				edgeAttributeId        = EDGE_GEOM_ATTRIBUTE_ID.UV0,
				size                   = 2 * 2, // sizeof(Half) is 2
				vertexProgramSlotIndex = igVertexFormatPS3.GetVPSlotIndex(IG_VERTEX_USAGE.IG_VERTEX_USAGE_TEXCOORD, 0),
				fixedBlockOffset       = 0,
				padding                = 0
			};
			rsxVertexesAttrs[1] = new EdgeGeomAttributeBlock()
			{
				offset                 = 0x04,
				format                 = EDGE_GEOM_ATTRIBUTE_FORMAT.U8N,
				componentCount         = 4,
				edgeAttributeId        = EDGE_GEOM_ATTRIBUTE_ID.COLOR,
				size                   = sizeof(byte) * 4,
				vertexProgramSlotIndex = igVertexFormatPS3.GetVPSlotIndex(IG_VERTEX_USAGE.IG_VERTEX_USAGE_COLOR, 0),
				fixedBlockOffset       = 0,
				padding                = 0
			};

			segment.SetStreamDesc(EPS3StreamDesc.Spu0,    spuVertexesAttrs0);
			segment.SetStreamDesc(EPS3StreamDesc.Spu1,    spuVertexesAttrs1);
			segment.SetStreamDesc(EPS3StreamDesc.RsxOnly, rsxVertexesAttrs);

			// Buffer vertices
			ref igMemory<byte> spuVertexes0 = ref segment._spuVertexes0;
			ref igMemory<byte> spuVertexes1 = ref segment._spuVertexes1;
			ref igMemory<byte> rsxVertexes  = ref segment._rsxOnlyVertexes;

			spuVertexes0.Alloc(segment.SpuVertexes0Stride * mesh.VertexCount);
			spuVertexes1.Alloc(segment.SpuVertexes1Stride * mesh.VertexCount);
			rsxVertexes.Alloc( segment.RsxVertexesStride  * mesh.VertexCount);

			Debug.Assert(spuVertexes0.Length <= ushort.MaxValue);
			Debug.Assert(spuVertexes1.Length <= ushort.MaxValue);
			Debug.Assert(rsxVertexes.Length  <= ushort.MaxValue);

			segment._spuVertexesSizes[0] = (ushort)spuVertexes0.Length;
			segment._spuVertexesSizes[1] = (ushort)spuVertexes1.Length;
			segment._rsxOnlyVertexesSize = (ushort)rsxVertexes.Length;

			for (int i = 0; i < mesh.VertexCount; i++)
			{
				short shortWork;

				Vector3 shortNormalised = mesh.Vertices[i];
				float inverseMagnitude = short.MaxValue / MathF.Max(shortNormalised.Length(), 1);
				shortNormalised *= inverseMagnitude;

				Debug.Assert(inverseMagnitude >= 1 && inverseMagnitude <= short.MaxValue);

				shortWork = (short)shortNormalised.X;
				Marshal.Copy(new IntPtr(&shortWork), spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x00, sizeof(short));
				shortWork = (short)shortNormalised.Y;
				Marshal.Copy(new IntPtr(&shortWork), spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x02, sizeof(short));
				shortWork = (short)shortNormalised.Z;
				Marshal.Copy(new IntPtr(&shortWork), spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x04, sizeof(short));
				shortWork = (short)inverseMagnitude;
				Marshal.Copy(new IntPtr(&shortWork), spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x06, sizeof(short));

				uint intWork = ((uint)(mesh.Normals[i].X * 0x7FF) << 21)
				             | ((uint)(mesh.Normals[i].Y * 0x7FF) << 10)
				             | ((uint)(mesh.Normals[i].Z * 0x3FF));
				Marshal.Copy(new IntPtr(&intWork), spuVertexes1.Buffer, i * segment.SpuVertexes1Stride + 0x00, sizeof(uint));

				if (mesh.HasTangentBasis)
				{
					intWork = ((uint)(mesh.Tangents[i].X * 0x7FF) << 21)
					        | ((uint)(mesh.Tangents[i].Y * 0x7FF) << 10)
					        | ((uint)(mesh.Tangents[i].Z * 0x3FF));
					Marshal.Copy(new IntPtr(&intWork), spuVertexes1.Buffer, i * segment.SpuVertexes1Stride + 0x04, sizeof(uint));
				}

				Half halfWork = (Half)mesh.TextureCoordinateChannels[0][i].X;
				Marshal.Copy(new IntPtr(&halfWork), rsxVertexes.Buffer, i * segment.RsxVertexesStride + 0x00, 2);
				halfWork = (Half)(1 - mesh.TextureCoordinateChannels[0][i].Y);
				Marshal.Copy(new IntPtr(&halfWork), rsxVertexes.Buffer, i * segment.RsxVertexesStride + 0x02, 2);

				if (mesh.HasVertexColors(0))
				{
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x04] = (byte)(255 * mesh.VertexColorChannels[0][i].X);
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x05] = (byte)(255 * mesh.VertexColorChannels[0][i].Y);
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x06] = (byte)(255 * mesh.VertexColorChannels[0][i].Z);
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x07] = (byte)(255 * mesh.VertexColorChannels[0][i].W);
				}
				else
				{
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x04] = 0xFF;
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x05] = 0xFF;
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x06] = 0xFF;
					rsxVertexes.Buffer[i * segment.RsxVertexesStride + 0x07] = 0xFF;
				}

				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x00, sizeof(short));
					Array.Reverse(spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x02, sizeof(short));
					Array.Reverse(spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x04, sizeof(short));
					Array.Reverse(spuVertexes0.Buffer, i * segment.SpuVertexes0Stride + 0x06, sizeof(short));

					Array.Reverse(spuVertexes1.Buffer, i * segment.SpuVertexes1Stride + 0x00, sizeof(uint));
					Array.Reverse(spuVertexes1.Buffer, i * segment.SpuVertexes1Stride + 0x04, sizeof(uint));

					Array.Reverse(rsxVertexes.Buffer,  i * segment.RsxVertexesStride  + 0x00, 2);
					Array.Reverse(rsxVertexes.Buffer,  i * segment.RsxVertexesStride  + 0x02, 2);
				}
			}

			// Buffer blend data
			if (hasBones)
			{
				segment._skinMatricesByteOffsets[0] = 0;
				segment._skinMatricesByteOffsets[1] = 0;
				segment._skinMatricesSizes[0] = (ushort)(mesh.BoneCount * 0x30);
				segment._skinMatricesSizes[1] = 0;

				ref igMemory<byte> skinBuffer = ref segment._skinIndexesAndWeights;
				skinBuffer.Alloc(mesh.VertexCount * 8);
				Debug.Assert((ushort)skinBuffer.Length < ushort.MaxValue);
				segment._skinIndexesAndWeightsSizes[0] = (ushort)skinBuffer.Length;

				byte[] influenceCounts = new byte[mesh.VertexCount];

				for (int b = 0; b < mesh.BoneCount; b++)
				{
					for (int v = 0; v < mesh.Bones[b].VertexWeightCount; v++)
					{
						VertexWeight weight = mesh.Bones[b].VertexWeights[v];

						ref byte influenceCount = ref influenceCounts[weight.VertexID];

						if (influenceCount < 4)
						{
							skinBuffer[weight.VertexID * 8 + influenceCount * 2 + 0x00] = (byte)(255 * weight.Weight);
							skinBuffer[weight.VertexID * 8 + influenceCount * 2 + 0x01] = (byte)b;
							influenceCount++;
						}
					}
				}
			}

			segment._ioBufferSize = 0xC000;
			segment._scratchSize  = segment._spuVertexesSizes[0] / 2u;

			spuConfigInfo.flagsAndUniformTableCount      = 0x83;
			spuConfigInfo.commandBufferHoldSize          = 0x13;
			spuConfigInfo.inputVertexFormatId            = byte.MaxValue;
			spuConfigInfo.secondaryInputVertexFormatId   = byte.MaxValue;
			spuConfigInfo.outputVertexFormatId           = 0x01;
			spuConfigInfo.indexesFlavorAndSkinningFlavor = (byte)(0x30 | (byte)(hasBones ? EDGE_GEOM_SKIN.NO_SCALING : EDGE_GEOM_SKIN.NONE));
			spuConfigInfo.skinningMatrixFormat           = 0x00;
			spuConfigInfo.numVertexes                    = (ushort)mesh.VertexCount;
			spuConfigInfo.indexesOffset                  = uint.MaxValue;

			segment._spuConfigInfo = new igMemory<byte>(igMemoryContext.VertexEdge, spuConfigInfo.GetBytes());

			geometry.Append(segment);

			drawCall._platformData = geometry;
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