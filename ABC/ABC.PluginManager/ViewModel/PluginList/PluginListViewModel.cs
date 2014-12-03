using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PluginManager.Model;
using PluginManager.ViewModel.PluginDetails;
using PluginManager.ViewModel.PluginList.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginList
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginListViewModel
	{
		[NotifyProperty( Binding.Properties.ApplicationDatails )]
		public PluginDetailsViewModel ApplicationDatails { get; set; }

		[NotifyProperty( Binding.Properties.SelectedConfigurationItem )]
		public Configuration SelectedConfigurationItem { get; set; }

		[NotifyPropertyChanged( Binding.Properties.SelectedConfigurationItem )]
		public void OnSelectedConfigurationItemChanged( Configuration oldConfiguration, Configuration newConfiguration )
		{
			if ( newConfiguration != null )
			{
				ApplicationDatails.SelectedConfigurationItem = newConfiguration;
			}
		}

		public string PluginListName { get; private set; }

		[NotifyProperty( Binding.Properties.PluginList )]
		public ObservableCollection<Configuration> PluginList { get; private set; }

		public PluginListViewModel()
		{
			Initialize();
		}

		public PluginListViewModel( string name, List<Configuration> configurations, PluginDetailsViewModel pluginViewModel )
		{
			PluginListName = name;

			// Check if configurations data contains all values.
			configurations.ForEach( configuration =>
			{
				configuration.Icon = configuration.Icon ?? new Uri( "pack://application:,,,/View/icons/conf.png" ).AbsolutePath;
				configuration.Version = configuration.Version ?? "1.0";
				configuration.Author = configuration.Author ?? "Unknown author";
				configuration.SupportedVersions = configuration.SupportedVersions.Any() && configuration.SupportedVersions != null 
					? configuration.SupportedVersions : new List<string> { "Any" };
			} );

			PluginList = new ObservableCollection<Configuration>( configurations );
			ApplicationDatails = pluginViewModel;
		}

		void Initialize()
		{
			PluginList = new ObservableCollection<Configuration>();
		}

		[CommandExecute( Commands.DownloadAndOpenConfig )]
		public void DownloadAndOpenConfig()
		{
			// TODO: Possible implementation of download config plug-in control.
			//if (ApplicationDatails.SelectedConfigurationItem.ConfigFile != null)
			//{
			//	const string fileName = "configurationFile.xml";
			//	var webClient = new WebClient();
			//	webClient.DownloadFileCompleted += (sender, args) => System.Diagnostics.Process.Start(fileName);
			//	webClient.DownloadFileAsync(new Uri(ApplicationDatails.SelectedConfigurationItem.ConfigFile), fileName);
			//}
		}

		[CommandExecute( Commands.DownloadAndOpenInstaller )]
		public void DownloadAndOpenInstaller()
		{
			// TODO: Possible implementation of install method for plug-in control.
			//if (ApplicationDatails.SelectedConfigurationItem.Installer != null)
			//{
			//	const string fileName = "installerfile.zip";
			//	const string installerDir = "installer";

			//	var webClient = new WebClient();
			//	webClient.DownloadFileCompleted += (sender, args) =>
			//	{
			//		var downloadedMessageInfo = new DirectoryInfo(installerDir);
			//		foreach (var file in downloadedMessageInfo.GetFiles())
			//		{
			//			file.Delete();
			//		}
			//		foreach (var dir in downloadedMessageInfo.GetDirectories())
			//		{
			//			dir.Delete(true);
			//		}

			//		ZipFile.ExtractToDirectory(fileName, installerDir);
			//		System.Diagnostics.Process.Start(installerDir + "\\setup.exe");
			//	};

			//	webClient.DownloadFileAsync(new Uri(ApplicationDatails.SelectedConfigurationItem.Installer), fileName);
			//}
		}
	}
}