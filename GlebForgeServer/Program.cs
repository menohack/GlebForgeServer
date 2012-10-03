
namespace GlebForgeServer
{
	class Program
	{
		static void Main(string[] args)
		{
			AuthenticationServer authServer = new AuthenticationServer();
			authServer.run();
		}
	}
}
