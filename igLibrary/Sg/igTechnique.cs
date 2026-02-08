/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Attrs;

namespace igLibrary.Sg
{
	public class igTechnique : igNamedObject
	{
		public igCachedAttrList _attrs;
		public igObject _passList; // igTechniquePassList
		public igTechniqueParameterList _parameterList;
		public igTechniqueSamplerList _samplerList;
		public igTechniqueVertexComponentList _vertexComponents;
		public int _appFlags;
	}

	public class igTechniqueList : igTObjectList<igTechnique>
	{
	}
}