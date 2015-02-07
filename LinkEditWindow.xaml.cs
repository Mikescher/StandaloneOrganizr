using System;
using System.Linq;
using System.Windows;

namespace StandaloneOrganizr
{
	/// <summary>
	/// Interaction logic for LinkEditWindow.xaml
	/// </summary>
	public partial class LinkEditWindow
	{
		private readonly ProgramLink link;
		private readonly Func<int> update;

		public LinkEditWindow(Func<int> u, ProgramLink p)
		{
			link = p;
			update = u;

			InitializeComponent();

			edName.Text = link.Name;
			edDirectory.Text = link.Directory;
			edKeywords.Text = string.Join(Environment.NewLine, link.Keywords);
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			link.Name = edName.Text.Replace(":", "_").Replace("\"", "_");
			link.Directory = edDirectory.Text;
			link.Keywords = edKeywords.Text.Split(new[] { Environment.NewLine, " " }, StringSplitOptions.None).ToList();

			update();

			Close();
		}

		private void btnAbort_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
