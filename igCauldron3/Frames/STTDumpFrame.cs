/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.Serialization;
using System.Text;
using igLibrary;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
    /// <summary>
    /// UI Frame for dumping open level.bld
    /// </summary>
    public class STTDumpFrame : Frame
    {
        protected string? _errorMsg = null;
        private readonly string _title;
        private readonly string _action;

        /// <summary>
        /// Constructor for the frame
        /// </summary>
        /// <param name="wnd">Reference to the main window object</param>
		/// <param name="title">title of the window</param>
		/// <param name="action">string of the action button</param>
        public STTDumpFrame(Window wnd, string title, string action) : base(wnd)
        {
            _title = title;
            _action = action;
        }

        public override void Render()
        {
            ImGui.Begin(_title, ImGuiWindowFlags.NoDocking);
            if (_errorMsg != null)
            {
                ImGui.TextColored(Styles._errorTxt, _errorMsg);
            }
            if (ImGui.Button(_action))
            {
                OnActionStart();
            }
            if (ImGui.Button("Close")) Close();
            ImGui.End();
        }

        public void OnActionStart()
        {
            string DumpsDirPath = Path.Combine(CauldronConfig.ConfigFolder, "Dumps");
            Directory.CreateDirectory(DumpsDirPath);
            var x = Directory.GetFiles(DumpsDirPath);
            string fileName = string.Format("Dump_{0}.txt", x.Length);
            FileStream fs = File.Create(Path.Combine(DumpsDirPath, fileName));
            List<igMetaObject> fileContents = igArkCore.MetaObjects.ToList();
            for (int i = 0; i < fileContents.Count() - 1; i++)
            {
                string cont = null;
                if (fileContents[i]._parent == null)
                {
                    cont = "PARENT_NULL";
                }
                else if (fileContents[i]._metaFields != null && fileContents[i]._metaFields.Count > 0)
                {
                    cont = string.Format("ORIGINAL : NEW === {0} : {1}", fileContents[i]._name, fileContents[i]._metaFields);
                    for (int k = 0; k < fileContents[i]._metaFields.Count; k++)
                    {
                        cont += $"\n ::: [metafield #{k}] {fileContents[i]._metaFields[k]._fieldName} --> {fileContents[i]._metaFields[k]._offset}";
                    }
                }
                else
                {
                    cont = "METAFIELDS_NULL";
                }
                fs.Write(Encoding.UTF8.GetBytes(cont));
                fs.WriteByte(0x0A);
            }
            fs.Close();
        }
	}
}