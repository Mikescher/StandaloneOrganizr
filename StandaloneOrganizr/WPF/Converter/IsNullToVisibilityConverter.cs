using MSHC.MVVM;
using System.Windows;

namespace StandaloneOrganizr.WPF.Converter
{
	class IsNullToVisibilityConverter : OneWayConverter<object, Visibility>
	{
		protected override Visibility Convert(object value, object parameter)
		{
			return (value != null) ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
