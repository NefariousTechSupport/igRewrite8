/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

namespace igLibrary.Graphics
{
	public class igGraphicsObject : igObject
	{
		/// <summary>
		/// Shallow comparison of two objects to assess whether or not they're equal
		/// </summary>
		/// <param name="other">The other object</param>
		/// <returns>Whether or not the two objects are shallowly equal</returns>
		public bool ShallowEquals(igGraphicsObject other)
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
				igMetaObject metaobject = GetMeta();
				equals = true;
				for (int f = 0; f < metaobject._metaFields.Count; f++)
				{
					FieldInfo fieldInfo = metaobject._metaFields[f]._fieldHandle!;
					equals &= Equals(fieldInfo.GetValue(this), fieldInfo.GetValue(other));
				}
			}

			return equals;
		}
	}
}