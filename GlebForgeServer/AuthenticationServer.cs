using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace GlebForgeServer
{
	class AuthenticationServer
	{
		private IList<Server> servers;

		private List<Player> players;

		public const int MAX_PLAYERS = 8;

		public AuthenticationServer()
		{
			servers = new List<Server>();
			players = new List<Player>();
		}

		public void run()
		{
			String ip = "127.0.0.1";
			//String ip = "128.220.251.35";
			IPAddress ipAddress = IPAddress.Parse(ip);
			IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 11000);
			try
			{
				TcpListener listener = new TcpListener(ipLocalEndPoint);
				listener.Start();

				while (servers.Count < MAX_PLAYERS)
				{
					Console.WriteLine("Waiting for connections...");
					TcpClient client = listener.AcceptTcpClient();
					

					Server server = new Server(client, players);
					servers.Add(server);
					//int playerID = servers.IndexOf(server);
					//server.playerID = playerID;
					Console.WriteLine("Player Connected!");
					server.start();
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Read();
			}
		}
	}
}
