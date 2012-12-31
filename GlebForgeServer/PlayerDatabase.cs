using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GlebForgeServer
{
	public class PlayerDatabase
	{
		/// <summary>
		/// This Dictionary contains all of the player data in memory, indexed by name.
		/// </summary>
		private ConcurrentDictionary<String, Player> dict;

		/// <summary>
		/// The Linq XML Document that contains the database.
		/// </summary>
		private XElement database;

		/// <summary>
		/// The filename used to load and save the database.
		/// </summary>
		private const String DATABASE_FILE_NAME = "database.xml";

		/// <summary>
		/// The default constructor loads the database from a file.
		/// </summary>
		public PlayerDatabase()
		{
			//if (System.IO.File.Exists(DATABASE_FILE_NAME))
			//	LoadDatabase();
			dict = new ConcurrentDictionary<string, Player>();
			database = new XElement("Players");
		}

		public void CreateTestDatabase()
		{
			IDictionary<String, Player> players = new ConcurrentDictionary<String, Player>();
			players.Add("James", new Player(new Position(200, 200), new Velocity(0, 0), "James"));
			players.Add("Gleb", new Player(new Position(400, 300), new Velocity(0, 0), "Gleb"));

			CreateDatabase(players);
		}

		/// <summary>
		/// Creates a database from the List of players.
		/// </summary>
		public void CreateDatabase(IDictionary<String, Player> players)
		{
			dict = new ConcurrentDictionary<String, Player>(players);

			foreach (var player in players)
			{
				Player value = player.Value;
				XElement p = new XElement("Player");
				p.Add(new XElement("Name", value.Name));

				XElement position = new XElement("Position");
				position.Add(new XElement("X", value.Position.x));
				position.Add(new XElement("Y", value.Position.y));
				p.Add(position);

				XElement velocity = new XElement("Velocity");
				velocity.Add(new XElement("X", value.Velocity.x));
				velocity.Add(new XElement("Y", value.Velocity.y));
				p.Add(velocity);

				database.Add(p); 
			}
		}

		/// <summary>
		/// Saves the database to a file.
		/// </summary>
		public void SaveDatabase()
		{
			database.Save(DATABASE_FILE_NAME);
		}

		/// <summary>
		/// Loads the database from a file.
		/// </summary>
		/// <returns>Returns a list of the players loaded from the database.</returns>
		public IList<Player> LoadDatabase()
		{
			database = XElement.Load(DATABASE_FILE_NAME);

			IList<Player> players = new List<Player>();

			foreach (var player in database.Descendants("Player"))
				player.Elements("Player");
			return players;
		}

		/// <summary>
		/// Returns a list of players near the player with name name. Currently this returns all players.
		/// </summary>
		/// <param name="name">The name of the player to search nearby.</param>
		/// <returns>A list of nearby players.</returns>
		public List<Player> GetNearbyPlayers(String name)
		{
			List<Player> nearby = new List<Player>();
			foreach (var p in dict)
				if (p.Value.Name != name)
					nearby.Add(p.Value);
			return nearby;
		}

		/// <summary>
		/// Searches for a player by name.
		/// </summary>
		/// <param name="name">The name of the player for which to search.</param>
		/// <returns>The Player if it is found, null otherwise.</returns>
		public Player FindPlayer(String name)
		{
			Player player;
			if (dict.TryGetValue(name, out player))
				return player;
			else
				return null;
		}

		/// <summary>
		/// Updates player data.
		/// </summary>
		/// <param name="player"></param>
		public void UpdatePlayer(Player player)
		{
			Player result;
			if (dict.TryGetValue(player.Name, out result))
				dict.TryUpdate(player.Name, player, result);
		}
	}
}
