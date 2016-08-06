using MSHC.MVVM;
using System.Windows.Media;

namespace StandaloneOrganizr.WPF.Converter
{
	class ResultToImageConverter : OneWayConverter<ImageSource, ImageSource>
	{
		protected override ImageSource Convert(ImageSource value, object parameter)
		{
			return value;
		}
	}
}
