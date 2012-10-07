using System;

namespace GlebForgeServer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("isLittleEndian: {0}", BitConverter.IsLittleEndian);
			AuthenticationServer authServer = new AuthenticationServer();
			authServer.run();
		}
	}
}
