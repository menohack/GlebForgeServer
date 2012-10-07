using System.Threading;
using System;
using System.Net.Sockets;
using System.IO;

namespace GlebForgeServer
{
	public class Server
	{
		private TcpClient client;

		private Thread thread;

		private NetworkStream stream;

		private PlayerData[] players;

		private uint numPlayers;

		private int playerID;

		private const int AUTHENTICATION_CLIENT_VALUE = 390458;
		private const int AUTHENTICATION_SERVER_VALUE = -283947;
		private const uint MESSAGE_SIZE = 4;

		private enum MESSAGE { ORIENTATION } 

		public Server(TcpClient client, PlayerData[] players, int playerID)
		{
			this.client = client;
			this.players = players;
			this.playerID = playerID;
			thread = new Thread(new ThreadStart(connection));
		}

		public void start()
		{
			if (thread != null)
				thread.Start();
		}

		public void connection()
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

			players[playerID] = new PlayerData();


			while (true)
			{
				//Read player position
				//translateMessage();
				try
				{
					result = stream.Read(buffer, 0, 8);
				}
				catch (IOException e)
				{
					Console.WriteLine("Client closed the connection!");
					client.Close();
					return;
				}
				Position newPos;
				newPos.x = BitConverter.ToSingle(buffer, 0);
				newPos.y = BitConverter.ToSingle(buffer, 4);
				players[playerID].Position = newPos;

				uint otherID = 0;
				if (playerID == 0)
					otherID = 1;
				float first, second;
				if (players[otherID] != null)
				{
					first = players[otherID].Position.x;
					second = players[otherID].Position.y;
				}
				else 
					first = second = 0.0f;

				Buffer.BlockCopy(BitConverter.GetBytes(first), 0, buffer, 0, 4);
				Buffer.BlockCopy(BitConverter.GetBytes(second), 0, buffer, 4, 4);
				try
				{
					stream.Write(buffer, 0, 8);
				}
				catch (IOException e)
				{
					Console.WriteLine("Client closed the connection!");
					client.Close();
					return;
				}
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
