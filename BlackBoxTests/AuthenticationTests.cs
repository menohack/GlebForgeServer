using System;
using System.Net.Sockets;

namespace BlackBoxTests
{
	class AuthenticationTests
	{
		private const uint NUM_CONNECTIONS = 1;
		public static void RunTests()
		{

			for (int i = 0; i < NUM_CONNECTIONS; i++)
			{
				using (TcpClient tcp = new TcpClient())
				{
					IAsyncResult ar = tcp.BeginConnect("127.0.0.1", 11000, null, null);
					System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
					try
					{
						if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
						{
							tcp.Close();
							throw new TimeoutException();
						}

						tcp.EndConnect(ar);
					}
					catch (TimeoutException e)
					{
						Console.WriteLine("Authentication thread timed out.");
					}
					finally
					{
						wh.Close();
					}
					System.Threading.Thread.Sleep(100000);
					NetworkStream stream = tcp.GetStream();
					byte[] buffer = BitConverter.GetBytes(390458);
					stream.Write(buffer, 0, 4);
					Console.WriteLine("Wrote int");
				}
			}
		}
	}
}
