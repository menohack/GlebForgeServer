using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace GlebForgeServer
{
	public delegate void CloseServerDelegate(Server server);
	/// <summary>
	/// This class accepts connections and spawns AuthenticationServer threads.
	/// </summary>
	public class ListenServer
	{
		private IList<Server> servers;

		//private List<Player> players;

		private static PlayerDatabase players;

		private const int LISTEN_LENGTH = 10000;

		public void CloseServer(Server server)
		{
			servers.Remove(server);
			Console.WriteLine("Removed server. {0} remaining servers.", servers.Count);
		}

		public const int MAX_PLAYERS = 8;

		public ListenServer()
		{
			servers = new List<Server>();
			players = PlayerDatabase.Instance;
			players.CreateTestDatabase();
			players.SaveDatabase();
		}

		public void Run()
		{
			String ip = "127.0.0.1";
			//String ip = "128.220.251.35";
			//String ip = "128.220.70.65";
			IPAddress ipAddress = IPAddress.Parse(ip);
			IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 11000);
			try
			{
				TcpListener listener = new TcpListener(ipLocalEndPoint);
				listener.Start();

				while (true)
				{
					while (servers.Count < MAX_PLAYERS)
					{
						Console.WriteLine("Waiting for connections...");
						//TcpClient client = listener.AcceptTcpClient();
						var task = listener.AcceptTcpClientAsync();
						
						if (!task.Wait(LISTEN_LENGTH))
							continue;

						TcpClient client = task.Result;

						Server server = new Server(client, players, new CloseServerDelegate(CloseServer));
						servers.Add(server);
						//int playerID = servers.IndexOf(server);
						//server.playerID = playerID;
						Console.WriteLine("{0} players connected", servers.Count);
						server.start();
					}
					//System.Threading.Thread.Sleep(1000);
				}
			}
			catch (SocketException e)
			{
				//Continue if there is a socket exception
				Console.WriteLine(e.ToString());
			}
			catch (Exception e)
			{
				//Crash if there is any other exception
				Console.WriteLine(e.ToString());
				Console.Read();
			}
		}
	}
}
