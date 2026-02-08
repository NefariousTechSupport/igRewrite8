/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Sg
{
	public class igEffect : igNamedObject
	{
		public igObject _parameterList; //igEffectParameterList
		public igObject _samplerList; //igEffectSamplerList
		public igObject _vertexComponentDefinitionList; //igVertexComponentDefinitionList
        public igStringIntHashTable _globalTechniqueTable;
		public int _globalTechniqueIndexCounter;
		public igTechniqueList _techniqueList;
		public igObjectAnnotationTable _annotations;
		public igIntList _globalTechniqueList;
		public ulong _globalTechniqueMask;
	}

	public class igEffectList : igTObjectList<igEffect>
	{
	}
}