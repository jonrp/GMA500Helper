using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GMA500Helper.Tools {
    public class ConsoleProcessor {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        public static bool IsConsoleAvailable { get { return GetConsoleWindow() != IntPtr.Zero; } }

        private class Command {
            public string Usage { get; private set; }
            public Action<string[]> Execute { get; private set; }

            public Command(string usage, Action<string[]> execute) {
                Usage = usage;
                Execute = execute;
            }
        }

        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();
        
        public void Register(string command, string[] args, Action<string[]> execute) {
            command = command.ToLowerInvariant();
            if (command != "exit") {
                commands[command] = new Command(command + (args.Length > 0 ? " " + string.Join(" ", args.Select(a => "<" + a.ToLowerInvariant() + ">")) : ""), execute);
            }
        }

        public void Run() {
            while (true) {
                Console.WriteLine("> Available commands:");
                Console.WriteLine(">   exit");
                foreach (var cmd in commands.Values) {
                    Console.WriteLine(">   " + cmd.Usage);
                }

                string[] tokens = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length > 0) {
                    tokens[0] = tokens[0].ToLowerInvariant();
                    if (tokens[0] == "exit") {
                        if (tokens.Length > 1) {
                            Console.WriteLine("> Invalid arguments, usage: exit");
                            continue;
                        }
                        break;
                    }
                    Command command;
                    if (!commands.TryGetValue(tokens[0], out command)) {
                        Console.WriteLine("> Unrecognized command");
                        continue;
                    }
                    try {
                        command.Execute(tokens);
                    } catch {
                        Console.WriteLine("> Invalid arguments, usage: " + command.Usage);
                    }
                }
            }
        }
    }
}
