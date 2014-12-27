using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

				return;
			}

			searchText();
		}

		private void searchText()
		{
			resultlist.Items.Clear();

			var searchwords = searchbox.Text.Split(' ').Select(p => p.Trim()).Where(p => p != "").ToList();

			if (searchwords.Count == 0)
				return;

			var results = searchwords.Select(p => plist.find(p)).Aggregate((a, b) => a.Concat(b).ToList());
			foreach (var result in results)
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
					resultlist.Items.Add(prog);
				}
			}
			else if (cmd == "a" || cmd == "all")
			{
				foreach (var prog in plist.programs)
				{
					resultlist.Items.Add(prog);
				}
			}
			else if (cmd == "n" || cmd == "new")
			{
				foreach (var prog in plist.programs.Where(p => p.newly))
				{
					resultlist.Items.Add(prog);
				}
			}
		}

		private void resultlist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = resultlist.SelectedItem as ProgramLink;

			if (sel == null)
				return;

			Process.Start("explorer.exe", Path.GetFullPath(sel.directory));
		}

		private void resultlist_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = resultlist.SelectedItem as ProgramLink;

			if (sel == null)
				return;

			var window = new LinkEditWindow(() =>
			{
				plist.Update(FILENAME);
				resultlist.Items.Refresh();
				return 0;
			}, sel);

			window.ShowDialog();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			searchbox.Text = "";
			searchbox_TextChanged(null, null);

			plist.programs.Clear();
			plist.Update(FILENAME);
		}

		private void Something_GotFocus(object sender, RoutedEventArgs e)
		{
			searchbox.SelectAll();
		}
	}
}