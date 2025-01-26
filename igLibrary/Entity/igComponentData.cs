/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Entity
{
	public class igComponentData : igLibrary.DotNet.Object
	{
		public uint _bitfield;
		public bool _isEnabled = true;
		public bool _isThreadSafe;
	}
}