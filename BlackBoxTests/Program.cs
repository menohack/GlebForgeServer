using GlebForgeServer;

namespace BlackBoxTests
{
	class Program
	{
		static void Main(string[] args)
		{
            BlackBoxTests test = new BlackBoxTests();
            test.Start();

			ListenServer listenServer = new ListenServer();
			listenServer.Run();
		}
	}
}
