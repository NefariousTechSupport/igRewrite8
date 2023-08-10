using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using igLibrary.Core;

namespace igCauldron3
{
	public class Window : GameWindow
	{
		ImGuiController controller;
		public List<Frame> frames = new List<Frame>();
		string[] args;

		public Window(GameWindowSettings gws, NativeWindowSettings nws, string[] args) : base(gws, nws)
		{
			this.args = args;
			igAlchemyCore.InitializeSystems();
		}
		protected override void OnLoad()
		{
			base.OnLoad();

			Title = "igCauldron";

			controller = new ImGuiController(ClientSize.X, ClientSize.Y);

			frames.Add(new ConfigFrame(this));
		}
		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);

			GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
			controller.WindowResized(ClientSize.X, ClientSize.Y);
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
			{
				Close();
			}

			base.OnUpdateFrame(e);
		}
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			controller.MouseScroll(e.Offset);
		}
		protected override void OnTextInput(TextInputEventArgs e)
		{
			base.OnTextInput(e);
			controller.PressChar((char)e.Unicode);
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			controller.Update(this, (float)e.Time);

			for(int i = 0; i < frames.Count; i++)
			{
				frames[i].Render();
			}

			controller.Render();

			//Title = e.Time.ToString();
			SwapBuffers();
		}
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
						ObjectManagerFrame._dirs.Add(igObjectStreamManager.Singleton.Load(args[i]));
						i++;
						break;
				}
			}
		}
	}
}