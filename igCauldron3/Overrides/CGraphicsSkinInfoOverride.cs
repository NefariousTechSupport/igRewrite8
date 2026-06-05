/*
	Copyright (c) 2022-2026, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Diagnostics;
using Assimp;
using Assimp.Configs;
using igCauldron3.Utils;
using igLibrary;
using igLibrary.AssetConversion.Models;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI override for rendering CGraphicsSkinInfo objects
	/// </summary>
	public class CGraphicsSkinInfoOverride : InspectorDrawOverride
	{
		private static bool _initialised = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		private static igMetaField _field_skin;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


		/// <summary>
		/// Constructor
		/// </summary>
		public CGraphicsSkinInfoOverride()
		{
			_t = typeof(CGraphicsSkinInfo);
		}


		/// <summary>
		/// Sets up the metafields used by this override
		/// </summary>
		private static void SetupMetaFields()
		{
			if (_initialised)
			{
				return;
			}

			igMetaObject? skinInfoMeta = igArkCore.GetObjectMeta(nameof(CGraphicsSkinInfo));

			Debug.Assert(skinInfoMeta != null);

			_field_skin = skinInfoMeta.GetFieldByName(nameof(CGraphicsSkinInfo._skin))!;

			Debug.Assert(_field_skin != null);

			_initialised = true;
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
			CGraphicsSkinInfo skinInfo = (CGraphicsSkinInfo)obj;

			SetupMetaFields();

			RenderField(id, skinInfo, _field_skin);

			if (ImGui.Button("Extract"))
			{
				
			}

			ImGui.SameLine();

			if (ImGui.Button("Replace"))
			{
				string input = CrossFileDialog.OpenFile("Choose a model file", ".3ds;.dae;.fbx;.gltf;.glb;.obj");

				if (!string.IsNullOrEmpty(input))
				{
					ImportActor(input, skinInfo);
				}
			}
		}


		private void ImportActor(string input, CGraphicsSkinInfo output)
		{
			AssimpContext ctx = new AssimpContext();
			ctx.SetConfig(new ColladaUseColladaNamesConfig(true));
			Scene scene = ctx.ImportFile(input, PostProcessSteps.JoinIdenticalVertices);

			SuperChargersModel sscmodel = new SuperChargersModel();
			string? error = sscmodel.ImportActor(scene, out CGraphicsSkinInfo? skinInfo);

			if (skinInfo != null)
			{
				igMetaObject materialHandleTableInfoMeta = igArkCore.GetObjectMeta("CMaterialHandleTableInfo")!;
				igObject materialHandleTableInfo = materialHandleTableInfoMeta.ConstructInstance(igMemoryContext.Default);
				igStringInsensitiveStringHashTable hashTable = igMetaObject.ConstructInstance<igStringInsensitiveStringHashTable>();
				materialHandleTableInfoMeta.GetFieldByName("_handleTable")!._fieldHandle!.SetValue(materialHandleTableInfo, hashTable);

				output._name      = skinInfo._name;
				output._skeleton  = skinInfo._skeleton;
				output._skin      = skinInfo._skin;
				output._boundsMin = skinInfo._boundsMin;
				output._boundsMax = skinInfo._boundsMax;
			}
			else if (error != null)
			{
				Window._instance._frames.Add(new ErrorModalFrame(Window._instance, "Actor Import Error", error));
			}
		}
	}
}