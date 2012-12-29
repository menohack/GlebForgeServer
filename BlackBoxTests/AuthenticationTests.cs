using System.Net.Sockets;

namespace BlackBoxTests
{
	class AuthenticationTests
	{
		public static void RunTests()
		{
			TcpClient client = new TcpClient();
			while (!client.Connected)
			{
				client.Connect("127.0.0.1", 11000);

			}
		}
	}
}
