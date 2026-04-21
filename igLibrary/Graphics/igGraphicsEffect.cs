/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/

using igLibrary.Graphics;


namespace igLibrary.Graphics
{
	public class igGraphicsEffect : igNamedObject
	{
		public igStringIntHashTable _globalTechniqueTable;
		public int _globalTechniqueIndexCounter;
		public igIntList _globalTechniqueList;
		public ulong _globalTechniqueMask;
		public igGraphicsObjectSet _graphicsObjects;
		public igStringRefList _techniqueNames;
		public igMemoryCommandStreamList _commandStreams;
		public igUnsignedCharList _instanceMatrixShaderConstants;
		public igUnsignedCharList _instanceShaderConstants;
		public uint _procVertexFormat;
	}
}
