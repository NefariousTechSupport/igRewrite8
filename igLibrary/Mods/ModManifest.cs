using System.Text.Json;
using System.Text.Json.Nodes;

namespace igLibrary.Mods
{
	/// <summary>
	/// Represents the files modified within a mod
	/// </summary>
	public abstract class ModManifest
	{
		public igArkCore.EGame Game => _game;

		private igArkCore.EGame _game;

		private static Dictionary<igArkCore.EGame, Type> _gameManifestTypes = new Dictionary<igArkCore.EGame, Type>()
		{
			{ igArkCore.EGame.EV_SkylandersSwapForce,                typeof(AlchemyLaboratoryModManifest)              },
			{ igArkCore.EGame.EV_SkylandersSuperchargers,            typeof(AlchemyLaboratoryModManifest)              },
			{ igArkCore.EGame.EV_SkylandersImaginators,              typeof(AlchemyLaboratoryModManifest)              },
			{ igArkCore.EGame.EV_CrashNSaneTrilogy,                  typeof(AlchemyLaboratoryModManifest)              },
			{ igArkCore.EGame.EV_CrashTeamRacingNitroFueled,         typeof(AlchemyLaboratoryModManifest)              }
		};

		public static ManifestError ReadManifest(JsonObject manifestJson, out ModManifest? manifest)
		{
			Logging.Info("Parsing manifest...");

			manifest = null;

			// Parsing version
			uint version;
			{
				if (!manifestJson.TryGetPropertyValue("version", out JsonNode? versionNode))
				{
					Logging.Error("Failed to locate version property");
					return ManifestError.MissingVersionProperty;
				}
				if (!versionNode!.AsValue().TryGetValue<uint>(out version))
				{
					Logging.Error("Version number malformed, must be an unsigned integer");
					return ManifestError.MalformedVersionProperty;
				}
			}

			// Parsing game
			igArkCore.EGame game;
			{
				if (!manifestJson.TryGetPropertyValue("game", out JsonNode? gameNode))
				{
					Logging.Error("Failed to locate game property");
					return ManifestError.MissingGameProperty;
				}
				if (!gameNode!.AsValue().TryGetValue<string>(out string? gameName))
				{
					Logging.Error("game name malformed, must be a string");
					return ManifestError.MalformedGameProperty;
				}
				if (!Enum.TryParse<igArkCore.EGame>(gameName, out game))
				{
					Logging.Error("unknown game name, got {0}", gameName);
					return ManifestError.UnknownGameName;
				}
			}

			if (!_gameManifestTypes.TryGetValue(game, out Type? manifestType))
			{
				Logging.Error("{0} is unsupported, Try upgrading to a newer version of igCauldron", game.ToString());
				return ManifestError.UnsupportedGame;
			}

			manifest = (ModManifest)Activator.CreateInstance(manifestType!)!;

			manifest._game = game;

			ManifestError error = manifest.Parse(manifestJson);
			if (error != ManifestError.Success)
			{
				manifest = null;
			}

			return error;
		}

		public static ModManifest? Create(igArkCore.EGame game)
		{
			if (!_gameManifestTypes.TryGetValue(game, out Type? manifestType))
			{
				return null;
			}

			ModManifest manifest = (ModManifest)Activator.CreateInstance(manifestType!)!;

			manifest._game = game;

			return manifest;
		}

		protected abstract ManifestError Parse(JsonObject manifestJson);
	}
}