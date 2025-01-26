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
	/// <summary>
	/// Renders the ImGui demo window
	/// </summary>
	public sealed class DemoWindowFrame : Frame
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">Window to parent it to</param>
		public DemoWindowFrame(Window wnd) : base(wnd)
		{
		}


		/// <summary>
		/// Renders the UI
		/// </summary>
		public override void Render()
		{
			ImGui.ShowDemoWindow();
		}
	}
}