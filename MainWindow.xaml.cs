using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StandaloneOrganizr
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private string RootPath;
		private string DatabasePath;

		private const string FN_SETTINGS = ".organizr";
		private const string VERSION = "1.0.4";
		private const string ABOUT_URL = "http://www.mikescher.de";

		private FileSystemScanner Scanner;
		private ProgramDatabase Database;

		public MainWindow()
		{
			InitializeComponent();

			try
			{
				Init();

				Title = string.Format("StandaloneOrganizr v{0} ({1})", VERSION, Path.GetFileName(RootPath));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), e.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Init()
		{
			var args = Environment.GetCommandLineArgs();
			RootPath = (args.Count() > 1) ? args[1] : Path.GetFullPath(".");
			DatabasePath = Path.Combine(RootPath, FN_SETTINGS);

			Database = new ProgramDatabase(DatabasePath);
			Scanner = new FileSystemScanner(RootPath);

			Database.TryLoad(Scanner);

			List<ProgramLink> removed;
			List<string> missing;

			Scanner.Scan(Database, out removed, out missing);

			foreach (var rem in removed)
			{
				var result = MessageBox.Show(
					"Program " + rem.Name + " was removed.\r\nDo you want to delete it from the database ?", 
					"Program removed", 
					MessageBoxButton.YesNoCancel, 
					MessageBoxImage.Question);

				switch (result)
				{
					case MessageBoxResult.Yes:
						Database.Remove(rem);
						break;
					default:
						break;
				}
			}

			if (missing.Any())
			{
				Searchbox.Text = ":new";
			}
			else if (Database.List().Any(p => p.Keywords.Count == 0))
			{
				Searchbox.Text = ":empty";
			}
		}

		private void searchbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			Resultlist.Items.Clear();

			Database
				.Find(Searchbox.Text)
				.ToList()
				.ForEach(p => Resultlist.Items.Add(p));

			if (Resultlist.Items.Count > 0)
				Select(((SearchResult)Resultlist.Items[0]).Program);
			else
				Select(null);
		}

		private void resultlist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = Resultlist.SelectedItem as SearchResult;

			if (sel == null)
				return;

			Start(sel.Program);
		}

		private void resultlist_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = Resultlist.SelectedItem as SearchResult;

			if (sel == null)
				return;

			var window = new LinkEditWindow(() =>
			{
				Database.Save();
				Resultlist.Items.Refresh();
				return 0;
			}, sel.Program);

			window.ShowDialog();
		}

		private void MenuItemReset_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = "";

			Database.Clear();
		}

		private void MenuItemInsertAll_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":all";
		}

		private void MenuItemInsertEmpty_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":empty";
		}

		private void MenuItemInsertNew_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":new";
		}

		private void MenuItemInsertRegex_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = "/Regex/";
		}

		private void MenuItemInsertNoIcon_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":no-icon";
		}

		private void Something_GotFocus(object sender, RoutedEventArgs e)
		{
			Searchbox.SelectAll();
		}

		private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			string text = string.Format("Standalone Organizr {0}// by Mike Schwörer (2014){0}@ {1}", Environment.NewLine, ABOUT_URL);
			string caption = string.Format("Standalone Organizr v{0}", VERSION);

			MessageBox.Show(text, caption);
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Start(ProgramLink r)
		{
			r.Start();
			Close();
		}

		private void Searchbox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && Resultlist.Items.Count > 0)
			{
				Start(((SearchResult)Resultlist.Items[0]).Program);
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Down && Resultlist.Items.Count > 0)
			{
				Resultlist.SelectedIndex = 0;

				var listBoxItem = (ListBoxItem) Resultlist
					.ItemContainerGenerator
					.ContainerFromItem(Resultlist.SelectedItem);

				listBoxItem.Focus();
				Keyboard.Focus(listBoxItem);

				e.Handled = true;
				return;
			}
		}

		private void Resultlist_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && Resultlist.SelectedItem != null)
			{
				Start(((SearchResult)Resultlist.SelectedItem).Program);
				return;
			}
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				e.Handled = true;
				Close();
				return;
			}
		}

		private void Resultlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Resultlist.SelectedItem != null)
				Select(((SearchResult) Resultlist.SelectedItem).Program);
			else
				Select(null);
		}

		private void Select(ProgramLink prog)
		{
			if (prog == null)
			{
				imgIcon.Source = null;
			}
			else
			{
				imgIcon.Source = prog.Icon;
			}
		}
	}
}

//TODO TaskList
/*

 [X]  Choose folder by cmd param
 [x]  Show Icon (+ find icons)
 [ ]  Only scan periodically + manual scan
 [x]  better pattern matching
 [x]  esc -> close
 [/]  TOML
 [x]  auto exec
 [ ]  keep alive + AltGr-Space shortcut
 [X]  chose prog with up+down
 [ ]  better skin

*/