using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace StandaloneOrganizr
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string FILENAME = ".organizr";
		private const string VERSION = "1.0";

		private ProgramList plist = new ProgramList();

		public MainWindow()
		{
			InitializeComponent();

			try
			{
				Init();

				Title = "StandaloneOrganizr" + " v" + VERSION + " (" + Path.GetFileName(Path.GetFullPath(".")) + ")";
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), e.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Init()
		{
			if (File.Exists(FILENAME))
			{
				string data = File.ReadAllText(FILENAME, Encoding.UTF8);

				plist.Load(data);
			}

			var directories = Directory.GetDirectories(".");

			var missing = directories
				.Select(p => Path.GetFileName(p))
				.Where(p => !plist.ContainsFolder(p))
				.ToList();

			var removed = plist.programs
				.Where(p => !directories.Any(q => Path.GetFileName(q).ToLower() == p.directory.ToLower()))
				.ToList();

			foreach (var rem in removed)
			{
				var result = MessageBox.Show("Program " + rem.name + " was removed.\r\nDo you want to delete it from the database ?", "Program removed", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				switch (result)
				{
					case MessageBoxResult.Yes:
						plist.programs.Remove(rem);
						plist.Update(FILENAME);
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
					default:
						Environment.Exit(-1);
						return;
				}

			}

			if (missing.Count() > 0)
			{
				foreach (var miss in missing)
				{
					plist.programs.Add(new ProgramLink()
					{
						directory = miss,
						name = miss,
						keywords = new List<string>(),
						newly = true,
					});
				}

				plist.Update(FILENAME);
			}

			if (missing.Count() > 0)
			{
				searchbox.Text = ":new";
				searchbox_TextChanged(null, null);
			}
			else if (plist.programs.Where(p => p.keywords.Count == 0).Count() > 0)
			{
				searchbox.Text = ":empty";
				searchbox_TextChanged(null, null);
			}
		}

		private void searchbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (searchbox.Text.StartsWith(":"))
			{
				searchCommand();
			}
			else if (searchbox.Text.StartsWith("/"))
			{
				searchRegex();
			}
			else
			{
				searchText();
			}

		}

		private void searchText()
		{
			resultlist.Items.Clear();

			var searchwords = searchbox.Text.Split(' ', '+', ',').Select(p => p.Trim()).Where(p => p != "").ToList();

			var results = new List<SearchResult>();

			foreach (var singleresultset in searchwords.Select(p => plist.Find(p)).SelectMany(p => p))
			{
				if (results.Any(p => p.program.directory.ToLower() == singleresultset.program.directory.ToLower()))
				{
					results.First(p => p.program.directory.ToLower() == singleresultset.program.directory.ToLower()).score += singleresultset.score;
				}
				else
				{
					results.Add(singleresultset);
				}
			}

			foreach (var result in results.Where(p => p.score > 0).OrderByDescending(p => p.score))
			{
				resultlist.Items.Add(result);
			}
		}

		private void searchRegex()
		{
			resultlist.Items.Clear();

			Regex regex;

			try
			{
				string rextext = searchbox.Text.Substring(1);
				if (rextext.EndsWith("/"))
					rextext = rextext.Substring(0, rextext.Length - 1);
				rextext = "^" + rextext + "$";

				regex = new Regex(rextext, RegexOptions.IgnoreCase);
			}
			catch (ArgumentException)
			{
				return;
			}

			var results = new List<SearchResult>();

			foreach (var singleresultset in plist.Find(regex))
			{
				if (results.Any(p => p.program.directory.ToLower() == singleresultset.program.directory.ToLower()))
				{
					results.First(p => p.program.directory.ToLower() == singleresultset.program.directory.ToLower()).score += singleresultset.score;
				}
				else
				{
					results.Add(singleresultset);
				}
			}

			foreach (var result in results.Where(p => p.score > 0).OrderByDescending(p => p.score))
			{
				resultlist.Items.Add(result);
			}
		}

		private void searchCommand()
		{
			resultlist.Items.Clear();

			string cmd = searchbox.Text.Trim(':').Trim().ToLower();

			if (cmd == "e" || cmd == "empty")
			{
				foreach (var prog in plist.programs.Where(p => p.keywords.Count == 0))
				{
					resultlist.Items.Add(new SearchResult(prog));
				}
			}
			else if (cmd == "a" || cmd == "all")
			{
				foreach (var prog in plist.programs)
				{
					resultlist.Items.Add(new SearchResult(prog));
				}
			}
			else if (cmd == "n" || cmd == "new")
			{
				foreach (var prog in plist.programs.Where(p => p.newly))
				{
					resultlist.Items.Add(new SearchResult(prog));
				}
			}
		}

		private void resultlist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = resultlist.SelectedItem as SearchResult;

			if (sel == null)
				return;

			Process.Start("explorer.exe", Path.GetFullPath(sel.program.directory));
		}

		private void resultlist_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = resultlist.SelectedItem as SearchResult;

			if (sel == null)
				return;

			var window = new LinkEditWindow(() =>
			{
				plist.Update(FILENAME);
				resultlist.Items.Refresh();
				return 0;
			}, sel.program);

			window.ShowDialog();
		}

		private void MenuItemReset_Click(object sender, RoutedEventArgs e)
		{
			searchbox.Text = "";
			searchbox_TextChanged(null, null);

			plist.programs.Clear();
			plist.Update(FILENAME);
		}

		private void MenuItemInsertAll_Click(object sender, RoutedEventArgs e)
		{
			searchbox.Text = ":all";
			searchbox_TextChanged(null, null);
		}

		private void MenuItemInsertEmpty_Click(object sender, RoutedEventArgs e)
		{
			searchbox.Text = ":empty";
			searchbox_TextChanged(null, null);
		}

		private void MenuItemInsertNew_Click(object sender, RoutedEventArgs e)
		{
			searchbox.Text = ":new";
			searchbox_TextChanged(null, null);
		}

		private void MenuItemInsertRegex_Click(object sender, RoutedEventArgs e)
		{
			searchbox.Text = "/Regex/";
			searchbox_TextChanged(null, null);
		}

		private void Something_GotFocus(object sender, RoutedEventArgs e)
		{
			searchbox.SelectAll();
		}

		private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Standalone Organizr " + Environment.NewLine + "// by Mike Schwörer (2014)" + Environment.NewLine + "@ http://www.mikescher.de", "Standalone Organizr v" + VERSION);
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}