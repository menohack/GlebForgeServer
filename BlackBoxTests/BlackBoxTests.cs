using System.Threading;

using System.Diagnostics;

namespace BlackBoxTests
{
    /// <summary>
    /// This class runs black-box tests on the server. It is meant to simulate network functionality
    /// like multiple connections, dropped connections, and illicit connections.
    /// </summary>
    public class BlackBoxTests
    {
        /// <summary>
        /// The thread that will be running the tests.
        /// </summary>
        Thread thread = new Thread(new ThreadStart(RunAllTests));

        /// <summary>
        /// Start the tests in a new thread.
        /// </summary>
        public void Start()
        {
            thread.Start();
        }

        /// <summary>
        /// This method launches the game in FlashPlayerDebugger. Note that the file location is machine-dependent.
        /// </summary>
        private void LaunchGame()
        {
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("C:/Program Files/FlashDevelop/Tools/flexlibs/runtimes/player/11.5/win/FlashPlayerDebugger.exe", "C:/Users/James/Documents/Projects/GlebForge/bin/GlebForge.swf");
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
        }

        /// <summary>
        /// The thread start location.
        /// </summary>
        private static void RunAllTests()
        {
			RunServerTests();
        }

		private static void RunServerTests()
		{
			ServerTests.RunTests();
		}
    }
}
