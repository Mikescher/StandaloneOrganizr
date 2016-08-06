using MSHC.MVVM;
using StandaloneOrganizr.Scanner;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StandaloneOrganizr
{
	public class MainWindowViewModel : ObservableObject
	{
		public string Title => string.Format("StandaloneOrganizr v{0} ({1}){2}", App.VERSION, Path.GetFileName(App.RootPath), App.DebugMode ? " [DEBUG]" : "");

		public ICommand TrayLeftClickCommand => new RelayCommand(TrayLeftClick);
		public ICommand ExecuteCommand => new RelayCommand(Execute);
		public ICommand SearchKeyDownCommand => new RelayCommand<KeyEventArgs>(SearchKeyDown);
		public ICommand ResultsKeyDownCommand => new RelayCommand<KeyEventArgs>(ResultsKeyDown);
		public ICommand GlobalKeyDownCommand => new RelayCommand<KeyEventArgs>(GlobalKeyDown);
		public ICommand EditCommand => new RelayCommand(Edit);

		public ICommand ResetCommand => new RelayCommand(ResetDatabase);
		public ICommand HideCommand  => new RelayCommand(Hide);
		public ICommand ExitCommand  => new RelayCommand(Exit);
		public ICommand ShowAllCommand  => new RelayCommand(() => SearchText = ":all");
		public ICommand ShowNewCommand  => new RelayCommand(() => SearchText = ":new");
		public ICommand ShowEmptyCommand => new RelayCommand(() => SearchText = ":empty");
		public ICommand ShowRegexCommand => new RelayCommand(() => SearchText = "/regex/");
		public ICommand ShowNoIconCommand => new RelayCommand(() => SearchText = ":no-icon");
		public ICommand AboutCommand => new RelayCommand(ShowAbout);

		private string _searchText = "";
		public string SearchText { get {return _searchText;} set {if (_searchText != value) {_searchText = value; OnPropertyChanged(); Search(); } } }

		private readonly ObservableCollection<SearchResult> _results = new ObservableCollectionNoReset<SearchResult>();
		public ObservableCollection<SearchResult> Results => _results;

		private SearchResult _selectedResult = null;
		public SearchResult SelectedResult { get { return _selectedResult; } set { _selectedResult = value; OnPropertyChanged(); } }
		
		public Action FocusResults = () => { };
		public Action HideWindow = () => { };
		public Action ShowWindow = () => { };

		private void TrayLeftClick()
		{
			ShowWindow();
		}

		private void Search()
		{
			Results.Clear();

			App.Database
				.Find(SearchText)
				.ToList()
				.ForEach(p => Results.Add(p));

			SelectedResult = Results.FirstOrDefault();
		}

		private void Execute()
		{
			if (SelectedResult == null) return;

			SelectedResult.Program.Start(App.Database);
			//TODO HIDE
		}

		private void SearchKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Enter && SelectedResult != null)
			{
				Execute();
				HideWindow();
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Down && Results.Any())
			{
				FocusResults();
				e.Handled = true;
				return;
			}
		}

		private void ResultsKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Enter && SelectedResult != null)
			{
				Execute();
				HideWindow();
				e.Handled = true;
				return;
			}
		}

		private void GlobalKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				HideWindow();
				e.Handled = true;
				return;
			}
			else
			if (e.Key == Key.F5)
			{
				SearchText = "";
				bool missing = App.RefreshDatabase();
				if (missing) SearchText = ":new";
				e.Handled = true;
				return;
			}
		}

		private void Edit()
		{
			if (SelectedResult == null)
				return;

			var window = new LinkEditWindow(App.Database.Save, SelectedResult.Program);

			window.ShowDialog();
		}
		
		private void ResetDatabase()
		{
			if (MessageBox.Show("Really clear the whole database and all its entries?", "Reset database?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
			{
				App.Database.Clear();
				SearchText = "";
			}
		}

		private void Hide()
		{
			HideWindow();
		}

		private void Exit()
		{
			Application.Current.Shutdown(0);
		}

		private void ShowAbout()
		{
			string text = string.Format("Standalone Organizr {0}// by Mike Schwörer (2014){0}@ {1}", Environment.NewLine, App.ABOUT_URL);
			string caption = string.Format("Standalone Organizr v{0}", App.VERSION);

			MessageBox.Show(text, caption);
		}
	}
}
