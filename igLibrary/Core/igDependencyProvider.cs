/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	//TODO: Define dependency providers for
	// -  igComponentDependencyProvider
	// -  CZoneInfoDependencyProvider
	// -  CWorldEntityDataDependencyProvider
	// -  CCRMSystemDataDependencyProvider
	// -  CStaticComponentDependencyProvider
	// -  CStaticEntityDataDependencyProvider
	// -  CEdcInfoDependencyProvider
	// -  CQuestInteractionCutsceneOnLoadDependencyProvider
	// -  CObjectiveDependencyProvider
	// -  CActorDataDependencyProvider
	// -  CCutsceneActorDataDependencyProvider
	// -  CQuestSystemDataDependencyProvider
	// -  CPortalMasterPerkDependencyProvider
	// -  CWorldDataDependencyProvider
	// -  CLevelBorderComponentDataDependencyProvider
	// -  CVehicleSystemDataDependencyProvider
	// -  CStaticCollisionComponentDependencyProvider
	// -  CVehiclePersonalizationDataDependencyProvider
	// -  CAudioArchiveDependencyProvider
	public class igDependencyProvider : igObject
	{
		public IG_CORE_PLATFORM _platform;
		public igObjectDirectory _directory;
		public virtual void GetFileDependencies(igObject obj, out igStringRefList? output){output = null;}
		public virtual void GetBuildDependencies(igObject obj, out igStringRefList? output){output = null;}
	}
}