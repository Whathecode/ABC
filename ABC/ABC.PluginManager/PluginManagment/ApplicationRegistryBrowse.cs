using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	class ApplicationRegistryBrowse
	{
		public List<Plugin> InstalledOnSystem { get; private set; }

		public static readonly string RegustryKeyToSeach1 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		public static readonly string RegustryKeyToSeach2 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

		public ApplicationRegistryBrowse()
		{
			InstalledOnSystem = new List<Plugin>();
			GetInstalledApps();
		}

		void GetInstalledApps()
		{
			// Search in LocalMachine_32.
			var registryKey = Registry.LocalMachine.OpenSubKey( RegustryKeyToSeach1 );
			GetValue( registryKey );

			// Search in CurrentUser.
			registryKey = Registry.CurrentUser.OpenSubKey( RegustryKeyToSeach1 );
			GetValue( registryKey );

			// Search in LocalMachine_64.
			registryKey = Registry.LocalMachine.OpenSubKey( RegustryKeyToSeach2 );
			GetValue( registryKey );
		}

		void GetValue( RegistryKey registryKey )
		{
			if ( registryKey == null ) return;
			foreach ( var subkeyName in registryKey.GetSubKeyNames() )
			{
				using ( var subkey = registryKey.OpenSubKey( subkeyName ) )
				{
					// Dismiss all broken keys.
					if ( subkey == null ) return;

					// Get all sub keys names.
					var subkeysValues = subkey.GetValueNames();

					// Dismiss all keys that do not have a name.
					if ( !subkeysValues.Contains( "DisplayName" ) || subkey.GetValue( "DisplayName" ).ToString() == "" ) continue;
					var plugin = new Plugin
					{
						Name = subkey.GetValue( "DisplayName" ).ToString().TrimStart()
					};
					// In each case we have to check if given key exists. Otherwise if we just call GetValue exception is thrown.
					if ( subkeysValues.Contains( "Publisher" ) )
					{
						plugin.CompanyName = subkey.GetValue( "Publisher" ).ToString();
					}
					if ( subkeysValues.Contains( "DisplayVersion" ) )
					{
						//plugin.Version2 = new Version(subkey.GetValue( "DisplayVersion" ).ToString());
					}
					if ( subkeysValues.Contains( "DisplayIcon" ) )
					{
						// TODO: Many of icons are attached to .exe file, improve this to extract icon in this case.
						var iconPath = subkey.GetValue( "DisplayIcon" ).ToString();
						if ( iconPath.EndsWith( ".ico" ) )
						{
							plugin.Icon = iconPath;
						}
					}
					if ( InstalledOnSystem.All( installed => installed.Name != plugin.Name ) )
					{
						InstalledOnSystem.Add( plugin );
					}
				}
			}
		}
	}
}