/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.CompilerServices;

namespace igLibrary.Graphics
{
	public class igShaderConstantValue : igObject
	{
		public igGraphicsShaderConstant _constant;

		public virtual bool ShallowEquals(igShaderConstantValue other)
		{
			return other.GetType() == this.GetType() && _constant.ShallowEquals(other._constant);
		}
	}

	public class igShaderConstantValueBool : igShaderConstantValue
	{
		public bool _value;

		public override bool ShallowEquals(igShaderConstantValue other)
		{
			return base.ShallowEquals(other) && _value == ((igShaderConstantValueBool)other)._value;
		}
	}

	public class igShaderConstantValueInt : igShaderConstantValue
	{
		public int _value;

		public override bool ShallowEquals(igShaderConstantValue other)
		{
			return base.ShallowEquals(other) && _value == ((igShaderConstantValueInt)other)._value;
		}
	}

	public class igShaderConstantValueFloat : igShaderConstantValue
	{
		public float _value;

		public override bool ShallowEquals(igShaderConstantValue other)
		{
			return base.ShallowEquals(other) && _value == ((igShaderConstantValueFloat)other)._value;
		}
	}

	public class igShaderConstantValueVector : igShaderConstantValue
	{
		public igVec4f _value;

		public override bool ShallowEquals(igShaderConstantValue other)
		{
			if (base.ShallowEquals(other))
			{
				igVec4f otherValue = ((igShaderConstantValueVector)other)._value;
				return otherValue._x == _value._x
				    && otherValue._y == _value._y
				    && otherValue._z == _value._z
				    && otherValue._w == _value._w;
			}
			return false;
		}
	}

	public class igShaderConstantValueMatrix : igShaderConstantValue
	{
		public igMatrix44f _value;

		public override bool ShallowEquals(igShaderConstantValue other)
		{
			if (base.ShallowEquals(other))
			{
				igMatrix44f otherValue = ((igShaderConstantValueMatrix)other)._value;
				return otherValue._m11 == _value._m11
				    && otherValue._m12 == _value._m12
				    && otherValue._m13 == _value._m13
				    && otherValue._m14 == _value._m14
				    && otherValue._m21 == _value._m21
				    && otherValue._m22 == _value._m22
				    && otherValue._m23 == _value._m23
				    && otherValue._m24 == _value._m24
				    && otherValue._m31 == _value._m31
				    && otherValue._m32 == _value._m32
				    && otherValue._m33 == _value._m33
				    && otherValue._m34 == _value._m34
				    && otherValue._m41 == _value._m41
				    && otherValue._m42 == _value._m42
				    && otherValue._m43 == _value._m43
				    && otherValue._m44 == _value._m44;
			}
			return false;
		}
	}

	public class igShaderConstantValueList : igGraphicsObject
	{
		public igVector<igShaderConstantValue> _values;

		public override bool ShallowEquals(igGraphicsObject other)
		{
			bool equals = other == this;

			// if they're not the same object and they're of different types
			bool confirmedFalse = !equals && other.GetMeta() != GetMeta();

			if (confirmedFalse)
			{
				equals = false;
			}
			else
			{
				igShaderConstantValueList otherList = (igShaderConstantValueList)other;

				equals = otherList._values.Count == _values.Count;
				for (int v = 0; equals && v < _values.Count; v++)
				{
					equals = _values[v].ShallowEquals(otherList._values[v]);
				}
			}

			return equals;
		}
	}
}
