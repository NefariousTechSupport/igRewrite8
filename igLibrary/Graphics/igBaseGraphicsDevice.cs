/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;

namespace igLibrary.Graphics
{
	public abstract class igBaseGraphicsDevice : igTContext<igBaseGraphicsDevice>
	{
		public igBaseGraphicsDevice() : base() {}
		public virtual int CreateTexture(igResourceUsage usage, igImage2 image) => throw new NotImplementedException("Create texture unimplemented");
		public virtual void FreeTexture(int texture) => throw new NotImplementedException("Free texture unimplemented");
	}
}