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

	public struct Velocity
	{
		public float x;
		public float y;
	}

	public class Player
	{
		public Position Position { get; set; }

		public Velocity Velocity { get; set; }

		public Player(Position position, Velocity velocity)
		{
			this.Position = position;
			this.Velocity = velocity;
		}

		public Player()
		{
			Position = new Position();
			Velocity = new Velocity();
		}
	}
}
