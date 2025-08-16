/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Tfb.Script
{
	public class ScriptController : tfbScriptObject
	{
		public bool _pointerEnabled;
		public int _saveState;
		public int _controllerType;
		public int _id;
		public uint _userId;
		public uint _trueUserId;
		public bool _wasSignedOut;
		public bool _forceSignIn;
		public bool _isBoundToInGameActor;
		public igObject _device; // igBaseInputDevice
		public uint _previousButtonsState;
		public uint _buttonsState;
		public float[] _buttons; // length 47
		public float _motor0;
		public float _motor1;
		public object[] _gestureArray; // 20 byte long struct repeated 14 times
		public int _richPresence;
		public static tfbScriptObject? _interface;
		public static ScriptController? _inactive;
		public static ScriptController? _dialogController;
		public static ScriptController? _primaryController;
		public static igVector<ScriptController>? _scriptControllerList;
		public static bool _allowMultipleBLEGamepads;
	}
}