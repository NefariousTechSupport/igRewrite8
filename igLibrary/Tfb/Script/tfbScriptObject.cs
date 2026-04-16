/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;
using System.Globalization;

namespace igLibrary.Tfb.Script
{
	public class tfbScriptObject : igNamedObject
	{
		public object? _getFunc;
		public object? _setFunc;
		public object? _resetFunc;
		public static tfbScriptObject? _undefinedObject;
		public static tfbScriptObject? _itsInterface;
		public static tfbScriptObject? _itsObject;
		public static StringInfo? _levelLoadRequest;
		public static float _gameLogicScale;
		public static float _scriptTime;
		public static igObject? _emptySet; //public static ScriptGroupStack _emptySet;
    }
}	
