/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Tests;

using igLibrary.Core;
using igLibrary.Vfx;

public class MetadataTest
{
	/// <summary>
	/// Just load a game and ensure igNamedObject is good
	/// </summary>
	/// <param name="game">The game to load</param>
	/// <param name="basePlatform">The platform the metaobjects file was from</param>
	private void LoadingAGame(igArkCore.EGame game, IG_CORE_PLATFORM basePlatform)
	{
		// Ensure loading actually works
		igArkCore.ReadFromXmlFile(game);

		// Grabbing a type
		igMetaObject? namedObjectMeta = igArkCore.GetObjectMeta("igNamedObject");
		Assert.NotNull(namedObjectMeta);

		// Check fields
		Assert.Single(namedObjectMeta._metaFields);

		Assert.IsType<igStringMetaField>(namedObjectMeta._metaFields[0]);
		igStringMetaField nameField = (igStringMetaField)namedObjectMeta._metaFields[0];

		Assert.Equal("_name", nameField._fieldName);

		uint offset = igAlchemyCore.isPlatform64Bit(basePlatform) ? 0x0Cu : 0x08u;
		Assert.Equal(offset, nameField._offset);

		// Reset
		igArkCore.Reset();
	}



	/// <summary>
	/// Load superchargers and check igNamedObject
	/// </summary>
	[Fact]
	public void LoadingSuperchargers()
	{
		LoadingAGame(igArkCore.EGame.EV_SkylandersSuperchargers, IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3);
	}



	/// <summary>
	/// Load imaginators and check igNamedObject
	/// </summary>
	[Fact]
	public void LoadingImaginators()
	{
		LoadingAGame(igArkCore.EGame.EV_SkylandersImaginators,   IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3);
	}



	/// <summary>
	/// Ensure issues for imaginators #60, #61, and #62 are not regressed
	/// </summary>
	[Fact]
	public void ImaginatorsMaterialTypeBug()
	{
		igArkCore.ReadFromXmlFile(igArkCore.EGame.EV_SkylandersImaginators);

		// igFxMateriaList

		igMetaObject? fxMaterialList = igArkCore.GetObjectMeta("igFxMaterialList");
		Assert.NotNull(fxMaterialList);
		Assert.NotNull(fxMaterialList._parent);
		Assert.Equal("igObjectList", fxMaterialList._parent._name);
		Assert.Equal(3, fxMaterialList._metaFields.Count);
		Assert.IsType<igMemoryRefMetaField>(fxMaterialList._metaFields[2]);
		Assert.Equal("_data", fxMaterialList._metaFields[2]._fieldName);

		igMemoryRefMetaField dataField = (igMemoryRefMetaField)fxMaterialList._metaFields[2];
		Assert.IsType<igObjectRefMetaField>(dataField._memType);
		igObjectRefMetaField dataMemType = (igObjectRefMetaField)dataField._memType;
		Assert.Equal("igMaterial", dataMemType._metaObject._name);

		// CGuiBehaviorSkylanderCreate

		igMetaObject? skylanderCreateMeta = igArkCore.GetObjectMeta("CGuiBehaviorSkylanderCreate");
		Assert.NotNull(skylanderCreateMeta);
		igMetaField? tassetIconField = skylanderCreateMeta.GetFieldByName("_tassetIcon");
		Assert.NotNull(tassetIconField);
		Assert.IsType<igObjectRefMetaField>(tassetIconField);
		igMetaObject? tassetMetaobject = ((igObjectRefMetaField)tassetIconField)._metaObject;
		Assert.NotNull(tassetMetaobject);
		Assert.Equal("igMaterial", tassetMetaobject._name);

		// CCYOSClassData

		igMetaObject? cyosMetaobject = igArkCore.GetObjectMeta("CCYOSClassData");
		Assert.NotNull(cyosMetaobject);
		igMetaField? classImageField = cyosMetaobject.GetFieldByName("_classImage");
		Assert.NotNull(classImageField);
		Assert.IsType<igObjectRefMetaField>(classImageField);
		igMetaObject? classImageMetaobject = ((igObjectRefMetaField)classImageField)._metaObject;
		Assert.NotNull(classImageMetaobject);
		Assert.Equal("igMaterial", classImageMetaobject._name);

		igArkCore.Reset();
	}



	/// <summary>
	/// Ensure that the metadata for igVfxModulationHelper has been corrected
	/// The one from the metadata dump is actually incorrect, screw you vv
	/// </summary>
	private void VfxModulationHelperChecks(igArkCore.EGame game)
	{
		igArkCore.ReadFromXmlFile(game);

		igCompoundMetaFieldInfo? modulationHelperMeta = igArkCore.GetCompoundFieldInfo(nameof(igVfxModulationHelperMetaField));
		Assert.NotNull(modulationHelperMeta);

		igMetaField? noiseDataField = modulationHelperMeta.GetFieldByName("_noiseData");
		igMetaField? rngField       = modulationHelperMeta.GetFieldByName("_rng");

		Assert.NotNull(noiseDataField);
		Assert.NotNull(rngField);

		Assert.IsType<igStaticMetaField>(noiseDataField);
		Assert.IsType<igStaticMetaField>(rngField);
	}



	/// <summary>
	/// Ensure that the metadata for igVfxModulationHelper has been corrected
	/// </summary>
	[Fact]
	public void SuperChargersVfxModulationHelper() => VfxModulationHelperChecks(igArkCore.EGame.EV_SkylandersSuperchargers);



	/// <summary>
	/// Ensure that the metadata for igVfxModulationHelper has been corrected
	/// </summary>
	[Fact]
	public void ImaginatorsVfxModulationHelper()   => VfxModulationHelperChecks(igArkCore.EGame.EV_SkylandersImaginators);



	/// <summary>
	/// Ensure that the metadata for igSamplerStateBundleDescMetaField has been corrected
	/// The one from the metadata dump forgets to list the default for a non-persistent boolean
	/// </summary>
	private void SamplerStateBundleDescChecks(igArkCore.EGame game)
	{
		igArkCore.ReadFromXmlFile(game);

		igCompoundMetaFieldInfo? samplerStateBundleMeta = igArkCore.GetCompoundFieldInfo("igSamplerStateBundleDescMetaField");
		Assert.NotNull(samplerStateBundleMeta);

		igMetaField? hashDirtyField = samplerStateBundleMeta.GetFieldByName("_hashDirty");

		Assert.NotNull(hashDirtyField);

		Assert.IsType<igBoolMetaField>(hashDirtyField);

		Assert.NotNull(hashDirtyField._default);
		Assert.True((bool)hashDirtyField._default);
	}



	/// <summary>
	/// Ensure that the metadata for igVfxModulationHelper has been corrected
	/// </summary>
	[Fact]
	public void SuperChargersSamplerStateBundleDesc() => SamplerStateBundleDescChecks(igArkCore.EGame.EV_SkylandersSuperchargers);



	/// <summary>
	/// Ensure that the metadata for igVfxModulationHelper has been corrected
	/// </summary>
	[Fact]
	public void ImaginatorsSamplerStateBundleDesc()   => SamplerStateBundleDescChecks(igArkCore.EGame.EV_SkylandersImaginators);
}