using System.Reflection;
using igLibrary.Core;
using igLibrary.DotNet;
using igLibrary.Math;
using ImGuiNET;

namespace igCauldron3
{
	public static class FieldRenderer
	{
		public delegate void FieldSetCallback(object? newRaw);
		private delegate void RenderFieldAction(string id, object? raw, igMetaField field, FieldSetCallback cb);
		private static Dictionary<Type, RenderFieldAction> _renderFuncLookup = new Dictionary<Type, RenderFieldAction>();
		public static void Init()
		{
			_renderFuncLookup.Add(typeof(igCharMetaField), RenderField_SByte);
			_renderFuncLookup.Add(typeof(igUnsignedCharMetaField), RenderField_Byte);
			_renderFuncLookup.Add(typeof(igShortMetaField), RenderField_Short);
			_renderFuncLookup.Add(typeof(igUnsignedShortMetaField), RenderField_UShort);
			_renderFuncLookup.Add(typeof(igIntMetaField), RenderField_Int);
			_renderFuncLookup.Add(typeof(igUnsignedIntMetaField), RenderField_UInt);
			_renderFuncLookup.Add(typeof(igLongMetaField), RenderField_Long);
			_renderFuncLookup.Add(typeof(igUnsignedLongMetaField), RenderField_ULong);
			_renderFuncLookup.Add(typeof(igSizeTypeMetaField), RenderField_ULong);
			_renderFuncLookup.Add(typeof(igFloatMetaField), RenderField_Float);
			_renderFuncLookup.Add(typeof(igDoubleMetaField), RenderField_Double);
			_renderFuncLookup.Add(typeof(igVec2ucMetaField), RenderField_Vec2uc);
			_renderFuncLookup.Add(typeof(igVec2fMetaField), RenderField_Vec2f);
			_renderFuncLookup.Add(typeof(igVec3ucMetaField), RenderField_Vec3uc);
			_renderFuncLookup.Add(typeof(igVec3fMetaField), RenderField_Vec3f);
			_renderFuncLookup.Add(typeof(igVec3fAlignedMetaField), RenderField_Vec3fAligned);
			_renderFuncLookup.Add(typeof(igVec3dMetaField), RenderField_Vec3d);
			_renderFuncLookup.Add(typeof(igVec4ucMetaField), RenderField_Vec4uc);
			_renderFuncLookup.Add(typeof(igVec4fMetaField), RenderField_Vec4f);
			_renderFuncLookup.Add(typeof(igVec4fUnalignedMetaField), RenderField_Vec4fUnaligned);
			_renderFuncLookup.Add(typeof(igVec4iMetaField), RenderField_Vec4i);
			_renderFuncLookup.Add(typeof(igQuaternionfMetaField), RenderField_Quaternionf);
			_renderFuncLookup.Add(typeof(igMatrix44fMetaField), RenderField_Matrix44f);
			_renderFuncLookup.Add(typeof(igStringMetaField), RenderField_String);
			_renderFuncLookup.Add(typeof(igBoolMetaField), RenderField_Bool);
			_renderFuncLookup.Add(typeof(igVectorMetaField), RenderField_Vector);
			_renderFuncLookup.Add(typeof(igMemoryRefMetaField), RenderField_MemoryRef);
			_renderFuncLookup.Add(typeof(igMemoryRefHandleMetaField), RenderField_MemoryRef);
			_renderFuncLookup.Add(typeof(igBitFieldMetaField), RenderField_BitField);
			_renderFuncLookup.Add(typeof(igObjectRefMetaField), RenderField_Object);
			_renderFuncLookup.Add(typeof(igHandleMetaField), RenderField_Handle);
			_renderFuncLookup.Add(typeof(igEnumMetaField), RenderField_Enum);
			_renderFuncLookup.Add(typeof(igCompoundMetaField), RenderField_Compound);
		}
		public static void RenderField(string id, string label, object? value, igMetaField field, FieldSetCallback cb)
		{
			if(field is igStaticMetaField) return;
			if(field is igPropertyFieldMetaField) return;
			ImGui.Text(label);
			ImGui.SameLine();
			RenderFieldNoLabel(id + label, value, field, cb);
		}
		private static void RenderFieldNoLabel(string id, object? value, igMetaField field, FieldSetCallback cb)
		{
			if(_renderFuncLookup.TryGetValue(field.GetType(), out RenderFieldAction? renderFunc))
			{
				renderFunc.Invoke(id, value, field, cb);
			}
			else
			{
				ImGui.Text($"{field.GetType().Name} is unimplemented.");
			}
		}

#region Primitive Numeric Renderers
		private static void RenderField_PrimitiveNumber(string id, object? raw, ElementType type, FieldSetCallback cb)
		{
			string val = raw!.ToString()!;
			ImGui.PushID(id);
			ImGui.PushItemWidth(128);
			bool changed = ImGui.InputText(string.Empty, ref val, 128, ImGuiInputTextFlags.NoUndoRedo);
			ImGui.PopItemWidth();
			ImGui.PopID();
			if(changed)
			{
				MethodInfo convertFunc;
				switch (type)
				{
					case ElementType.kElementTypeI1: convertFunc = ((Func<string, sbyte>)Convert.ToSByte).Method; break;
					case ElementType.kElementTypeU1: convertFunc = ((Func<string, byte>)Convert.ToByte).Method; break;
					case ElementType.kElementTypeI2: convertFunc = ((Func<string, short>)Convert.ToInt16).Method; break;
					case ElementType.kElementTypeU2: convertFunc = ((Func<string, ushort>)Convert.ToUInt16).Method; break;
					case ElementType.kElementTypeI4: convertFunc = ((Func<string, int>)Convert.ToInt32).Method; break;
					case ElementType.kElementTypeU4: convertFunc = ((Func<string, uint>)Convert.ToUInt32).Method; break;
					case ElementType.kElementTypeI8: convertFunc = ((Func<string, long>)Convert.ToInt64).Method; break;
					case ElementType.kElementTypeU8: convertFunc = ((Func<string, ulong>)Convert.ToUInt64).Method; break;
					case ElementType.kElementTypeR4: convertFunc = ((Func<string, float>)Convert.ToSingle).Method; break;
					case ElementType.kElementTypeR8: convertFunc = ((Func<string, double>)Convert.ToDouble).Method; break;
					default: throw new ArgumentException($"Element Type {type} is not a primitive type");
				}
				try
				{
					cb.Invoke(convertFunc.Invoke(null, new object?[]{val}));
				}
				catch(Exception){ changed = false; }	//change nothing
			}
		}
		private static void RenderField_SByte(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeI1, cb);
		private static void RenderField_Byte(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeU1, cb);
		private static void RenderField_Short(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeI2, cb);
		private static void RenderField_UShort(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeU2, cb);
		private static void RenderField_Int(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeI4, cb);
		private static void RenderField_UInt(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeU4, cb);
		private static void RenderField_Long(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeI8, cb);
		private static void RenderField_ULong(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeU8, cb);
		private static void RenderField_Float(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeR4, cb);
		private static void RenderField_Double(string id, object? raw, igMetaField field, FieldSetCallback cb) => RenderField_PrimitiveNumber(id, raw, ElementType.kElementTypeR8, cb);
#endregion
#region Math Structure Renderers
		private static void RenderField_Vec2f(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec2f value = (igVec2f)raw!;
			RenderField_Float(id + " _x", value._x, igFloatMetaField._MetaField, (newValue) => { value._x = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _y", value._y, igFloatMetaField._MetaField, (newValue) => { value._y = (float)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec3f(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec3f value = (igVec3f)raw!;
			RenderField_Float(id + " _x", value._x, igFloatMetaField._MetaField, (newValue) => { value._x = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _y", value._y, igFloatMetaField._MetaField, (newValue) => { value._y = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _z", value._z, igFloatMetaField._MetaField, (newValue) => { value._z = (float)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec3fAligned(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec3fAligned value = (igVec3fAligned)raw!;
			RenderField_Float(id + " _x", value._x, igFloatMetaField._MetaField, (newValue) => { value._x = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _y", value._y, igFloatMetaField._MetaField, (newValue) => { value._y = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _z", value._z, igFloatMetaField._MetaField, (newValue) => { value._z = (float)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec3d(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec3d value = (igVec3d)raw!;
			RenderField_Double(id + " _x", value._x, igDoubleMetaField._MetaField, (newValue) => { value._x = (double)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Double(id + " _y", value._y, igDoubleMetaField._MetaField, (newValue) => { value._y = (double)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Double(id + " _z", value._z, igDoubleMetaField._MetaField, (newValue) => { value._z = (double)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec4f(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec4f value = (igVec4f)raw!;
			RenderField_Float(id + " _x", value._x, igFloatMetaField._MetaField, (newValue) => { value._x = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _y", value._y, igFloatMetaField._MetaField, (newValue) => { value._y = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _z", value._z, igFloatMetaField._MetaField, (newValue) => { value._z = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _w", value._w, igFloatMetaField._MetaField, (newValue) => { value._w = (float)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec4fUnaligned(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec4fUnaligned value = (igVec4fUnaligned)raw!;
			RenderField_Float(id + " _x", value._x, igFloatMetaField._MetaField, (newValue) => { value._x = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _y", value._y, igFloatMetaField._MetaField, (newValue) => { value._y = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _z", value._z, igFloatMetaField._MetaField, (newValue) => { value._z = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _w", value._w, igFloatMetaField._MetaField, (newValue) => { value._w = (float)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec2uc(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec2uc value = (igVec2uc)raw!;
			RenderField_Byte(id + " _x", value._x, igCharMetaField._MetaField, (newValue) => { value._x = (byte)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _y", value._y, igCharMetaField._MetaField, (newValue) => { value._y = (byte)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec3uc(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec3uc value = (igVec3uc)raw!;
			RenderField_Byte(id + " _x", value._x, igCharMetaField._MetaField, (newValue) => { value._x = (byte)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _y", value._y, igCharMetaField._MetaField, (newValue) => { value._y = (byte)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _z", value._z, igCharMetaField._MetaField, (newValue) => { value._z = (byte)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec4uc(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec4uc value = (igVec4uc)raw!;
			RenderField_Byte(id + " _r", value._r, igCharMetaField._MetaField, (newValue) => { value._r = (byte)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _g", value._g, igCharMetaField._MetaField, (newValue) => { value._g = (byte)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _b", value._b, igCharMetaField._MetaField, (newValue) => { value._b = (byte)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _a", value._a, igCharMetaField._MetaField, (newValue) => { value._a = (byte)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Vec4i(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igVec4i value = (igVec4i)raw!;
			RenderField_Byte(id + " _x", value._x, igIntMetaField._MetaField, (newValue) => { value._x = (int)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _y", value._y, igIntMetaField._MetaField, (newValue) => { value._y = (int)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _z", value._z, igIntMetaField._MetaField, (newValue) => { value._z = (int)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Byte(id + " _w", value._w, igIntMetaField._MetaField, (newValue) => { value._w = (int)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Quaternionf(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igQuaternionf value = (igQuaternionf)raw!;
			RenderField_Float(id + " _x", value._x, igFloatMetaField._MetaField, (newValue) => { value._x = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _y", value._y, igFloatMetaField._MetaField, (newValue) => { value._y = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _z", value._z, igFloatMetaField._MetaField, (newValue) => { value._z = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _w", value._w, igFloatMetaField._MetaField, (newValue) => { value._w = (float)newValue!; cb.Invoke(value); });
		}
		private static void RenderField_Matrix44f(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igMatrix44f value = (igMatrix44f)raw!;
			RenderField_Float(id + " _m11", value._m11, igFloatMetaField._MetaField, (newValue) => { value._m11 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m12", value._m12, igFloatMetaField._MetaField, (newValue) => { value._m12 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m13", value._m13, igFloatMetaField._MetaField, (newValue) => { value._m13 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m14", value._m14, igFloatMetaField._MetaField, (newValue) => { value._m14 = (float)newValue!; cb.Invoke(value); });
			RenderField_Float(id + " _m21", value._m21, igFloatMetaField._MetaField, (newValue) => { value._m21 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m22", value._m22, igFloatMetaField._MetaField, (newValue) => { value._m22 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m23", value._m23, igFloatMetaField._MetaField, (newValue) => { value._m23 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m24", value._m24, igFloatMetaField._MetaField, (newValue) => { value._m24 = (float)newValue!; cb.Invoke(value); });
			RenderField_Float(id + " _m31", value._m31, igFloatMetaField._MetaField, (newValue) => { value._m31 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m32", value._m32, igFloatMetaField._MetaField, (newValue) => { value._m32 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m33", value._m33, igFloatMetaField._MetaField, (newValue) => { value._m33 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m34", value._m34, igFloatMetaField._MetaField, (newValue) => { value._m34 = (float)newValue!; cb.Invoke(value); });
			RenderField_Float(id + " _m41", value._m41, igFloatMetaField._MetaField, (newValue) => { value._m41 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m42", value._m42, igFloatMetaField._MetaField, (newValue) => { value._m42 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m43", value._m43, igFloatMetaField._MetaField, (newValue) => { value._m43 = (float)newValue!; cb.Invoke(value); }); ImGui.SameLine();
			RenderField_Float(id + " _m44", value._m44, igFloatMetaField._MetaField, (newValue) => { value._m44 = (float)newValue!; cb.Invoke(value); });
		}
#endregion
		private static void RenderField_String(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			ImGui.PushID(id);
			string value = (string)raw ?? string.Empty;
			bool changed = ImGui.InputText(string.Empty, ref value, ushort.MaxValue, ImGuiInputTextFlags.NoUndoRedo);
			ImGui.PopID();
			if(changed) cb.Invoke(value);
		}
		private static void RenderField_Bool(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			ImGui.PushID(id);
			bool value = (bool)raw!;
			bool changed = ImGui.Checkbox(string.Empty, ref value);
			ImGui.PopID();
			if(changed) cb.Invoke(value);
		}
#region Array Renderers
		public static void RenderArrayField(string id, object? value, igMetaField field, FieldSetCallback cb)
		{
			FieldInfo fi = field.GetType().GetField("_num")!;
			short num = (short)fi.GetValue(field)!;
			Array values = (Array)value!;
			for(int i = 0; i < num; i++)
			{
				int capturedIndex = i;
				RenderField(id + i.ToString(), $"Element {i}", values.GetValue(i), field, (newValue) => values.SetValue(newValue, capturedIndex));
			}
		}
		private static void RenderField_Vector(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			if(ImGui.TreeNode(id, "Data"))
			{
				igVectorCommon vector = (igVectorCommon)raw!;
				IigMemory memValue = vector.GetData();
				igMetaField memType = ((igVectorMetaField)field).GetTemplateParameter(0);

				Array data = memValue.GetData();
				if(data != null)
				{
					//ADD REMOVE BUTTON
					for(int i = 0; i < vector.GetCount(); i++)
					{
						int capturedIndex = i;
						RenderField(id + i.ToString(), $"Element {i}", vector.GetItem(i), memType, (newValue) => vector.SetItem(capturedIndex, newValue));
					}
				}
				ImGui.PushID(id + "$create$");
				bool create = ImGui.Button("+");
				ImGui.PopID();
				if(create)
				{
					vector.SetCapacity((int)vector.GetCount() + 1);
					vector.SetCount(vector.GetCount() + 1u);
				}
				ImGui.TreePop();
			}
		}
		private static void RenderField_MemoryRef(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			if(ImGui.TreeNode(id, "Data"))
			{
				IigMemory memValue = (IigMemory)raw!;
				igMetaField memType;
				if(field is igMemoryRefMetaField memoryRefMetaField)
				{
					memType = memoryRefMetaField._memType;
				}
				else if(field is igMemoryRefHandleMetaField memoryRefHandleMetaField)
				{
					memType = memoryRefHandleMetaField._memType;
				}
				else throw new NotImplementedException($"yo you forgot to implement {field.GetType().Name} into this func");

				Array data = memValue.GetData();
				if(data != null)
				{
					int remove = -1;
					for(int i = 0; i < data.Length; i++)
					{
						ImGui.PushID(id + i.ToString() + "$remove$");
						if(ImGui.Button("-"))
						{
							remove = i;
						}
						ImGui.PopID();
						ImGui.SameLine();
						int capturedIndex = i;
						RenderField(id + i.ToString(), $"Element {i}", data.GetValue(i), memType, (newValue) => data.SetValue(newValue, capturedIndex));
					}
					if(remove >= 0)
					{
						Array newData = Array.CreateInstance(data.GetType().GetElementType()!, data.Length - 1);
						for(int r = 0, w = 0; r < data.Length; r++)
						{
							if(r == remove) continue;
							newData.SetValue(data.GetValue(r), w);
							w++;
						}
						memValue.SetData(newData);
						cb.Invoke(memValue);
					}
				}
				ImGui.PushID(id + "$add$");
				if(ImGui.Button("+"))
				{
					memValue.Realloc(memValue.Length + 1);
					cb.Invoke(memValue);
				}
				ImGui.PopID();
				ImGui.TreePop();
			}
		}
#endregion
		public static void RenderField_BitField(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igBitFieldMetaField bfmf = (igBitFieldMetaField)field;
			RenderFieldNoLabel(id, raw, bfmf._assignmentMetaField, cb);
		}
		public static void RenderField_Object(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			DirectoryManagerFrame._instance.RenderObject(id, (igObject?)raw);
			if(ImGui.BeginPopupContextItem(id))
			{
				if(ImGui.Selectable("Change Reference"))
				{
					Window._instance._frames.Add(new ObjectPickerFrame(Window._instance, DirectoryManagerFrame._instance.CurrentDir, ((igObjectRefMetaField)field)._metaObject, (handle) => cb.Invoke(handle)));
				}
				ImGui.EndPopup();
			}
			if(raw == null)
			{
				ImGui.SameLine();
				ImGui.PushID(id + "$create$");
				bool create = ImGui.Button("+");
				ImGui.PopID();
				if(create)
				{
					igObjectDirectory capturedDir = DirectoryManagerFrame._instance.CurrentDir;
					Window._instance._frames.Add(new CreateObjectFrame(Window._instance, capturedDir, ((igObjectRefMetaField)field)._metaObject, (obj) => cb.Invoke(obj)));
				}
			}
		}
		public static void RenderField_Handle(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			string display = "NullHandle";
			if(raw != null)
			{
				display = raw.ToString()!;
			}
			ImGui.PushID(id);
			bool shouldEdit = ImGui.Selectable(display);
			ImGui.PopID();
			if(shouldEdit)
			{
				Window._instance._frames.Add(new HandlePickerFrame(Window._instance, ((igHandleMetaField)field)._metaObject, (handle) => cb.Invoke(handle)));
			}
		}
		public static void RenderField_Enum(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igEnumMetaField enumMetaField = (igEnumMetaField)field;
			if(enumMetaField._metaEnum != null)
			{
				string valueName = raw!.ToString()!;
				int selectedItem = enumMetaField._metaEnum._names.FindIndex(x => x == valueName);
				ImGui.PushID(id);
				bool changed = ImGui.Combo(string.Empty, ref selectedItem, enumMetaField._metaEnum._names.ToArray(), enumMetaField._metaEnum._names.Count);
				ImGui.PopID();
				if(changed)
				{
					cb.Invoke(enumMetaField._metaEnum.GetEnumFromName(enumMetaField._metaEnum._names[selectedItem]));
				}
			}
			else
			{
				int intValue = (int)raw!;
				ImGui.PushID(id);
				ImGui.InputInt(string.Empty, ref intValue);
				ImGui.PopID();
				cb.Invoke((int)Math.Clamp(intValue, int.MinValue, int.MaxValue));
			}
		}
		public static void RenderField_Compound(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igCompoundMetaField compound = (igCompoundMetaField)field;
			if(ImGui.TreeNode(id, compound._compoundFieldInfo._name))
			{
				List<igMetaField> fieldList = compound._compoundFieldInfo._fieldList;
				for(int i = 0; i < fieldList.Count; i++)
				{
					FieldInfo fi = fieldList[i]._fieldHandle!;
					object? fieldValue = fi.GetValue(raw);
					RenderField(id, fieldList[i]._fieldName!, fieldValue, fieldList[i], (newValue) => {
						fi.SetValue(raw, newValue);
						cb.Invoke(raw);
					});
				}
				ImGui.TreePop();
			}
		}
	}
}