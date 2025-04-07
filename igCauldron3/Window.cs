/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using igLibrary.Core;
using igCauldron3.Graphics;
using igLibrary.Graphics;
using System.ComponentModel;

namespace igCauldron3
{
	/// <summary>
	/// The window class
	/// </summary>
	public class Window : GameWindow
	{
		public static Window _instance { get; private set; }
		ImGuiController controller;
		public List<Frame> _frames = new List<Frame>();
		string[] args;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="gws">The game window settings</param>
		/// <param name="nws">The native window settings</param>
		/// <param name="args">The program arguments</param>
		public Window(GameWindowSettings gws, NativeWindowSettings nws, string[] args) : base(gws, nws)
		{
			_instance = this;
			this.args = args;

			// Do library initialisation
			igAlchemyCore.InitializeSystems();
			FieldRenderer.Init();
		}


		/// <summary>
		/// On loading the window
		/// </summary>
		protected override void OnLoad()
		{
			base.OnLoad();

			Title = "igCauldron";

			controller = new ImGuiController(ClientSize.X, ClientSize.Y);
			igTContext<igBaseGraphicsDevice>._instance = new igOpenGLGraphicsDevice();

			_frames.Add(new ConfigFrame(this));
		}


		/// <summary>
		/// On resizing the window
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);

			GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
			controller.WindowResized(ClientSize.X, ClientSize.Y);
		}


		/// <summary>
		/// Per frame update
		/// </summary>
		/// <param name="e">event arguments</param>
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if(!IsFocused)
			{
				return;
			}

			if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
			{
				//Close();
			}

			if(ImGui.GetIO().WantSaveIniSettings)
			{
				ImGui.SaveIniSettingsToDisk(CauldronConfig.ImGuiConfigFilePath);
			}


			base.OnUpdateFrame(e);
		}


		/// <summary>
		/// On mouse wheel scroll
		/// </summary>
		/// <param name="e">event arguments</param>
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			controller.MouseScroll(e.Offset);
		}


		/// <summary>
		/// on text input
		/// </summary>
		/// <param name="e">event arguments</param>
		protected override void OnTextInput(TextInputEventArgs e)
		{
			base.OnTextInput(e);
			controller.PressChar((char)e.Unicode);
		}


		/// <summary>
		/// per frame render
		/// </summary>
		/// <param name="e">event arguments</param>
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if(!IsFocused)
			{
				return;
			}

			base.OnRenderFrame(e);

			controller.Update(this, (float)e.Time);

			for(int i = 0; i < _frames.Count; i++)
			{
				_frames[i].Render();
			}

			controller.Render();

			//Title = e.Time.ToString();
			SwapBuffers();
		}


		protected override void OnClosing(CancelEventArgs e)
		{
			controller.Dispose();

			base.OnClosing(e);
		}


		/// <summary>
		/// Parse command line arguments
		/// </summary>
		private void ParseArgs()
		{
			igRegistry registry = igRegistry.GetRegistry();
			
			for(int i = 0; i < args.Length;)
			{
				switch(args[i].ToLower())
				{
					case "-p":
						i++;
						switch(args[i].ToLower())
						{
							case "ps3":
							case "playstation3":
								registry._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3;
								break;
							case "aspen":
							case "aspen32":
							case "aspenlow":
							case "ios32":
							case "iosold":
							case "ioslow":
								registry._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN;
								break;
							case "aspen64":
							case "aspenhigh":
							case "ios64":
							case "iosnew":
							case "ioshigh":
								registry._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64;
								break;
						}
						i++;
						break;
					case "-c":
						i++;
						igFileContext.Singleton.Initialize(args[i]);
						i++;
						break;
					case "-u":
						i++;
						igFileContext.Singleton.InitializeUpdate(args[i]);
						i++;
						break;
					case "-a":
						i++;
						igArchive arc = igFileContext.Singleton.LoadArchive(args[i]);
						i++;
						break;
					case "-f":
						i++;
						DirectoryManagerFrame._instance.AddDirectory(igObjectStreamManager.Singleton.Load(args[i]));
						i++;
						break;
				}
			}
		}
	}
}