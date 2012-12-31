using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BlackBoxTests
{
	class ServerTests
	{
		private const uint NUM_CONNECTIONS = 10;
		private const int WAIT_BEFORE_DROP = 5000;

		public static void RunTests()
		{
			ConnectThenDrop();
			ConnectThenDrop();
		}


		/// <summary>
		/// Spawn NUM_CONNECTIONS, some of which represent players that exist in the database,
		/// some of which represent players that do not exist. The threads connect and run until
		/// WAIT_BEFORE_DROP milliseconds pass, then all of them drop unexpectedly.
		/// </summary>
		public static void ConnectThenDrop()
		{
			Thread[] threads = new Thread[NUM_CONNECTIONS];
			PlayerThread[] pt = new PlayerThread[NUM_CONNECTIONS];
			String[] names = { "James", "Gleb", "Anthony" };
			int i = 0;
			for (; i < NUM_CONNECTIONS; i++)
			{
				pt[i] = new PlayerThread(i > 2 ? names[0] : names[i],13,22, i == 0 ? 2 : 0, false);
				threads[i] = new Thread(new ThreadStart(pt[i].SingleConnection));
				threads[i].Start();
			}
			Thread.Sleep(WAIT_BEFORE_DROP);
			foreach (var p in pt)
				p.done = true;
		}
	}

	class PlayerThread
	{
		private String name;

		private float x;
		private float X
		{
			//The getter randomly moves x in a range of [-2.0,2.0]
			get
			{
				x += (float)(rand.NextDouble() * 4.0 - 2.0);

				if (x > 800.0f)
					x = 800.0f;
				else if (x < 0.0f)
					x = 0.0f;
				
				return x;
			}
			set { }
		}

		public bool done = false;

		private float delta;

		private float y;

		private static Random rand = new Random();

		private const int WAIT_MILLIS_LENGTH_MAX = 200;
		private const float WAIT_PROBABILITY = 0.001f;


		private bool networkDebug = true;

		public PlayerThread(String name, float x, float y, float delta, bool networkDebug)
		{
			this.name = name;
			this.x = x;
			this.y = y;
			this.delta = delta;
			this.networkDebug = networkDebug;
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

			TcpClient tcp = new TcpClient();
			IAsyncResult ar = tcp.BeginConnect("127.0.0.1", 11000, null, null);
			System.Threading.WaitHandle wh = ar.AsyncWaitHandle;

			if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
			{
				tcp.Close();
				throw new TimeoutException("Authentication thread timed out.");
			}

			tcp.EndConnect(ar);


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

			while (!done)
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

				//float a = BitConverter.ToSingle(buffer, 0);
				//float b = BitConverter.ToSingle(buffer, 4);
			}
		}

		private void MaybeWait()
		{
			if (rand.NextDouble() > WAIT_PROBABILITY || !networkDebug)
				return;

			int millisToWait = rand.Next(WAIT_MILLIS_LENGTH_MAX);
			Thread.Sleep(millisToWait);
			//Console.WriteLine("Sleeping for {0} millis...", millisToWait);
		}
	}
}
