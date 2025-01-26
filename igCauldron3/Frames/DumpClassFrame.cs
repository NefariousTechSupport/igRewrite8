/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI frame for dumping class metadata as c# source files
	/// </summary>
	public class DumpClassFrame : Frame
	{
		private igMetaObject _meta = null;
		private List<igMetaObject> _alphabeticalMetas;
		private HashSet<igBaseMeta> _dumpedMetas = new HashSet<igBaseMeta>();


		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="wnd">The window to parent to</param>
		public DumpClassFrame(Window wnd) : base(wnd)
		{
			_alphabeticalMetas = igArkCore.MetaObjects.Where(x => x is not igDynamicMetaObject).OrderBy(x => x._name).ToList();
		}


		/// <summary>
		/// Renders the UI
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("Dump Class", ImGuiWindowFlags.NoDocking);

			ImGui.Text("Type");
			ImGui.SameLine();
			ImGui.PushID("Type");
			bool metaErrored = _meta == null;
			if(metaErrored) ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
			bool comboing = ImGui.BeginCombo(string.Empty, _meta == null ? "Select a type" : _meta._name);
			if(metaErrored) ImGui.PopStyleColor();
			if(comboing)
			{
				for(int i = 0; i < _alphabeticalMetas.Count; i++)
				{
					ImGui.PushID(i);
					if(ImGui.Selectable(_alphabeticalMetas[i]._name, _meta == _alphabeticalMetas[i]))
					{
						_meta = _alphabeticalMetas[i];
					}
					if(_meta == _alphabeticalMetas[i])
					{
						ImGui.SetItemDefaultFocus();
					}
					ImGui.PopID();
				}
				ImGui.EndCombo();
			}
			ImGui.PopID();

			bool pressed = ImGui.Button("Dump");
			if(pressed && !metaErrored)
			{
				DumpClass(_meta);
				Close();
			}
			if(ImGui.Button("Close")) Close();
			ImGui.End();
		}


		/// <summary>
		/// Dumps a class
		/// </summary>
		/// <param name="meta">The class to dump</param>
		private void DumpClass(igMetaObject meta)
		{
			if(meta is igDynamicMetaObject) return;
			if(!igArkCore.IsDynamicType(meta)) return;
			if(_dumpedMetas.Contains(meta)) return;

			_dumpedMetas.Add(meta);

			DumpClass(meta._parent!);

			meta.GatherDependancies();
			igArkCore.FlushPendingTypes();

			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				CheckShouldDump(meta._metaFields[i]);
			}

			StringBuilder output = new StringBuilder();
			      output.Append("namespace igLibrary\n");
			      output.Append("{\n");
			output.AppendFormat("\tpublic class {0} : {1}\n", meta._name, DemangleTypeName(meta._vTablePointer!.BaseType!));
			      output.Append("\t{\n");

			for(int i = meta._parent._metaFields.Count; i < meta._metaFields.Count; i++)
			{
				output.AppendFormat("\t\tpublic {0} {1};\n", DemangleTypeName(meta._metaFields[i].GetOutputType()), meta._metaFields[i]._fieldName);
			}

			      output.Append("\t}\n");
			      output.Append("}");

			File.WriteAllText($"{meta._name}.cs", output.ToString());
		}


		/// <summary>
		/// Dumps a struct
		/// </summary>
		/// <param name="compoundFieldInfo">The compound field to dump</param>
		private void DumpStruct(igCompoundMetaFieldInfo compoundFieldInfo)
		{
			if(_dumpedMetas.Contains(compoundFieldInfo)) return;

			_dumpedMetas.Add(compoundFieldInfo);

			for(int i = 0; i < compoundFieldInfo._fieldList.Count; i++)
			{
				CheckShouldDump(compoundFieldInfo._fieldList[i]);
			}

			StringBuilder output = new StringBuilder();
			      output.Append("namespace igLibrary\n");
			      output.Append("{\n");
			      output.Append("\t[igStruct]\n");
			output.AppendFormat("\tpublic struct {0}\n", compoundFieldInfo._name);
			      output.Append("\t{\n");
			for(int i = 0; i < compoundFieldInfo._fieldList.Count; i++)
			{
				output.AppendFormat("\t\tpublic {0} {1};\n", compoundFieldInfo._fieldList[i].GetOutputType().Name, compoundFieldInfo._fieldList[i]._fieldName, compoundFieldInfo._fieldList);
			}
			      output.Append("\t}\n");
			      output.Append("}");

			File.WriteAllText($"{compoundFieldInfo._name}.cs", output.ToString());
		}


		/// <summary>
		/// Dumps an enum
		/// </summary>
		/// <param name="metaenum">The enum to dump</param>
		private void DumpEnum(igMetaEnum? metaenum)
		{
			// This is possible
			if(metaenum == null) return;
			if(_dumpedMetas.Contains(metaenum)) return;

			_dumpedMetas.Add(metaenum);

			StringBuilder output = new StringBuilder();
			      output.Append("namespace igLibrary\n");
			      output.Append("{\n");
			output.AppendFormat("\tpublic enum {0} : int\n", metaenum._name);
			      output.Append("\t{\n");
			for(int i = 0; i < metaenum._names.Count; i++)
			{
				output.AppendFormat("\t\t{0} = {1},\n", metaenum._names[i], metaenum._values[i]);
			}
			      output.Append("\t}\n");
			      output.Append("}");

			File.WriteAllText($"{metaenum._name}.cs", output.ToString());
		}


		/// <summary>
		/// Traverse the fields to find the next thing to dump
		/// </summary>
		/// <param name="field">The current field to traverse</param>
		private void CheckShouldDump(igMetaField field)
		{
			if(field is igObjectRefMetaField objField) DumpClass(objField._metaObject);
			else if(field is igMemoryRefMetaField memField) CheckShouldDump(memField._memType);
			else if(field is igMemoryRefHandleMetaField memHndField) CheckShouldDump(memHndField._memType);
			else if(field is igStaticMetaField staticField) CheckShouldDump(staticField._storageMetaField);
			else if(field is igCompoundMetaField compoundField) DumpStruct(compoundField._compoundFieldInfo);
			else if(field is igEnumMetaField enumMetaField) DumpEnum(enumMetaField._metaEnum);
			else if(field is igVectorMetaField vectorMetaField) CheckShouldDump(vectorMetaField.GetTemplateParameter(0));
			else if(field is igBitFieldMetaField bfMetaField) CheckShouldDump(bfMetaField._assignmentMetaField);
			else if(field is igPropertyFieldMetaField) return; // unsupported
			else if(field is igOrderedMapMetaField omField)
			{
				CheckShouldDump(omField._t);
				CheckShouldDump(omField._u);
			}
		}


		/// <summary>
		/// Demangles a dotnet type name
		/// </summary>
		/// <param name="t">The type</param>
		/// <returns>The demangled name</returns>
		public static string DemangleTypeName(Type t)
		{
			return DemangleTypeName(t.ToString());
		}


		/// <summary>
		/// Demangles a dotnet type name
		/// </summary>
		/// <param name="name">The mangled name</param>
		/// <returns>The demangled name</returns>
		public static string DemangleTypeName(string name)
		{
			string demangledName = string.Empty;
			for(int i = 0; i < name.Length; i++)
			{
				if(name[i] == '`')
				{
					string strGenNames = string.Empty;
					demangledName += '<';
					while(true)
					{
						if(++i == name.Length)
						{
							break;
						}
						char currentChar = name[i];
						if(currentChar == '[') break;
						strGenNames += currentChar;
					}
					int genNamesCount = int.Parse(strGenNames);
					string[] genNames = new string[genNamesCount];
					int genIndex = 0;
					while(true)
					{
						if(i == name.Length)
						{
							if(++genIndex == genNamesCount) break;
							demangledName += ',';
							continue;
						}
						if(genNames[genIndex] == null) genNames[genIndex] = string.Empty;
						char currentChar = name[++i];
						if(currentChar == ',' || currentChar == ']')
						{
							demangledName += DemangleTypeName(genNames[genIndex]);
							if(currentChar == ',')
							{
								demangledName += ',';
							}
							if(++genIndex == genNamesCount) break;
							continue;
						}
						genNames[genIndex] += currentChar;
					}
					demangledName += '>';
				}
				else
				{
					demangledName += name[i];
				}
			}
			return demangledName;
		}
	}
}