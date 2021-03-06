﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Management;

namespace OriginSteamOverlayLauncher
{
    class Program
    {
        #region Imports
        // for custom modal support
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string msg, string caption, int type);

        // for BringToFront() support
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_MINIMIZE = 2;
        public const int SW_SHOW = 5;

        public static string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        public static string appName = Path.GetFileNameWithoutExtension(codeBase);
        #endregion

        [STAThread]
        private static void Main(string[] args)
        {
            // get our current mutex id based off our AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            // simple global mutex, courtesy of: https://stackoverflow.com/a/1213517
            using (var mutex = new Mutex(false, mutexId))
            {
                try
                {
                    try
                    {
                        if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                            Environment.Exit(0);
                    }
                    catch (AbandonedMutexException)
                    {
                        Logger("MUTEX", "Mutex is held by another instance, but seems abandoned!");
                        Environment.Exit(0);
                    }

                    /*
                     * Run our actual entry point here...
                     */

                    if (CliArgExists(args, "help"))
                    {// display an INI settings overview if run with /help
                        DisplayHelpDialog();
                    }
                    else
                    {
                        ProcessTracking procTrack = new ProcessTracking();
                        Settings curSet = new Settings();
                        // path to our local config
                        IniFile iniFile = new IniFile(appName + ".ini");

                        // overwrite/create log upon startup
                        File.WriteAllText(appName + "_Log.txt", String.Empty);
                        Logger("NOTE", "OSOL is running as: " + appName);

                        if (Settings.CheckINI(iniFile)
                            && Settings.ValidateINI(curSet, iniFile, iniFile.Path))
                        {
                            procTrack.ProcessLauncher(curSet, iniFile); // normal functionality
                        }
                        else
                        {// ini doesn't match our comparison, recreate from stubs
                            Logger("WARNING", "Config file partially invalid or doesn't exist, re-stubbing...");
                            Settings.CreateINI(curSet, iniFile);
                            Settings.ValidateINI(curSet, iniFile, iniFile.Path);
                            Settings.PathChooser(curSet, iniFile);
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                    Environment.Exit(0);
                }
            }
        }

        #region ProcessHelpers
        private static bool CliArgExists(string[] args, string ArgName)
        {// courtesy of: https://stackoverflow.com/a/30569947
            var singleFound = args.Where(w => w.ToLower() == "/" + ArgName.ToLower()).FirstOrDefault();
            if (singleFound != null)
                return ArgName.Equals(ArgName.ToLower());
            else
                return false;
        }

        public static void Logger(String cause, String message)
        {
            using (StreamWriter stream = File.AppendText(appName + "_Log.txt"))
            {
                stream.Write("[{0}] [{1}] {2}\r\n", DateTime.Now.ToUniversalTime(), cause, message);
            }
        }

        public static void BringToFront(IntPtr wHnd)
        {// force the window handle owner to restore and activate to focus
            ShowWindowAsync(wHnd, SW_SHOWDEFAULT);
            ShowWindowAsync(wHnd, SW_SHOW);
            SetForegroundWindow(wHnd);
        }

        public static void MinimizeWindow(IntPtr wHnd)
        {// force the window handle to minimize
            ShowWindowAsync(wHnd, SW_MINIMIZE);
        }

        public static bool IsRunning(String name) { return Process.GetProcessesByName(name).Any(); }

        public static bool IsRunningPID(Int64 pid) { return Process.GetProcesses().Any(x => x.Id == pid); }

        public static int ValidateProcTree(Process[] procTree, int timeout)
        {
            var procChildren = procTree.Count();
            Thread.Sleep(timeout * 1000); // let process stabilize before gathering data

            if (procChildren > 1)
            {// our parent is likely a caller or proxy
                for (int i = 0; i < procChildren - 1; i++)
                {// iterate through each process in the tree and determine which process we should bind to
                    var proc = procTree[i];

                    if (proc.Id > 0 && !proc.HasExited)
                    {// return the first PID with an hwnd
                        if (proc.MainWindowHandle != IntPtr.Zero && proc.MainWindowTitle.Length > 0)
                        {// probably a real process (launcher or game) because it has an hwnd and title
                            return proc.Id;
                        }
                        else if (procChildren > 2 && proc.MainWindowHandle == IntPtr.Zero && !procTree[0].HasExited)
                        {// probably a headless process due to having more than one child, return the PID of the parent
                            return procTree[0].Id;
                        }
                    }
                }
            }
            else if (procChildren != 0 && !procTree[0].HasExited)
                return procTree[0].Id; // no children, just return the PID

            return 0;
        }

        public static Process[] GetProcessTreeByName(String procName)
        {
            return Process.GetProcessesByName(procName);
        }

        public static int GetRunningPIDByName(String procName)
        {
            Process tmpProc = Process.GetProcessesByName(procName).FirstOrDefault();
            if (tmpProc != null)
                return tmpProc.Id;
            else
                return 0;
        }

        public static Process RebindProcessByID(int PID)
        {
            return Process.GetProcessById(PID);
        }

        public static void KillProcTreeByName(String procName)
        {
            Process[] foundProcs = Process.GetProcessesByName(procName);
            foreach (Process proc in foundProcs)
            {
                proc.Kill();
            }
        }

        public static string GetCommandLineToString(Process process, String startPath)
        { // credit to: https://stackoverflow.com/a/40501117
            String cmdLine = String.Empty;
            String _parsedPath = String.Empty;

            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {// use WMI to grab the CommandLine string by looking up the PID
                    var matchEnum = searcher.Get().GetEnumerator();

                    // include a space to clean up the output parsed args
                    if (startPath.Contains(" "))
                        _parsedPath = String.Format("\"{0}\" ", startPath);
                    else
                        _parsedPath = String.Format("{0} ", startPath);
                    

                    if (matchEnum.MoveNext())
                    {// this will always return at most 1 result
                        cmdLine = matchEnum.Current["CommandLine"]?.ToString();
                    }
                }

                if (cmdLine == null)
                {
                    // Not having found a command line implies 1 of 2 exceptions, which the
                    // WMI query masked:
                    // An "Access denied" exception due to lack of privileges.
                    // A "Cannot process request because the process (<pid>) has exited."
                    // exception due to the process having terminated.
                    // We provoke the same exception again simply by accessing process.MainModule.
                    var dummy = process.MainModule; // Provoke exception.
                }
            }
            // Catch and ignore "access denied" exceptions.
            catch (Win32Exception ex) when (ex.HResult == -2147467259) { }
            // Catch and ignore "Cannot process request because the process (<pid>) has
            // exited." exceptions.
            // These can happen if a process was initially included in 
            // Process.GetProcesses(), but has terminated before it can be
            // examined below.
            catch (InvalidOperationException ex) when (ex.HResult == -2146233079) { }

            // remove the full path from our parsed arguments
            return RemoveInPlace(cmdLine, _parsedPath);
        }

        public static void ExecuteExternalElevated(String filePath, String fileArgs)
        {// generic process delegate for executing pre-launcher/post-game
            try
            {
                Process execProc = new Process();

                // sanity check our future process path first
                if (Settings.ValidatePath(filePath))
                {
                    execProc.StartInfo.UseShellExecute = true;
                    execProc.StartInfo.FileName = filePath;
                    execProc.StartInfo.Arguments = fileArgs;
                    execProc.StartInfo.Verb = "runas"; // ask the user for contextual UAC privs in case they need elevation
                    Logger("OSOL", "Attempting to run external process: " + filePath + " " + fileArgs);
                    execProc.Start();
                    execProc.WaitForExit(); // idle waiting for outside process to return
                    Logger("OSOL", "External process delegate returned, continuing...");
                }
                else if (filePath != null && filePath.Length > 0)
                {
                    Logger("WARNING", "External process path is invalid: " + filePath + " " + fileArgs);
                }
            }
            catch (Exception e)
            {
                Logger("WARNING", "Process delegate failed on [" + filePath + " " + fileArgs + "], due to: " + e.ToString());
            }
        }

        public static void DisplayHelpDialog()
        {
            Form helpForm = new HelpForm();
            helpForm.ShowDialog();

            if (helpForm.DialogResult == DialogResult.OK || helpForm.DialogResult == DialogResult.Cancel)
            {
                Process.GetCurrentProcess().Kill(); // exit the assembly after the modal
            }
        }

        public static bool OrdinalContains(String match, String container, StringComparison _comp = StringComparison.InvariantCultureIgnoreCase)
        {// if container string contains match string, via valid index, then true
            if (container.IndexOf(match, _comp) >= 0)
                return true;

            return false;
        }

        private static string RemoveInPlace(String input, String match)
        {
            if (OrdinalContains(match, input))
            {
                string _result = input.Replace(match, String.Empty);
                return _result;
            }

            return String.Empty;
        }

        public static void StoreCommandline(Settings setHnd, IniFile iniHnd, String cmdLine)
        {// save the passed commandline string to our ini for later
            if (cmdLine.Length > 0)
            {
                setHnd.DetectedCommandline = cmdLine;
                iniHnd.Write("DetectedCommandline", cmdLine, "Paths");
            }
        }

        public static bool CompareCommandlines(String storedCmdline, String comparatorCmdline)
        {// compared stored against active to prevent unnecessary relaunching
            if (storedCmdline.Length > 0 && comparatorCmdline.Length > 0 && Settings.StringEquals(comparatorCmdline, storedCmdline))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
