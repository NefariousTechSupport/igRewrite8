/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;

namespace igLibrary
{
	public static class CPrecacheFileLoader
	{
		/// <summary>
		/// The current task of the loader, tells it what to do for the next files
		/// </summary>
		private enum LoaderTask
		{
			Unknown,
			LoosePak,
			FullPackage,
			LoosePackage,
			Noop
		};


		/// <summary>
		/// Struct combining info for each task
		/// </summary>
		struct TaskEntry
		{
			public LoaderTask _fullTask;
			public LoaderTask _looseTask;

			public TaskEntry(LoaderTask fullTask, LoaderTask looseTask)
			{
				_fullTask = fullTask;
				_looseTask = looseTask;
			}
		}


		/// <summary>
		/// Lookup table from task name to task enum
		/// </summary>
		private static readonly Dictionary<string, TaskEntry> _taskLookup = new Dictionary<string, TaskEntry>()
		{
			{ "loose_pak_lab",                   new(LoaderTask.LoosePak,                  LoaderTask.LoosePak                  ) },
			{ "full_package_lab",                new(LoaderTask.FullPackage,               LoaderTask.Noop                      ) },
		};

		private static readonly Dictionary<string, Func<string>> _envLookup = new Dictionary<string, Func<string>>()
		{
			{ "platform_string",                 () => igAlchemyCore.GetPlatformString(igRegistry.GetRegistry()._platform) },
		};


		public static void LoadInitialPackages(igArkCore.EGame game, bool loose)
		{
			LoadInitialPackages($"{igArkCoreFile.ArkCoreFolder}/{game}/packages", loose);
		}


		/// <summary>
		/// Loads all packages from a given file
		/// </summary>
		/// <param name="filePath">The filepath to load from</param>
		public static void LoadInitialPackages(string filePath, bool loose)
		{
			StreamReader precacheFile = File.OpenText(filePath);

			ReadLoop(precacheFile, loose);
		}

		private static void ReadLoop(StreamReader precacheFile, bool loose)
		{
			LoaderTask task = LoaderTask.LoosePak;

			uint lineNumber = 0;

			while(true)
			{
				lineNumber++;

				string? line = precacheFile.ReadLine();
				if (line == null)
				{
					break;
				}

				line = line.Trim();

				if (line.Length == 0)
				{
					continue;
				}

				if (line[0] == '[')
				{
					if (line[line.Length - 1] != ']')
					{
						Logging.Error("Unterminated '[' on line {0}", lineNumber);
						break;
					}

					task = ParseTask(line, loose);

					if (task == LoaderTask.Unknown)
					{
						Logging.Error("Unknown task type on line {0}", lineNumber);
						break;
					}
				}
				else
				{
					string? path = ParseFilePath(line);
					if (path == null)
					{
						Logging.Error("Malformed filepath on line {0}", lineNumber);
						break;
					}
					ProcessTask(task, path);
				}
			}
		}


		
		private static LoaderTask ParseTask(string line, bool loose)
		{
			LoaderTask task = LoaderTask.Unknown;

			if (_taskLookup.TryGetValue(line.Substring(1, line.Length - 2), out TaskEntry entry))
			{
				task = loose ? entry._looseTask : entry._fullTask;
			}

			return task;
		}


		private static string? ParseFilePath(string line)
		{
			StringBuilder processed = new StringBuilder();

			for (int i = 0; i < line.Length; i++)
			{
				if (line[i] == '$')
				{
					if (i+1 != line.Length)
					{
						int ending = line.IndexOf('}', i+1);
						if (line[i+1] != '{' || ending < 0)
						{
							// Missing {}
							return null;
						}

						string token = line.Substring(i+2, ending-i-2);

						if (!_envLookup.TryGetValue(token, out Func<string>? executor))
						{
							// Unknown env
							return null;
						}

						processed.Append(executor.Invoke());
						i = ending;
					}
				}
				else
				{
					processed.Append(line[i]);
				}
			}

			return processed.ToString();
		}

		private static void ProcessTask(LoaderTask task, string line)
		{
			switch (task)
			{
				case LoaderTask.Unknown:
					return;
				case LoaderTask.LoosePak:
					igFileContext.Singleton.LoadArchive(line);
					break;
				case LoaderTask.FullPackage:
					CPrecacheManager._Instance.PrecachePackage(line, EMemoryPoolID.MP_DEFAULT);
					break;
				case LoaderTask.LoosePackage:
					igFileContext.Singleton.LoadArchive($"app:/archives/{Path.GetFileName(line)}.pak");
					break;
				case LoaderTask.Noop:
					break;
			}
		}
	}	
}