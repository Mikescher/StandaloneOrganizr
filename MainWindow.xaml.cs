using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace StandaloneOrganizr
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string FILENAME = "sao.mson";

		private ProgramList plist = new ProgramList();

		public MainWindow()
		{
			InitializeComponent();

			try
			{
				Init();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, e.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Init() //TODO Cache IO Errors
		{
			if (File.Exists(FILENAME))
			{
				string data = File.ReadAllText(FILENAME);

				plist.Load(data); //TODO Cache IO Errors
			}
			else
			{
				File.Create(FILENAME);
			}

			var directories = Directory.GetDirectories(".");

			var missing = directories
				.Select(p => Path.GetFileName(p))
				.Where(p => !plist.ContainsFolder(p))
				.ToList();

			var removed = plist.programs
				.Where(p => !directories.Any(q => Path.GetFileName(q).ToLower() == p.directory.ToLower()))
				.ToList();

			// TODO SHOW removed and missing 

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
			resultlist.Items.Clear();

			if (searchbox.Text.StartsWith(":"))
			{
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


				return;
			}

			var results = plist.find(searchbox.Text);
			foreach (var result in results)
			{
				resultlist.Items.Add(result);
			}
		}

		private void resultlist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var sel = resultlist.SelectedItem as ProgramLink;

			if (sel == null)
				return;

			Process.Start("explorer.exe", Path.GetFullPath(sel.directory));
		}
	}
}