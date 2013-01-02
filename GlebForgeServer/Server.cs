using System.Threading;
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace GlebForgeServer
{
	/// <summary>
	/// The Server class encapsulates a thread that communicates with a single player.
	/// </summary>
	public class Server
	{
		private TcpClient client;

		private Thread thread;

		private NetworkStream stream;

		private static PlayerDatabase players = PlayerDatabase.Instance;

		private Player player;

		public const int AUTHENTICATION_CLIENT_VALUE = 390458;
		private const int AUTHENTICATION_SERVER_VALUE = -283947;
		private const uint MESSAGE_SIZE = 4;

		private const int TIMEOUT_TIME = 15000;


		private enum MESSAGE { ORIENTATION }

		ListenServer.CloseServerDelegate closeServer;

		/// <summary>
		/// Construct the Server.
		/// </summary>
		/// <param name="client">The accepted TcpClient from the ListenServer.</param>
		/// <param name="closeServer">A delegate to the method that should be called once the server closes.</param>
		public Server(TcpClient client, ListenServer.CloseServerDelegate closeServer)
		{
			this.client = client;
			thread = new Thread(new ThreadStart(Connection));
			this.closeServer = closeServer;
		}

		/// <summary>
		/// Start running the thread.
		/// </summary>
		public void Start()
		{
			if (thread != null)
				thread.Start();
		}

		public void Connection()
		{
			try
			{
				stream = client.GetStream();
				StateRun();
			}
			catch (Exception e)
			{
				//IOExceptions and TimeoutExceptions have the same behavior
				Console.WriteLine(e.Message);
				client.Close();
				if (player != null)
					player.loggedIn = false;
				closeServer(this);
				return;
			}
		}

		private int ReadInt()
		{
			byte[] buffer = ReadAsync(4);

			return BitConverter.ToInt32(buffer, 0);
		}

		/// <summary>
		/// Reads length bytes. If the read takes longer than TIMEOUT_TIME it throws an exception.
		/// </summary>
		/// <param name="length">The length, in bytes, to be read.</param>
		/// <returns>Returns a byte array of the data read.</returns>
		private byte[] ReadAsync(uint length)
		{
			byte[] buffer = new byte[length];

			//This will immediately return if the client closes.
			IAsyncResult result = stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback((x) => { /*(x.AsyncState as NetworkStream).EndRead(x);*/ }), stream);
			WaitHandle waitHandle = result.AsyncWaitHandle;
			bool completed = waitHandle.WaitOne(TIMEOUT_TIME);
			//int bytesRead = stream.EndRead(result);
			if (!completed)
				throw new TimeoutException(String.Format("Timed out while trying to read {0} bytes.", length));
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

		/// <summary>
		/// This is temporary. It should be "asynchronous in the future.
		/// </summary>
		/// <param name="buffer"></param>
		private void Write(byte[] buffer, int offset, int size)
		{
			stream.Write(buffer, 0, size);
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

		public void StateRun()
		{
			StateContext context = new StateContext(this);

			while (true)
				context.Next();
		}

		public void Run()
		{
			Console.WriteLine("Thread started!");

#if true
			NetworkState state = new AuthenticationState();
			state.Receive(this);
#else
			int value = ReadInt();

			//We may need this later
			/*
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buffer);
			 */

			Console.WriteLine("Player authenticated with {0}", value);
			if (value != AUTHENTICATION_CLIENT_VALUE)
				throw new ApplicationException("Player authenticated with invalid value.");
#endif

#if true
			state.Send(this);
#else
			stream.Write(BitConverter.GetBytes(AUTHENTICATION_SERVER_VALUE), 0, 4);
			Console.WriteLine("Authentication successful!");
#endif

#if true
			state = new UsernameAndPasswordState();
			state.Receive(this);
#else
			//Get the player's credentials (just name for now)	
			//stream.Read(buffer, 0, 4);
			//int length = BitConverter.ToInt32(buffer, 0);
			uint length = (uint)ReadInt();
			if (length > Player.MAX_PLAYER_NAME_LENGTH)
				throw new ApplicationException("Name too long");


			String name = ReadString(length);
			Console.WriteLine("Player attempted to join with name: " + name);

			//player = new Player();
			player = players.FindPlayer(name);

			if (player == null)
				throw new ApplicationException("Player not found");
			if (player.loggedIn)
				throw new ApplicationException("Player " + player.Name + " already logged in");

			player.loggedIn = true;

			Console.WriteLine("{0} logged in successfully!", name);
#endif

			while (true)
			{
				//Read player position
				//translateMessage();
#if true
				state = new PositionState();
				state.Receive(this);
#else
				Position newPos;
				newPos.x = ReadSingle();
				newPos.y = ReadSingle();
				player.Position = newPos;
				//players.updatePlayer(player);
#endif
#if true
				state.Send(this);
#else
				//Find the first other player.
				List<Player> otherPlayers = players.GetNearbyPlayers(player.Name);
				int numNearbyPlayers = otherPlayers.Count;

				//Write the number of players for which we are going to send data
				stream.Write(BitConverter.GetBytes(numNearbyPlayers), 0, 4);

				WritePlayerData(otherPlayers);
#endif
			}
		}

		private void translateMessage()
		{
			byte[] message = new byte[MESSAGE_SIZE];
			int result = stream.Read(message, 0, 4);
			MESSAGE m = (MESSAGE)(BitConverter.ToInt32(message, 0));
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
			int size = sizeof(float) * 2;
			byte[] buffer = new byte[size];
			int result = stream.Read(buffer, 0, size);
		}

		class StateContext
		{
			private NetworkState state = new AuthenticationState();
			private Server server;

			public StateContext(Server server)
			{
				this.server = server;
			}

			public void Next()
			{
				state.Receive(server);
				state = state.Send(server);
			}
		}

		interface NetworkState
		{
			NetworkState Send(Server server);
			void Receive(Server server);
		}

		class AuthenticationState : NetworkState
		{
			public NetworkState Send(Server server)
			{
				server.Write(BitConverter.GetBytes(AUTHENTICATION_SERVER_VALUE), 0, 4);
				Console.WriteLine("Authentication successful!");

				return new UsernameAndPasswordState();
			}
			public void Receive(Server server)
			{
				int value = server.ReadInt();

				Console.WriteLine("Player authenticated with {0}", value);
				if (value != AUTHENTICATION_CLIENT_VALUE)
					throw new ApplicationException("Player authenticated with invalid value.");
			}
		}

		class UsernameAndPasswordState : NetworkState
		{
			public NetworkState Send(Server server)
			{
				//Password not implemented yet, just move on to next state
				return new PositionState();
			}

			public void Receive(Server server) 
			{
				//Get the player's credentials (just name for now)	
				//stream.Read(buffer, 0, 4);
				//int length = BitConverter.ToInt32(buffer, 0);
				uint length = (uint)server.ReadInt();
				if (length > Player.MAX_PLAYER_NAME_LENGTH)
					throw new ApplicationException("Name too long");


				String name = server.ReadString(length);
				Console.WriteLine("Player attempted to join with name: " + name);

				//player = new Player();
				server.player = players.FindPlayer(name);

				if (server.player == null)
					throw new ApplicationException("Player not found");
				if (server.player.loggedIn)
					throw new ApplicationException("Player " + server.player.Name + " already logged in");

				String password = server.ReadString(128);
				if (!server.player.Password.Equals(password))
					throw new ApplicationException(String.Format("Player gave invalid password {0}.", password));

				server.player.loggedIn = true;

				Console.WriteLine("{0} logged in successfully!", name);
			}
		}

		class PositionState : NetworkState
		{
			public NetworkState Send(Server server)
			{
				//Find the first other player.
				List<Player> otherPlayers = players.GetNearbyPlayers(server.player.Name);
				int numNearbyPlayers = otherPlayers.Count;

				//Write the number of players for which we are going to send data
				server.Write(BitConverter.GetBytes(numNearbyPlayers), 0, 4);

				server.WritePlayerData(otherPlayers);

				return this;
			}

			public void Receive(Server server)
			{
				Position newPos;
				newPos.x = server.ReadSingle();
				newPos.y = server.ReadSingle();
				server.player.Position = newPos;
			}
		}
	}
}
