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
using igLibrary.Math;
using igLibrary.Tfb;
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
            List<tfbBindings> fileContents = igArkCore.tfbBindings.ToList();
            fs.Write(Encoding.UTF8.GetBytes(fileContents.ToString()));
            fs.Close();
        }
    }
}