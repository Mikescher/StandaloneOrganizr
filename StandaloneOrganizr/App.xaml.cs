using MSHC.Helper;
using StandaloneOrganizr.Scanner;
using StandaloneOrganizr.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace StandaloneOrganizr
{
	public partial class App
	{
		public const string FN_SETTINGS_DB = ".organizr";
		public const string FN_SETTINGS_PRIORITIES = ".organizr_ratings";
		public const string ABOUT_URL = "http://www.mikescher.com";

		public static readonly string VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

		public static string RootPath;
		public static string DatabasePath;
		public static string PrioritiesPath;
		public static bool DebugMode;

		public static FileSystemScanner Scanner;
		public static ProgramDatabase Database;

		public static List<ProgramLink> InitialScanRemoved;
		public static List<string> InitialScanMissing;

		public static bool FirstWindowShow = true;

		public App()
		{
			var args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

			RootPath = args.GetStringDefault("folder", Path.GetFullPath("."));
			DebugMode = args.IsSet("debug");
			DatabasePath = Path.Combine(RootPath, FN_SETTINGS_DB);
			PrioritiesPath = Path.Combine(RootPath, FN_SETTINGS_PRIORITIES);

			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			try
			{
				Database = new ProgramDatabase(DatabasePath, PrioritiesPath);
				Scanner = new FileSystemScanner(RootPath);

				Database.TryLoad(Scanner);

				Scanner.Scan(Database, out InitialScanRemoved, out InitialScanMissing);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), e.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
				
				Shutdown(-1);
			}

			MainWindow mw = new MainWindow();
			mw.Init();

			InterceptKeys.OnHotkey = mw.ShowWindow;

			InterceptKeys.Start();
		}
		
		private void App_OnExit(object sender, ExitEventArgs e)
		{
			InterceptKeys.Stop();
		}

		public static bool RefreshDatabase()
		{
			List<ProgramLink> tmpRemoved;
			List<string> tmpMissing;

			Scanner = new FileSystemScanner(RootPath);
			Scanner.Scan(Database, out tmpRemoved, out tmpMissing);
			
			foreach (var rem in tmpRemoved)
			{
				var result = MessageBox.Show(
					"Program " + rem.Name + " was removed.\r\nDo you want to delete it from the database ?",
					"Program removed",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question);

				if (result == MessageBoxResult.Yes) Database.Remove(rem);
			}

			return tmpMissing.Any();
		}
	}
}
