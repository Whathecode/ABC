using System;
using System.Windows;
using PluginManager.common;
using Whathecode.System.Windows.Data;


namespace PluginManager.View.PluginList
{
	class InstallButtonVisibilityConverter : AbstractMultiValueConverter<object, Visibility>
	{
		public override Visibility Convert( object[] value )
		{
			int id1, id2;
			PluginState state;

			if ( !int.TryParse( value[ 0 ].ToString(), out id1 ) )
				return Visibility.Collapsed;
			if ( !int.TryParse( value[ 1 ].ToString(), out id2 ) )
				return Visibility.Collapsed;
			if ( !Enum.TryParse( value[ 2 ].ToString(), out state ) )
				return Visibility.Collapsed;

			return id1 == id2 && state == PluginState.Availible ? Visibility.Visible : Visibility.Collapsed;
		}

		public override object[] ConvertBack( Visibility value )
		{
			throw new NotImplementedException();
		}
	}
}