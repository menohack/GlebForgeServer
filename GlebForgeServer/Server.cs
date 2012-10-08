using System.Threading;
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace GlebForgeServer
{
	public class Server
	{
		private TcpClient client;

		private Thread thread;

		private NetworkStream stream;

		private List<Player> players;

		private Player player;

		private const int AUTHENTICATION_CLIENT_VALUE = 390458;
		private const int AUTHENTICATION_SERVER_VALUE = -283947;
		private const uint MESSAGE_SIZE = 4;

		private enum MESSAGE { ORIENTATION } 

		public Server(TcpClient client, List<Player> players)
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
				players.Remove(player);
				return;
			}
		}

		public void derp()
		{
			Console.WriteLine("Thread started!");
			stream = client.GetStream();
			byte[] buffer = new byte[8];

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

			player = new Player();
			players.Add(player);

			while (true)
			{
				//Read player position
				//translateMessage();
				result = stream.Read(buffer, 0, 8);

				Position newPos;
				newPos.x = BitConverter.ToSingle(buffer, 0);
				newPos.y = BitConverter.ToSingle(buffer, 4);
				player.Position = newPos;

				Player otherPlayer;
				float first = 0.0f, second = 0.0f;
				if (players.Count > 1)
				{
					//Find the first other player.
					foreach (Player p in players)
						if (p != player)
						{
							otherPlayer = p;
							first = otherPlayer.Position.x;
							second = otherPlayer.Position.y;
						}
				}

				Buffer.BlockCopy(BitConverter.GetBytes(first), 0, buffer, 0, 4);
				Buffer.BlockCopy(BitConverter.GetBytes(second), 0, buffer, 4, 4);

				stream.Write(buffer, 0, 8);
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
