using System;
using System.Windows.Media;
using PluginManager.Common;
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
					return PluginIcon.AvailableColorBrush;
				case PluginState.Installed:
					return PluginIcon.InstalledColorBrush;
				case PluginState.Updates:
					return PluginIcon.UpdateColorBrush;
				default:
					return PluginIcon.AvailableColorBrush;
			}
		}

		public override PluginState ConvertBack( Brush value )
		{
			throw new NotSupportedException();
		}
	}
}