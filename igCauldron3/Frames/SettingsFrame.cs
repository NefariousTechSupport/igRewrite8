/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using ImGuiNET;

namespace igCauldron3
{
	public class SettingsFrame : Frame
	{
		private static readonly (CauldronConfig.EFontType, string)[] sFontNames = new (CauldronConfig.EFontType, string)[]
		{
			(CauldronConfig.EFontType.kArial,         "Arial"),
			(CauldronConfig.EFontType.kVerdana,       "Verdana"),
			(CauldronConfig.EFontType.kConsolas,      "Consolas"),
			(CauldronConfig.EFontType.kOpenDyslexic,  "OpenDyslexic3"),
			(CauldronConfig.EFontType.kComicSans,     "Comic Sans")
		};

		public SettingsFrame(Window wnd) : base(wnd)
		{
		}

		public override void Render()
		{
			bool windowOpen = true;
			ImGui.Begin("Settings", ref windowOpen);

			if (!windowOpen)
			{
				Close();
			}

			bool fontChanged = false;

			fontChanged |= UIUtil.EnumComboBox("Font", sFontNames, ref CauldronConfig._config._preferences._fontName);
			fontChanged |= UIUtil.RenderFloatField("Font Scale", "fontScale", ref CauldronConfig._config._preferences._fontScale, 0.5f, 5);
			UIUtil.RenderUIntField("Line Spacing", "lineSpacing", ref CauldronConfig._config._preferences._lineSpacing, 1, 50);

			if (fontChanged)
			{
				CauldronConfig.ReloadFont();
			}

			ImGui.End();
		}
	}
}