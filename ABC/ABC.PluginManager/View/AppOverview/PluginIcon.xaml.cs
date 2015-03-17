using System;
using System.Windows;
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
			//IconBackground,
			IconText
		}

		// TODO: Binding is not working, fix and use aspect property.
		//[DependencyProperty( Properties.IconBackground )]
		//public Brush IconBackground { get; set; }

		[DependencyProperty( Properties.IconText )]
		public string IconText { get; set; }

		public static SolidColorBrush AvailableColorBrush = new SolidColorBrush( Color.FromArgb( 255, 23, 150, 235 ) );
		public static SolidColorBrush InstalledColorBrush = new SolidColorBrush( Color.FromArgb( 255, 25, 128, 42 ) );
		public static SolidColorBrush UpdateColorBrush = new SolidColorBrush( Color.FromArgb( 255, 230, 224, 62 ) );

		// The dependency property which will be accessible on the UserControl
		public static readonly DependencyProperty IconBackground1Property =
			DependencyProperty.Register( "IconBackground1", typeof( Brush ), typeof( PluginIcon ) );

		public Brush IconBackground1
		{
			get { return (Brush)GetValue( IconBackground1Property ); }
			set { SetValue( IconBackground1Property, value ); }
		}

		public PluginIcon()
		{
			InitializeComponent();
		}
	}
}