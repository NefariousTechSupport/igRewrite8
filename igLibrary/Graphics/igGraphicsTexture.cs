/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;

namespace igLibrary.Graphics
{
	public class igGraphicsTexture : igGraphicsObject
	{
		public igResourceUsage _usage;
		public igImage2? _image;
		public igHandle? _imageHandle;
		public ulong _resource;
	}
}
