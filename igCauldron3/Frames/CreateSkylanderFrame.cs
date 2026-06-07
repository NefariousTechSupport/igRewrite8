/*
	Copyright (c) 2022-2026, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Diagnostics;
using igLibrary;
using igLibrary.Core;
using igLibrary.Entity;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// The UI frame for creating a new skylander
	/// </summary>
	public class CreateSkylanderFrame : Frame
	{
		private string _name = string.Empty;
		const string kTemplateName = "BallChain";


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">The window to parent the frame to</param>
		public CreateSkylanderFrame(Window wnd) : base(wnd)
		{
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("New Skylander", ImGuiWindowFlags.NoDocking);

			UIUtil.RenderTextField("Name", "$name$", ref _name, "The name of the new skylander");

			if (ImGui.Button("Create"))
			{
				Create();
			}

			if (ImGui.Button("Close"))
			{
				Close();
			}
			ImGui.End();
		}


		/// <summary>
		/// Creates the skylander
		/// </summary>
		private void Create()
		{
			// Load ballchain as our template
			CPrecacheManager._Instance.PrecachePackage($"generated/characters/{kTemplateName}", EMemoryPoolID.MP_DEFAULT);

			igArchive archive = new igArchive();
			archive._name = $"app:/archives/{_name}.pak";
			archive._path = igFilePath.GetNativePath(archive._name);
			archive._archiveHeader._version = 0x0B;
			archive._archiveHeader._sectorSize = 0x800;
			archive._archiveHeader._flags = 1;

			igObjectDirectory pkgDir = new igObjectDirectory($"packages/generated/Characters/{_name}_pkg.igz");
			pkgDir._nameList = new igNameList();
			pkgDir._useNameList = true;
			pkgDir._type = igObjectDirectory.FileType.kIGZ;
			igStringRefList pkg = new igStringRefList();
			pkgDir.AddObject(pkg, default, new igName("list"));

			igObjectDirectory actorskinDir = CreateActorSkinDir();

			CopyFileToArchive(archive, pkg, "script",             "scripts/{0}_script.vvl");
			pkg.Add("actorskin");
			pkg.Add(actorskinDir._path);
			CopyFileToArchive(archive, pkg, "havokanimdb",        "anims/Skylanders/{0}.hka");
			CopyFileToArchive(archive, pkg, "behavior",           "behaviors/Skylanders/{0}/{0}.hkp");
			CopyFileToArchive(archive, pkg, "graphdata_behavior", "behaviors/Skylanders/{0}/{0}.igz");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/AirLocomotion_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/{0}_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/EmotionTemplate_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/GroundLocomotion_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/HitReacts_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/Interactions_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/MagicMoment_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "hkb_behavior",       "behaviors/Skylanders/{0}/behaviors/PlayerMain_Behavior.hkb");
			CopyFileToArchive(archive, pkg, "asset_behavior",     "behaviors/Skylanders/{0}/characterAssets/{0}.hkx");
			CopyFileToArchive(archive, pkg, "hkc_character",      "behaviors/Skylanders/{0}/characters/{0}_character.hkc");
			CopyFileToArchive(archive, pkg, "events_behavior",    "behavior_events/Skylanders/{0}_combat.igz");
			CopyFileToArchive(archive, pkg, "character_data",     "Characters/Skylanders/{0}_CharacterData.igz");


			MemoryStream actorskinStream = new MemoryStream();
			actorskinDir.WriteFile(actorskinStream, igRegistry.GetRegistry()._platform);
			igArchive.FileInfo actorskinFile = archive.GetAddFile(actorskinDir._path);
			archive.Compress(actorskinFile, actorskinStream);

			MemoryStream pkgStream = new MemoryStream();
			pkgDir.WriteFile(pkgStream, igRegistry.GetRegistry()._platform);
			igArchive.FileInfo pkgFile = archive.GetAddFile(pkgDir._path);
			archive.Compress(pkgFile, pkgStream);

			archive.Save($"{_name}.pak");

			igFileContext.Singleton._archiveManager.RegisterArchive($"app:/archives/{_name}.pak", archive);

			// Load the character
			CPrecacheManager._Instance.PrecachePackage($"generated/characters/{_name}", EMemoryPoolID.MP_DEFAULT);

			// Set up their character data
			igObjectDirectory? characterData = igObjectStreamManager.Singleton.Load($"Characters/Skylanders/{_name}_CharacterData.igz");
			Debug.Assert(characterData != null);
			if (characterData._useNameList && characterData._nameList != null)
			{
				igHandleName oldName = new igHandleName($"{_name}_CharacterData.fake");
				igHandleName newName = oldName;
				for (int i = 0; i < characterData._nameList.Count; i++)
				{
					if (characterData._nameList[i]._string.StartsWith(kTemplateName))
					{
						oldName._name = characterData._nameList[i];
						characterData._nameList[i] = new igName(characterData._nameList[i]._string.ReplaceBeginning(kTemplateName, _name));
						newName._name = characterData._nameList[i];
						igObjectHandleManager.Singleton.RenameHandle(oldName, newName);
					}
				}
			}
			CActorData? actorData = characterData.GetObjectOfType<CActorData>();
			Debug.Assert(actorData != null);
			actorData._character = _name;
			actorData._skin = _name;
			actorData._characterAnimations = $"Skylanders/{_name}";
			actorData._characterScript = $"scripts/{_name}";
			CBehaviorComponentData? behaviorData = actorData.GetComponentData<CBehaviorComponentData>();
			Debug.Assert(behaviorData != null);
			behaviorData._behaviorFile = $"behaviors:\\Skylanders\\{kTemplateName}\\{kTemplateName}.hkp";
			behaviorData._behaviorEventsFile = $"behavior_events:\\Skylanders\\{kTemplateName}_combat.igz";
			// Directly modify keys and fix the hashtable
			for (int k = 0; k < behaviorData._handlers._keys.Length; k++)
			{
				if (behaviorData._handlers._keys[k] != behaviorData._handlers.KeyTraitsInvalid())
				{
					behaviorData._handlers._keys[k] = behaviorData._handlers._keys[k].ReplaceBeginning(kTemplateName, _name);
				}
			}
			behaviorData._handlers.SetCapacity(behaviorData._handlers._hashItemCount);
			// This one's defined in a vvl so we need to get ugly to access it
			igMetaObject? portalMasterPerkLogicMeta = igArkCore.GetObjectMeta("Scripts.Graph.PortalMasterPerkLogicSkylandersData");
			Debug.Assert(portalMasterPerkLogicMeta != null);
			igComponentData? portalMasterPerkLogic = actorData.GetComponentData(portalMasterPerkLogicMeta);
			Debug.Assert(portalMasterPerkLogic != null);
			FixHandle(portalMasterPerkLogic, "CrisisTrainingAttributeList");
			FixHandle(portalMasterPerkLogic, "EliteTrainingAttributeList");
			FixHandle(portalMasterPerkLogic, "SkylanderCommanderAttributeList");

			MemoryStream characterDataStream = new MemoryStream();
			characterData.WriteFile(characterDataStream, igRegistry.GetRegistry()._platform);
			igArchive.FileInfo characterDataFile = archive.GetAddFile(characterData._path);
			archive.Compress(characterDataFile, characterDataStream);
			archive.Save($"{_name}.pak");
		}


		/// <summary>
		/// Creates an empty actor skin for the user to modify later
		/// </summary>
		/// <returns>the newly created igObjectdirectory</returns>
		private igObjectDirectory CreateActorSkinDir()
		{
			igObjectDirectory dir = new igObjectDirectory($"actors/{_name}.igz");
			dir._nameList = new igNameList();
			dir._useNameList = true;
			dir._type = igObjectDirectory.FileType.kIGZ;
			igStringRefList pkg = new igStringRefList();

			CGraphicsSkinInfo skinInfo = igMetaObject.ConstructInstance<CGraphicsSkinInfo>(igMemoryContext.Default);

			igMetaObject materialHandleTableInfoMeta = igArkCore.GetObjectMeta("CMaterialHandleTableInfo")!;
			igObject materialHandleTableInfo = materialHandleTableInfoMeta.ConstructInstance(igMemoryContext.Default);
			igStringInsensitiveStringHashTable hashTable = igMetaObject.ConstructInstance<igStringInsensitiveStringHashTable>();
			materialHandleTableInfoMeta.GetFieldByName("_handleTable")!._fieldHandle!.SetValue(materialHandleTableInfo, hashTable);

			dir.AddObject(skinInfo, default, new igName("CGraphicsSkinInfo"));
			dir.AddObject(materialHandleTableInfo, default, new igName("CMaterialHandleTableInfo"));

			return dir;
		}


		/// <summary>
		/// Fixes handles where the namespace is pointing to an old namespace
		/// </summary>
		/// <param name="component">The component to target</param>
		/// <param name="fieldName">The field name to target</param>
		private void FixHandle(igComponentData component, string fieldName)
		{
			igMetaObject metaobject = component.GetMeta();
			igMetaField? metafield = metaobject.GetFieldByName(fieldName);
			Debug.Assert(metafield != null && metafield._fieldHandle != null);
			igHandle? handle = metafield._fieldHandle.GetValue(component) as igHandle;
			if (handle != null)
			{
				string newNamespace = handle._namespace._string.ReplaceBeginning(kTemplateName, _name);
				handle = igObjectHandleManager.Singleton.LookupHandle(new igName(newNamespace), handle._alias);
				metafield._fieldHandle.SetValue(component, handle);
			}
		}


		/// <summary>
		/// Copies files from the existing files archive to the destination archive, and renames them
		/// </summary>
		/// <param name="archive">The new archive</param>
		/// <param name="pkg">The pkg to add them to</param>
		/// <param name="type">The pkg type</param>
		/// <param name="fmt">The format string for the old and new file name</param>
		private void CopyFileToArchive(igArchive archive, igStringRefList pkg, string type, string fmt)
		{
			string src = string.Format(fmt, kTemplateName);
			string dst = string.Format(fmt, _name);

			igArchive.FileInfo file = archive.GetAddFile(dst);

			igFileContext.Singleton.Open(src, igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out igFileDescriptor fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
			archive.Compress(file, fd._handle);

			pkg.Add(type);
			pkg.Add(dst);
		}
	}
}