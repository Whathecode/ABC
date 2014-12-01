using System;
using System.Windows.Media;
using PluginManager.common;
using Whathecode.System.Windows.Data;


namespace PluginManager.View.AppOverview
{
	class StateToBrushConverter : AbstractValueConverter<PluginState, Brush>
	{
		public override Brush Convert( PluginState value )
		{
			switch ( value )
			{
				case PluginState.Availible:
					return new SolidColorBrush( Color.FromArgb( 255, 23, 150, 235 ) );
				case PluginState.Installed:
					return new SolidColorBrush( Color.FromArgb( 255, 25, 128, 42 ) );
				case PluginState.Updates:
					return new SolidColorBrush( Color.FromArgb( 255, 230, 224, 62 ) );
				default:
					return new SolidColorBrush( Color.FromArgb( 255, 23, 150, 235 ) );
			}
		}

		public override PluginState ConvertBack( Brush value )
		{
			throw new NotSupportedException();
		}
	}
}