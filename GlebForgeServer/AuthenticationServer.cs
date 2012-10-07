using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace GlebForgeServer
{
	class AuthenticationServer
	{
		private Server[] servers;

		public PlayerData[] players;

		public const int MAX_PLAYERS = 8;

		public AuthenticationServer()
		{
			servers = new Server[8];
			players = new PlayerData[2];
		}

		public void run()
		{
			//String ip = "127.0.0.1";
			String ip = "128.220.251.35";
			IPAddress ipAddress = IPAddress.Parse(ip);
			IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 11000);
			try
			{
				TcpListener server = new TcpListener(ipLocalEndPoint);
				server.Start();
				int numPlayers = 0;
				while (numPlayers < MAX_PLAYERS)
				{
					Console.WriteLine("Waiting for connections...");
					TcpClient client = server.AcceptTcpClient();
					Console.WriteLine("Player {0} Connected!", numPlayers);

					servers[numPlayers] = new Server(client, players, numPlayers);
					servers[numPlayers].start();
					numPlayers++;
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
