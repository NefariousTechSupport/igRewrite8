/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CGameEntityData : CEntityData
	{
		public uint _gameEntityFlags = 512;
		public DistanceCullImportance _distanceCullImportance = DistanceCullImportance.kMedium; 
		public ETeamFilterLayers _collisionLayer = ETeamFilterLayers.eTFL_Entity;
		public ECharacterCollisionPriority _collisionPriority = ECharacterCollisionPriority.eCCP_None;
		public string _modelName;
		public string _skinName;
		public igObject _materialOverrides;
		public igHandle _collisionMaterial;
		public bool _castsShadows = true;
		public EMobileShadowState _mobileShadowState;
		public float _lifetime;
		public float _lifetimeModifier;
		public EMemoryPoolID _cachedAssetPool = EMemoryPoolID.MP_MAX_POOL;
		public EMaterial _material;	//technically a property but that doesn't work well here
	}
}