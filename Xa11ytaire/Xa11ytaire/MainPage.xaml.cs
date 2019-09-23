﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xa11ytaire.Source;
using Xa11ytaire.Source.ViewModels;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using Xa11ytaire.Source.Classes;
using System.Threading;
using Xamarin.Forms.Xaml;
using System.Globalization;
using System.Resources;
using System.Reflection;

// Barker: Learn about the Switch control.

namespace Xa11ytaire
{
    public interface IXa11ytairePlatformAction
    {
        Settings LoadSettings();

        void SaveSettings(Settings settings);

        Task<string> LocalRecognizeImage(Stream imageStream);

        Task<bool> InitializeMicrophone();

        Task<byte[]> GetPixelValuesForMLReco();

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
        private bool _dealingCards = false;

        private int cTargetPiles = 4;
        private List<Card>[] _targetPiles = new List<Card>[4];

        private bool cardHasBeenMoved = false;

        private Settings settings;

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

            RestartGame(false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // The page is appearing either for the first time,
            // or following the Settings page being close.

            // Respect the current settings.
            SuggestionContainer.IsVisible = settings.ShowSuggestionsButton;

            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                var items = list.ItemsSource as ObservableCollection<PlayingCard>;

                for (int j = 0; j < items.Count; j++)
                {
                    var playingCard = (items[items.Count - 1] as PlayingCard);

                    // Force the row idnex to change in order to refresh the HelpText.
                    int rowIndex = playingCard.VisibleRowIndex;
                    playingCard.VisibleRowIndex = -1;
                    playingCard.VisibleRowIndex = rowIndex;
                }
            }
        }

        private PlayingCard CreatePlayingCard()
        {
            return new PlayingCard(this.settings);
        }

        private void DealCards()
        {
            _dealingCards = true;

            int cardIndex = 0;

            Debug.WriteLine("Deal, start with " + _deckRemaining.Count + " cards.");

            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

                // XBarker: Can't access items in the list directly.
                //if (list.Items.Count > 0)
                //{
                //    list.SelectedIndex = 0;
                //}

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

            _dealingCards = false;

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
            CardPile2.WidthRequest = dealtCardPileWidth;
            CardPile3.WidthRequest = dealtCardPileWidth;
            CardPile4.WidthRequest = dealtCardPileWidth;
            CardPile5.WidthRequest = dealtCardPileWidth;
            CardPile6.WidthRequest = dealtCardPileWidth;
            CardPile7.WidthRequest = dealtCardPileWidth;

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

        private void SuggestionsButton_Clicked(object sender, EventArgs e)
        {
            string suggestion;
            if (GetMoveSuggestion(out suggestion))
            {
                RaiseNotificationEvent(suggestion);
            }
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

            //MostRecentNotificationTextBox.Text = "";

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

            //NextCardDeck.Focus(FocusState.Keyboard);

            RaiseNotificationEvent(
                Resource1.GameRestarted);

            //if (screenReaderAnnouncement)
            //{
            //    RaiseNotificationEvent(
            //        AutomationNotificationKind.Other,
            //        AutomationNotificationProcessing.ImportantMostRecent,
            //        "Game restarted",
            //        NotificationActivityID_Default,
            //        NextCardDeck);
            //}

            //CardRecoStatus.Text = "";
        }

        //private void TakePhoto_Clicked(object sender, EventArgs e)
        //{
        //    CardRecoStatus.Text = "";

        //    MoveByImageReco();
        //}


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

                //RaiseNotificationEvent(
                //    AutomationNotificationKind.ActionCompleted,
                //     AutomationNotificationProcessing.ImportantAll,
                //     upturnedAnnouncement,
                //     NotificationActivityID_Default,
                //     NextCardDeck);

                if (this.ViewModel.SingleKeyToMove)
                {
                    // MoveUpturnedCardWithSingleKeyPressIfPossible();
                }
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

        private bool GetMoveSuggestion(out string suggestion)
        {
            // Important: When checking for a move between dealt card piles,
            // only check the lowest face-up card. We don't want to just be 
            // moving cards back and forth between piles.

            // BARKER TODO!
            // - Check for moving a card from a target card pile back down to a dealt card pile.
            // - Check whether any reachable card in the remaining card pile can be moved.

            // Also:
            // - Use localized resources.
            // - Add helper functions to avoid the code duplication below.

            suggestion = "";

            bool canMoveCard = false;

            // First check whether the upturned card can be moved to 
            // a target card pile.
            if (_deckUpturned.Count > 0)
            {
                Card upturnedCard = _deckUpturned[_deckUpturned.Count - 1];

                CardPileToggleButton targetCardButton = null;

                if (upturnedCard.Suit == Suit.Clubs)
                {
                    targetCardButton = TargetPileC;
                }
                else if (upturnedCard.Suit == Suit.Diamonds)
                {
                    targetCardButton = TargetPileD;
                }
                else if (upturnedCard.Suit == Suit.Hearts)
                {
                    targetCardButton = TargetPileH;
                }
                else if (upturnedCard.Suit == Suit.Spades)
                {
                    targetCardButton = TargetPileS;
                }

                if ((upturnedCard != null) && (targetCardButton != null))
                {
                    // Is there a card on this target card pile yet?
                    if (targetCardButton.Card == null)
                    {
                        // No, so a move is only possible if the upturned card is an ace.
                        if (upturnedCard.Rank == 1)
                        {
                            canMoveCard = true;
                        }
                    }
                    else
                    {
                        // Check if the upturned card can be moved on top of the card
                        // that's currently at the top of the target card pile.
                        if (upturnedCard.Rank == targetCardButton.Card.Rank + 1)
                        {
                            canMoveCard = true;
                        }
                    }

                    if (canMoveCard)
                    {
                        suggestion = "Consider moving the upturned " +
                            upturnedCard.ToString() + " to the " +
                            targetCardButton.Suit + " pile.";
                    }
                }

                // If necessary, consider moving the upturned card to a dealt card pile.
                if (!canMoveCard)
                {
                    for (int i = 0; i < cCardPiles; i++)
                    {
                        ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                        var items = list.ItemsSource as ObservableCollection<PlayingCard>;

                        if (items.Count > 0)
                        {
                            var topCardInDealtCardPile = (items[items.Count - 1] as PlayingCard);

                            // If this deaklt card pile empty?
                            if (topCardInDealtCardPile.CardState == CardState.KingPlaceHolder)
                            {
                                // Only a King can be moved to an empty pile.
                                if (upturnedCard.Rank == 13)
                                {
                                    canMoveCard = true;
                                }
                            }
                            else
                            {
                                var playingCardUpturned = CreatePlayingCard();
                                playingCardUpturned.Card = _deckUpturned[_deckUpturned.Count - 1];

                                if (CanMoveCard(topCardInDealtCardPile, playingCardUpturned))
                                {
                                    canMoveCard = true;
                                }
                            }

                            if (canMoveCard)
                            {
                                suggestion = "Consider moving the upturned " +
                                    upturnedCard.ToString() + " to the " +
                                    topCardInDealtCardPile.Name + " in pile " +
                                    (i + 1);

                                break;
                            }
                        }
                    }
                }
            }

            // If necessary, check if a card can be moved away from a dealt card piles.
            if (!canMoveCard)
            {
                for (int i = 0; i < cCardPiles; i++)
                {
                    // Check each dealt card list in turn.
                    ListView listSource = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                    var itemsSource = listSource.ItemsSource as ObservableCollection<PlayingCard>;

                    if (itemsSource.Count > 0)
                    {
                        var cardToMoveInSourceCardPile = (itemsSource[itemsSource.Count - 1] as PlayingCard);

                        // If this is a King placeholder, we're not moving it.
                        if (cardToMoveInSourceCardPile.IsKingDropZone)
                        {
                            continue;
                        }

                        CardPileToggleButton targetCardButton = null;

                        if (cardToMoveInSourceCardPile.Suit == Suit.Clubs)
                        {
                            targetCardButton = TargetPileC;
                        }
                        else if (cardToMoveInSourceCardPile.Suit == Suit.Diamonds)
                        {
                            targetCardButton = TargetPileD;
                        }
                        else if (cardToMoveInSourceCardPile.Suit == Suit.Hearts)
                        {
                            targetCardButton = TargetPileH;
                        }
                        else if (cardToMoveInSourceCardPile.Suit == Suit.Spades)
                        {
                            targetCardButton = TargetPileS;
                        }

                        // Is there a card on this target card pile yet?
                        if (targetCardButton.Card == null)
                        {
                            // No, so a move is only possible if the upturned card is an ace.
                            if (cardToMoveInSourceCardPile.Rank == 1)
                            {
                                canMoveCard = true;
                            }
                        }
                        else
                        {
                            // Check if the dealt card can be moved on top of the card
                            // that's currently at the top of the target card pile.
                            if (cardToMoveInSourceCardPile.Rank == targetCardButton.Card.Rank + 1)
                            {
                                canMoveCard = true;
                            }
                        }

                        if (canMoveCard)
                        {
                            suggestion = "Consider moving the " +
                                cardToMoveInSourceCardPile.Name + " in pile " + 
                                (i + 1).ToString() + 
                                " to the " +
                                targetCardButton.Suit + " pile.";

                            break;
                        }
                        else
                        {
                            // Now look for the lowest face-up card in the source pile.
                            int indexToLowestFaceUpCardInSourceCardPile =
                                (itemsSource[0] as PlayingCard).FaceDown ?
                                    1 : 0;

                            cardToMoveInSourceCardPile = 
                               (itemsSource[indexToLowestFaceUpCardInSourceCardPile] as PlayingCard);

                            // Don't bother moving a King from the bottom of a pile.
                            if ((cardToMoveInSourceCardPile.Card.Rank == 13) &&
                                (indexToLowestFaceUpCardInSourceCardPile == 0))
                            {
                                continue;
                            }

                            // Look for a move between dealt card piles.
                            for (int j = 0; j < cCardPiles; j++)
                            {
                                if (i == j)
                                {
                                    continue;
                                }

                                ListView listDestination =
                                    (ListView)CardPileGrid.FindByName("CardPile" + (j + 1));
                                var itemsDestination = listDestination.ItemsSource as ObservableCollection<PlayingCard>;

                                if (itemsDestination.Count > 0)
                                {
                                    var topCardInDestinationCardPile =
                                        (itemsDestination[itemsDestination.Count - 1] as PlayingCard);

                                    if (topCardInDestinationCardPile.CardState == CardState.KingPlaceHolder)
                                    {
                                        // Move a King to the empty pile.
                                        if (cardToMoveInSourceCardPile.Card.Rank == 13)
                                        {
                                            canMoveCard = true;
                                        }
                                    }
                                    else
                                    {
                                        if (CanMoveCard(
                                                topCardInDestinationCardPile,
                                                cardToMoveInSourceCardPile))
                                        {
                                            canMoveCard = true;
                                        }
                                    }

                                    if (canMoveCard)
                                    {
                                        suggestion = "Consider moving the " +
                                            cardToMoveInSourceCardPile.Name +
                                            " in pile " + 
                                            (i + 1).ToString() + " to the " +
                                            topCardInDestinationCardPile.Name +
                                            " in pile " + 
                                            (j + 1).ToString() + ".";

                                        break;
                                    }
                                }
                            }

                            if (canMoveCard)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return canMoveCard;
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
