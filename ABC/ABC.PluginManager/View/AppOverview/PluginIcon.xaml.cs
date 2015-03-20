using System;
using System.Windows.Media;
using Whathecode.System.Windows.DependencyPropertyFactory.Aspects;
using Whathecode.System.Windows.DependencyPropertyFactory.Attributes;


namespace PluginManager.View.AppOverview
{
	/// <summary>
	/// Interaction logic for PluginIcon.xaml
	/// </summary>
	[WpfControl( typeof( Properties ) )]
	public partial class PluginIcon
	{
		[Flags]
		public enum Properties
		{
			IconBackground,
			IconText
		}

		[DependencyProperty( Properties.IconBackground )]
		public Brush IconBackground { get; set; }

		[DependencyProperty( Properties.IconText )]
		public string IconText { get; set; }


		public PluginIcon()
		{
			InitializeComponent();
		}
	}
}