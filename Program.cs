using GMA500Helper.Runners;
using GMA500Helper.Tools;

namespace GMA500Helper {
    public class Program {
        public static void Main() {
            if (ConsoleProcessor.IsConsoleAvailable) {
                ConsoleRunner.Run();
            } else {
                TrayRunner.Run();
            }
        }
    }
}
