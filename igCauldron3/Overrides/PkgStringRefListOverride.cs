/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Windows.Forms.VisualStyles;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI override for rendering pkg lists
	/// </summary>
	public class PkgStringRefListOverride : InspectorDrawOverride
	{
		private Dictionary<string, igStringRefList> _lists;


		/// <summary>
		/// Constructor
		/// </summary>
		public PkgStringRefListOverride()
		{
			_t = typeof(igStringRefList);
			_lists = new Dictionary<string, igStringRefList>();
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <param name="dirFrame">The directory manager frame</param>
		/// <param name="id">the id to render with</param>
		/// <param name="obj">the object</param>
		/// <param name="meta">the type of the object</param>
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			igStringRefList stringList = (igStringRefList)obj;

			// do initialisation
			if (_lists.Count == 0)
			{
				Recompute(stringList);
			}

			ImGui.NewLine();

			ImGui.Indent();
			ImGui.PushID(id);
			foreach (KeyValuePair<string, igStringRefList> kvp in _lists)
			{
				IigMemory memValue = kvp.Value._data;

				FieldRenderer.RenderField(
					kvp.Key,
					$"{kvp.Key} ({kvp.Value.Count} {(kvp.Value.Count == 1 ? "item" : "items")})",
					memValue,
					meta._metaFields[2],
					(value) => {
						kvp.Value.SetData(memValue);
						kvp.Value.SetCount(memValue.GetData().Length);
						kvp.Value.SetCapacity(memValue.GetData().Length);

						Populate(stringList);
					}
				);
			}
			ImGui.PopID();
			ImGui.Unindent();
		}


		/// <summary>
		/// Repopulate the ui data
		/// </summary>
		/// <param name="stringList">the string list to repopulate from</param>
		private void Recompute(igStringRefList stringList)
		{
			_lists.Clear();

			_lists.Add(               "pkg", new igStringRefList());
			_lists.Add(    "character_data", new igStringRefList());
			_lists.Add(         "actorskin", new igStringRefList());
			_lists.Add(       "havokanimdb", new igStringRefList());
			_lists.Add(    "havokrigidbody", new igStringRefList());
			_lists.Add("havokphysicssystem", new igStringRefList());
			_lists.Add(           "texture", new igStringRefList());
			_lists.Add(            "effect", new igStringRefList());
			_lists.Add(            "shader", new igStringRefList());
			_lists.Add(        "motionpath", new igStringRefList());
			_lists.Add(          "igx_file", new igStringRefList());
			_lists.Add("material_instances", new igStringRefList());
			_lists.Add(      "igx_entities", new igStringRefList());
			_lists.Add(       "gui_project", new igStringRefList());
			_lists.Add(              "font", new igStringRefList());
			_lists.Add(         "lang_file", new igStringRefList());
			_lists.Add(         "spawnmesh", new igStringRefList());
			_lists.Add(             "model", new igStringRefList());
			_lists.Add(         "sky_model", new igStringRefList());
			_lists.Add(          "behavior", new igStringRefList());
			_lists.Add("graphdata_behavior", new igStringRefList());
			_lists.Add(   "events_behavior", new igStringRefList());
			_lists.Add(    "asset_behavior", new igStringRefList());
			_lists.Add(      "hkb_behavior", new igStringRefList());
			_lists.Add(     "hkc_character", new igStringRefList());
			_lists.Add(           "navmesh", new igStringRefList());
			_lists.Add(            "script", new igStringRefList());

			// floor to closest multiple of 2
			int safeCount = stringList._count & ~1;

			for (int i = 0; i < safeCount; i += 2)
			{
				igStringRefList? assetList;
				if (!_lists.TryGetValue(stringList[i], out assetList))
				{
					assetList = new igStringRefList();
					_lists.Add(stringList[i], assetList);
				}

				assetList.Append(stringList[i+1]);
			}

			foreach (KeyValuePair<string, igStringRefList> kvp in _lists)
			{
				kvp.Value.SetCapacity(kvp.Value.Count);
			}
		}


		/// <summary>
		/// Repopulate the string list from the ui data
		/// </summary>
		/// <param name="stringList">The string list to repopulate</param>
		private void Populate(igStringRefList stringList)
		{
			stringList.Clear();

			// This is guessed
			PopulateItem(stringList, "script");
			PopulateItem(stringList, "lang_file");
			PopulateItem(stringList, "havokphysicssystem");
			PopulateItem(stringList, "igx_file");
			PopulateItem(stringList, "shader");
			PopulateItem(stringList, "texture");
			PopulateItem(stringList, "material_instances");
			PopulateItem(stringList, "model");
			PopulateItem(stringList, "havokrigidbody");
			PopulateItem(stringList, "actorskin");
			PopulateItem(stringList, "sky_model");
			PopulateItem(stringList, "spawnmesh");
			PopulateItem(stringList, "havokanimdb");
			PopulateItem(stringList, "effect");
			PopulateItem(stringList, "hkb_behavior");
			PopulateItem(stringList, "asset_behavior");
			PopulateItem(stringList, "hkc_character");
			PopulateItem(stringList, "font");
			PopulateItem(stringList, "motionpath");
			PopulateItem(stringList, "behavior");
			PopulateItem(stringList, "graphdata_behavior");
			PopulateItem(stringList, "events_behavior");
			PopulateItem(stringList, "character_data");
			PopulateItem(stringList, "igx_entities");
			PopulateItem(stringList, "navmesh");
			PopulateItem(stringList, "gui_project");
			PopulateItem(stringList, "pkg");
		}


		/// <summary>
		/// populate a specific asset type
		/// </summary>
		/// <param name="stringList">the string list to repopulate into</param>
		/// <param name="item">the asset type</param>
		private void PopulateItem(igStringRefList stringList, string item)
		{
			if (_lists.TryGetValue(item, out igStringRefList? itemList))
			{
				for (int i = 0; i < itemList.Count; i++)
				{
					stringList.Add(item);
					stringList.Add(itemList[i]);
				}
			}
		}
	}
}