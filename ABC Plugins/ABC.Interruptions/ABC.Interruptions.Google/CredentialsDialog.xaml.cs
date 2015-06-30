using System.Security;
using System.Windows;


namespace ABC.Interruptions.Google
{
	/// <summary>
	/// Interaction logic for CredentialsDialog.xaml
	/// </summary>
	public partial class CredentialsDialog
	{
		public CredentialsDialog( string username = "" )
		{
			InitializeComponent();
			Username.Text = username;
		}


		void SaveClicked( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}

		void CancelClicked( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
	}
}