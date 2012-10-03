using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlebForgeServer
{
	public struct Position
	{
		public float x;
		public float y;
	}

	public class PlayerData
	{
		public Position Position { get; set; }

		public PlayerData(Position position)
		{
			this.Position = position;
		}

		public PlayerData()
		{
			Position = new Position();
		}
	}
}
