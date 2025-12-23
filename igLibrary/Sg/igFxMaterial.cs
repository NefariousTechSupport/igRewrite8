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
	public class igFxMaterial : igCustomMaterial
	{
		public string _fxFilename;
		public uint _userFxMaterialFlag;
		public igCachedAttrListList _instanceAttrs;
		public igHandle _effectHandle;
		public uint _procVertexFormat;
		public int _textureCoordCount;
		public ulong _globalTechniqueMask;
		public igStringRefList _filesLoaded;
		public ulong _addFileDependencyCallback;
		public ulong _proceduralSamplerCallback;
		public ulong _getOptScriptPathCallback;
		public ulong _getEffectObjectNameCallback;
		public ulong _imagePostCreationCallback;
		public ulong _generateImageOutputFilenameCallback;
		public ulong _textureBindAttrPostCreationCallback;
		public ulong _generateShaderOutputFilenameCallback;
		public bool _limitFileNameLength;
	}
}