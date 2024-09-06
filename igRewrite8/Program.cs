using igLibrary.Core;
using igLibrary;
using igRewrite8.Devel;

using System;
using System.Reflection;
using System.Reflection.Emit;
using igLibrary.AssetConversion.Models;
using igLibrary.Render;

using Assimp;

namespace igRewrite8
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			//DumpToyDataToRunes(args); return;
			//DumpAllArchives(args); return;
			//ReadFromTextFile(args); return;
			//TestArchives(args); return;

			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			igAlchemyCore.InitializeSystems();

			igMemoryContext.Singleton.GetHashCode();

			igFileContext.Singleton.Initialize(args[0]);
			igFileContext.Singleton.InitializeUpdate(args[1]);
			igRegistry.GetRegistry()._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3;
			igRegistry.GetRegistry()._gfxPlatform = igLibrary.Gfx.IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3;

			AssimpContext ctx = new AssimpContext();
			Scene scene = ctx.ImportFile("C:/Users/neffy/Documents/MikuSupercharger.obj", PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.CalculateTangentSpace);

			SuperChargersModel sscmodel = new SuperChargersModel();
			igModelInfo modelInfo = sscmodel.ImportModel(scene);

			igMetaObject materialHandleTableInfoMeta = igArkCore.GetObjectMeta("CMaterialHandleTableInfo")!;
			igObject materialHandleTableInfo = materialHandleTableInfoMeta.ConstructInstance(igMemoryContext.Default);
			igStringInsensitiveStringHashTable hashTable = igMetaObject.ConstructInstance<igStringInsensitiveStringHashTable>();
			materialHandleTableInfoMeta.GetFieldByName("_handleTable")!._fieldHandle!.SetValue(materialHandleTableInfo, hashTable);

			igObjectDirectory dir = new igObjectDirectory("models/jasleencube.igz");
			dir._useNameList = true;
			dir._nameList = igMetaObject.ConstructInstance<igNameList>();
			dir._type = igObjectDirectory.FileType.kIGZ;
			dir.AddObject(modelInfo, default, new igName("igSceneInfo0"));
			dir.AddObject(materialHandleTableInfo, default, new igName("CMaterialHandleTableInfo"));
			FileStream dst = File.Create("modeltest.igz");
			dir.WriteFile(dst, IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3);
			dst.Seek(0, SeekOrigin.Begin);

			igFilePath fp = new igFilePath();
			fp.Set(dir._path);
			igArchive arc = igFileContext.Singleton._archiveManager._patchArchives[0];
			arc.GetAddFile(fp._path);
			arc.Compress(fp._path, dst);
			dst.Close();
			if(arc._path[1] == ':') arc.Save(arc._path);
			else arc.Save($"{igFileContext.Singleton._root}/archives/{Path.GetFileName(arc._path)}");

			dst.Close();

			return;
		}
		private static void DumpToyDataToRunes(string[] args)
		{
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			igFileContext.Singleton.Initialize(args[0]);

			igFileContext.Singleton.LoadArchive("data:/archives/permanent.pak");

			//igFileContext.Singleton.Open("packages:/generated/packageXmls/permanent_pkg.igz", igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out igFileDescriptor fd, igBlockingType.kBlocking, igFileWorkItem.Priority.kPriorityNormal);

			igObjectDirectory dir = igObjectStreamManager.Singleton.Load("packages:/generated/packageXmls/permanent_pkg.igz");

			List<igObject> toydata = new List<igObject>();

			igMetaObject toydataType = igArkCore.GetObjectMeta("CToyData");
			igMetaObject vidListType = igArkCore.GetObjectMeta("CVariantIdentifierList");
			igMetaObject vidType = igArkCore.GetObjectMeta("CVariantIdentifier");

			igStringRefList list = (igStringRefList)dir._objectList[0];

			StreamWriter f = File.CreateText("runes.csv");
			for(int i = 0; i < list._count; i += 2)
			{
				if(list[i] == "igx_file")
				{
					if(list[i+1].Contains("toydata"))
					{
						igObjectDirectory toydataDir = igObjectStreamManager.Singleton.Load(list[i+1]);
						for(int j = 0; j < toydataDir._objectList._count; j++)
						{
							if(toydataDir._objectList[j].GetMeta().CanBeAssignedTo(toydataType))
							{
								toydata.Add(toydataDir._objectList[j]);
							}
						}

						string langFilePath = list[i+1].Remove(list[i+1].Length - 4) + "_en.lng";

						try
						{
							igObjectDirectory toydataLangDir = igObjectStreamManager.Singleton.Load(langFilePath);
							for(int j = 0; j < toydataLangDir._objectList._count; j++)
							{
								if(toydataLangDir._objectList[j] is igLocalizedStringDataList localData)
								{
									for(int l = 0; l < localData._count; l++)
									{
										igObject target = localData[l]._object.GetObjectAlias<igObject>();
										Type? t = target.GetType();
										FieldInfo? stringField = localData[l]._field._fieldHandle;
										stringField.SetValue(target, localData[l]._string);
									}
								}
							}
						}
						catch(IOException){}
					}
				}
			}

			FieldInfo? ttField = toydataType._vTablePointer.GetField("_toyId");
			FieldInfo? tnField = toydataType._vTablePointer.GetField("_toyName");
			FieldInfo? varField = toydataType._vTablePointer.GetField("_variants");

			FieldInfo? decoField = vidType._vTablePointer.GetField("_decoId");
			FieldInfo? ycField = vidType._vTablePointer.GetField("_yearCode");
			FieldInfo? lcField = vidType._vTablePointer.GetField("_lightCoreFlag");
			FieldInfo? fadField = vidType._vTablePointer.GetField("_fullAltDecoFlag");
			FieldInfo? wowField = vidType._vTablePointer.GetField("_wowPowFlag");
			FieldInfo? varTxtField = vidType._vTablePointer.GetField("_variantText");
			FieldInfo? varTnField = vidType._vTablePointer.GetField("_toyName");

			for(int i = 0; i < toydata.Count; i++)
			{
				IigDataList? variants = (IigDataList)varField.GetValue(toydata[i]);
				f.WriteLine($"{(int)ttField.GetValue(toydata[i])},{(variants == null ? 0 : variants.GetCount())},{tnField.GetValue(toydata[i])}");
				for(int j = 0; j < (variants == null ? 0 : variants.GetCount()); j++)
				{
					object variant = variants.GetObject(j);
					f.WriteLine($"{(int)decoField.GetValue(variant)},{(int)ycField.GetValue(variant)},{(bool)lcField.GetValue(variant)},{(bool)fadField.GetValue(variant)},{(bool)wowField.GetValue(variant)},{varTxtField.GetValue(variant)},{varTnField.GetValue(variant)}");
				}
			}
			f.Close();
			return;
		}
		private static void DumpAllArchives(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			DirectoryInfo di = new DirectoryInfo(args[0]);
			FileInfo[] archiveFilePaths = di.GetFiles("*.iga*");
			for(int i = 0; i < archiveFilePaths.Length; i++)
			{
				string fileName = Path.GetFileName(archiveFilePaths[i].FullName);
				igArchive arc = igFileContext.Singleton.LoadArchive(fileName);
				for(int j = 0; j < arc._files.Count; j++)
				{
					string outputPath = Path.Combine(args[1], arc._files[j]._logicalName);
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
					FileStream fs = File.Create(outputPath);
					arc.Decompress(arc._files[j], fs);
					fs.Flush();
					fs.Close();
				}
			}
			return;
		}
		private static void ReadFromTextFile(string[] args)
		{
			TextParser parser = new TextParser();
			parser.ReadMetaEnumFile(args[0]);
			parser.ReadMetaFieldFile(args[1]);
			parser.ReadMetaObjectFile(args[2]);	//regular one
			//parser.ReadMetaObjectFile(args[3]);	//extra one
			
			igArkCore.WriteToFile(igArkCore.EGame.EV_SkylandersSuperchargers);
			
			igArkCore.Reset();
		}
		private static void TestArchives(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			igArchive archive = new igArchive();
			archive.Open("archives/permanent.backup.pak", igBlockingType.kMayBlock);
			for(int i = 0; i < archive._files.Count; i++)
			{
				MemoryStream uwu = new MemoryStream();
				archive.Decompress(archive._files[0], uwu);
				archive.Compress(archive._files[0], uwu);
				uwu.Close();
			}
			archive.Save("test.pak");
			return;
		}
	}
}