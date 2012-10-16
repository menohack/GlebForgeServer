using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlebForgeServer
{
	public class PlayerDatabase
	{
		private List<Player> players;

		public PlayerDatabase()
		{
			players = new List<Player>();
			players.Add(new Player(new Position(200, 200), new Velocity(0, 0), "James"));
			players.Add(new Player(new Position(400, 300), new Velocity(0, 0), "Gleb"));
		}

		public List<Player> getNearbyPlayers(String name)
		{
			List<Player> nearby = new List<Player>();
			foreach (Player p in players)
				if (p.name != name)
					nearby.Add(p);
			return nearby;
		}

		public Player findPlayer(String name)
		{
			foreach (Player p in players)
				if (p.name == name)
					return p;
			return null;
		}

		public void updatePlayer(Player player)
		{
			foreach (Player p in players)
				if (p.name == player.name)
				{
					players.Remove(p);
					players.Add(player);
				}
		}
	}
}
