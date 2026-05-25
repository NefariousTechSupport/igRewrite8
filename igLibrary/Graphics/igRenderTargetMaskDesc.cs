/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	[igStruct]
	public struct igRenderTargetMaskDesc
	{
		public uint _mask;
		public uint _hash;
		public bool _hashDirty;
	}
}
