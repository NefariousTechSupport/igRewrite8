/*
	Copyright (c) 2022-2025, The VvlToDll Contributors.
	VvlToDll and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;
using igLibrary.Core;
using igLibrary.Gfx;

namespace igLibrary
{
	public static class VvlPackagePrecacher
	{
		public static void Intialize()
		{
			// We're gonna really mess up the precacher lol

			CPrecacheManager manager = CPrecacheManager._Instance;
			manager._resourcePrecachers = new CResourcePrecacherList();
			manager._resourcePrecachers.SetCapacity(0x24);
			manager._resourcePrecacherLookup = new CStringResourcePrecacherHashTable();
			manager._resourcePrecacherLookup.Activate(0x24);
			RegisterResourcePrecacher(manager,                "pkg", new COtherPackagePrecacher());
			RegisterResourcePrecacher(manager,     "character_data", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,          "actorskin", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,        "havokanimdb", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,     "havokrigidbody", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager, "havokphysicssystem", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,            "texture", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,             "effect", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,             "shader", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,         "motionpath", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,           "igx_file", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager, "material_instances", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,       "igx_entities", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,        "gui_project", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,               "font", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,          "lang_file", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,          "spawnmesh", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,              "model", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,          "sky_model", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,           "behavior", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager, "graphdata_behavior", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,    "events_behavior", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,     "asset_behavior", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,       "hkb_behavior", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,      "hkc_character", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,            "navmesh", new CDoNothingPrecacher());
			RegisterResourcePrecacher(manager,             "script", new CScriptPrecacher());
		}


		private static void RegisterResourcePrecacher(CPrecacheManager manager, string name, CResourcePrecacher precacher)
		{
			manager._resourcePrecachers.Append(precacher);
			manager._resourcePrecacherLookup.Add(name, precacher);
		}
	}

	public class CDoNothingPrecacher : CResourcePrecacher
	{
		public override void Precache(string filePath)
		{
			// Do nothing
		}
	}
}