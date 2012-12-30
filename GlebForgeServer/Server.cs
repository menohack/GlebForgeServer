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

		public const int AUTHENTICATION_CLIENT_VALUE = 390458;
		private const int AUTHENTICATION_SERVER_VALUE = -283947;
		private const uint MESSAGE_SIZE = 4;

		private const int TIMEOUT_TIME = 5000;


		private enum MESSAGE { ORIENTATION } 

		GlebForgeServer.CloseServerDelegate closeServer;

		public Server(TcpClient client, PlayerDatabase players, GlebForgeServer.CloseServerDelegate closeServer)
		{
			this.client = client;
			this.players = players;
			thread = new Thread(new ThreadStart(connection));
			this.closeServer = closeServer;
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
			catch (Exception e)
			{ 
				//IOExceptions and TimeoutExceptions have the same behavior
				Console.WriteLine(e.Message);
				client.Close();
				//players.Remove(player);
				closeServer(this);
				return;
			}
		}

		private int ReadInt()
		{
			byte[] buffer = ReadAsync(4);

			return BitConverter.ToInt32(buffer, 0);
		}

		private byte[] ReadAsync(uint length)
		{
			byte[] buffer = new byte[length];

			//This will immediately return if the client closes.
			IAsyncResult result = stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback((x) => { /*(x.AsyncState as NetworkStream).EndRead(x);*/ }), stream);
			WaitHandle waitHandle = result.AsyncWaitHandle;
			bool completed = waitHandle.WaitOne(TIMEOUT_TIME);
			//int bytesRead = stream.EndRead(result);
			if (!completed)
				throw new TimeoutException("Timed out while waiting for authentication value.");
			return buffer;
		}

		private String ReadString(uint length)
		{
			byte[] buffer = ReadAsync(length);
			return Encoding.UTF8.GetString(buffer, 0, (int)length);
		}

		private float ReadSingle()
		{
			byte[] buffer = ReadAsync(4);
			return BitConverter.ToSingle(buffer, 0);
		}

		private void WritePlayerData(IList<Player> otherPlayers)
		{
			byte[] buffer = new byte[otherPlayers.Count * 8];
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

		public void derp()
		{
			Console.WriteLine("Thread started!");
			stream = client.GetStream();

			int value = ReadInt();
		
			//We may need this later
			/*
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buffer);
			 */

			Console.WriteLine("Player authenticated with {0}", value);
			if (value != AUTHENTICATION_CLIENT_VALUE)
				throw new ApplicationException("Player authenticated with invalid value.");

			stream.Write(BitConverter.GetBytes(AUTHENTICATION_SERVER_VALUE), 0, 4);
			Console.WriteLine("Authentication successful!");

			//Get the player's credentials (just name for now)	
			//stream.Read(buffer, 0, 4);
			//int length = BitConverter.ToInt32(buffer, 0);
			uint length = (uint)ReadInt();
			if (length > Player.MAX_PLAYER_NAME_LENGTH)
				throw new ApplicationException("Name too long");

			
			String name = ReadString(length);
			Console.WriteLine("Player attempted to join with name: " + name);

			player = new Player();
			player = players.findPlayer(name);

			if (player == null)
				throw new ApplicationException("Player not found");
			if (player.loggedIn)
				throw new ApplicationException("Player " + player.name + " already logged in");

			player.loggedIn = true;

			Console.WriteLine("{0} logged in successfully!", name);

			while (true)
			{
				//Read player position
				//translateMessage();

				Position newPos;
				newPos.x = ReadSingle();
				newPos.y = ReadSingle();
				player.Position = newPos;
				//players.updatePlayer(player);
				
				//Find the first other player.
				List<Player> otherPlayers = players.getNearbyPlayers(player.name);
				int numNearbyPlayers = otherPlayers.Count;

				//Write the number of players for which we are going to send data
				stream.Write(BitConverter.GetBytes(numNearbyPlayers), 0, 4);

				WritePlayerData(otherPlayers);
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
