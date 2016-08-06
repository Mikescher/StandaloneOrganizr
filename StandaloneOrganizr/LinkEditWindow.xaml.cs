using System;
using System.Linq;
using System.Windows;
using StandaloneOrganizr.Scanner;

namespace StandaloneOrganizr
{
	/// <summary>
	/// Interaction logic for LinkEditWindow.xaml
	/// </summary>
	public partial class LinkEditWindow
	{
		private readonly ProgramLink link;
		private readonly Action update;

		public LinkEditWindow(Action u, ProgramLink p)
		{
			link = p;
			update = u;

			InitializeComponent();

			edName.Text = link.Name;
			edDirectory.Text = link.Directory;
			edKeywords.Text = string.Join(Environment.NewLine, link.Keywords);
			lblPriority.Text = (link.Priority < 0) ? link.Priority.ToString() : String.Format("+{0}", link.Priority);
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			link.Name = edName.Text.Replace(":", "_").Replace("\"", "_");
			link.Directory = edDirectory.Text;
			link.Keywords = edKeywords.Text.Split(new[] { Environment.NewLine, " " }, StringSplitOptions.None).ToList();
			link.IsNew = false;

			update();

			Close();
		}

		private void btnAbort_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
