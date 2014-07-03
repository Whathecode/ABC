using System;
using Newtonsoft.Json;


namespace ABC.Applications.Chrome
{
	[Serializable]
	public struct ChromePersistedWindow
	{
		[JsonProperty( PropertyName = "openTabs" )]
		public ChromePersistedTab[] OpenTabs;
	}

	[Serializable]
	public struct ChromePersistedTab
	{
		[JsonProperty( PropertyName = "url" )]
		public string Url;

		[JsonProperty( PropertyName = "isPinned" )]
		public bool IsPinned;
	}
}
