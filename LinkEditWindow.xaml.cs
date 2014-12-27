using System;
using System.Linq;
using System.Windows;

namespace StandaloneOrganizr
{
	/// <summary>
	/// Interaction logic for LinkEditWindow.xaml
	/// </summary>
	public partial class LinkEditWindow : Window
	{
		private readonly ProgramLink link;
		private readonly Func<int> update;

		public LinkEditWindow(Func<int> u, ProgramLink p)
		{
			this.link = p;
			this.update = u;

			InitializeComponent();

			edName.Text = link.name;
			edDirectory.Text = link.directory;
			edKeywords.Text = string.Join(Environment.NewLine, link.keywords);
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			link.name = edName.Text;
			link.directory = edDirectory.Text;
			link.keywords = edKeywords.Text.Split(new string[] { Environment.NewLine, " " }, StringSplitOptions.None).ToList();

			update();

			Close();
		}

		private void btnAbort_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
