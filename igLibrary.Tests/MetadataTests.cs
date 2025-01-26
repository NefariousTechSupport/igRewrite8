/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Tests;

using igLibrary.Core;

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
}