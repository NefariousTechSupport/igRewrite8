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
using Kovah;
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

			// Copy files to output archive, havok data will be fixed up later

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
			actorData._characterScript = $"scripts/{_name}";
			CBehaviorComponentData? behaviorData = actorData.GetComponentData<CBehaviorComponentData>();
			Debug.Assert(behaviorData != null);
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

			// Setup havok stuff, these are already added to the pkg, we just need
			// to edit the data itself.

			hkRootLevelContainer container = ReadHavokFile(behaviorData._behaviorFile);

			for (uint v = 0; v < container.GetNumVariants(); v++)
			{
				hkRootLevelContainerNamedVariant variant = container.GetNamedVariant(v);
				if (variant.Variant is hkbProjectData havokProject)
				{
					hkbProjectStringData? stringData = havokProject.StringData;
					if (stringData != null)
					{
						RenameHavokPaths(stringData.AnimationFilenames);

						RenameHavokBehaviours(stringData.BehaviorFilenames, archive);
						RenameHavokCharacters(stringData.CharacterFilenames, archive);
						RenameHavokPaths(stringData.BehaviorFilenames);
						RenameHavokPaths(stringData.CharacterFilenames);

						stringData.FullPathToSource = RenameHavokPath(stringData.FullPathToSource);
					}
				}
			}

			behaviorData._behaviorFile       = $"behaviors:/Skylanders/{_name}/{_name}.hkp";
			behaviorData._behaviorEventsFile = $"behavior_events:/Skylanders/{_name}_combat.igz";
			WriteHavokFile(behaviorData._behaviorFile, archive, container);

			// Havok animation

			container = ReadHavokFile($"anims/{actorData._characterAnimations}.hka");

			for (uint v = 0; v < container.GetNumVariants(); v++)
			{
				hkRootLevelContainerNamedVariant variant = container.GetNamedVariant(v);
				if (variant.Variant is hkaAnimationContainer animdb)
				{
					hkaAnimationBinding?[]? bindings = animdb.Bindings;
					if (bindings != null)
					{
						RenameHavokAnimBindings(animdb.Bindings);
					}
				}
			}

			actorData._characterAnimations = $"Skylanders/{_name}";
			WriteHavokFile($"anims/{actorData._characterAnimations}.hka", archive, container);

			// Write alchemy characterdata out

			MemoryStream characterDataStream = new MemoryStream();
			characterData.WriteFile(characterDataStream, igRegistry.GetRegistry()._platform);
			igArchive.FileInfo characterDataFile = archive.GetAddFile(characterData._path);
			archive.Compress(characterDataFile, characterDataStream);

			// Finally, save the archive

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


		/// <summary>
		/// Rename an array of strings containing filepaths
		/// </summary>
		/// <param name="paths">The array of strings</param>
		private void RenameHavokPaths(string?[]? paths)
		{
			if (paths == null)
			{
				return;
			}

			for (uint p = 0; p < paths.Length; p++)
			{
				RenameHavokPath(ref paths[p]);
			}
		}


		/// <summary>
		/// Rename a strings containing a filepath
		/// </summary>
		/// <param name="path">The file path</param>
		private void RenameHavokPath(ref string? path)
		{
			path = RenameHavokPath(path);
		}


		/// <summary>
		/// Rename a strings containing a filepath
		/// </summary>
		/// <param name="path">The file path</param>
		/// <returns>The renamed string, or null if path is null</returns>
		private string? RenameHavokPath(string? path)
		{
			if (path == null)
			{
				return null;
			}

			// Risk of this being a clbuttic mistake but let's hope not
			return path.Replace(kTemplateName, _name, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Renames the references within the havok behaviour packfiles at the given paths
		/// </summary>
		/// <param name="paths">The paths</param>
		/// <param name="archive">The archive to save the modified packfiles to</param>
		private void RenameHavokBehaviours(string?[]? paths, igArchive archive)
		{
			if (paths == null)
			{
				return;
			}

			for (uint p = 0; p < paths.Length; p++)
			{
				RenameHavokBehaviour(paths[p], archive);
			}
		}


		/// <summary>
		/// Renames the references within a havok behaviour packfiles at the given path
		/// </summary>
		/// <param name="path">the path of the packfile</param>
		/// <param name="archive">The archive to save the modified packfile to</param>
		private void RenameHavokBehaviour(string? path, igArchive archive)
		{
			if (path == null)
			{
				return;
			}

			path = path.Replace('\\', '/');

			string alchemyPath = $"behaviors:/Skylanders/{kTemplateName}/{path}";
			hkRootLevelContainer container = ReadHavokFile(alchemyPath);

			for (uint v = 0; v < container.GetNumVariants(); v++)
			{
				hkRootLevelContainerNamedVariant variant = container.GetNamedVariant(v);
				if (variant.Variant is hkbBehaviorGraph graph)
				{
					graph.Name = $"{_name}_behavior.hkb";

					if (graph.RootGenerator is hkbStateMachine stateMachine)
					{
						RenameHavokStateMachine(stateMachine);
					}
				}
			}

			WriteHavokFile(alchemyPath.Replace(kTemplateName, _name), archive, container);
		}


		/// <summary>
		/// Rename references in a hkbStateMachine
		/// </summary>
		/// <param name="stateMachine">the hkbStateMachine</param>
		private void RenameHavokStateMachine(hkbStateMachine? stateMachine)
		{
			if (stateMachine == null || stateMachine.States == null)
			{
				return;
			}

			for (int s = 0; s < stateMachine.States.Length; s++)
			{
				hkbStateMachineStateInfo? state = stateMachine.States[s];
				if (state != null)
				{
					RenameHavokGenerator(state.Generator);
				}
			}
		}


		/// <summary>
		/// Rename references in a hkbGenerator
		/// </summary>
		/// <param name="generator">the hkbGenerator</param>
		private void RenameHavokGenerator(hkbGenerator? generator)
		{
			if (generator == null)
			{
				return;
			}

			if (generator is hkbStateMachine stateMachine)
			{
				RenameHavokStateMachine(stateMachine);
			}
			else if (generator is hkbModifierGenerator modifierGenerator)
			{
				RenameHavokGenerator(modifierGenerator.Generator);
			}
			else if (generator is hkbClipGenerator clipGenerator)
			{
				clipGenerator.AnimationName = RenameHavokPath(clipGenerator.AnimationName);
			}
			else if (generator is hkbLayerGenerator layerGenerator)
			{
				if (layerGenerator.Layers != null)
				{
					for (int l = 0; l < layerGenerator.Layers.Length; l++)
					{
						hkbLayer? layer = layerGenerator.Layers[l];
						if (layer != null)
						{
							RenameHavokGenerator(layer.Generator);
						}
					}
				}
			}
		}


		/// <summary>
		/// Renames the references within the havok character packfiles at the given paths
		/// </summary>
		/// <param name="paths">The paths</param>
		/// <param name="archive">The archive to save the modified packfiles to</param>
		private void RenameHavokCharacters(string?[]? paths, igArchive archive)
		{
			if (paths == null)
			{
				return;
			}

			for (uint p = 0; p < paths.Length; p++)
			{
				RenameHavokCharacter(paths[p], archive);
			}
		}


		/// <summary>
		/// Renames the references within a havok character packfile at the given path
		/// </summary>
		/// <param name="path">The path</param>
		/// <param name="archive">The archive to save the modified packfiles to</param>
		private void RenameHavokCharacter(string? path, igArchive archive)
		{
			if (path == null)
			{
				return;
			}

			path = path.Replace('\\', '/');

			string alchemyPath = $"behaviors:/Skylanders/{kTemplateName}/{path}";
			hkRootLevelContainer container = ReadHavokFile(alchemyPath);

			for (uint v = 0; v < container.GetNumVariants(); v++)
			{
				hkRootLevelContainerNamedVariant variant = container.GetNamedVariant(v);
				if (variant.Variant is hkbCharacterData character)
				{
					hkbCharacterStringData? stringData = character.StringData;
					if (stringData != null)
					{
						stringData.BehaviorFilename = RenameHavokPath(stringData.BehaviorFilename);
						stringData.Name             = RenameHavokPath(stringData.Name);
						stringData.RigName          = RenameHavokPath(stringData.RigName);

						RenameHavokAssetBundeStringData(stringData.AnimationBundleFilenameData);
						RenameHavokAssetBundeStringData(stringData.AnimationBundleNameData);

						RenameHavokFileNameMeshNamePairs(stringData.SkinNames);
					}
				}
			}

			WriteHavokFile(alchemyPath.Replace(kTemplateName, _name), archive, container);
		}


		/// <summary>
		/// Renames the references within an array of hkbAssetBundleStringData objects
		/// </summary>
		/// <param name="stringDatas">The hkbAssetBundleStringDatas</param>
		private void RenameHavokAssetBundeStringData(hkbAssetBundleStringData?[]? stringDatas)
		{
			if (stringDatas == null)
			{
				return;
			}

			for (int i = 0; i < stringDatas.Length; i++)
			{
				hkbAssetBundleStringData? stringData = stringDatas[i];
				if (stringData == null)
				{
					continue;
				}

				RenameHavokPaths(stringData.AssetNames);
				RenameHavokPath(stringData.BundleName);
			}
		}


		/// <summary>
		/// Renames the references within an array of hkbCharacterStringDataFileNameMeshNamePair objects
		/// </summary>
		/// <param name="pairs">The hkbCharacterStringDataFileNameMeshNamePairs</param>
		private void RenameHavokFileNameMeshNamePairs(hkbCharacterStringDataFileNameMeshNamePair?[]? pairs)
		{
			if (pairs == null)
			{
				return;
			}

			for (int i = 0; i < pairs.Length; i++)
			{
				hkbCharacterStringDataFileNameMeshNamePair? pair = pairs[i];
				if (pair == null)
				{
					continue;
				}

				pair.FileName = RenameHavokPath(pair.FileName);
				pair.MeshName = RenameHavokPath(pair.MeshName);
			}
		}


		/// <summary>
		/// Renames the references within an array of hkaAnimationBinding objects
		/// </summary>
		/// <param name="bindings">The hkaAnimationBindings</param>
		private void RenameHavokAnimBindings(hkaAnimationBinding?[]? bindings)
		{
			if (bindings == null)
			{
				return;
			}

			for (uint b = 0; b < bindings.Length; b++)
			{
				hkaAnimationBinding? binding = bindings[b];
				if (binding == null)
				{
					continue;
				}

				binding.OriginalSkeletonName = RenameHavokPath(binding.OriginalSkeletonName);
			}
		}


		/// <summary>
		/// Reads a havok packfile
		/// </summary>
		/// <param name="path">The alchemy path of the packfile</param>
		/// <returns>The hkRootLevelContainer</returns>
		private hkRootLevelContainer ReadHavokFile(string path)
		{
			igFile behaviourFile = new igFile();
			behaviourFile.Open(path);
			bool success = HavokPackfileReader.Load(behaviourFile._file._handle, out hkRootLevelContainer? container);
			Debug.Assert(success);
			Debug.Assert(container != null);
			return container;
		}


		/// <summary>
		/// Writes a havok packfile
		/// </summary>
		/// <param name="path">The alchemy path to write to</param>
		/// <param name="archive">The archive to save to</param>
		/// <param name="container">The hkRootLevelContainer</param>
		private void WriteHavokFile(string path, igArchive archive, hkRootLevelContainer container)
		{
			igFilePath filePath = new igFilePath();
			filePath.Set(path);
			path = filePath._nativePath;

			igArchive.FileInfo fileInfo = archive.GetAddFile(path);
			MemoryStream output = new MemoryStream();
			GetHavokSaveInfo(out uint containerVersion, out EVersion metadataVersion, out bool is64bit, out Sapphire.StreamHelper.Endianness endianness, out bool reusePadding, out bool emptyBaseClassOptimisation);
			HavokPackfileWriter.Save(output, containerVersion, metadataVersion, is64bit, endianness, reusePadding, emptyBaseClassOptimisation, 0, container);
			output.Seek(0, SeekOrigin.Begin);
			FileStream debug = File.OpenWrite(Path.GetFileName(path));
			output.WriteTo(debug);
			debug.Close();

			archive.Compress(fileInfo, output);
		}


		/// <summary>
		/// Helper function to get version information for the current game/platform
		/// </summary>
		/// <param name="containerVersion">The container version</param>
		/// <param name="metadataVersion">The metadata version</param>
		/// <param name="is64bit">Whether the platform is 64-bit</param>
		/// <param name="endianness">The endianness of the platform</param>
		/// <param name="reusePadding">Whether to reuse padding of a base class</param>
		/// <param name="emptyBaseClassOptimisation">Whether to whatever this is</param>
		/// <exception cref="NotImplementedException">Thrown if a game is not implemented</exception>
		private static void GetHavokSaveInfo(out uint containerVersion, out EVersion metadataVersion, out bool is64bit, out Sapphire.StreamHelper.Endianness endianness, out bool reusePadding, out bool emptyBaseClassOptimisation)
		{
			IG_CORE_PLATFORM platform = igRegistry.GetRegistry()._platform;
			is64bit = igAlchemyCore.isPlatform64Bit(platform);
			endianness = igAlchemyCore.isPlatformBigEndian(platform) ? Sapphire.StreamHelper.Endianness.Big : Sapphire.StreamHelper.Endianness.Little;
			// MSVC compiler
			reusePadding = platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32
			            || platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64
			            || platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO;
			// Unsure
			emptyBaseClassOptimisation = false;

			switch (igArkCore.Game)
			{
				case igArkCore.EGame.EV_SkylandersSuperchargers:
				case igArkCore.EGame.EV_SkylandersImaginators:
					containerVersion = 0x0B;
					metadataVersion = EVersion.hk_2014_1_0_r1;
					break;
				default:
					throw new NotImplementedException($"Havok support for version {igArkCore.Game} unimplemented");
			}
		}
	}
}