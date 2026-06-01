/*
	Copyright (c) 2022-2026, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/

using igLibrary.Core;
using igLibrary.Graphics;
using igLibrary.Math;
using ImGuiNET;


namespace igCauldron3
{
	/// <summary>
	/// UI override for rendering data lists
	/// </summary>
	public class igGraphicsMaterialOverride : InspectorDrawOverride
	{
		private igMetaField? _nameField;
		private igMetaField? _sortDepthOffsetField;
		private igMetaField? _sortKeyField;
		private igMetaField? _effectHandleField;
		private igMetaField? _graphicsObjectsField;
		private igMetaField? _drawTypeField;
		private igMetaField? _timeSourceField;
		private igMetaField? _resourceField;

		private igMetaField? _primitiveType_typeField;


		private readonly (igGraphicsMaterial.ConstantType, string)[] kTypeNames =
		{
			(igGraphicsMaterial.ConstantType.Bool,      "bool"),
			(igGraphicsMaterial.ConstantType.Int,       "int"),
			(igGraphicsMaterial.ConstantType.Float,     "float"),
			(igGraphicsMaterial.ConstantType.Vec4f,     "Vector 4"),
			(igGraphicsMaterial.ConstantType.Matrix44f, "Matrix 4x4")
		};


		/// <summary>
		/// Constructor
		/// </summary>
		public igGraphicsMaterialOverride()
		{
			_t = typeof(igGraphicsMaterial);
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <param name="dirFrame">The directory manager frame</param>
		/// <param name="id">the id to render with</param>
		/// <param name="obj">the object</param>
		/// <param name="meta">the type of the object</param>
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			igGraphicsMaterial material = (igGraphicsMaterial)obj;

			if (_nameField == null
			 || _sortDepthOffsetField == null
			 || _sortKeyField == null
			 || _effectHandleField == null
			 || _graphicsObjectsField == null
			 || _drawTypeField == null
			 || _timeSourceField == null
			 || _primitiveType_typeField == null)
			{
				_nameField            = meta.GetFieldByName("_name")!;
				_sortDepthOffsetField = meta.GetFieldByName("_sortDepthOffset")!;
				_sortKeyField         = meta.GetFieldByName("_sortKey")!;
				_effectHandleField    = meta.GetFieldByName("_effectHandle")!;
				_graphicsObjectsField = meta.GetFieldByName("_graphicsObjects")!;
				_drawTypeField        = meta.GetFieldByName("_drawType")!;
				_timeSourceField      = meta.GetFieldByName("_timeSource")!;

				_resourceField = new igObjectRefMetaField() { _metaObject = igArkCore.GetObjectMeta(nameof(igGraphicsObject))! };

				_primitiveType_typeField = igArkCore.GetCompoundFieldInfo("igCommandSetPrimitiveTypeParametersMetaField")!.GetFieldByName("_type")!;
			}

			FieldRenderer.RenderField(id, "_name",       material._name,       _nameField,       (value) => material._name       = (string)value!);
			FieldRenderer.RenderField(id, "_drawType",   material._drawType,   _drawTypeField,   (value) => material._drawType   = (igDrawType)value!);
			FieldRenderer.RenderField(id, "_timeSource", material._timeSource, _timeSourceField, (value) => material._timeSource = (igGraphicsMaterialAnimationTimeSource)value!);

			FieldRenderer.RenderField(id, "_sortKey",         material._sortKey,         _sortKeyField,         (value) => material._sortKey         = (byte)value!);
			FieldRenderer.RenderField(id, "_sortDepthOffset", material._sortDepthOffset, _sortDepthOffsetField, (value) => material._sortDepthOffset = (float)value!);

			FieldRenderer.RenderField(id, "_effectHandle",    material._effectHandle,    _effectHandleField,    (value) => material._effectHandle    = (igHandle)value!);

			DrawDecompiledMaterial("_commonState", material.GetDecompiledCommonState());

			if (ImGui.TreeNode("Techniques"))
			{
				for (int t = 0; t < material.GetDecompiledTechniqueCount(); t++)
				{
					igGraphicsMaterial.DecompiledMaterial? technique = material.GetDecompiledTechnique(t);
					if (technique == null)
					{
						ImGui.Text(t.ToString());
					}
					else
					{
						DrawDecompiledMaterial(t.ToString(), technique);
					}
				}
			}
		}


		/// <summary>
		/// Renders a decompiled material
		/// </summary>
		/// <param name="name">The ui name of the material</param>
		/// <param name="material">The decompiled material to render</param>
		/// <returns>Whether or not the material was changed</returns>
		private bool DrawDecompiledMaterial(string name, igGraphicsMaterial.DecompiledMaterial material)
		{
			bool changed = false;
			if (ImGui.TreeNode(name))
			{
				if (ImGui.TreeNode("Textures"))
				{
					for (int t = 0; t < material._textures.Count; t++)
					{
						igGraphicsMaterial.DecompiledTexture texture = material._textures[t];
						string displayName = texture._imageHandle == null ? "(null)" : texture._imageHandle.ToString();
						if (ImGui.TreeNode((IntPtr)t, $"Register {texture._register}: {displayName}"))
						{
							ImGui.TreePop();
						}
					}

					ImGui.TreePop();
				}
				if (ImGui.TreeNode("Constants"))
				{
					ImGui.BeginTable("constants", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders);
					ImGui.TableSetupColumn("Names");
					ImGui.TableSetupColumn("Values");
					ImGui.TableHeadersRow();

					ImGui.TableNextRow();
					ImGui.TableSetColumnIndex(0);

					float cellCursorPos = ImGui.GetCursorPosX();
					float paddingX = ImGui.GetStyle().FramePadding.X;

					int removeIndex = -1;

					for (int i = 0; i < material._constants.Count; i++)
					{
						ImGui.PushID(i);

						ImGui.PushID("$remove$");
						if (ImGui.Button("-"))
						{
							removeIndex = i;
						}
						ImGui.PopID();

						ImGui.SameLine();

						ImGui.PushID("$name$");
						ImGui.SetNextItemWidth(ImGui.GetColumnWidth() - paddingX);
						changed |= ImGui.InputText(string.Empty, ref material._constants[i]._name, byte.MaxValue);
						ImGui.PopID();

						ImGui.TableNextColumn();

						ref object value = ref material._constants[i]._value;

						ImGui.PushID("$type$");
						ImGui.SetNextItemWidth(ImGui.GetColumnWidth() - paddingX);
						bool typeChanged = UIUtil.EnumComboBox(string.Empty, kTypeNames, ref material._constants[i]._type);
						if (typeChanged)
						{
							switch (material._constants[i]._type)
							{
								case igGraphicsMaterial.ConstantType.Bool:
									value = default(bool);
									break;
								case igGraphicsMaterial.ConstantType.Int:
									value = default(int);
									break;
								case igGraphicsMaterial.ConstantType.Float:
									value = default(float);
									break;
								case igGraphicsMaterial.ConstantType.Vec4f:
									value = default(igVec4f);
									break;
								case igGraphicsMaterial.ConstantType.Matrix44f:
									value = default(igMatrix44f);
									break;
							}

							changed = true;
						}
						ImGui.PopID();

						ImGui.PushID("$value$");
						if (value is int intValue)
						{
							changed |= ImGui.InputInt(string.Empty, ref intValue);
							value = intValue;
						}
						else if (value is float floatValue)
						{
							changed |= ImGui.InputFloat(string.Empty, ref floatValue);
							value = floatValue;
						}
						else if (value is bool boolValue)
						{
							changed |= ImGui.Checkbox(string.Empty, ref boolValue);
							value = boolValue;
						}
						else if (value is igVec4f vectorValue)
						{
							System.Numerics.Vector4 numericsVector = vectorValue;

							// colour tint
							if (material._constants[i]._name == "ig_color_value")
							{
								if (ImGui.CalcItemWidth() > 200)
								{
									ImGui.SetNextItemWidth(200);
								}
								changed |= ImGui.ColorPicker4(string.Empty, ref numericsVector);
							}
							else
							{
								changed |= ImGui.InputFloat4(string.Empty, ref numericsVector);
							}

							value = (igVec4f)numericsVector;
						}
						else if (value is igMatrix44f matrixValue)
						{
							float startCursorX = ImGui.GetCursorPosX();
							System.Numerics.Vector4 matrixRow = new System.Numerics.Vector4(matrixValue._m11, matrixValue._m12, matrixValue._m13, matrixValue._m14);
							changed |= ImGui.InputFloat4("#m1", ref matrixRow);
							matrixValue._m11 = matrixRow.X; matrixValue._m12 = matrixRow.Y; matrixValue._m13 = matrixRow.Z; matrixValue._m14 = matrixRow.W;

							ImGui.SetCursorPosX(startCursorX);
							matrixRow = new System.Numerics.Vector4(matrixValue._m21, matrixValue._m22, matrixValue._m23, matrixValue._m24);
							changed |= ImGui.InputFloat4("#m2", ref matrixRow);
							matrixValue._m21 = matrixRow.X; matrixValue._m22 = matrixRow.Y; matrixValue._m23 = matrixRow.Z; matrixValue._m24 = matrixRow.W;

							ImGui.SetCursorPosX(startCursorX);
							matrixRow = new System.Numerics.Vector4(matrixValue._m31, matrixValue._m32, matrixValue._m33, matrixValue._m34);
							changed |= ImGui.InputFloat4("#m3", ref matrixRow);
							matrixValue._m31 = matrixRow.X; matrixValue._m32 = matrixRow.Y; matrixValue._m33 = matrixRow.Z; matrixValue._m34 = matrixRow.W;

							ImGui.SetCursorPosX(startCursorX);
							matrixRow = new System.Numerics.Vector4(matrixValue._m41, matrixValue._m42, matrixValue._m43, matrixValue._m44);
							changed |= ImGui.InputFloat4("#m4", ref matrixRow);
							matrixValue._m41 = matrixRow.X; matrixValue._m42 = matrixRow.Y; matrixValue._m43 = matrixRow.Z; matrixValue._m44 = matrixRow.W;

							value = matrixValue;
						}
						ImGui.PopID();

						ImGui.TableNextColumn();
						ImGui.PopID();
					}

					ImGui.EndTable();

					ImGui.PushID("$add$");
					bool add = ImGui.Button("+");
					ImGui.PopID();
					if (add)
					{
						material._constants.Add(new igGraphicsMaterial.DecompiledConstant(string.Empty, false));
					}
					if (removeIndex >= 0)
					{
						material._constants.RemoveAt(removeIndex);
					}

					ImGui.TreePop();
				}

				ImGui.TreePop();
			}
			return changed;
		}
	}
}
