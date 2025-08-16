/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Tfb.Script
{
	public class StringConverter : tfbScriptObject
	{
		public static ScriptSetReference? _valueSet;
		public static tfbScriptObject? _scriptString; // StringInfo
		public static bool _isTime;
		public static int _integerDigits;
		public static int _decimalDigits;
		public static bool _useComma;
		public static tfbScriptObject? _interface;
	}
}