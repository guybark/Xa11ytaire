﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xa11ytaire.Source;
using Xa11ytaire.Source.Classes;
using Xa11ytaire.Source.ViewModels;
//using Plugin.LocalNotifications;

// Barker: Learn about the Switch control.

namespace Xa11ytaire
{
    public interface IXa11ytairePlatformAction
    {
        Settings LoadSettings();

        void SaveSettings(Settings settings);

        void ScreenReaderAnnouncement(string notification);
    }

    public enum CardState
    {
        KingPlaceHolder,
        FaceDown,
        FaceUp
    }

    public enum Suit
    {
        NoSuit,
        Clubs,
        Diamonds,
        Hearts,
        Spades,
    }

    public partial class MainPage : ContentPage
    {
        public PlayingCardViewModel ViewModel { get; set; }

        private int cCardPiles = 7;
        private Shuffler _shuffler;
        private List<Card> _deckRemaining = new List<Card>();
        private List<Card> _deckUpturned = new List<Card>();

        private int cTargetPiles = 4;
        private List<Card>[] _targetPiles = new List<Card>[4];

        private Settings settings;

        private bool firstAppearing = true;

        public MainPage()
        {
            InitializeComponent();

            var service = DependencyService.Get<IXa11ytairePlatformAction>();
            this.settings = service.LoadSettings();

            this.ViewModel = new PlayingCardViewModel();

            for (int i = 0; i < cTargetPiles; ++i)
            {
                _targetPiles[i] = new List<Card>();
            }

            SuggestionButton.IsVisible = settings.ShowSuggestionsButton;

            Grid.SetRowSpan(CardPile1,
                SuggestionButton.IsVisible ? 1 : 2);

            RestartGame(false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // The page is appearing either for the first time,
            // or following the Settings page being close.

            // Respect the current settings.
            SuggestionButton.IsVisible = settings.ShowSuggestionsButton;

            Grid.SetRowSpan(CardPile1,
                SuggestionButton.IsVisible ? 1 : 2);

            if (!firstAppearing)
            {
                Card card;

                for (int i = 0; i < cCardPiles; i++)
                {
                    ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                    var items = list.ItemsSource as ObservableCollection<PlayingCard>;

                    for (int j = 0; j < items.Count; j++)
                    {
                        PlayingCard playingCard = (items[j] as PlayingCard);
                        card = playingCard.Card;
                        playingCard.Card = null;
                        playingCard.Card = card;
                    }
                }

                card = CardDeckUpturnedObscuredLower.Card;
                CardDeckUpturnedObscuredLower.Card = null;
                CardDeckUpturnedObscuredLower.Card = card;

                card = CardDeckUpturnedObscuredHigher.Card;
                CardDeckUpturnedObscuredHigher.Card = null;
                CardDeckUpturnedObscuredHigher.Card = card;

                card = CardDeckUpturned.Card;
                CardDeckUpturned.Card = null;
                CardDeckUpturned.Card = card;

                card = TargetPileC.Card;
                TargetPileC.Card = null;
                TargetPileC.Card = card;

                card = TargetPileD.Card;
                TargetPileD.Card = null;
                TargetPileD.Card = card;

                card = TargetPileH.Card;
                TargetPileH.Card = null;
                TargetPileH.Card = card;

                card = TargetPileS.Card;
                TargetPileS.Card = null;
                TargetPileS.Card = card;
            }

            firstAppearing = false;
        }

        private PlayingCard CreatePlayingCard()
        {
            return new PlayingCard(this.settings);
        }

        private void DealCards()
        {
            int cardIndex = 0;

            Debug.WriteLine("Deal, start with " + _deckRemaining.Count + " cards.");

            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

                this.ViewModel.PlayingCardsB[i].Clear();
                this.ViewModel.PlayingCards[i].Clear();

                for (int j = 0; j < (i + 1); j++)
                {
                    var card = CreatePlayingCard();

                    card.Card = _deckRemaining[cardIndex];

                    EnableCard(card, (j == i));

                    card.IsCardVisible = (j == 0) || (j == i);

                    card.FaceDownCount = i;

                    card.IsObscured = (j != i);

                    card.InitialIndex = j + 1;

                    card.ListIndex = i + 1;

                    ++cardIndex;

                    this.ViewModel.PlayingCardsB[i].Add(card);

                    if (card.IsCardVisible)
                    {
                        this.ViewModel.PlayingCards[i].Add(card);
                    }
                }
            }

            CardPile1.ItemsSource = ViewModel.PlayingCards1;
            CardPile2.ItemsSource = ViewModel.PlayingCards2;
            CardPile3.ItemsSource = ViewModel.PlayingCards3;
            CardPile4.ItemsSource = ViewModel.PlayingCards4;
            CardPile5.ItemsSource = ViewModel.PlayingCards5;
            CardPile6.ItemsSource = ViewModel.PlayingCards6;
            CardPile7.ItemsSource = ViewModel.PlayingCards7;

            _deckRemaining.RemoveRange(0, cardIndex);

            Debug.WriteLine("Left with " + _deckRemaining.Count + " cards remaining.");

            for (int i = 0; i < cTargetPiles; ++i)
            {
                _targetPiles[i].Clear();
            }

            ClearTargetPileButtons();
            ClearUpturnedPileButton();

            SetCardPileSize();

            this.SizeChanged += MainPage_SizeChanged;

            SetStateDealtCardPiles();
        }

        private void MainPage_SizeChanged(object sender, EventArgs e)
        {
            SetCardPileSize();
        }

        private int mostRecentDealtCardPileWidth = 0;

        private void SetCardPileSize()
        {
            if (this.Width <= 0)
            {
                return;
            }

            // Barker: How does running on Xbox, affect all this?

            double width = (this.Width - 40) / 7;

            TargetPileC.WidthRequest = width;
            TargetPileD.WidthRequest = width;
            TargetPileH.WidthRequest = width;
            TargetPileS.WidthRequest = width;

            int dealtCardPileWidth = (int)width;

            mostRecentDealtCardPileWidth = dealtCardPileWidth;

            CardPile1.WidthRequest = dealtCardPileWidth;
            SuggestionButton.WidthRequest = dealtCardPileWidth;
            CardPile2.WidthRequest = dealtCardPileWidth;
            CardPile3.WidthRequest = dealtCardPileWidth;
            CardPile4.WidthRequest = dealtCardPileWidth;
            CardPile5.WidthRequest = dealtCardPileWidth;
            CardPile6.WidthRequest = dealtCardPileWidth;
            CardPile7.WidthRequest = dealtCardPileWidth;
            ButtonsContainer.WidthRequest = dealtCardPileWidth;

            MainPage.LastCardHeight = (int)((dealtCardPileWidth * 346) / 259);

            // Prevent the app from flashing due to a resize of the TopCornerPiles.
            TopCornerPiles.HeightRequest = MainPage.LastCardHeight;

            NextCardDeck.WidthRequest = dealtCardPileWidth;

            UpturnedCardsGrid.WidthRequest = dealtCardPileWidth * 1.5;

            CardDeckUpturnedObscuredLower.WidthRequest = dealtCardPileWidth;
            CardDeckUpturnedObscuredHigher.WidthRequest = dealtCardPileWidth;
            CardDeckUpturned.WidthRequest = dealtCardPileWidth;

            CardDeckUpturnedObscuredHigher.Margin = new Thickness(
                -dealtCardPileWidth * 0.8, 0, 0, 0);

            CardDeckUpturned.Margin = new Thickness(
                -dealtCardPileWidth * 0.8, 0, 0, 0);

            // Trigger a resize of the last card in each dealt card pile.
            // Barker: Figure out how to remove this.
            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                var items = list.ItemsSource as ObservableCollection<PlayingCard>;

                PlayingCard playingCard = (items[items.Count - 1] as PlayingCard);
                playingCard.IsObscured = false;
            }
        }

        // Barker: Remove us of this LastCardHeight. (This is currently used to 
        // help set the size of the last card in each dealt card pile.)
        public static int LastCardHeight = 0;

        private void ClearTargetPileButtons()
        {
            TargetPileC.Card = null;
            TargetPileD.Card = null;
            TargetPileH.Card = null;
            TargetPileS.Card = null;
        }

        private void EnableCard(PlayingCard card, bool enable)
        {
            card.FaceDown = !enable;
            card.CardState = (enable ? CardState.FaceUp : CardState.FaceDown);
        }

        private void SetUpturnedCardsVisuals()
        {
            if (_deckUpturned.Count == 0)
            {
                CardDeckUpturned.IsToggled = false;
                CardDeckUpturned.IsEnabled = false;

                CardDeckUpturned.Card = null;
                CardDeckUpturnedObscuredHigher.Card = null;
                CardDeckUpturnedObscuredLower.Card = null;
            }
            else
            {
                if (_deckUpturned.Count > 1)
                {
                    CardDeckUpturnedObscuredHigher.Card = _deckUpturned[_deckUpturned.Count - 2];
                }
                else
                {
                    CardDeckUpturnedObscuredHigher.Card = null;
                }

                if (_deckUpturned.Count > 2)
                {
                    CardDeckUpturnedObscuredLower.Card = _deckUpturned[_deckUpturned.Count - 3];
                }
                else
                {
                    CardDeckUpturnedObscuredLower.Card = null;
                }

                if (_deckUpturned.Count == 1)
                {
                    CardDeckUpturnedObscuredHigher.Margin = new Thickness();

                    CardDeckUpturned.Margin = new Thickness();
                }
                else if (_deckUpturned.Count == 2)
                {
                    CardDeckUpturnedObscuredHigher.Margin = new Thickness();

                    CardDeckUpturned.Margin = new Thickness(
                        -mostRecentDealtCardPileWidth * 0.8, 0, 0, 0);
                }
                else
                {
                    CardDeckUpturnedObscuredHigher.Margin = new Thickness(
                        -mostRecentDealtCardPileWidth * 0.8, 0, 0, 0);

                    CardDeckUpturned.Margin = new Thickness(
                        -mostRecentDealtCardPileWidth * 0.8, 0, 0, 0);
                }

                CardDeckUpturned.Card = _deckUpturned[_deckUpturned.Count - 1];

                CardDeckUpturned.IsEnabled = true;
                CardDeckUpturned.IsToggled = false;
            }
        }

        private bool CanMoveCard(PlayingCard cardBelow, PlayingCard cardAbove)
        {
            bool canMove = false;

            if ((cardBelow != null) && (cardAbove != null))
            {
                if (cardBelow.Card.Rank == cardAbove.Card.Rank + 1)
                {
                    bool isBelowRed = ((cardBelow.Card.Suit == Suit.Diamonds) || (cardBelow.Card.Suit == Suit.Hearts));
                    bool isAboveRed = ((cardAbove.Card.Suit == Suit.Diamonds) || (cardAbove.Card.Suit == Suit.Hearts));

                    canMove = (isBelowRed != isAboveRed);
                }
            }

            return canMove;
        }

        // Barker: Use the approved method of getting the items source.
        private ObservableCollection<PlayingCard> GetListSource(ListView list)
        {
            ObservableCollection<PlayingCard> col = null;

            //int index = int.Parse(list.Name.Replace("CardPile", ""));
            int index = int.Parse(list.StyleId.Replace("CardPile", ""));
            col = ViewModel.PlayingCards[index - 1];

            return col;
        }

        private void RestartButton_Clicked(object sender, EventArgs e)
        {
            RestartGame(true);
        }

        private async void SettingsButton_Clicked(object sender, EventArgs e)
        {
            var settingsPage = new SettingsPage(this.settings);

            await Navigation.PushModalAsync(settingsPage);
        }

        private async void SuggestionsButton_Clicked(object sender, EventArgs e)
        {
            string suggestion;

            if (GameOver())
            {
                suggestion = "You've done brilliant and won the game! Why not have another game?";
            }
            else if (!GetMoveSuggestion(out suggestion))
            {
                suggestion = "Sorry, I don't have a suggestion at the moment.";
            }

            RaiseNotificationEvent(suggestion);

            await DisplayAlert(
                "Suggestion",
                suggestion,
                "Close");

            // Tested successful on Android if we want to have a notification raised.
            //try
            //{
            //    CrossLocalNotifications.Current.Show(
            //        "Xa11ytaire",
            //        suggestion);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }

        private void ClearUpturnedPileButton()
        {
            SetUpturnedCardsVisuals();
        }

        private void UncheckToggleButtons(bool includeUpturnedCard)
        {
            if (includeUpturnedCard)
            {
                CardDeckUpturned.IsToggled = false;
            }

            TargetPileC.IsToggled = false;
            TargetPileD.IsToggled = false;
            TargetPileH.IsToggled = false;
            TargetPileS.IsToggled = false;
        }

        private async void RestartGame(bool doQuery)
        {
            if (doQuery)
            {
                var answerIsYes = await DisplayAlert(
                    "Xa11ytaire",
                    "Are you sure you want to restart the game?",
                    "Yes", "No");

                if (!answerIsYes)
                {
                    return;
                }
            }

            UncheckToggleButtons(true);

            _deckUpturned.Clear();

            CardDeckUpturned.Card = null;
            CardDeckUpturnedObscuredLower.Card = null;
            CardDeckUpturnedObscuredHigher.Card = null;

            SetUpturnedCardsVisuals();

            _deckRemaining.Clear();

            for (int rank = 1; rank <= 13; ++rank)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    if (suit == Suit.NoSuit)
                    {
                        continue;
                    }

                    _deckRemaining.Add(new Card { Rank = rank, Suit = suit });
                }
            }

            _shuffler = new Shuffler();
            _shuffler.Shuffle(_deckRemaining);

            DealCards();

            NextCardDeck.IsEmpty = false;

            RaiseNotificationEvent(
                Resource1.GameRestarted);
        }

        private void CardDeckUpturned_Toggled(object sender, ToggledEventArgs e)
        {
            // Only act when the upturned card is toggled On.
            if (!e.Value)
            {
                return;
            }

            if (_deckUpturned.Count > 0)
            {
                // Always deselect all dealt cards and the target card piles 
                // when the upturned card is selected.
                DeselectDealtCards();
                UncheckToggleButtons(false);

                string upturnedAnnouncement =
                    Resource1.Upturned + " " +
                    CardDeckUpturned.Card.ToString() + " " +
                    Resource1.Selected + ".";

                RaiseNotificationEvent(
                     upturnedAnnouncement);
            }
        }

        private void RaiseNotificationEvent(string notification)
        {
            Debug.WriteLine("Announced: \"" + notification + "\"");

            var service = DependencyService.Get<IXa11ytairePlatformAction>();
            service.ScreenReaderAnnouncement(notification);
        }

        private void DeselectDealtCards()
        {
            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

                ClearListSelection(list);
            }
        }

        private async void ShowEndOfGameDialog()
        {
            var answerIsYes = await DisplayAlert(
                "Xa11ytaire",
                "Congratulations, you have won! Would you like to start a new game?",
                "Yes", "No");

            if (answerIsYes)
            {
                RestartGame(false);
            }
        }
    }

    // The code below was copied from:
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/localization/text?tabs=windows

    public interface ILocalize
    {
        CultureInfo GetCurrentCultureInfo();
        void SetLocale(CultureInfo ci);
    }

    // You exclude the 'Extension' suffix when using in XAML
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        readonly CultureInfo ci = null;
        //const string ResourceId = "UsingResxLocalization.Resx.AppResources";
        const string ResourceId = "Xa11ytaire.Resource1";

        static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager(ResourceId, IntrospectionExtensions.GetTypeInfo(typeof(TranslateExtension)).Assembly));

        public string Text { get; set; }

        public TranslateExtension()
        {
            if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android)
            {
                ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return string.Empty;

            var translation = ResMgr.Value.GetString(Text, ci);
            if (translation == null)
            {
#if DEBUG
                throw new ArgumentException(
                    string.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", Text, ResourceId, ci.Name),
                    "Text");
#else
            translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }
    }
}
