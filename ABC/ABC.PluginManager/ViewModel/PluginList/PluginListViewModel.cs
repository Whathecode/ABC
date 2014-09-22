using System.Collections.Generic;
using System.Collections.ObjectModel;
using PluginManager.Model;
using PluginManager.ViewModel.PluginDetails;
using PluginManager.ViewModel.PluginList.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginList
{
	[ViewModel(typeof (Binding.Properties), typeof (Commands))]
	public class PluginListViewModel
	{
		[NotifyProperty(Binding.Properties.ApplicationDatails)]
		public PluginDetailsViewModel ApplicationDatails { get; set; }

		[NotifyProperty(Binding.Properties.SelectedConfigurationItem)]
		public Configuration SelectedConfigurationItem { get; set; }

		[NotifyPropertyChanged(Binding.Properties.SelectedConfigurationItem)]
		public void OnSelectedConfigurationItemChanged(Configuration oldConfiguration, Configuration newConfiguration)
		{
			if (newConfiguration != null)
			{
				ApplicationDatails.SelectedConfigurationItem = newConfiguration;
			}
		}

		public string PluginListName { get; private set; }
		
		[NotifyProperty(Binding.Properties.PluginList)]
		public ObservableCollection<Configuration> PluginList { get; private set; }

		public PluginListViewModel()
		{
			Initialize();
		}

		public PluginListViewModel(string name, IEnumerable<Configuration> configurations, PluginDetailsViewModel pluginViewModel)
		{
			PluginListName = name;
			PluginList = new ObservableCollection<Configuration>(configurations);
			ApplicationDatails = pluginViewModel;
		}

		void Initialize()
		{
			PluginList = new ObservableCollection<Configuration>();
		}

		[CommandExecute(Commands.DownloadAndOpenConfig)]
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

		[CommandExecute(Commands.DownloadAndOpenInstaller)]
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