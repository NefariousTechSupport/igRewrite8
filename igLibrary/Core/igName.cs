/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	/// <summary>
	/// Represents a name along with its fnv1a32 hash
	/// </summary>
	[igStruct]
	public struct igName
	{
		public string _string = string.Empty;
		public uint _hash = 0;



		/// <summary>
		/// Constructor
		/// </summary>
		public igName()
		{
		}



		/// <summary>
		/// Constructor taking in a name 
		/// </summary>
		/// <param name="name">The name to set</param>
		public igName(string name)
		{
			SetString(name);
		}



		/// <summary>
		/// Constructor taking in a hash without a string
		/// </summary>
		/// <param name="hash">The hash to set</param>
		public igName(uint hash)
		{
			_hash = hash;
		}



		/// <summary>
		/// Set the string value and update the hash
		/// </summary>
		/// <param name="newString">The new string to set it to</param>
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



		/// <summary>
		/// Allow for printing the igName, either showing the string or the fnv1a32 hash in hex
		/// </summary>
		/// <returns>The string representation</returns>
		public override string ToString()
		{
			return _string ?? _hash.ToString("08X");
		}
	}
}