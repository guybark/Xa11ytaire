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

            // Barker: I couldn't get LabeledBy to work with a Checkbox.

            AutomationProperties.SetName(
                ShowSuggestionsButtonCheckBox,
                "Show the TalkBack suggestions button.");

            AutomationProperties.SetName(
                TurnOverOneCardCheckBox,
                "Turn over remaining cards one at a time.");

            AutomationProperties.SetName(
                IncludeRowNumberCheckBox,
                "Include the row number in the dealt card TalkBack announcement.");

            AutomationProperties.SetHelpText(
                IncludeRowNumberCheckBox,
                "Takes effect after the next game restart.");

            AutomationProperties.SetName(
                HideUICheckBox,
                "Hide all the visuals in the game.");

            ShowSuggestionsButtonCheckBox.IsChecked = this.settings.ShowSuggestionsButton;
            TurnOverOneCardCheckBox.IsChecked = this.settings.TurnOverOneCard;
            IncludeRowNumberCheckBox.IsChecked = this.settings.IncludeRowNumber;
            HideUICheckBox.IsChecked = Settings.HideUI;
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

        private void HideUICheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Settings.HideUI = (sender as CheckBox).IsChecked;
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            var service = DependencyService.Get<IXa11ytairePlatformAction>();
            service.SaveSettings(this.settings);

            await Navigation.PopModalAsync();
        }
    }
}