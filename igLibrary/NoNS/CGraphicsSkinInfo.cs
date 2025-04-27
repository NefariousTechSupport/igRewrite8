/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Anim;
using igLibrary.Render;


namespace igLibrary
{
	public class CGraphicsSkinInfo : igInfo
	{
		public igSkeleton2 _skeleton;
		public igModelData _skin;
		public igStringIntHashTable? _boltPointIndexArray;
		public CHavokSkeleton? _havokSkeleton;
		public igVec3f _boundsMin;
		public igVec3f _boundsMax;
	}
}