using System;
using System.Diagnostics;
using MobileManager.Logging.Logger;
using ToolBox.Bridge;
using ToolBox.Notification;
using ToolBox.Platform;

namespace MobileManager.Services
{
    /// <summary>
    /// External processes.
    /// </summary>
    public static class ExternalProcesses
    {
        private static INotificationSystem NotificationSystem { get; }

        private static IBridgeSystem BridgeSystem { get; }

        private static ShellConfigurator Shell { get; }

        private static readonly ManagerLogger ManagerLogger = new ManagerLogger();

        static ExternalProcesses()
        {
            NotificationSystem = ToolBox.Notification.NotificationSystem.Default;
            switch (OS.GetCurrent())
            {
                case "win":
                    BridgeSystem = ToolBox.Bridge.BridgeSystem.Bat;
                    break;
                case "mac":
                case "gnu":
                    BridgeSystem = ToolBox.Bridge.BridgeSystem.Bash;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Shell = new ShellConfigurator(BridgeSystem, NotificationSystem);
        }

        /// <summary>
        /// Runs the process and read output.
        /// </summary>
        /// <returns>The process and read output.</returns>
        /// <param name="processName">Process name.</param>
        /// <param name="processArgs">Process arguments.</param>
        /// <param name="timeout">Timeout.</param>
        public static string RunProcessAndReadOutput(string processName, string processArgs, int timeout = 5000)
        {
            ManagerLogger.Debug(string.Format("RunProcessAndReadOutput processName: [{0}] args: [{1}]", processName,
                processArgs));

            var psi = new ProcessStartInfo()
            {
                FileName = processName,
                Arguments = processArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };

            proc.Start();

            proc.WaitForExit(timeout);

            var output = proc.StandardOutput.ReadToEnd();
            ManagerLogger.Debug(string.Format("RunProcessAndReadOutput output: [{0}]", string.Join("\n", output)));

            var errorOutput = proc.StandardError.ReadToEnd();
            ManagerLogger.Debug(string.Format("RunProcessAndReadOutput errorOutput: [{0}]",
                string.Join("\n", errorOutput)));

            return output + errorOutput;
        }

        /// <summary>
        /// Runs the shell process.
        /// </summary>
        /// <returns>The shell process.</returns>
        /// <param name="processName">Process name.</param>
        /// <param name="processArgs">Process arguments.</param>
        /// <param name="timeout">Timeout.</param>
        public static string RunShellProcess(string processName, string processArgs, int timeout = 5000)
        {
            ManagerLogger.Debug(string.Format("RunProcessAndReadOutput processName: [{0}] args: [{1}]", processName,
                processArgs));

            var psi = new ProcessStartInfo()
            {
                FileName = processName,
                Arguments = processArgs,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };
            var proc = new Process
            {
                StartInfo = psi
            };

            proc.Start();

            proc.WaitForExit(timeout);
            return string.Empty;
        }

        /// <summary>
        /// Runs the process in background.
        /// </summary>
        /// <returns>The process in background.</returns>
        /// <param name="processName">Process name.</param>
        /// <param name="processArgs">Process arguments.</param>
        public static int RunProcessInBackground(string processName, string processArgs)
        {
            var proc = $"{processName} {processArgs}";
            var response = Shell.Term(proc, Output.External);

            return response.code;
        }

        /// <summary>
        /// Ises the process in background running.
        /// </summary>
        /// <returns><c>true</c>, if process in background running was ised, <c>false</c> otherwise.</returns>
        /// <param name="processId">Process identifier.</param>
        public static bool IsProcessInBackgroundRunning(int processId)
        {
            return !Process.GetProcessById(processId).HasExited;
        }

        /// <summary>
        /// Stops the process running in background.
        /// </summary>
        /// <param name="containsStringInName">Contains string in name.</param>
        public static void StopProcessRunningInBackground(string containsStringInName)
        {
            var result = RunProcessAndReadOutput("/bin/bash", $"close.sh {containsStringInName}");
            ManagerLogger.Debug($"StopProcessRunningInBackground output: [{result}]");
        }
    }
}