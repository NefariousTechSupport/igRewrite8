/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;
using igLibrary.Core;
using igLibrary.DotNet;
using igLibrary.Math;
using igLibrary.Vfx;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// Methods for rendering each field type
	/// </summary>
	public static class FieldRenderer
	{
		// Delegates
		public delegate void FieldSetCallback(object? newRaw);
		private delegate void RenderFieldAction(string id, object? raw, igMetaField field, FieldSetCallback cb);

		// The lookup table
		private static Dictionary<Type, RenderFieldAction> _renderFuncLookup = new Dictionary<Type, RenderFieldAction>();

		/// <summary>
		/// Sets up the lookup table
		/// </summary>
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
			_renderFuncLookup.Add(typeof(igTimeMetaField), RenderField_Time);
			_renderFuncLookup.Add(typeof(igDotNetEnumMetaField), RenderField_Enum);
			_renderFuncLookup.Add(typeof(igDotNetDynamicMetaEnum), RenderField_Enum);
			_renderFuncLookup.Add(typeof(igRangedFloatMetaField), RenderField_RangedFloat);
			_renderFuncLookup.Add(typeof(igRawRefMetaField), RenderField_RawRef);
			_renderFuncLookup.Add(typeof(igStructMetaField), RenderField_Struct);
			_renderFuncLookup.Add(typeof(igVfxRangedCurveMetaField), RenderField_RangedCurve);
		}


		/// <summary>
		/// Renders a field with a label
		/// </summary>
		/// <param name="id">The id to render with</param>
		/// <param name="label">The label to display</param>
		/// <param name="value">The value to display</param>
		/// <param name="field">The metafield to use</param>
		/// <param name="cb">The callback for when a new value is entered</param>
		public static void RenderField(string id, string label, object? value, igMetaField field, FieldSetCallback cb)
		{
			if(field is igStaticMetaField) return;
			if(field is igPropertyFieldMetaField) return;
			ImGui.Text(label);
			ImGui.SameLine();
			RenderFieldNoLabel(id + label, value, field, cb);
		}


		/// <summary>
		/// Render a field without a label
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <param name="field"></param>
		/// <param name="cb"></param>
		private static void RenderFieldNoLabel(string id, object? value, igMetaField field, FieldSetCallback cb)
		{
			RenderFieldAction? renderFunc;
			Type queryType = field.GetType();

			if(field.IsArray)
			{
				queryType = field.GetType().BaseType!;
			}

			if(_renderFuncLookup.TryGetValue(queryType, out renderFunc))
			{
				if(field.IsArray)
				{
					ImGui.PushID(id);
					bool opened = ImGui.TreeNode("Data");
					ImGui.PopID();
					if(opened)
					{
						Array arrValue = (Array)value!;
						for(int i = 0; i < field.ArrayNum; i++)
						{
							ImGui.Text("Element " + i.ToString());
							ImGui.SameLine();
							int capturedI = i;
							renderFunc.Invoke(i.ToString("%08X"), arrValue.GetValue(i), field, (newValue) => {
								arrValue.SetValue(newValue, capturedI);
								cb.Invoke(arrValue);
							});
						}
						ImGui.TreePop();
					}
				}
				else
				{
					renderFunc.Invoke(id, value, field, cb);
				}
			}
			else
			{
				ImGui.Text($"{field.GetType().Name} is unimplemented.");
			}
		}


#region Primitive Numeric Renderers
		/// <summary>
		/// Renders a primitive number
		/// </summary>
		/// <param name="id">The id to render with</param>
		/// <param name="raw">The value to render with</param>
		/// <param name="type">The type of value</param>
		/// <param name="cb">The callback on setting the value</param>
		/// <exception cref="ArgumentException">If a non-numeric type was passed</exception>
		private static void RenderField_PrimitiveNumber(string id, object? raw, ElementType type, FieldSetCallback cb)
		{
			string val = raw!.ToString()!;
			ImGui.PushID(id);
			ImGui.PushItemWidth(128);
			bool changed = ImGui.InputText(string.Empty, ref val, 128);
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
		// I'm not commenting these
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
			bool changed = ImGui.InputText(string.Empty, ref value, ushort.MaxValue);
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
			ImGui.PushID(id);
			igEnumMetaField enumMetaField = (igEnumMetaField)field;
			if(enumMetaField._metaEnum != null)
			{
				// Enum representation
				string valueName = raw!.ToString()!;
				int selectedItem = enumMetaField._metaEnum._names.FindIndex(x => x == valueName);
				ImGui.PushID("$enum$");
				ImGui.PushItemWidth(258);
				bool changed = ImGui.Combo(string.Empty, ref selectedItem, enumMetaField._metaEnum._names.ToArray(), enumMetaField._metaEnum._names.Count);
				ImGui.PopID();
				if(changed)
				{
					cb.Invoke(enumMetaField._metaEnum.GetEnumFromName(enumMetaField._metaEnum._names[selectedItem]));
				}

				// We'll render the int representation too
				ImGui.SameLine();
				ImGui.PushItemWidth(258);
			}

			// Int representation
			int intValue = (int)raw!;
			ImGui.PushID("$int$");
			bool intChanged = ImGui.InputInt(string.Empty, ref intValue);
			ImGui.PopID();

			ImGui.PopID();
			if (intChanged)
			{
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
		public static void RenderField_Time(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			RenderField_PrimitiveNumber(id, ((igTime)raw!)._elapsedDays, ElementType.kElementTypeR4, (value) => cb.Invoke(new igTime((float)value!)));
			ImGui.SameLine();
			ImGui.Text("days");
		}
		public static void RenderField_RangedFloat(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			igRangedFloat rangedFloat = (igRangedFloat)raw!;

			ImGui.PushID(id);

			ImGui.Text("min");
			ImGui.SameLine();
			RenderField_PrimitiveNumber("$min$", rangedFloat._min, ElementType.kElementTypeR4, (value) =>
			{
				rangedFloat._min = (float)value!;
				cb.Invoke(rangedFloat);
			});

			ImGui.SameLine();
			ImGui.Spacing();
			ImGui.SameLine();

			ImGui.Text("max");
			ImGui.SameLine();
			RenderField_PrimitiveNumber("$max$", rangedFloat._max, ElementType.kElementTypeR4, (value) =>
			{
				rangedFloat._max = (float)value!;
				cb.Invoke(rangedFloat);
			});

			ImGui.PopID();
		}
		public static void RenderField_RawRef(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			ImGui.Text("Editing \"igRawRefMetaField\" is not allowed");
		}
		public static void RenderField_Struct(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			// Just treat structs as raw bytes
			if (raw is not byte[] data)
			{
				ImGui.Text($"Oopsie doopsie something went wrong here, log a bug and mention the following: {field._parentMeta?._name}::{field._fieldName}");
				return;
			}

			if (ImGui.TreeNode(id, "Struct Data"))
			{
				for(int i = 0; i < data.Length; i++)
				{
					int capturedIndex = i;

					ImGui.Text($"Element {i}");
					ImGui.SameLine();
					RenderField_PrimitiveNumber(i.ToString(), data.GetValue(i), ElementType.kElementTypeU1, (newValue) => data.SetValue(newValue, capturedIndex));
				}
				ImGui.TreePop();
			}
		}

		static igCompoundMetaField? curveKeyframesField;
		public static void RenderField_RangedCurve(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			// Idrk how this works so just expose all the fields
			if (curveKeyframesField == null)
			{
				igCompoundMetaFieldInfo? fieldInfo = igArkCore.GetCompoundFieldInfo("igVfxCurveKeyframeMetaField");
				if (fieldInfo == null)
				{
					ImGui.Text("Mising metadata for \"igVfxCurveKeyframeMetaField\", log a bug for this");
					return;
				}

				curveKeyframesField = new igCompoundMetaField();
				curveKeyframesField._compoundFieldInfo = fieldInfo;
			}

			bool changed = false;
			igVfxRangedCurve rangedCurve = (igVfxRangedCurve)raw!;

			if (ImGui.TreeNode(id, "VfxRangedCurve"))
			{
				for (int i = 0; i < rangedCurve._keyframes.Length; i++)
				{
					int capturedI = i;
					ImGui.Text($"Keyframe {i}");
					ImGui.SameLine();
					RenderFieldNoLabel(i.ToString(), rangedCurve._keyframes[i], curveKeyframesField, (newKeyframe) =>
					{
						rangedCurve._keyframes[capturedI] = (igVfxCurveKeyframe)newKeyframe!;
						changed = true;
					});
				}

				ImGui.Text(nameof(igVfxRangedCurve._field_0x3C));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x3C), rangedCurve._field_0x3C, ElementType.kElementTypeR4, (newValue) =>
				{
					rangedCurve._field_0x3C = (float)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x40));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x40), rangedCurve._field_0x40, ElementType.kElementTypeR4, (newValue) =>
				{
					rangedCurve._field_0x40 = (float)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x44));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x44), rangedCurve._field_0x44, ElementType.kElementTypeR4, (newValue) =>
				{
					rangedCurve._field_0x44 = (float)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x48));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x48), rangedCurve._field_0x48, ElementType.kElementTypeR4, (newValue) =>
				{
					rangedCurve._field_0x48 = (float)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x4C));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x4C), rangedCurve._field_0x4C, ElementType.kElementTypeU2, (newValue) =>
				{
					rangedCurve._field_0x4C = (ushort)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x4E));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x4E), rangedCurve._field_0x4E, ElementType.kElementTypeU1, (newValue) =>
				{
					rangedCurve._field_0x4E = (byte)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._flags));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._flags), rangedCurve._flags, ElementType.kElementTypeU1, (newValue) =>
				{
					rangedCurve._flags = (byte)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x50));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x50), rangedCurve._field_0x50, ElementType.kElementTypeU2, (newValue) =>
				{
					rangedCurve._field_0x50 = (ushort)newValue!;
					changed = true;
				});
				ImGui.Text(nameof(igVfxRangedCurve._field_0x52));
				ImGui.SameLine();
				RenderField_PrimitiveNumber(nameof(igVfxRangedCurve._field_0x52), rangedCurve._field_0x52, ElementType.kElementTypeU2, (newValue) =>
				{
					rangedCurve._field_0x52 = (ushort)newValue!;
					changed = true;
				});

				if (changed)
				{
					cb.Invoke(rangedCurve);
				}

				ImGui.TreePop();
			}
		}
	}
}