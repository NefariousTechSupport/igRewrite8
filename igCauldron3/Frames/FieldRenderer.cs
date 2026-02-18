/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igCauldron3.Frames;
using igLibrary.Core;
using igLibrary.DotNet;
using igLibrary.Math;
using igLibrary.Tfb.Game;
using igLibrary.Tfb.Script;
using igLibrary.Vfx;
using ImGuiNET;
using System.Reflection;

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
			_renderFuncLookup.Add(typeof(igVfxRgbCurveMetaField), RenderField_RgbCurve);
			_renderFuncLookup.Add(typeof(igVfxModulationHelperMetaField), RenderField_ModulationHelper);
			_renderFuncLookup.Add(typeof(DotNetTypeMetaField), RenderField_DotNetType);
			_renderFuncLookup.Add(typeof(DotNetDataMetaField), RenderField_DotNetData);
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
			if(field is igRawRefMetaField) return;
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
			
			if (raw is tfbScriptInfo ts)
			{
                ImGui.SameLine();
                ImGui.PushID(id);
                bool editscript = ImGui.Button("edit script");
                if (editscript)
                {
					OpCodeList codeList = ts._opList;
                    OpCreateVariableList varList = ts._masterVarList;
					igObjectDirectory capturedDir = DirectoryManagerFrame._instance.CurrentDir!;
                    var scriptDependencies = new Dictionary<List<OpAbstractCreateVariable>, string>(StreamContext.globalScriptDependencies);
					if (capturedDir._name._string != "app:/permanent/global.bld (level bld)")
					{
                        Dictionary<List<OpAbstractCreateVariable>, string>? localScriptVariables = ScriptParser.ReadDependencies(capturedDir);
                        foreach (var kv in localScriptVariables)
                        {
                            scriptDependencies.Add(kv.Key, kv.Value);
                        }
                    }
                    Window._instance._frames.Add(new TfbScriptEditor(Window._instance, capturedDir, codeList, varList, scriptDependencies));

                }
                ImGui.PopID();
            }
            if (ImGui.BeginPopupContextItem(id))
			{
				if(ImGui.Selectable("Change Reference"))
				{
					Window._instance._frames.Add(new ObjectPickerFrame(Window._instance, DirectoryManagerFrame._instance.CurrentDir!, ((igObjectRefMetaField)field)._metaObject, (handle) => cb.Invoke(handle)));
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
					igObjectDirectory capturedDir = DirectoryManagerFrame._instance.CurrentDir!;
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
					if (fieldList[i] is igStaticMetaField) continue;
					if (fieldList[i] is igPropertyFieldMetaField) continue;

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

				RenderField(nameof(rangedCurve._modulationHelper), nameof(rangedCurve._modulationHelper), rangedCurve._modulationHelper, igVfxModulationHelperMetaField._MetaField, (newValue) =>
				{
					rangedCurve._modulationHelper = (igVfxModulationHelper)newValue!;
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

		public static void RenderField_RgbCurve(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			if (ImGui.TreeNode(id, "VfxRgbCurve"))
			{
				bool changed = false;
				igVfxRgbCurve curve = (igVfxRgbCurve)raw!;

				RenderField(nameof(curve._enableInterpolation), nameof(curve._enableInterpolation), curve._enableInterpolation, igBoolMetaField._MetaField, (newValue) =>
				{
					curve._enableInterpolation = (bool)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._enableRandomness), nameof(curve._enableRandomness), curve._enableRandomness, igBoolMetaField._MetaField, (newValue) =>
				{
					curve._enableRandomness = (bool)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._modulationHelper), nameof(curve._modulationHelper), curve._modulationHelper, igVfxModulationHelperMetaField._MetaField, (newValue) =>
				{
					curve._modulationHelper = (igVfxModulationHelper)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c00), nameof(curve._c00), curve._c00, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c00 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c01), nameof(curve._c01), curve._c01, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c01 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c01), nameof(curve._c01), curve._c01, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c01 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c02), nameof(curve._c02), curve._c02, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c02 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c01), nameof(curve._c01), curve._c01, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c01 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c02), nameof(curve._c02), curve._c02, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c02 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c03), nameof(curve._c03), curve._c03, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c03 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c04), nameof(curve._c04), curve._c04, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c04 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c05), nameof(curve._c05), curve._c05, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c05 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c06), nameof(curve._c06), curve._c06, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c06 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c07), nameof(curve._c07), curve._c07, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c07 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c08), nameof(curve._c08), curve._c08, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c08 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c09), nameof(curve._c09), curve._c09, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c09 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c10), nameof(curve._c10), curve._c10, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c10 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c11), nameof(curve._c11), curve._c11, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c11 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c12), nameof(curve._c12), curve._c12, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c12 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c13), nameof(curve._c13), curve._c13, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c13 = (igVec4f)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._c14), nameof(curve._c14), curve._c14, igVec4fMetaField._MetaField, (newValue) =>
				{
					curve._c14 = (igVec4f)newValue!;
					changed = true;
				});

				if (changed)
				{
					cb.Invoke(curve);
				}

				ImGui.TreePop();
			}
		}

		static igBitFieldMetaField? modulationTypeField = null;
		static igBitFieldMetaField? distributionField = null;
		static igBitFieldMetaField? mixTypeField = null;
		public static void RenderField_ModulationHelper(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			if (modulationTypeField == null
			 || distributionField == null
			 || mixTypeField == null)
			{
				igCompoundMetaFieldInfo? fieldInfo = igVfxModulationHelperMetaField._MetaField._compoundFieldInfo;
				if (fieldInfo == null)
				{
					ImGui.Text("Missing metadata for \"igVfxModulationHelperMetaField\", log a bug for this");
					return;
				}

				modulationTypeField = fieldInfo.GetFieldByName(nameof(igVfxModulationHelper._modulationType)) as igBitFieldMetaField;
				distributionField   = fieldInfo.GetFieldByName(nameof(igVfxModulationHelper._distribution))   as igBitFieldMetaField;
				mixTypeField        = fieldInfo.GetFieldByName(nameof(igVfxModulationHelper._mixType))        as igBitFieldMetaField;

				if (modulationTypeField == null
				 || distributionField == null
				 || mixTypeField == null)
				{
					ImGui.Text("Invalid metadata for \"igVfxModulationHelperMetaField\", log a bug for this");
					return;
				}
			}

			if (ImGui.TreeNode(id, "ModulationHelper"))
			{
				bool changed = false;
				igVfxModulationHelper curve = (igVfxModulationHelper)raw!;

				RenderField(nameof(curve._mixAmount), nameof(curve._mixAmount), curve._mixAmount, igFloatMetaField._MetaField, (newValue) =>
				{
					curve._mixAmount = (float)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._phaseOffset), nameof(curve._phaseOffset), curve._phaseOffset, igFloatMetaField._MetaField, (newValue) =>
				{
					curve._phaseOffset = (float)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._distributionArgument), nameof(curve._distributionArgument), curve._distributionArgument, igFloatMetaField._MetaField, (newValue) =>
				{
					curve._distributionArgument = (float)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._modulationCycles), nameof(curve._modulationCycles), curve._modulationCycles, igFloatMetaField._MetaField, (newValue) =>
				{
					curve._modulationCycles = Math.Clamp((float)newValue!, ushort.MinValue, ushort.MaxValue);
					// Round to nearest, do not floor
					curve._modulationCyclesInt = (ushort)(curve._modulationCycles + 0.5f);
					changed = true;
				});

				RenderField(nameof(curve._modulationType), nameof(curve._modulationType), curve._modulationType, modulationTypeField, (newValue) =>
				{
					curve._modulationType = (igVfxModulationHelper.ModulationType)newValue!;
					curve._hasModulation = curve._modulationType != igVfxModulationHelper.ModulationType.kModulationNone;
					changed = true;
				});

				RenderField(nameof(curve._distribution), nameof(curve._distribution), curve._distribution, distributionField, (newValue) =>
				{
					curve._distribution = (igVfxModulationHelper.Distribution)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._mixType), nameof(curve._mixType), curve._mixType, mixTypeField, (newValue) =>
				{
					curve._mixType = (igVfxModulationHelper.ModulationMix)newValue!;
					changed = true;
				});

				RenderField(nameof(curve._randomPhase), nameof(curve._randomPhase), curve._randomPhase, igBoolMetaField._MetaField, (newValue) =>
				{
					curve._randomPhase = (bool)newValue!;
					changed = true;
				});

				if (changed)
				{
					cb.Invoke(curve);
				}

				ImGui.TreePop();
			}
		}
		static readonly (ElementType, string)[] kElementTypeComboItems = new (ElementType, string)[]{
			(ElementType.kElementTypeEnd,       "End"                        ),
			(ElementType.kElementTypeVoid,      "Void"                       ),
			(ElementType.kElementTypeBoolean,   "Boolean"                    ),
			(ElementType.kElementTypeChar,      "Char"                       ),
			(ElementType.kElementTypeI1,        "Integer (8-bit, signed)"    ),
			(ElementType.kElementTypeU1,        "Integer (8-bit, unsigned)"  ),
			(ElementType.kElementTypeI2,        "Integer (16-bit, signed)"   ),
			(ElementType.kElementTypeU2,        "Integer (16-bit, unsigned)" ),
			(ElementType.kElementTypeI4,        "Integer (32-bit, signed)"   ),
			(ElementType.kElementTypeU4,        "Integer (32-bit, unsigned)" ),
			(ElementType.kElementTypeI8,        "Integer (64-bit, signed)"   ),
			(ElementType.kElementTypeU8,        "Integer (64-bit, unsigned)" ),
			(ElementType.kElementTypeR4,        "Float (32-bit)"             ),
			(ElementType.kElementTypeString,    "String"                     ),
			(ElementType.kElementTypeValueType, "ValueType"                  ),
			(ElementType.kElementTypeClass,     "Class"                      ),
			(ElementType.kElementTypeObject,    "Object"                     )
		};
		static int sRegisteredEnumCount = 0;
		static int sRegisteredClassCount = 0;
		static List<igBaseMeta>? sMetaList = null;

		public static void RenderField_DotNetType(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			bool expanded = ImGui.TreeNode($"DotNetType##{id}");
			if (!expanded)
			{
				return;
			}

			DotNetType type = (DotNetType)raw!;
			bool changed = false;

			int index = Array.FindIndex(kElementTypeComboItems, x => x.Item1 == type._elementType);
			if(ImGui.BeginCombo("##elementType", kElementTypeComboItems[index].Item2))
			{
				for (uint i = 0; i < kElementTypeComboItems.Length; i++)
				{
					if(ImGui.Selectable(kElementTypeComboItems[i].Item2, type._elementType == kElementTypeComboItems[i].Item1))
					{
						changed = true;
						type._elementType = kElementTypeComboItems[i].Item1;
					}
					if(type._elementType == kElementTypeComboItems[i].Item1)
					{
						ImGui.SetItemDefaultFocus();
					}
				}
				ImGui.EndCombo();
			}

			RenderField(nameof(type._isSimple), nameof(type._isSimple), type._isSimple, igBoolMetaField._MetaField, (newValue) =>
			{
				type._isSimple = (bool)newValue!;
				changed = true;
			});

			RenderField(nameof(type._isArray), nameof(type._isArray), type._isArray, igBoolMetaField._MetaField, (newValue) =>
			{
				type._isArray = (bool)newValue!;
				changed = true;
			});

			if (sRegisteredEnumCount != igArkCore.MetaEnums.Count()
			 || sRegisteredClassCount != igArkCore.MetaObjects.Count()
			 || sMetaList == null)
			{
				sRegisteredEnumCount = igArkCore.MetaEnums.Count();
				sRegisteredClassCount = igArkCore.MetaObjects.Count();

				sMetaList = new List<igBaseMeta>(sRegisteredEnumCount + sRegisteredClassCount);

				sMetaList.AddRange(igArkCore.MetaObjects);
				sMetaList.AddRange(igArkCore.MetaEnums);

				sMetaList = sMetaList.OrderBy(x => x._name).ToList();
			}

			if (type._elementType == ElementType.kElementTypeValueType
			 || type._elementType == ElementType.kElementTypeClass
			 || type._elementType == ElementType.kElementTypeObject)
			{
				if(ImGui.BeginCombo("##meta", type._baseMeta == null ? "<null>" : type._baseMeta._name))
				{
					for (int i = 0; i < sMetaList.Count; i++)
					{
						if (ImGui.Selectable(sMetaList[i]._name, type._baseMeta == sMetaList[i]))
						{
							type._baseMeta = sMetaList[i];
							changed = true;
						}
						if(type._baseMeta == sMetaList[i])
						{
							ImGui.SetItemDefaultFocus();
						}
					}
					ImGui.EndCombo();
				}
			}

			if (changed)
			{
				cb.Invoke(type);
			}

			ImGui.TreePop();
		}
		public static void RenderField_DotNetData(string id, object? raw, igMetaField field, FieldSetCallback cb)
		{
			bool expanded = ImGui.TreeNode($"DotNetData##{id}");
			if (!expanded)
			{
				return;
			}

			DotNetData data = (DotNetData)raw!;
			bool changed = false;

			RenderField($"{id}$DotNetData._type$", "Type", data._type, DotNetTypeMetaField._MetaField, (newValue) =>
			{
				data._type = (DotNetType)newValue!;
				data.Reset();
				changed = true;
			});

			igMetaField? metaField = null;
			igBaseMeta? originalMeta = null;
			switch (data._type._elementType)
			{
				case ElementType.kElementTypeEnd:
				case ElementType.kElementTypeVoid:
					break;
				case ElementType.kElementTypeBoolean:
					metaField = igBoolMetaField._MetaField;
					break;
				case ElementType.kElementTypeChar:
					metaField = igWideCharMetaField._MetaField;
					break;
				case ElementType.kElementTypeI1:
					metaField = igCharMetaField._MetaField;
					break;
				case ElementType.kElementTypeU1:
					metaField = igUnsignedCharMetaField._MetaField;
					break;
				case ElementType.kElementTypeI2:
					metaField = igShortMetaField._MetaField;
					break;
				case ElementType.kElementTypeU2:
					metaField = igUnsignedShortMetaField._MetaField;
					break;
				case ElementType.kElementTypeI4:
					metaField = igIntMetaField._MetaField;
					break;
				case ElementType.kElementTypeU4:
					metaField = igUnsignedIntMetaField._MetaField;
					break;
				case ElementType.kElementTypeI8:
					metaField = igLongMetaField._MetaField;
					break;
				case ElementType.kElementTypeU8:
					metaField = igUnsignedLongMetaField._MetaField;
					break;
				case ElementType.kElementTypeR4:
					metaField = igFloatMetaField._MetaField;
					break;
				case ElementType.kElementTypeString:
					metaField = igStringMetaField._MetaField;
					break;
				case ElementType.kElementTypeValueType:
					metaField = igEnumMetaField._MetaField;
					originalMeta = igEnumMetaField._MetaField._metaEnum;
					igEnumMetaField._MetaField._metaEnum = (igMetaEnum?)data._type._baseMeta!;
					break;
				case ElementType.kElementTypeClass:
				case ElementType.kElementTypeObject:
					metaField = igObjectRefMetaField._MetaField;
					originalMeta = igObjectRefMetaField._MetaField._metaObject;
					igObjectRefMetaField._MetaField._metaObject = (igMetaObject?)data._type._baseMeta!;
					break;
			}

			if (metaField != null)
			{
				RenderField($"{id}$data$", "Data", data._data, metaField, (newValue) => 
				{
					data._data = newValue;
					changed = true;
					// Do the invoke cos igObject ones don't happen on the same frame
					cb.Invoke(data);
				});
			}

			if (data._type._elementType == ElementType.kElementTypeValueType)
			{
				igEnumMetaField._MetaField._metaEnum = (igMetaEnum?)originalMeta!;
			}
			else if (data._type._elementType == ElementType.kElementTypeClass
			      || data._type._elementType == ElementType.kElementTypeObject)
			{
				igObjectRefMetaField._MetaField._metaObject = (igMetaObject?)originalMeta!;
			}

			if (changed)
			{
				cb.Invoke(data);
			}

			ImGui.TreePop();
		}
	}
}