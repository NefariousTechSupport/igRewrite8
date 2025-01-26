/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	//Inherits from igContext
	public class igMemoryContext : igSingleton<igMemoryContext>
	{
		public Dictionary<string, igMemoryPool> _pools { get; private set; }

		private static bool _initialized;

		public igMemoryContext()
		{
			if(_initialized) throw new InvalidOperationException("Cannot create multiple igMemoryContexts");

			_initialized = true;
			_pools = new Dictionary<string, igMemoryPool>();
			_pools.Add("Bootstrap", 			new igMemoryPool("Bootstrap"));
			_pools.Add("System", 				new igMemoryPool("System"));
			_pools.Add("Static", 				new igMemoryPool("Static"));
			_pools.Add("MetaData", 				new igMemoryPool("MetaData"));
			_pools.Add("String", 				new igMemoryPool("String"));
			_pools.Add("Kernel", 				new igMemoryPool("Kernel"));
			_pools.Add("SystemDebug", 			new igMemoryPool("SystemDebug"));
			_pools.Add("Debug",					new igMemoryPool("Debug"));
			_pools.Add("Default", 				new igMemoryPool("Default"));
			_pools.Add("Current", 				new igMemoryPool("Current"));
			_pools.Add("Fast", 					new igMemoryPool("Fast"));
			_pools.Add("AGP", 					new igMemoryPool("AGP"));
			_pools.Add("VRAM", 					new igMemoryPool("VRAM"));
			_pools.Add("Auxiliary", 			new igMemoryPool("Auxiliary"));
			_pools.Add("VisualContext", 		new igMemoryPool("VisualContext"));
			_pools.Add("Graphics", 				new igMemoryPool("Graphics"));
			_pools.Add("Actor", 				new igMemoryPool("Actor"));
			_pools.Add("AnimationData", 		new igMemoryPool("AnimationData"));
			_pools.Add("Geometry", 				new igMemoryPool("Geometry"));
			_pools.Add("Vertex", 				new igMemoryPool("Vertex"));
			_pools.Add("VertexEdge", 			new igMemoryPool("VertexEdge"));
			_pools.Add("VertexObject", 			new igMemoryPool("VertexObject"));
			_pools.Add("Image", 				new igMemoryPool("Image"));
			_pools.Add("ImageObject", 			new igMemoryPool("ImageObject"));
			_pools.Add("Attribute", 			new igMemoryPool("Attribute"));
			_pools.Add("Node", 					new igMemoryPool("Node"));
			_pools.Add("Audio", 				new igMemoryPool("Audio"));
			_pools.Add("AudioDsp", 				new igMemoryPool("AudioDsp"));
			_pools.Add("AudioSample", 			new igMemoryPool("AudioSample"));
			_pools.Add("AudioSampleSecondary", 	new igMemoryPool("AudioSampleSecondary"));
			_pools.Add("AudioStreamFile", 		new igMemoryPool("AudioStreamFile"));
			_pools.Add("AudioStreamDecode", 	new igMemoryPool("AudioStreamDecode"));
			_pools.Add("Video", 				new igMemoryPool("Video"));
			_pools.Add("Temporary", 			new igMemoryPool("Temporary"));
			_pools.Add("DMA", 					new igMemoryPool("DMA"));
			_pools.Add("Shader", 				new igMemoryPool("Shader"));
			_pools.Add("ShaderBinary", 			new igMemoryPool("ShaderBinary"));
			_pools.Add("RenderList", 			new igMemoryPool("RenderList"));
			_pools.Add("Texture", 				new igMemoryPool("Texture"));
			_pools.Add("DriverData", 			new igMemoryPool("DriverData"));
			_pools.Add("Handles", 				new igMemoryPool("Handles"));
			_pools.Add("List", 					new igMemoryPool("List"));
			_pools.Add("Exporter", 				new igMemoryPool("Exporter"));
			_pools.Add("Optimizer", 			new igMemoryPool("Optimizer"));
			_pools.Add("Network", 				new igMemoryPool("Network"));
			_pools.Add("VRAMTopDown", 			new igMemoryPool("VRAMTopDown"));
			_pools.Add("DotNet", 				new igMemoryPool("DotNet"));
			_pools.Add("VramA", 				new igMemoryPool("VramA"));
			_pools.Add("VramB", 				new igMemoryPool("VramB"));
			_pools.Add("VramStaging", 			new igMemoryPool("VramStaging"));
			_pools.Add("MEM1", 					_pools["Default"]);
			_pools.Add("MEM2", 					_pools["Default"]);
			_pools.Add("VRAMBottomUp", 			_pools["VRAM"]);
		}
		public igMemoryPool? GetMemoryPoolByName(string name)
		{
			if(_pools.ContainsKey(name)) return _pools[name];
			else return null;
		}
	}
}