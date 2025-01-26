/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;

namespace igLibrary.Core
{
	/// <summary>
	/// Reflection metadata for an enum
	/// </summary>
	public class igMetaEnum : igBaseMeta
	{
		/// <summary>
		/// No clue
		/// </summary>
		public bool _flags;


		/// <summary>
		/// The names of all the enum members
		/// </summary>
		public List<string> _names = new List<string>();


		/// <summary>
		/// The integer values of all the enum members
		/// </summary>
		public List<int> _values = new List<int>();


		/// <summary>
		/// The attributes of this igMetaEnum
		/// </summary>
		public igObject _attributes;


		/// <summary>
		/// The attributes of the members of this igMetaEnum
		/// </summary>
		public igObject _valueAttributes;


		/// <summary>
		/// The type that declared this enum inside it
		/// </summary>
		public string _declaringType;




		/// <summary>
		/// Called after reading the igMetaEnum from the igArkCoreFile
		/// </summary>
		public override void PostUndump()
		{
			Type? t = igArkCore.GetEnumDotNetType(_name);

			if(t == null)
			{
				EnumBuilder eb = igArkCore.GetNewEnumBuilder(_name);

				for(int i = 0; i < _names.Count; i++)
				{
					eb.DefineLiteral(_names[i], _values[i]);
				}

				_internalType = eb.CreateType();
				return;
			}
			else
			{
				_internalType = t;
			}
		}


		/// <summary>
		/// Get the DotNet enum from the integer value
		/// </summary>
		/// <param name="value">The integer value</param>
		/// <returns>The enum member as its DotNet type</returns>
		/// <exception cref="NotImplementedException">Exception thrown when there is no associated generated type for this enum</exception>
		public object GetEnumFromValue(int value)
		{
			if(_internalType == null) throw new NotImplementedException("this enum is not connected to any type. This feature will be implemented in the future");

			int index = _values.IndexOf(value);
			if(index < 0)
			{
				return Activator.CreateInstance(_internalType);
			}

			return Enum.Parse(_internalType, _names[index]);
		}


		/// <summary>
		/// Get the integer value from the DotNet enum member
		/// </summary>
		/// <param name="enumValue">The DotNet enum member</param>
		/// <returns>The integer value</returns>
		/// <exception cref="NotImplementedException">Exception thrown when there is no associated generated type for this enum</exception>
		public int GetValueFromEnum(object enumValue)
		{
			if(_internalType == null) throw new NotImplementedException("this enum is not connected to any type. This feature will be implemented in the future");

			int index = _names.IndexOf(enumValue.ToString());
			if(index < 0) return (int)enumValue;

			return _values[index];
		}


		/// <summary>
		/// Get the DotNet enum member from its string name
		/// </summary>
		/// <param name="name">The name of the DotNet enum member</param>
		/// <returns>The enum member as its DotNet type</returns>
		/// <exception cref="NotImplementedException">Exception thrown when there is no associated generated type for this enum</exception>
		public object GetEnumFromName(string name)
		{
			if(_internalType == null) throw new NotImplementedException("this enum is not connected to any type. This feature will be implemented in the future");

			return Enum.Parse(_internalType, name);
		}


		/// <summary>
		/// Adds the igMetaEnum to igArkCore
		/// </summary>
		public virtual void AppendToArkCore()
		{
			if(_inArkCore) return;
			igArkCore.AddEnumMeta(this);
			_inArkCore = true;
		}
	}
}