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

            ShowSuggestionsButtonCheckBox.IsChecked = this.settings.ShowSuggestionsButton;
            TurnOverOneCardCheckBox.IsChecked = this.settings.TurnOverOneCard;
            IncludeRowNumberCheckBox.IsChecked = this.settings.IncludeRowNumber;
        }

        private void ShowSuggestionsButtonCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            this.settings.ShowSuggestionsButton = (sender as CheckBox).IsChecked;
        }

        private void TurnOverOneCardCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            this.settings.TurnOverOneCard = (sender as CheckBox).IsChecked;
        }

        private void IncludeRowNumberCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            this.settings.IncludeRowNumber = (sender as CheckBox).IsChecked;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            var service = DependencyService.Get<IXa11ytairePlatformAction>();
            service.SaveSettings(this.settings);

            await Navigation.PopModalAsync();
        }
    }
}