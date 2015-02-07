using System;
using System.Collections.Generic;
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
	public partial class MainWindow
	{
		private const string Filename = ".organizr";
		private const string Version = "1.0.1";

		private readonly ProgramList plist = new ProgramList();

		public MainWindow()
		{
			InitializeComponent();

			try
			{
				Init();

				Title = "StandaloneOrganizr" + " v" + Version + " (" + Path.GetFileName(Path.GetFullPath(".")) + ")";
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString(), e.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Init()
		{
			if (File.Exists(Filename))
			{
				var data = File.ReadAllText(Filename, Encoding.UTF8);

				plist.Load(data);
			}

			var directories = Directory.GetDirectories(".");

			var missing = directories
				.Select(Path.GetFileName)
				.Where(p => !plist.ContainsFolder(p))
				.ToList();

			var removed = plist.Programs
				.Where(p => directories.All(q => (Path.GetFileName(q) ?? string.Empty).ToLower() != p.Directory.ToLower()))
				.ToList();

			foreach (var rem in removed)
			{
				var result = MessageBox.Show("Program " + rem.Name + " was removed.\r\nDo you want to delete it from the database ?", "Program removed", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				switch (result)
				{
					case MessageBoxResult.Yes:
						plist.Programs.Remove(rem);
						plist.Update(Filename);
						break;
					case MessageBoxResult.No:
						break;
					default:
						Environment.Exit(-1);
						return;
				}

			}

			if (missing.Any())
			{
				foreach (var miss in missing)
				{
					plist.Programs.Add(new ProgramLink()
					{
						Directory = miss,
						Name = miss,
						Keywords = new List<string>(),
						Newly = true,
					});
				}

				plist.Update(Filename);
			}

			if (missing.Any())
			{
				Searchbox.Text = ":new";
				searchbox_TextChanged(null, null);
			}
			else if (plist.Programs.Any(p => p.Keywords.Count == 0))
			{
				Searchbox.Text = ":empty";
				searchbox_TextChanged(null, null);
			}
		}

		private void searchbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (Searchbox.Text.StartsWith(":"))
			{
				SearchCommand();
			}
			else if (Searchbox.Text.StartsWith("/"))
			{
				SearchRegex();
			}
			else
			{
				SearchText();
			}

		}

		private void SearchText()
		{
			Resultlist.Items.Clear();

			var searchwords = Searchbox.Text.Split(' ', '+', ',').Select(p => p.Trim()).Where(p => p != "").ToList();

			var results = new List<SearchResult>();

			foreach (var singleresultset in searchwords.Select(p => plist.Find(p)).SelectMany(p => p))
			{
				if (results.Any(p => p.program.Directory.ToLower() == singleresultset.program.Directory.ToLower()))
				{
					results.First(p => p.program.Directory.ToLower() == singleresultset.program.Directory.ToLower()).Score += singleresultset.Score;
				}
				else
				{
					results.Add(singleresultset);
				}
			}

			foreach (var result in results.Where(p => p.Score > 0).OrderByDescending(p => p.Score))
			{
				Resultlist.Items.Add(result);
			}
		}

		private void SearchRegex()
		{
			Resultlist.Items.Clear();

			Regex regex;

			try
			{
				string rextext = Searchbox.Text.Substring(1);
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
				if (results.Any(p => p.program.Directory.ToLower() == singleresultset.program.Directory.ToLower()))
				{
					results.First(p => p.program.Directory.ToLower() == singleresultset.program.Directory.ToLower()).Score += singleresultset.Score;
				}
				else
				{
					results.Add(singleresultset);
				}
			}

			foreach (var result in results.Where(p => p.Score > 0).OrderByDescending(p => p.Score))
			{
				Resultlist.Items.Add(result);
			}
		}

		private void SearchCommand()
		{
			Resultlist.Items.Clear();

			string cmd = Searchbox.Text.Trim(':').Trim().ToLower();

			if (cmd == "e" || cmd == "empty")
			{
				foreach (var prog in plist.Programs.Where(p => p.Keywords.Count == 0))
				{
					Resultlist.Items.Add(new SearchResult(prog));
				}
			}
			else if (cmd == "a" || cmd == "all")
			{
				foreach (var prog in plist.Programs)
				{
					Resultlist.Items.Add(new SearchResult(prog));
				}
			}
			else if (cmd == "n" || cmd == "new")
			{
				foreach (var prog in plist.Programs.Where(p => p.Newly))
				{
					Resultlist.Items.Add(new SearchResult(prog));
				}
			}
		}

		private void resultlist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = Resultlist.SelectedItem as SearchResult;

			if (sel == null)
				return;

			sel.program.Start();
			Environment.Exit(0);
		}

		private void resultlist_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = Resultlist.SelectedItem as SearchResult;

			if (sel == null)
				return;

			var window = new LinkEditWindow(() =>
			{
				plist.Update(Filename);
				Resultlist.Items.Refresh();
				return 0;
			}, sel.program);

			window.ShowDialog();
		}

		private void MenuItemReset_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = "";
			searchbox_TextChanged(null, null);

			plist.Programs.Clear();
			plist.Update(Filename);
		}

		private void MenuItemInsertAll_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":all";
			searchbox_TextChanged(null, null);
		}

		private void MenuItemInsertEmpty_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":empty";
			searchbox_TextChanged(null, null);
		}

		private void MenuItemInsertNew_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = ":new";
			searchbox_TextChanged(null, null);
		}

		private void MenuItemInsertRegex_Click(object sender, RoutedEventArgs e)
		{
			Searchbox.Text = "/Regex/";
			searchbox_TextChanged(null, null);
		}

		private void Something_GotFocus(object sender, RoutedEventArgs e)
		{
			Searchbox.SelectAll();
		}

		private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Standalone Organizr " + Environment.NewLine + "// by Mike Schwörer (2014)" + Environment.NewLine + "@ http://www.mikescher.de", "Standalone Organizr v" + Version);
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void searchbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter && Resultlist.Items.Count > 0)
			{
				((SearchResult)Resultlist.Items[0]).Start();
				Environment.Exit(0);
			}
		}
	}
}