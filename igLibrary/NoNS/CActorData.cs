/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class CActorData : CPhysicalEntityData
	{
		public uint _actorDataFlags = 16;
		public string _character;
		public string _skin;
		public string _magicMomentModel;
		public float _magicMomentSpawnBackgroundVfxOverrideTime = -1;
		public float _magicMomentSpawnOutroVfxOverrideTime = -1;
		public float _magicMomentStartEndVfxOverrideTime = -1;
		public float _magicMomentShowNameOverrideTime = -1;
		public float _magicMomentHideNameOverrideTime = -1;
		public float _magicMomentPauseIntroAnimationOverrideTime = -1;
		public float _magicMomentJumpOutTimeFromEndOverride = -1;
		public string _characterAnimations;
		public string _characterAnimationBase;
		public igObject _soundBankHandleList;
		public string _characterScript;
		public float _aiAlertRange;
		public bool _isShapeshifter;
		public EAllowedHitReactDirections _takeHitReactDirections = EAllowedHitReactDirections.eAHRD_NoDirection;
		public EAllowedHitReactDirections _partialHitReactDirections;
		public EAllowedHitReactDirections _knockawayReactDirections = EAllowedHitReactDirections.eAHRD_NoDirection;
		public EAllowedHitReactDirections _deathReactDirections = EAllowedHitReactDirections.eAHRD_Front;
		public EAllowedHitReactDirections _knockawayDeathReactDirections;
		public igHandle _hudPortrait;
		public igHandle _racingHudPortrait;
		public igHandle _footstepEffect;
		public igHandle _magicMomentNameEffect;
		public CActorData()
		{
			_gameEntityFlags = 512;
			_collisionPriority = ECharacterCollisionPriority.eCCP_High;
			_vulnerability = EVulnerability.eV_CanBeDamaged;
		}

	}
}