using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StandaloneOrganizr
{
	public partial class MainWindow
	{
		private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

		public MainWindow()
		{
			InitializeComponent();
		}

		public void Init()
		{
			DataContext = _viewModel;

			_viewModel.FocusResults = FocusResults;
			_viewModel.ShowWindow = ShowWindow;
			_viewModel.HideWindow = HideWindow;
		}

		private void FocusResults()
		{
			Resultlist.SelectedIndex = 0;
			
			var listBoxItem = (ListBoxItem)Resultlist
				.ItemContainerGenerator
				.ContainerFromItem(Resultlist.SelectedItem);
			
			listBoxItem.Focus();
			Keyboard.Focus(listBoxItem);
		}

		private void Reshow()
		{
			if (App.FirstWindowShow)
			{
				foreach (var rem in App.InitialScanRemoved)
				{
					var result = MessageBox.Show(
						"Program " + rem.Name + " was removed.\r\nDo you want to delete it from the database ?",
						"Program removed",
						MessageBoxButton.YesNoCancel,
						MessageBoxImage.Question);

					if (result == MessageBoxResult.Yes) App.Database.Remove(rem);
				}

				if (App.InitialScanMissing.Any())
				{
					_viewModel.SearchText = ":new";
				}
				else if (App.Database.List().Any(p => p.Keywords.Count == 0))
				{
					_viewModel.SearchText = ":empty";
				}
			}

			_viewModel.SearchText = string.Empty;
			Searchbox.Focus();
			Keyboard.Focus(Searchbox);
			Activate();
			new Thread(() => { Thread.Sleep(350); Application.Current.Dispatcher.Invoke(() => { Activate(); }); }).Start();

			App.FirstWindowShow = false;
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		private void HideWindow()
		{
			Hide();
		}

		public void ShowWindow()
		{
			Show();
			
			RecenterWindow();

			Reshow();
		}

		private void RecenterWindow()
		{
			var w = SystemParameters.PrimaryScreenWidth;
			var h = SystemParameters.PrimaryScreenHeight;

			Left = (w - ActualWidth) / 2;
			Top = (h - ActualHeight) / 2;
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