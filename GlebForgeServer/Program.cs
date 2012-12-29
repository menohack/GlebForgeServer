using System;

using BlackBoxTests;

namespace GlebForgeServer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("isLittleEndian: {0}", BitConverter.IsLittleEndian);

            BlackBoxTests.BlackBoxTests test = new BlackBoxTests.BlackBoxTests();
            test.Start();

			AuthenticationServer authServer = new AuthenticationServer();
			authServer.run();
		}
	}
}
