/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Tfb.Script
{
	/// <summary>
	/// Stores bindings in a slightly more optimised format until the time is right
	/// </summary>
	public static class BindingManager
	{
		public struct FunctionBinding
		{
			public FunctionBinding(string type, string name)
			{
				_type = type;
				_name = name;
			}
			public string _type;
			public string _name;
		}
		public class ClassBinding
		{
			public string _name { get; private set; }
			public readonly List<FunctionBinding> _functions;

			public ClassBinding(string name)
			{
				_name = name;
				_functions = new List<FunctionBinding>();
			}

			public void AddFunction(string type, string name)
			{
				_functions.Add(new FunctionBinding(type, name));
			}
		}

		private static List<ClassBinding> _bindings = new List<ClassBinding>();

		public static ClassBinding MakeNewClassBinding(string name)
		{
			ClassBinding binding = new ClassBinding(name);

			_bindings.Add(binding);

			return binding;
		}

		public static void SetupBindings()
		{
			for (int i = 0; i < _bindings.Count; i++)
			{
				ClassBinding binding = _bindings[i];

				igObjectDirectory bindingTypeDir = new igObjectDirectory();
				bindingTypeDir._name = new igName(binding._name);
				bindingTypeDir._useNameList = true;
				bindingTypeDir._nameList = new igNameList();
				igObjectStreamManager.Singleton.AddObjectDirectory(bindingTypeDir, bindingTypeDir._name._string);
				igObjectHandleManager.Singleton.AddSystemNamespace(binding._name);

				for (int f = 0; f < binding._functions.Count; f++)
				{
					igMetaObject? meta = igArkCore.GetObjectMeta(binding._functions[f]._type);
					Type? type = typeof(tfbScriptObject);
					if (meta != null)
					{
						meta.GatherDependancies();
						igArkCore.FlushPendingTypes();
						type = meta._vTablePointer!;
					}

					tfbScriptObject functionBinding = (tfbScriptObject)Activator.CreateInstance(type)!;
					functionBinding._name = binding._functions[f]._name;
					bindingTypeDir.AddObject(functionBinding, default(igName), new igName(functionBinding._name));
				}
			}
		}
	}
}