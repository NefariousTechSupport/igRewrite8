/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	[igStruct]
	public struct igName
	{
		public string _string = string.Empty;
		public uint _hash = 0;
		public igName(){}
		public igName(string name)
		{
			SetString(name);
		}
		public igName(uint hash)
		{
			_hash = hash;
		}
		public void SetString(string? newString)
		{
			if (newString == null || newString == "(null)")
			{
				_string = null;
				_hash = 0;
			}
			else
			{
				_string = newString;
				_hash = igHash.HashI(newString);
			}
		}
	}
}