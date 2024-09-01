namespace igLibrary.Core
{
	//Inherits from igContext
	public class igMemoryContext : igSingleton<igMemoryContext>
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static igMemoryPool Bootstrap { get; private set; }
        public static igMemoryPool System { get; private set; }
		public static igMemoryPool Static { get; private set; }
		public static igMemoryPool MetaData { get; private set; }
		public static igMemoryPool String { get; private set; }
		public static igMemoryPool Kernel { get; private set; }
		public static igMemoryPool SystemDebug { get; private set; }
		public static igMemoryPool Default { get; private set; }
		public static igMemoryPool Current { get; private set; }
		public static igMemoryPool Fast { get; private set; }
		public static igMemoryPool AGP { get; private set; }
		public static igMemoryPool VRAM { get; private set; }
		public static igMemoryPool Auxiliary { get; private set; }
		public static igMemoryPool VisualContext { get; private set; }
		public static igMemoryPool Graphics { get; private set; }
		public static igMemoryPool Actor { get; private set; }
		public static igMemoryPool AnimationData { get; private set; }
		public static igMemoryPool Geometry { get; private set; }
		public static igMemoryPool Vertex { get; private set; }
		public static igMemoryPool VertexEdge { get; private set; }
		public static igMemoryPool VertexObject { get; private set; }
		public static igMemoryPool Image { get; private set; }
		public static igMemoryPool ImageObject { get; private set; }
		public static igMemoryPool Attribute { get; private set; }
		public static igMemoryPool Node { get; private set; }
		public static igMemoryPool Audio { get; private set; }
		public static igMemoryPool AudioDsp { get; private set; }
		public static igMemoryPool AudioSample { get; private set; }
		public static igMemoryPool AudioSampleSecondary { get; private set; }
		public static igMemoryPool AudioStreamFile { get; private set; }
		public static igMemoryPool AudioStreamDecode { get; private set; }
		public static igMemoryPool Video { get; private set; }
		public static igMemoryPool DMA { get; private set; }
		public static igMemoryPool Shader { get; private set; }
		public static igMemoryPool ShaderBinary { get; private set; }
		public static igMemoryPool RenderList { get; private set; }
		public static igMemoryPool Texture { get; private set; }
		public static igMemoryPool DriverData { get; private set; }
		public static igMemoryPool Handles { get; private set; }
		public static igMemoryPool List { get; private set; }
		public static igMemoryPool Exporter { get; private set; }
		public static igMemoryPool Optimizer { get; private set; }
		public static igMemoryPool Network { get; private set; }
		public static igMemoryPool VRAMTopDown { get; private set; }
		public static igMemoryPool DotNet { get; private set; }
		public static igMemoryPool VramA { get; private set; }
		public static igMemoryPool VramStaging { get; private set; }
		public static igMemoryPool MEM1 { get; private set; }
		public static igMemoryPool MEM2 { get; private set; }
		public static igMemoryPool VRAMBottomUp { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

		public Dictionary<string, igMemoryPool> _pools { get; private set; }

		private static bool _initialized;


		public igMemoryContext()
		{
			if(_initialized) throw new InvalidOperationException("Cannot create multiple igMemoryContexts");

			_initialized = true;
			_pools = new Dictionary<string, igMemoryPool>();
			Bootstrap = new igMemoryPool("Bootstrap");                       _pools.Add("Bootstrap", Bootstrap);
			System = new igMemoryPool("System");                             _pools.Add("System", System);
			Static = new igMemoryPool("Static");                             _pools.Add("Static", Static);
			MetaData = new igMemoryPool("MetaData");                         _pools.Add("MetaData", MetaData);
			String = new igMemoryPool("String");                             _pools.Add("String", String);
			Kernel = new igMemoryPool("Kernel");                             _pools.Add("Kernel", Kernel);
			SystemDebug = new igMemoryPool("SystemDebug");                   _pools.Add("SystemDebug", SystemDebug);
			Default = new igMemoryPool("Default");                           _pools.Add("Default", Default);
			Current = new igMemoryPool("Current");                           _pools.Add("Current", Current);
			Fast = new igMemoryPool("Fast");                                 _pools.Add("Fast", Fast);
			AGP = new igMemoryPool("AGP");                                   _pools.Add("AGP", AGP);
			VRAM = new igMemoryPool("VRAM");                                 _pools.Add("VRAM", VRAM);
			Auxiliary = new igMemoryPool("Auxiliary");                       _pools.Add("Auxiliary", Auxiliary);
			VisualContext = new igMemoryPool("VisualContext");               _pools.Add("VisualContext", VisualContext);
			Graphics = new igMemoryPool("Graphics");                         _pools.Add("Graphics", Graphics);
			Actor = new igMemoryPool("Actor");                               _pools.Add("Actor", Actor);
			AnimationData = new igMemoryPool("AnimationData");               _pools.Add("AnimationData", AnimationData);
			Geometry = new igMemoryPool("Geometry");                         _pools.Add("Geometry", Geometry);
			Vertex = new igMemoryPool("Vertex");                             _pools.Add("Vertex", Vertex);
			VertexEdge = new igMemoryPool("VertexEdge");                     _pools.Add("VertexEdge", VertexEdge);
			VertexObject = new igMemoryPool("VertexObject");                 _pools.Add("VertexObject", VertexObject);
			Image = new igMemoryPool("Image");                               _pools.Add("Image", Image);
			ImageObject = new igMemoryPool("ImageObject");                   _pools.Add("ImageObject", ImageObject);
			Attribute = new igMemoryPool("Attribute");                       _pools.Add("Attribute", Attribute);
			Node = new igMemoryPool("Node");                                 _pools.Add("Node", Node);
			Audio = new igMemoryPool("Audio");                               _pools.Add("Audio", Audio);
			AudioDsp = new igMemoryPool("AudioDsp");                         _pools.Add("AudioDsp", AudioDsp);
			AudioSample = new igMemoryPool("AudioSample");                   _pools.Add("AudioSample", AudioSample);
			AudioSampleSecondary = new igMemoryPool("AudioSampleSecondary"); _pools.Add("AudioSampleSecondary", AudioSampleSecondary);
			AudioStreamFile = new igMemoryPool("AudioStreamFile");           _pools.Add("AudioStreamFile", AudioStreamFile);
			AudioStreamDecode = new igMemoryPool("AudioStreamDecode");       _pools.Add("AudioStreamDecode", AudioStreamDecode);
			Video = new igMemoryPool("Video");                               _pools.Add("Video", Video);
			DMA = new igMemoryPool("DMA");                                   _pools.Add("DMA", DMA);
			Shader = new igMemoryPool("Shader");                             _pools.Add("Shader", Shader);
			ShaderBinary = new igMemoryPool("ShaderBinary");                 _pools.Add("ShaderBinary", ShaderBinary);
			RenderList = new igMemoryPool("RenderList");                     _pools.Add("RenderList", RenderList);
			Texture = new igMemoryPool("Texture");                           _pools.Add("Texture", Texture);
			DriverData = new igMemoryPool("DriverData");                     _pools.Add("DriverData", DriverData);
			Handles = new igMemoryPool("Handles");                           _pools.Add("Handles", Handles);
			List = new igMemoryPool("List");                                 _pools.Add("List", List);
			Exporter = new igMemoryPool("Exporter");                         _pools.Add("Exporter", Exporter);
			Optimizer = new igMemoryPool("Optimizer");                       _pools.Add("Optimizer", Optimizer);
			Network = new igMemoryPool("Network");                           _pools.Add("Network", Network);
			VRAMTopDown = new igMemoryPool("VRAMTopDown");                   _pools.Add("VRAMTopDown", VRAMTopDown);
			DotNet = new igMemoryPool("DotNet");                             _pools.Add("DotNet", DotNet);
			VramA = new igMemoryPool("VramA");                               _pools.Add("VramA", VramA);
			VramStaging = new igMemoryPool("VramStaging");                   _pools.Add("VramStaging", VramStaging);
			MEM1 = Default;                                                  _pools.Add("MEM1", Default);
			MEM2 = Default;                                                  _pools.Add("MEM2", Default);
			VRAMBottomUp = VRAM;                                             _pools.Add("VRAMBottomUp", VRAM);
		}
		public igMemoryPool? GetMemoryPoolByName(string name)
		{
			if(_pools.ContainsKey(name)) return _pools[name];
			else return null;
		}
	}
}