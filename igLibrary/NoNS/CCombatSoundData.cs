/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CCombatSoundData : igObject
	{
		public bool _playAttackSoundOnVictim = true; 
		public string _attackSound = null;
		public string _victimSound = "COM_Impact_Victim_Basic";
		public igHandle _attackSoundHandle;		//It's a CSound
		public igHandle _victimSoundHandle;		//It's a CSound
		public CUpgradeableValue _attackSoundUpgradeable;	//This SHOULD be CUpgradeableSound but hotdog's combat file has an instance that references a CUpgradeableVfx object
		public CUpgradeableValue _victimSoundUpgradeable;	//This SHOULD be CUpgradeableSound but hotdog's combat file has an instance that references a CUpgradeableVfx object
	}
}