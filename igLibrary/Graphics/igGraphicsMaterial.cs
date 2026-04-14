/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public class igGraphicsMaterial : igMaterial
	{
		public ulong _globalTechniqueMask;
		public uint _materialBitField;
		public float _sortDepthOffset;
		public igHandle _effectHandle;
		public igMemoryCommandStream _commonState;
		public igVector<igMemoryCommandStream> _techniques;
		public igGraphicsMaterialAnimationList _animations;
		public igGraphicsObjectSet _graphicsObjects;
		public byte _sortKey;
		public igDrawType _drawType;
		public igGraphicsMaterialAnimationTimeSource _timeSource;
	}
}