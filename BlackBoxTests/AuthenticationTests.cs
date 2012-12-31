using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BlackBoxTests
{
	class AuthenticationTests
	{
		private const uint NUM_CONNECTIONS = 10;

		public static void RunTests()
		{
			Thread[] threads = new Thread[NUM_CONNECTIONS];
			String[] names = { "James", "Gleb", "Anthony" };
			int i = 0;
			for (; i < NUM_CONNECTIONS; i++)
			{
				PlayerThread pt = new PlayerThread(i > 2 ? names[0] : names[i],13,22, i == 0 ? 2 : 0, i == 0 ? true : false);
				threads[i] = new Thread(new ThreadStart(pt.SingleConnection));
				threads[i].Start();
				Thread.Sleep(500);
			}	
		}
	}

	class PlayerThread
	{
		private String name;

		private float x;
		private float X {
			get { if (x > 200 || x < 0) delta *= -1.0f; x += delta; return x; }
			set { }
		}

		private float delta;

		private float y;

		private bool kill;

		private static Random rand = new Random();

		private const int WAIT_MILLIS_LENGTH_MAX = 200;
		private const float WAIT_PROBABILITY = 0.001f;


		private const bool NETWORK_DEBUG = true;

		public PlayerThread(String name, float x, float y, float delta, bool kill)
		{
			this.name = name;
			this.x = x;
			this.y = y;
			this.delta = delta;
			this.kill = kill;
		}

		public void SingleConnection()
		{
			try
			{
				Run();
			}
			catch (IOException e)
			{
				if (e.InnerException.GetType() == typeof(SocketException))
					Console.WriteLine("Testing thread {0} threw exception: " + e.Message, name);
				else
					throw e;
			}
		}
		private void Run()
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

				MaybeWait();

				//System.Threading.Thread.Sleep(2000);
				NetworkStream stream = tcp.GetStream();
				byte[] buffer = BitConverter.GetBytes(390458);
				stream.Write(buffer, 0, 4);

				MaybeWait();

				stream.Read(buffer, 0, 4);
				long value = BitConverter.ToInt32(buffer, 0);

				MaybeWait();

				buffer = System.Text.Encoding.UTF8.GetBytes(name);
				stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
				MaybeWait();
				stream.Write(buffer, 0, buffer.Length);

				MaybeWait();

				while (true)
				{
					stream.Write(BitConverter.GetBytes(X), 0, 4);
					MaybeWait();
					stream.Write(BitConverter.GetBytes(y), 0, 4);
					MaybeWait();

					buffer = new byte[4];
					stream.Read(buffer, 0, 4);
					int numNearbyPlayers = BitConverter.ToInt32(buffer, 0);
					int numBytes = numNearbyPlayers * 8;

					MaybeWait();

					buffer = new byte[numBytes];
					stream.Read(buffer, 0, numBytes);

					MaybeWait();

					float a = BitConverter.ToSingle(buffer, 0);
					float b = BitConverter.ToSingle(buffer, 4);

					if (kill)
						return;
				}
			}
		}

		private void MaybeWait()
		{
			if (rand.NextDouble() > WAIT_PROBABILITY || !NETWORK_DEBUG)
				return;

			int millisToWait = rand.Next(WAIT_MILLIS_LENGTH_MAX);
			Thread.Sleep(millisToWait);
			//Console.WriteLine("Sleeping for {0} millis...", millisToWait);
		}
	}
}
