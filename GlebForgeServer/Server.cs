using System.Threading;
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GlebForgeServer
{
	public class Server
	{
		private TcpClient client;

		private Thread thread;

		private NetworkStream stream;

		private PlayerDatabase players;

		private Player player;

		private const int AUTHENTICATION_CLIENT_VALUE = 390458;
		private const int AUTHENTICATION_SERVER_VALUE = -283947;
		private const uint MESSAGE_SIZE = 4;


		private enum MESSAGE { ORIENTATION } 

		public Server(TcpClient client, PlayerDatabase players)
		{
			this.client = client;
			this.players = players;
			thread = new Thread(new ThreadStart(connection));
		}

		public void start()
		{
			if (thread != null)
				thread.Start();
		}

		public void connection()
		{
			try 
			{
				derp();
			}
			catch (IOException e)
			{ 
				Console.WriteLine(e.Message);
				client.Close();
				//players.Remove(player);
				return;
			}
		}

		public void derp()
		{
			Console.WriteLine("Thread started!");
			stream = client.GetStream();
			byte[] buffer = new byte[256];

			int result = stream.Read(buffer, 0, 4);

			long value = BitConverter.ToInt32(buffer, 0);

			//We may need this later
			/*
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buffer);
			 */

			Console.WriteLine("Player authenticated with {0}", BitConverter.ToString(buffer));
			if (value != AUTHENTICATION_CLIENT_VALUE)
			{
				Console.WriteLine("Disconnecting...");
				client.Close();
				return;
			}

			stream.Write(BitConverter.GetBytes(AUTHENTICATION_SERVER_VALUE), 0, 4);
			Console.WriteLine("Authentication successful!");

			//Get the player's credentials (just name for now)	
			stream.Read(buffer, 0, 4);
			int length = BitConverter.ToInt32(buffer, 0);
			if (length > Player.MAX_PLAYER_NAME_LENGTH)
				throw new Exception("Name too long");

			stream.Read(buffer, 0, length);
			String name = Encoding.UTF8.GetString(buffer, 0, length);
			Console.WriteLine("Player attempted to join with name: " + name);

			player = new Player();
			player = players.findPlayer(name);

			if (player == null)
				throw new Exception("Player not found");
			if (player.loggedIn)
				throw new Exception("Player " + player.name + " already logged in");

			player.loggedIn = true;

			while (true)
			{
				//Read player position
				//translateMessage();
				result = stream.Read(buffer, 0, 8);

				Position newPos;
				newPos.x = BitConverter.ToSingle(buffer, 0);
				newPos.y = BitConverter.ToSingle(buffer, 4);
				player.Position = newPos;
				//players.updatePlayer(player);
				
				//Find the first other player.
				List<Player> otherPlayers = players.getNearbyPlayers(player.name);
				int numNearbyPlayers = otherPlayers.Count;

				//Write the number of players for which we are going to send data
				stream.Write(BitConverter.GetBytes(numNearbyPlayers), 0, 4);

				int index = 0;
				//Write the data for each player into a buffer
				foreach (Player p in otherPlayers)
				{
					Buffer.BlockCopy(BitConverter.GetBytes(p.Position.x), 0, buffer, index, 4);
					Buffer.BlockCopy(BitConverter.GetBytes(p.Position.y), 0, buffer, index + 4, 4);

					index += 8;
					//Check for buffer full
				}

				//Write the buffer of player data
				stream.Write(buffer, 0, index);
			}
		}

		private void translateMessage()
		{
			byte[] message = new byte[MESSAGE_SIZE];
			int result = stream.Read(message, 0, 4);
			MESSAGE m = (MESSAGE)(BitConverter.ToInt32(message,0));
			switch (m)
			{
				case MESSAGE.ORIENTATION:
					readOrientation();
					break;
				default:
					//stream.Flush();
					break;
			}
		}

		private void readOrientation()
		{
			int size = sizeof(float)*2;
			byte[] buffer = new byte[size];
			int result = stream.Read(buffer, 0, size);
		}
	}
}
