using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xa11ytaire
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
        private Settings settings;

		public SettingsPage(Settings settings)
		{
			InitializeComponent();

            this.settings = settings;

            TurnOverOneCardCheckBox.IsChecked = this.settings.TurnOverOneCard;
        }

        private void TurnOverOneCardCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            this.settings.TurnOverOneCard = (sender as CheckBox).IsChecked;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            var service = DependencyService.Get<IXa11ytairePlatformAction>();
            service.SaveSettings(this.settings);

            await Navigation.PopModalAsync();
        }
    }
}