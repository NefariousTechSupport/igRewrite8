/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Tests;

using igLibrary.Core;
using igLibrary.Tfb.Script;
using igLibrary.Vfx;
using igLibrary.Tfb;

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
	/// Ensure that the metadata for igVfxModulationHelper has been corrected
	/// </summary>
	[Fact]
	public void TrapTeamVfxModulationHelper()      => VfxModulationHelperChecks(igArkCore.EGame.EV_SkylandersTrapTeam);



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



	/// <summary>
	/// Ensure a type has its element type set to tfbScriptObject
	/// instead of whatever it was before. Tfb love assigning the wrong type to things.
	/// </summary>
	private void TfbScriptObjectElementTypeChecks(string metaobjectName)
	{
		igMetaObject? meta = igArkCore.GetObjectMeta(metaobjectName);
		Assert.NotNull(meta);

		igMetaField? dataField = meta.GetFieldByName("_data");
		Assert.NotNull(dataField);

		Assert.IsType<igMemoryRefMetaField>(dataField);
		igMemoryRefMetaField dataFieldMem = (igMemoryRefMetaField)dataField;

		Assert.NotNull(dataFieldMem._memType);
		Assert.IsType<igObjectRefMetaField>(dataFieldMem._memType);

		igObjectRefMetaField memTypeField = (igObjectRefMetaField)dataFieldMem._memType;
		Assert.NotNull(memTypeField._metaObject);
		Assert.Equal("tfbScriptObject", memTypeField._metaObject._name);
	}


    /// <summary>
    ///  Ensure metaenums which share the same name are overwritten
    ///  to "(Object)_Metaenum" 
    ///  example: State -> AbstractPlacement_State
    /// </summary>
    [Fact]
    public void MetaEnumCheck()
	{
		igArkCore.ReadFromXmlFile(igArkCore.EGame.EV_SkylandersTrapTeam);
		CheckEnum("AbstractPlacement_State");
		CheckEnum("igIGZLoader_State");
	}
	private void CheckEnum(string metaenum2)
	{
		igMetaEnum? metaEnum = igArkCore.GetMetaEnum(metaenum2);
        Assert.NotNull(metaEnum);
		switch (metaEnum._name)
		{
			case "AbstractPlacement_State":
                Assert.Equal("FIRST_PLACEMENT_STATE", metaEnum._names[0]);
                Assert.Equal(0, metaEnum._values[0]);
                Assert.Equal("PLACEMENT_PLAYER", metaEnum._names[1]);
                Assert.Equal(0, metaEnum._values[1]);
                Assert.Equal("PLACEMENT_ACTIVE", metaEnum._names[2]);
                Assert.Equal(1, metaEnum._values[2]);
                Assert.Equal("PLACEMENT_INACTIVE", metaEnum._names[3]);
                Assert.Equal(2, metaEnum._values[3]);
                Assert.Equal("NUM_ITERATED_PLACEMENT_STATES", metaEnum._names[4]);
                Assert.Equal(3, metaEnum._values[4]);
                Assert.Equal("PLACEMENT_REMOVED", metaEnum._names[5]);
                Assert.Equal(3, metaEnum._values[5]);
                Assert.Equal("NUM_MANAGED_PLACEMENT_STATES", metaEnum._names[6]);
                Assert.Equal(4, metaEnum._values[6]);
                Assert.Equal("PLACEMENT_UNMANAGED", metaEnum._names[7]);
                Assert.Equal(4, metaEnum._values[7]);
                Assert.Equal("PLACEMENT_TEMPLATE", metaEnum._names[8]);
                Assert.Equal(5, metaEnum._values[8]);
                break;
			case "igIGZLoader_State":
                Assert.Equal("kStateIdle", metaEnum._names[0]);
                Assert.Equal(0, metaEnum._values[0]);
                Assert.Equal("kStateOpening", metaEnum._names[1]);
                Assert.Equal(1, metaEnum._values[1]);
                Assert.Equal("kStateOpened", metaEnum._names[2]);
                Assert.Equal(2, metaEnum._values[2]);
                Assert.Equal("kStateReadingHeader", metaEnum._names[3]);
                Assert.Equal(3, metaEnum._values[3]);
                Assert.Equal("kStateReadHeader", metaEnum._names[4]);
                Assert.Equal(4, metaEnum._values[4]);
                Assert.Equal("kStateReadingSections", metaEnum._names[5]);
                Assert.Equal(5, metaEnum._values[5]);
                Assert.Equal("kStateReadSections", metaEnum._names[6]);
                Assert.Equal(6, metaEnum._values[6]);
                Assert.Equal("kStateFinished", metaEnum._names[7]);
                Assert.Equal(7, metaEnum._values[7]);
                Assert.Equal("kStateAborting", metaEnum._names[8]);
                Assert.Equal(8, metaEnum._values[8]);
                Assert.Equal("kStateFailed", metaEnum._names[9]);
                Assert.Equal(9, metaEnum._values[9]);
                break;
			default:
				break;
		}
    }

	/// <summary>
	/// Ensure various types have their element type set to tfbScriptObject
	/// instead of whatever it was before. Tfb love assigning the wrong type to things.
	/// </summary>
	[Fact]
	private void ValueStackElementType()
	{
		igArkCore.ReadFromXmlFile(igArkCore.EGame.EV_SkylandersTrapTeam);

		TfbScriptObjectElementTypeChecks("AbstractPlacementList");
		TfbScriptObjectElementTypeChecks("PositionStack");
		TfbScriptObjectElementTypeChecks("ReferenceStack");
		TfbScriptObjectElementTypeChecks("RHSReferenceStack");
		TfbScriptObjectElementTypeChecks("RHSValueStack");
		TfbScriptObjectElementTypeChecks("SetStack");
		TfbScriptObjectElementTypeChecks("ValueStack");
		TfbScriptObjectElementTypeChecks("VectorStack");
		TfbScriptObjectElementTypeChecks("ScriptVariantList");
        TfbScriptObjectElementTypeChecks("TagList");
        TfbScriptObjectElementTypeChecks("OpCreateVariableList");
        TfbScriptObjectElementTypeChecks("OpCodeList");
        TfbScriptObjectElementTypeChecks("SoundList");
        TfbScriptObjectElementTypeChecks("SpriteInfoList");
    }



	/// <summary>
	/// Ensure AbstractScriptGroup's bindings are of type AbstractScriptVariant
	/// instead of tfbScriptObject
	/// </summary>
	[Fact]
	private void AbstractScriptGroupBindingsTest()
	{
		igArkCore.ReadFromXmlFile(igArkCore.EGame.EV_SkylandersTrapTeam);

		igHandle itsHandle = igObjectHandleManager.Singleton.LookupHandle(new igHandleName("AbstractScriptGroup.its"));
		igHandle nullHandle = igObjectHandleManager.Singleton.LookupHandle(new igHandleName("AbstractScriptGroup.null"));

		// Baseline to make sure it exists
		Assert.NotNull(itsHandle.GetObjectAlias<igObject>());
		Assert.NotNull(nullHandle.GetObjectAlias<igObject>());

		Assert.NotNull(itsHandle.GetObjectAlias<AbstractScriptVariant>());
		Assert.NotNull(nullHandle.GetObjectAlias<AbstractScriptVariant>());
	}
}