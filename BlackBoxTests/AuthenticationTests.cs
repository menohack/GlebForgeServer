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
					//System.Threading.Thread.Sleep(2000);
					NetworkStream stream = tcp.GetStream();
					byte[] buffer = BitConverter.GetBytes(390458);
					stream.Write(buffer, 0, 4);

					stream.Read(buffer, 0, 4);
					long value = BitConverter.ToInt32(buffer, 0);

					buffer = System.Text.Encoding.UTF8.GetBytes("James");
					stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
					stream.Write(buffer, 0, buffer.Length);

					while (true)
					{
						stream.Write(BitConverter.GetBytes(17.0f), 0, 4);
						stream.Write(BitConverter.GetBytes(13.0f), 0, 4);

						buffer = new byte[4];
						stream.Read(buffer, 0, 4);
						int numNearbyPlayers = BitConverter.ToInt32(buffer, 0);
						int numBytes = numNearbyPlayers * 8;
						
						buffer = new byte[numBytes];
						stream.Read(buffer, 0, numBytes);

						float x = BitConverter.ToSingle(buffer, 0);
						float y = BitConverter.ToSingle(buffer, 4);
						Console.WriteLine("Read ({0}, {1})", x, y);
					}
				}
			}
		}
	}
}
