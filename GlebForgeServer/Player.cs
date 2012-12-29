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

		public Position(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public struct Velocity
	{
		public float x;
		public float y;

		public Velocity(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class Player
	{
		public Position Position { get; set; }

		public Velocity Velocity { get; set; }

		public const int MAX_PLAYER_NAME_LENGTH = 6;

		public String name;

		public Boolean loggedIn = false;


		public Player(Position position, Velocity velocity, String name)
		{
			this.Position = position;
			this.Velocity = velocity;
			this.name = name;
		}

		public Player()
		{
			Position = new Position();
			Velocity = new Velocity();
			name = "NoName";
		}
	}
}
