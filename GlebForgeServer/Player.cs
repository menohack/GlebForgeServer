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

		public Velocity Velocity { get; private set; }

		public const int MAX_PLAYER_NAME_LENGTH = 32;

		public String Name { get; private set; }

		public String Password { get; private set; }

		public bool loggedIn = false;


		public Player(Position position, Velocity velocity, String name, String password)
		{
			this.Position = position;
			this.Velocity = velocity;
			this.Name = name;
			
			this.Password = password.PadRight(128);
		}

		public Player()
		{
			Position = new Position();
			Velocity = new Velocity();
			Name = "NoName";
		}
	}
}
