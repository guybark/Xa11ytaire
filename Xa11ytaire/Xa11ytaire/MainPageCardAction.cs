using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Xamarin.Forms;

using Xa11ytaire.Source;
using Xa11ytaire.Source.Classes;

// Notes on the summary grid element.
//
// The HelpText on that is set:
// - After a deal of the cards.
// - Following any time a card has been selected, (regardless of whether a move has happened).
// - Followiny any time a target card has been toggled, (regardless of whether a move has happened).
//
// Originally the HelpText included the face down count, and the 
// start and end of all face up cards. This seemed too much for 
// the common use of the feature. So this was replaced to only
// include the top-most face up card, (or the empty slot).

namespace Xa11ytaire
{
    public sealed partial class MainPage : ContentPage
    {
        // Turn over the next card in the Remaining Card pile.

        private async void NextCardDeck_Clicked(object sender, EventArgs e)
        {
            DeselectDealtCards();
            UncheckToggleButtons(true);

            DoNextCardClick();
        }

        private void DoNextCardClick()
        {
            // XBarker:
            UncheckToggleButtons(true);

            string screenReaderAnnouncement = "";

            // Can we turn over at least one card?
            if (_deckRemaining.Count > 0)
            {
                // Yes, so how many cards can we turn over?
                int countCardsToTurn;
                
                if (this.settings.TurnOverOneCard)
                {
                    countCardsToTurn = 1;
                }
                else
                {
                    countCardsToTurn = (_deckRemaining.Count >= 3 ? 3 : _deckRemaining.Count);
                }

                // Turn over each card in turn.

                for (int i = 0; i < countCardsToTurn; ++i)
                {
                    Card card = _deckRemaining[_deckRemaining.Count - 1];
                    _deckRemaining.Remove(card);
                    _deckUpturned.Add(card);

                    // Reverse the polarity of the screen reader announcement,
                    // to have the topmost upturned card announced first.
                    screenReaderAnnouncement =
                        (i < countCardsToTurn - 1 ? ", " + Resource1.On + " " : " ") +
                        card +
                        screenReaderAnnouncement;
                }
            }
            else
            {
                // There are no cards left to turn over, so move all the upturned cards back to 
                // the Remaining Cards pile.
                while (_deckUpturned.Count > 0)
                {
                    Card card = _deckUpturned[_deckUpturned.Count - 1];
                    _deckUpturned.Remove(card);
                    _deckRemaining.Add(card);
                }

                ClearUpturnedPileButton();

                screenReaderAnnouncement = Resource1.RestackedCards;
            }

            SetUpturnedCardsVisuals();

            NextCardDeck.IsEmpty = (_deckRemaining.Count == 0);

            // XBarker:
            //var resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader();

            string ttsText = screenReaderAnnouncement + 
                     (_deckRemaining.Count == 0 ? ", " + Resource1.NoCardLeft : ".");

            RaiseNotificationEvent(ttsText);

            //RaiseNotificationEvent(
            //    AutomationNotificationKind.ItemAdded,
            //    AutomationNotificationProcessing.MostRecent,
            //    ttsText,
            //    NotificationActivityID_Default,
            //    NextCardDeck);
        }

        private bool GameOver()
        {
            //we have moved a card to the TargetPile
            //now let's see if the game is over
            for (int i = 0; i < cTargetPiles; i++)
            {
                if (_targetPiles[i].Count != 13)
                {
                    return false;
                }
            }
            return true;
        }

        // A card in one of the Target Card piles has been checked.


        private void TargetPile_Toggled(object sender, ToggledEventArgs e)
        {
            // No action required if button's being toggled off.
            if (!e.Value)
            {
                Debug.WriteLine("Target pile is being toggled off, so take no action.");

                return;
            }

            CardPileToggleButton btn = (CardPileToggleButton)sender;

            // Is the top card in the Upturned Card pile checked?
            if ((CardDeckUpturned.IsToggled) && (_deckUpturned.Count > 0))
            {
                // Attempt to move the upturned card to the target pile.
                MoveUpturnedCardToTargetPileAsAppropriate(btn);
            }
            else
            {
                // Attempt to move a card from one of the Dealt Card piles to the Target Card pile.
                MoveDealtCardToTargetPileAsAppropriate(btn);
            }

            if (GameOver())
            {
                ShowEndOfGameDialog();
            }
            else
            {
                SetStateDealtCardPiles();
            }
        }

        // A Target Card pile button has been checked while the top card in the Upturned Card pile is checked.
        private void MoveUpturnedCardToTargetPileAsAppropriate(CardPileToggleButton btn)
        {
            // Clear all selection from the card pile lists.
            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

                ClearListSelection(list);
            }

            if (_deckUpturned.Count == 0)
            {
                return;
            }

            // cardAbove here is the upturned card.
            PlayingCard cardAbove = CreatePlayingCard();
            cardAbove.CardState = CardState.FaceUp;
            cardAbove.IsCardVisible = true;

            cardAbove.Card = _deckUpturned[_deckUpturned.Count - 1];

            if (CardDeckUpturned.IsToggled)
            {
                // Figure out which TargetPile has been invoked.

                int index = GetTargetPileListIndex(btn);

                // No action required if the upturned card doesn;t match the suit of the TargetPile.
                if (((index == 0) && (cardAbove.Card.Suit != Suit.Clubs)) ||
                    ((index == 1) && (cardAbove.Card.Suit != Suit.Diamonds)) ||
                    ((index == 2) && (cardAbove.Card.Suit != Suit.Hearts)) ||
                    ((index == 3) && (cardAbove.Card.Suit != Suit.Spades)))
                {
                    DeselectDealtCards();
                    UncheckToggleButtons(true);

                    PlaySound(false);
                    return;
                }

                // Figure out if we should move the card.
                bool moveCard = false;

                if (cardAbove.Card.Rank == 1)
                {
                    if (_targetPiles[index].Count == 0)
                    {
                        moveCard = true;
                    }
                }
                else
                {
                    List<Card> list = _targetPiles[index];

                    try
                    {
                        if ((cardAbove.Card.Rank == list[list.Count - 1].Rank + 1) &&
                            (cardAbove.Card.Suit == list[list.Count - 1].Suit))
                        {
                            moveCard = true;
                        }
                    }
                    catch (Exception)
                    {
                        moveCard = false;
                    }
                }

                if (moveCard)
                {
                    // Move the upturned card to the Target Pile.

                    _targetPiles[index].Add(cardAbove.Card);

                    btn.Card = cardAbove.Card;

                    //AutomationProperties.SetName(btn, (string)cardAbove.Name);

                    _deckUpturned.Remove(cardAbove.Card);

                    SetUpturnedCardsVisuals();

                    var suitName = _targetPiles[index][0].Suit.ToString();

                    var announcement =
                        Resource1.Moved + " " +
                        cardAbove.Name + " " +
                        Resource1.To + " " +
                        suitName + " " +
                        Resource1.Pile + ". ";

                    RaiseNotificationEvent(
                         announcement);

                    PlaySound(true);
                }
                else
                {
                    PlaySound(false);
                }
            }

            // XBarker:
            UncheckToggleButtons(true);
        }

        private int GetTargetPileListIndex(CardPileToggleButton targetPileButton)
        {
            int index = -1;

            //string pileId = targetPileButton.Name.Replace("TargetPile", "");
            string pileId = targetPileButton.StyleId.Replace("TargetPile", "");
            switch (pileId)
            {
                case "C":
                    index = 0;
                    break;
                case "D":
                    index = 1;
                    break;
                case "H":
                    index = 2;
                    break;
                case "S":
                    index = 3;
                    break;
            }

            return index;
        }

        // The SetCardDetails function changes the properties set on the supplied cardDestination card.
        private void SetCardDetails(PlayingCard cardSource, PlayingCard cardDestination)
        {
            // If a cardSource card was supplied, copy important details of that card over to cardDestination.
            if (cardSource != null)
            {
                cardDestination.Card = new Card();
                cardDestination.Card.Suit = cardSource.Card.Suit;
                cardDestination.Card.Rank = cardSource.Card.Rank;

                cardDestination.CardState = CardState.FaceUp;
                cardDestination.IsKingDropZone = false;
            }
            else
            {
                // No cardSource was supplied, so effectively turn cardDestination into a empty card 
                // which becomes the drop zome for a king.

                cardDestination.Card.Suit = Suit.NoSuit;
                cardDestination.Card.Rank = 0;

                cardDestination.CardState = CardState.KingPlaceHolder;
                cardDestination.IsKingDropZone = true;
                cardDestination.IsObscured = false;

                for (int i = 0; i < cCardPiles; ++i)
                {
                    ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

                    // XBarker:
                    //if (list.Items.Contains(cardDestination))
                    //{
                    //    cardDestination.ListIndex = (i + 1);

                    //    break;
                    //}
                }
            }

            // Barker todo: Take action to force a refresh of the UI. Change the PlayingCard
            // class such that this action is not necessary.
            cardDestination.FaceDown = true;
            cardDestination.FaceDown = false;
        }

        // A Target Card pile button has been checked while the top card in the Upturned Card pile is not checked,
        // so attempt to move a card from one of the Dealt Card piles.
        private void MoveDealtCardToTargetPileAsAppropriate(CardPileToggleButton btn)
        {
            // Determine which TargetPile has been invoked.

            int targetListIndex = GetTargetPileListIndex(btn);

            bool setButtonVisuals = false;

            // XBarker:
            //var resourceLoader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string inDealtCardPile = Resource1.InDealtCardPile;
            string revealedString = Resource1.Revealed;

            // Is anything selected in a CardPile list?
            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                if (list.SelectedItem != null)
                {
                    // Ok, we've found a selected item in one of the Dealt Card lists.

                    PlayingCard cardAbove = (PlayingCard)list.SelectedItem;

                    string nameCardMoved = cardAbove.Name;

                    // No action if the select card's suit does not match the Target Pile.
                    if (((targetListIndex == 0) && (cardAbove.Card.Suit != Suit.Clubs)) ||
                        ((targetListIndex == 1) && (cardAbove.Card.Suit != Suit.Diamonds)) ||
                        ((targetListIndex == 2) && (cardAbove.Card.Suit != Suit.Hearts)) ||
                        ((targetListIndex == 3) && (cardAbove.Card.Suit != Suit.Spades)))
                    {
                        DeselectDealtCards();
                        UncheckToggleButtons(true);

                        PlaySound(false);
                        return;
                    }

                    string cardRevealedAnnouncement = "";

                    var items = GetListSource(list);

                    // Figure out if we need to turn over a facedown card.
                    bool cardRevealedIsFaceDown = true;

                    PlayingCard firstItem = null;

                    int movingCardVisibleIndex = items.IndexOf(cardAbove);
                    if (movingCardVisibleIndex > 0)
                    {
                        if (!items[movingCardVisibleIndex - 1].FaceDown)
                        {
                            cardRevealedIsFaceDown = false;
                        }
                        else
                        {
                            var cardEnumerator = items.GetEnumerator();
                            cardEnumerator.MoveNext();
                            firstItem = (PlayingCard)cardEnumerator.Current;
                            --firstItem.FaceDownCount;
                        }
                    }

                    // Should we move an Ace?
                    if ((cardAbove.Card.Rank == 1) && (_targetPiles[targetListIndex].Count == 0))
                    {
                        // Create a new Card object for use in the target pile.
                        Card newCard = new Card();
                        newCard.Rank = cardAbove.Card.Rank;
                        newCard.Suit = cardAbove.Card.Suit;

                        _targetPiles[targetListIndex].Add(newCard);

                        setButtonVisuals = true;

                        PlaySound(true);

                        // Now take action to apparently remove the source card from the Dealt Pile list.

                        // Is this the last card in the dealt card pile?
                        if (items.Count > 1)
                        {
                            //No, so we'll not be showing the empty slot.
                            if (cardRevealedIsFaceDown)
                            {
                                // Get the next card from the shadown list.
                                int sourceListIndex = int.Parse(list.StyleId.Replace("CardPile", ""));
                                var shadowListIndex = this.ViewModel.PlayingCardsB.ElementAt(sourceListIndex - 1);
                                PlayingCard cardRevealed = (PlayingCard)shadowListIndex[
                                    firstItem.FaceDownCount + items.Count - 2];

                                SetCardDetails(cardRevealed, cardAbove);

                                if (firstItem.FaceDownCount == 0)
                                {
                                    items.Remove(cardRevealed);
                                }

                                cardAbove.IsObscured = false;
                            }
                            else
                            {
                                // Remove the card being moved.
                                items.Remove(cardAbove);

                                // The face-up card beneath the moved card is no longer obscured.
                                var nowTopCard = items[movingCardVisibleIndex - 1];
                                nowTopCard.IsObscured = false;

                                // Barker: Refresh hack.
                                items.Remove(nowTopCard);
                                items.Add(nowTopCard);
                            }
                        }
                        else
                        {
                            // That was the last card so show the empty slot.
                            SetCardDetails(null, cardAbove);
                        }

                        cardRevealedAnnouncement = cardAbove.Name;
                    }
                    else if (_targetPiles[targetListIndex].Count > 0)
                    {
                        // We're not moving an Ace, and the TargetPile already contains a card.
                        Card cardBelow = (Card)_targetPiles[targetListIndex][_targetPiles[targetListIndex].Count - 1];

                        if ((cardBelow.Suit == cardAbove.Card.Suit) &&
                            (cardBelow.Rank == cardAbove.Card.Rank - 1))
                        {
                            // Create a new Card object for use in the target pile.
                            Card newCard = new Card();
                            newCard.Rank = cardAbove.Card.Rank;
                            newCard.Suit = cardAbove.Card.Suit;

                            _targetPiles[targetListIndex].Add(newCard);

                            // Now take action to apparently remove the source card from the Dealt Pile list.
                            // Is this the last card in the pile?
                            if (items.Count > 1)
                            {
                                // No. Do we need to turn over a card?
                                if (cardRevealedIsFaceDown)
                                {
                                    // Yes, use the shadown list.
                                    int sourceListIndex = int.Parse(list.StyleId.Replace("CardPile", ""));
                                    var shadowListIndex = this.ViewModel.PlayingCardsB.ElementAt(sourceListIndex - 1);
                                    PlayingCard cardRevealed = (PlayingCard)shadowListIndex[
                                        firstItem.FaceDownCount + items.Count - 2];

                                    SetCardDetails(cardRevealed, cardAbove);

                                    if (firstItem.FaceDownCount == 0)
                                    {
                                        items.Remove(cardRevealed);
                                    }

                                    cardAbove.IsObscured = false;
                                }
                                else
                                {
                                    // Remove the card being moved.
                                    items.Remove(cardAbove);

                                    // The face-up card beneath the moved card is no longer obscured.
                                    var nowTopCard = items[movingCardVisibleIndex - 1];
                                    nowTopCard.IsObscured = false;

                                    // Barker: Refresh hack.
                                    items.Remove(nowTopCard);
                                    items.Add(nowTopCard);
                                }
                            }
                            else
                            {
                                // That was the last card so show the empty slot.
                                SetCardDetails(null, cardAbove);
                            }

                            cardRevealedAnnouncement = cardAbove.Name;

                            setButtonVisuals = true;

                            PlaySound(true);
                        }
                        else
                        {
                            DeselectDealtCards();
                            UncheckToggleButtons(true);

                            // illegal move
                            // you can only put the next sequential card on the pile
                            PlaySound(false);
                        }
                    }
                    else
                    {
                        DeselectDealtCards();
                        UncheckToggleButtons(true);

                        // illegal move
                        // attempted to move a card that was not in the right order
                        PlaySound(false);
                    }

                    ClearListSelection(list);

                    // Update the Target Pile button as appropriate.
                    if (setButtonVisuals)
                    {
                        // We know the target pile list isn't empty if we're here.
                        int count = _targetPiles[targetListIndex].Count;
                        Card card = _targetPiles[targetListIndex][count - 1];
                        btn.Card = card;

                        //btn.Focus(FocusState.Keyboard);

                        // Have screen readers make a related announcement.

                        var suitName = _targetPiles[targetListIndex][0].Suit.ToString();

                        var announcement =
                            Resource1.Moved + " " +
                            card.ToString() + " " +
                            Resource1.To + " " +
                            suitName + " " +
                            Resource1.Pile + ". ";

                        announcement += 
                            revealedString + " " +
                            cardRevealedAnnouncement +
                            " " + inDealtCardPile + " " +
                            (i + 1) + ".";

                        RaiseNotificationEvent(
                             announcement);

                        //RaiseNotificationEvent(
                        //     AutomationNotificationKind.ItemAdded,
                        //     AutomationNotificationProcessing.ImportantAll,
                        //     ttsText,
                        //     NotificationActivityID_Default,
                        //     NextCardDeck);
                    }

                    btn.IsToggled = false;

                    return;
                }
            }

            // If there's nothing in this target pile, leave it untoggled.
            if (_targetPiles[targetListIndex].Count == 0)
            {
                btn.IsToggled = false;
            }
            else
            {
                if (targetListIndex != 0)
                {
                    TargetPileC.IsToggled = false;
                }

                if (targetListIndex != 1)
                {
                    TargetPileD.IsToggled = false;
                }

                if (targetListIndex != 2)
                {
                    TargetPileH.IsToggled = false;
                }

                if (targetListIndex != 3)
                {
                    TargetPileS.IsToggled = false;
                }

                var announcement = btn.Name + " " + Resource1.Selected;

                RaiseNotificationEvent(announcement);
            }
        }

        // Barker: Interesting to consider whether a Tapped handler on the 
        // dealt card piles ListViews might easily enable a card deselect
        // on tap. Doesn't really seem tat that's the case though. (If this
        // is revisited, I need to check the Tapped handler gets called for
        // all of touch/mouse/switch/voice/etc.)

        //private void CardPile_Tapped(object sender, ItemTappedEventArgs e)
        //{
        //    var tappedCard = e.Item as PlayingCard;

        //}

        // The selection state of one of the card in the Dealt Card piles has changed.

        private void CardPile_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            // Only take action when a card has been selected.

            ListView listSelectionChanged = sender as ListView;

            if (listSelectionChanged.SelectedItem == null)
            {
                return;
            }

            // XBarker:
            //if (e.AddedItems.Count == 0)
            //{
            //    return;
            //}

            // Always deselect the target card piles.

            // XBarker:
            // UncheckToggleButtons(false /* include upturned card. */);

            // Is this an "empty" card pile?

            var items = listSelectionChanged.ItemsSource as ObservableCollection<PlayingCard>;

            // Barker: Tidy this up...
            var selectedCard = (listSelectionChanged.SelectedItem as PlayingCard);
            if (selectedCard != null)
            {
                foreach (PlayingCard card in items)
                {
                    card.IsSelected = (card == selectedCard);
                }
            }

            if (items.Count == 1)
            {
                if ((items[0] as PlayingCard).IsKingDropZone)
                {
                    EmptyCardItem_Select(listSelectionChanged);

                    //LookForAHint();

                    return;
                }
            }

            // Are we trying to move the upturned card to this list?
            if (CardDeckUpturned.IsToggled)
            {
                // cardAbove here is the upturned card.
                PlayingCard cardAbove = CreatePlayingCard();
                cardAbove.IsCardVisible = true;
                cardAbove.CardState = CardState.FaceUp;
                cardAbove.Card = _deckUpturned[_deckUpturned.Count - 1];

                // cardBelow is the selected card in the CardPile list.
                PlayingCard cardBelow = listSelectionChanged.SelectedItem as PlayingCard;
                if (CanMoveCard(cardBelow, cardAbove))
                {
                    cardBelow.IsObscured = true;

                    // Move the upturned card to the CardPile list.
                    var itemsAdded = GetListSource(listSelectionChanged);

                    // Barker: Without this, the updated height of the obscured card
                    // does not kick in until the app's been resized. Figure out how
                    // to avoid removing/adding the obscured card in the list.
                    itemsAdded.Remove(cardBelow);
                    itemsAdded.Add(cardBelow);
                    
                    itemsAdded.Add(cardAbove);

                    EnableCard(cardAbove, true);

                    _deckUpturned.Remove(cardAbove.Card);

                    var announcement =
                        Resource1.Moved + " " +
                        cardAbove.Name + " " +
                        Resource1.To + " " +
                        cardBelow.Name + ".";

                    RaiseNotificationEvent(
                         announcement);

                    SetUpturnedCardsVisuals();

                    UncheckToggleButtons(true);

                    PlaySound(true);

                    cardHasBeenMoved = true;

                    ClearListSelection(listSelectionChanged);

                    // Make sure focus is on the CardPile list.
                    FocusLastItemInList(listSelectionChanged);

                    // Moving upturned card to a dealt card pile list.
                    SetStateDealtCardPiles();
                }
                else
                {
                    PlaySound(false);

                    DeselectDealtCards();
                    UncheckToggleButtons(true);

                    ClearListSelection(listSelectionChanged);
                }

                // XBarker:
                // LookForAHint();

                return;
            }

            // Are we trying to move a card from a Target Pile to this list?

            if (MoveTargetPileCardToCardPileAsAppropriate(listSelectionChanged))
            {
                UncheckToggleButtons(true);
                ClearListSelection(listSelectionChanged);

                cardHasBeenMoved = true;
            }
            else
            {
                MoveCardBetweenDealtCardPiles(listSelectionChanged);

                cardHasBeenMoved = false;
            }

            // Moving either dealt card to target card to a dealt card pile list.
            SetStateDealtCardPiles();

            // XBarker:
            // LookForAHint();
        }

        private void EmptyCardItem_Select(ListView listTarget)
        {
            // If the empty card slot is being unselected, 
            // there's no action to be taken here.
            if (listTarget.SelectedItem == null)
            {
                return;
            }

            cardHasBeenMoved = false;

            // Consider moving the upturned card to this Card Pile.
            if ((CardDeckUpturned.IsToggled) && (_deckUpturned.Count > 0))
            {
                // cardAbove here will be the card being moved from the upturned card pile.
                PlayingCard cardAbove = CreatePlayingCard();
                cardAbove.IsCardVisible = true;
                cardAbove.CardState = CardState.FaceUp;
                cardAbove.Card = _deckUpturned[_deckUpturned.Count - 1];

                // Is the upturned card a King?
                if (cardAbove.Card.Rank == 13)
                {
                    var itemsAdded = GetListSource(listTarget);

                    SetCardDetails(cardAbove, itemsAdded[0]);

                    UncheckToggleButtons(true);

                    _deckUpturned.Remove(cardAbove.Card);

                    SetUpturnedCardsVisuals();

                    SetStateDealtCardPiles();

                    PlaySound(true);
                    cardHasBeenMoved = true;
                }
                else
                {
                    PlaySound(false);
                }

                FocusLastItemInList(listTarget);

                //listTarget.Focus(FocusState.Keyboard);
            }
            else
            {
                // Is anything selected in a CardPile list?
                for (int i = 0; i < cCardPiles; i++)
                {
                    ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                    if ((list.SelectedItem != null) && (list != listTarget))
                    {
                        PlayingCard cardAbove = (PlayingCard)list.SelectedItem;
                        if (cardAbove.Card.Rank == 13)
                        {
                            var items = list.ItemsSource as ObservableCollection<PlayingCard>;

                            // A King is selected in the other Card Pile list.
                            int movingCardIndex = items.IndexOf(cardAbove);

                            // Barker: Investigate why and when this is needed.
                            // AutomationProperties.SetName(cardRevealed, (string)cardRevealed.Content);

                            PlayingCard cardRevealed = null;

                            var itemsAdded = GetListSource(listTarget);
                            var itemsRemoved = GetListSource(list);

                            // Move the King, along with any other cards above it to the empty card pile.
                            SetCardDetails(cardAbove, itemsAdded[0]);

                            var nextCardIndex = movingCardIndex;

                            bool cardRevealedIsFaceDown = true;

                            PlayingCard firstItem = null;

                            // Is the king being moved the first card in that list?
                            if (movingCardIndex > 0)
                            {
                                if (!items[movingCardIndex - 1].FaceDown)
                                {
                                    cardRevealedIsFaceDown = false;
                                }
                                else
                                {
                                    var cardEnumerator = list.ItemsSource.GetEnumerator();
                                    cardEnumerator.MoveNext();
                                    firstItem = (PlayingCard)cardEnumerator.Current;
                                    --firstItem.FaceDownCount;
                                }

                                // No, so show the card that was previously beneath the moving King.
                                //cardRevealed = (PlayingCard)items[movingCardIndex - 1];

                                // Barker: Use shadow list.
                                if (cardRevealedIsFaceDown)
                                {
                                    int sourceListIndex = int.Parse(list.StyleId.Replace("CardPile", ""));
                                    var shadowListIndex = this.ViewModel.PlayingCardsB.ElementAt(sourceListIndex - 1);
                                    cardRevealed = (PlayingCard)shadowListIndex[
                                        firstItem.FaceDownCount + movingCardIndex - 1];

                                    SetCardDetails(cardRevealed, cardAbove);
                                }
                                else
                                {
                                    itemsRemoved.Remove(cardAbove);

                                    items[movingCardIndex - 1].IsObscured = false;
                                }

                                cardAbove.IsObscured = false;
                            }
                            else
                            {
                                // Effectively turn the moving card into the source list's empty item.
                                SetCardDetails(null, cardAbove);
                            }

                            // Remove an item from the source list if necessary.
                            if (cardRevealed != null)
                            {
                                if (firstItem.FaceDownCount == 0)
                                {
                                    itemsRemoved.Remove(cardRevealed);
                                }
                                else
                                {
                                    ++nextCardIndex;
                                }
                            }
                            else
                            {
                                ++nextCardIndex;
                            }

                            var previousCard = itemsAdded[0];

                            // Move multiple cards if necessary.
                            while (nextCardIndex < items.Count)
                            {
                                // Shrink down the moved King, now obscured.
                                previousCard.IsObscured = true;

                                // Barker: The resize back.
                                itemsAdded.Remove(previousCard);
                                itemsAdded.Add(previousCard);

                                var nextCard = (PlayingCard)items[nextCardIndex];

                                itemsRemoved.Remove(nextCard);

                                nextCard.IsObscured = false;
                                itemsAdded.Add(nextCard);

                                // Barker: Is this hack still needed?
                                itemsAdded.Remove(nextCard);
                                itemsAdded.Add(nextCard);

                                previousCard = nextCard;
                            }

                            ClearListSelection(list);
                            ClearListSelection(listTarget);

                            FocusLastItemInList(listTarget);

                            SetStateDealtCardPiles();

                            // listTarget.Focus(FocusState.Keyboard);

                            PlaySound(true);
                            cardHasBeenMoved = true;
                        }
                        else
                        {
                            PlaySound(false);
                        }
                    }
                }
            }

            // Did we move a card to the empty slot?
            if (!cardHasBeenMoved)
            {
                // No, so there's no need to leave the empty slot selected.
                // First unselect the PlayingCard that is the empty slot.
                var listSource = GetListSource(listTarget);
                if (listSource.Count > 0)
                {
                    var emptyCard = listSource[0] as PlayingCard;
                    if (emptyCard != null)
                    {
                        emptyCard.IsSelected = false;
                    }
                }

                // Next unselect the item in the list.
                listTarget.SelectedItem = null;
            }
        }

        private bool MoveTargetPileCardToCardPileAsAppropriate(ListView listCardPile)
        {
            bool movedCard = false;

            CardPileToggleButton btnTargetPile = null;
            List<Card> listTargetPile = null;
            if (TargetPileC.IsToggled)
            {
                btnTargetPile = TargetPileC;
                listTargetPile = _targetPiles[0];
            }
            else if (TargetPileD.IsToggled)
            {
                btnTargetPile = TargetPileD;
                listTargetPile = _targetPiles[1];
            }
            else if (TargetPileH.IsToggled)
            {
                btnTargetPile = TargetPileH;
                listTargetPile = _targetPiles[2];
            }
            else if (TargetPileS.IsToggled)
            {
                btnTargetPile = TargetPileS;
                listTargetPile = _targetPiles[3];
            }

            if ((listTargetPile != null) && (listTargetPile.Count > 0))
            {
                PlayingCard cardAbove = CreatePlayingCard();
                cardAbove.IsCardVisible = true;
                cardAbove.CardState = CardState.FaceUp;

                cardAbove.Card = listTargetPile[listTargetPile.Count - 1];

                if (listCardPile.SelectedItem != null)
                {
                    PlayingCard cardBelow = listCardPile.SelectedItem as PlayingCard;

                    if (CanMoveCard(cardBelow, cardAbove))
                    {
                        // Move the card from the TargetPile to this CardPile list.
                        listTargetPile.Remove(cardAbove.Card);

                        var itemsAdded = GetListSource(listCardPile);

                        cardBelow.IsObscured = true;

                        // Barker: Without this, the updated height of the obscured card
                        // does not kick in until the app's been resized. Figure out how
                        // to avoid removing/adding the obscured card in the list.
                        itemsAdded.Remove(cardBelow);
                        itemsAdded.Add(cardBelow);

                        itemsAdded.Add(cardAbove);

                        EnableCard(cardAbove, true);

                        var announcement =
                            Resource1.Moved + " " +
                            cardAbove.Name + " " +
                            Resource1.To + " " +
                            cardBelow.Name + ". ";

                        RaiseNotificationEvent(
                             announcement);

                        //listCardPile.Focus(FocusState.Keyboard);

                        if (listTargetPile.Count == 0)
                        {
                            btnTargetPile.Card = null;
                        }
                        else
                        {
                            btnTargetPile.Card = listTargetPile[listTargetPile.Count - 1];
                        }

                        movedCard = true;
                    }
                    else
                    {
                        DeselectDealtCards();
                        UncheckToggleButtons(false);
                    }
                }

                //FocusLastItemInList(listCardPile);
            }

            return movedCard;
        }

        private void MoveCardBetweenDealtCardPiles(ListView listSelectionChanged)
        {
            bool foundOtherDealtCardPileSelected = false;

            // Is any card selected in another CardPile list?
            for (int i = 0; i < cCardPiles; i++)
            {
                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
                if (list != listSelectionChanged)
                {
                    if (list.SelectedItem != null)
                    {
                        foundOtherDealtCardPileSelected = true;

                        PlayingCard cardAbove = list.SelectedItem as PlayingCard;
                        PlayingCard cardBelow = listSelectionChanged.SelectedItem as PlayingCard;

                        if (CanMoveCard(cardBelow, cardAbove))
                        {
                            // Move the card (or cards) from the other list to this CardPile list.
                            PlayingCard cardRevealed = null;

                            var itemsAdded = GetListSource(listSelectionChanged);
                            var itemsRemoved = GetListSource(list);

                            int movingCardVisibleIndex = itemsRemoved.IndexOf(cardAbove);

                            bool cardRevealedIsFaceDown = true;

                            PlayingCard firstItem = null;

                            if (movingCardVisibleIndex > 0)
                            {
                                if (!itemsRemoved[movingCardVisibleIndex - 1].FaceDown)
                                {
                                    cardRevealedIsFaceDown = false;
                                }
                                else
                                {
                                    var cardEnumerator = list.ItemsSource.GetEnumerator();
                                    cardEnumerator.MoveNext();
                                    firstItem = (PlayingCard)cardEnumerator.Current;
                                    --firstItem.FaceDownCount;
                                }
                            }

                            PlayingCard cardNowTop = null;

                            // Is this the only card in the pile?
                            if (movingCardVisibleIndex > 0)
                            {
                                // No, use shadow list.
                                if (cardRevealedIsFaceDown)
                                {
                                    int sourceListIndex = int.Parse(list.StyleId.Replace("CardPile", ""));
                                    var shadowListIndex = this.ViewModel.PlayingCardsB.ElementAt(sourceListIndex - 1);
                                    cardRevealed = (PlayingCard)shadowListIndex[
                                        firstItem.FaceDownCount + movingCardVisibleIndex - 1];

                                    cardAbove.IsObscured = false;
                                }
                                else
                                {
                                    // Remove the card from the list.
                                    itemsRemoved.Remove(cardAbove);

                                    // The face-up card beneath the card 
                                    // being moved is no longer obscured.
                                    cardNowTop = itemsRemoved[movingCardVisibleIndex - 1];
                                    cardNowTop.IsObscured = false;
                                }
                            }

                            // Reduce the height of the card being partially obscured
                            // by the new card in the destination list.
                            if (itemsAdded.Count > 0)
                            {
                                var obscuredCard = itemsAdded[itemsAdded.Count - 1] as PlayingCard;
                                obscuredCard.IsObscured = true;

                                // Barker: Without this, the updated height of the obscured card
                                // does not kick in until the app's been resized. Figure out how
                                // to avoid removing/adding the obscured card in the list.
                                itemsAdded.Remove(obscuredCard);
                                itemsAdded.Add(obscuredCard);
                            }

                            // Create a new card which will be added to the target list.
                            PlayingCard newCard = CreatePlayingCard();
                            newCard.IsCardVisible = true;
                            newCard.Card = new Card();
                            newCard.Card.Suit = cardAbove.Card.Suit;
                            newCard.Card.Rank = cardAbove.Card.Rank;
                            newCard.CardState = CardState.FaceUp;

                            itemsAdded.Add(newCard);

                            int nextCardIndex = movingCardVisibleIndex;

                            // Was the card being moved the only item in the source list?
                            if (cardRevealed != null)
                            {
                                // No, so take action to apparently remove the card from the list.
                                SetCardDetails(cardRevealed, cardAbove);

                                cardAbove.IsObscured = false;

                                // Are there any facedown cards left?
                                if (firstItem.FaceDownCount == 0)
                                {
                                    // No, so remove the card
                                    itemsRemoved.Remove(cardRevealed);
                                }
                                else
                                {
                                    ++nextCardIndex;
                                }
                            }
                            else
                            {
                                // Effectively turn the moving card into the source list's empty item.
                                SetCardDetails(null, cardAbove);

                                ++nextCardIndex;
                            }

                            // Move multiple cards if necessary.

                            var previousCard = newCard;

                            // Barker Shadow List: Re-enable something like this.
                            while (nextCardIndex < itemsRemoved.Count)
                            {
                                previousCard.IsObscured = true;

                                // Barker: The resize hack.
                                itemsAdded.Remove(previousCard);
                                itemsAdded.Add(previousCard);

                                var nextCard = (PlayingCard)itemsRemoved[nextCardIndex];

                                itemsRemoved.Remove(nextCard);
                                nextCard.IsObscured = false;

                                itemsAdded.Add(nextCard);

                                // Barker: Is this hack still needed?
                                itemsAdded.Remove(nextCard);
                                itemsAdded.Add(nextCard);

                                previousCard = nextCard;
                            }

                            if (cardNowTop != null)
                            {
                                itemsRemoved.Remove(cardNowTop);
                                itemsRemoved.Add(cardNowTop);
                            }

                            // setting the SelectItem here to null
                            // so that it doesn't trigger another pass through 
                            // CardPile_SelectionChanged as that was causing
                            // the failure sound to hit, as well as the success sound later

                            ClearListSelection(list);
                            ClearListSelection(listSelectionChanged);

                            FocusLastItemInList(listSelectionChanged);

                            PlaySound(true);
                            cardHasBeenMoved = true;

                            // Have screen readers make a related announcement.
                            var sourceItemsCount = itemsRemoved.Count;
                            if (sourceItemsCount > 0)
                            {
                                var topCard = itemsRemoved[sourceItemsCount - 1];
                                var cardRevealedAnnouncement = topCard.Name;

                                var announcement =
                                    Resource1.Moved + " " +
                                    newCard.Name + " " +
                                    Resource1.To + " " +
                                    cardBelow.Name + ". ";

                                announcement +=
                                    Resource1.Revealed + " " +
                                    cardRevealedAnnouncement +
                                    " " + Resource1.InDealtCardPile + " " +
                                    (i + 1) + ".";

                                RaiseNotificationEvent(
                                     announcement);
                            }
                        }
                        else if (!cardHasBeenMoved)
                        {
                            PlaySound(false);
                            cardHasBeenMoved = false;
                        }

                        ClearListSelection(list);
                        ClearListSelection(listSelectionChanged);

                        IEnumerable<PropertyInfo> pInfos = (listSelectionChanged as ItemsView<Cell>).GetType().GetRuntimeProperties();
                        var templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
                        if (templatedItems != null)
                        {
                            var cells = templatedItems.GetValue(listSelectionChanged);
                            foreach (ViewCell cell in cells as
                                Xamarin.Forms.ITemplatedItemsList<Xamarin.Forms.Cell>)
                            {
                                // Without this, a scroll is required on Android to get the size change 
                                // to take effect.
                                cell.ForceUpdateSize();
                            }
                        }

                    }
                }

                // When the app starts, the height of the ScrollViewer containing the dealt card piles,
                // is not always as high as expected. Until the cause of this is understood, explicitly
                // resize the UI on the first attempt to move a card here, now that all elements have 
                // their actual heights calculated. Barker: Figure out this, and remove the resize here.
                //if (firstMoveToDealtCardPile)
                //{
                //    firstMoveToDealtCardPile = false;

                //    SetCardPileSize();
                //}
            }

            if (!foundOtherDealtCardPileSelected)
            {
                // Have TalkBack announce the selection of the card.
                var cardSelected = listSelectionChanged.SelectedItem as PlayingCard;
                var announcement = cardSelected.Name + " " + Resource1.Selected;
                RaiseNotificationEvent(announcement);

                // A dealt card was selected, but no available move was found, and no other
                // dealt card pile was found to be selected. So check if we should move with 
                // only this card selection.
                if (this.ViewModel.SingleKeyToMove)
                {
                    MoveDealtCardWithSingleKeyPressIfPossible(listSelectionChanged);
                }
            }
        }

        // XBarker:
        private bool _inMoveDealtCardWithSingleKeyPressIfPossible = false;

        // XBarker:

        private void MoveDealtCardWithSingleKeyPressIfPossible(ListView listSelectionChanged)
        {

            //if (_dealingCards || _inMoveDealtCardWithSingleKeyPressIfPossible)
            //{
            //    return;
            //}

            //_inMoveDealtCardWithSingleKeyPressIfPossible = true;

            //PlayingCard selectedDealtCard = listSelectionChanged.SelectedItem as PlayingCard;

            //bool moveCardToTargetPile = false;

            //CardPileToggleButton targetCardButton = null;

            //if (selectedDealtCard.Suit == Suit.Clubs)
            //{
            //    targetCardButton = TargetPileC;
            //}
            //else if (selectedDealtCard.Suit == Suit.Diamonds)
            //{
            //    targetCardButton = TargetPileD;
            //}
            //else if (selectedDealtCard.Suit == Suit.Hearts)
            //{
            //    targetCardButton = TargetPileH;
            //}
            //else if (selectedDealtCard.Suit == Suit.Spades)
            //{
            //    targetCardButton = TargetPileS;
            //}

            //if (targetCardButton != null)
            //{
            //    if (targetCardButton.Card == null)
            //    {
            //        moveCardToTargetPile = (selectedDealtCard.Card.Rank == 1);
            //    }
            //    else
            //    {
            //        moveCardToTargetPile = (selectedDealtCard.Card.Rank == targetCardButton.Card.Rank + 1);
            //    }
            //}

            //if (moveCardToTargetPile)
            //{
            //    MoveDealtCardToTargetPileAsAppropriate(targetCardButton);
            //}
            //else
            //{
            //    bool moveCardToDealtCardPile = false;

            //    for (int i = 0; i < cCardPiles; i++)
            //    {
            //        ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
            //        if (list.Items.Count > 0)
            //        {
            //            PlayingCard topCardInDealtCardPile = (list.Items[list.Items.Count - 1] as PlayingCard);

            //            if (topCardInDealtCardPile.CardState == CardState.KingPlaceHolder)
            //            {
            //                // Move a King to the empty pile.
            //                moveCardToDealtCardPile = (selectedDealtCard.Card.Rank == 13);
            //            }
            //            else
            //            {
            //                if (CanMoveCard(topCardInDealtCardPile, selectedDealtCard))
            //                {
            //                    moveCardToDealtCardPile = true;
            //                }
            //            }

            //            if (moveCardToDealtCardPile)
            //            {
            //                list.SelectedIndex = list.Items.Count - 1;

            //                break;
            //            }
            //        }
            //    }
            //}

            //_inMoveDealtCardWithSingleKeyPressIfPossible = false;
        }

        // XBarker:
        private void FocusLastItemInList(ListView list)
        {
            //int cItems = list.Items.Count;
            //if (cItems > 0)
            //{
            //    list.SelectedIndex = cItems - 1;
            //    ClearListSelection(list);
            //}
        }

        // The AccessKey for a Dealt Card pile list has been triggered. So select and focus the last 
        // item in the associated list.

        // XBarker:

        //private void CardPile_AccessKeyInvoked(UIElement sender, AccessKeyInvokedEventArgs args)
        //{
        //    ListView list = sender as ListView;
        //    if (list != null)
        //    {
        //        if (list.Items.Count > 0)
        //        {
        //            list.Focus(FocusState.Keyboard);

        //            list.SelectedIndex = list.Items.Count - 1;
        //        }
        //    }
        //}

        // XBarker:
        private void PlaySound(bool success)
        {
            //if (PlaySoundEffectsCheckBox.IsChecked == true)
            //{
            //    notifications.PlaySound(success);
            //}

            //// If not success, clear all checked and selected elements.
            //if (!success)
            //{
            //    CardDeckUpturned.IsChecked = false;

            //    UncheckToggleButtons(true);

            //    // Clear all selection from the card pile lists.
            //    for (int i = 0; i < cCardPiles; i++)
            //    {
            //        ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));
            //        ClearListSelection(list);
            //    }
            //}
        }

        private void ClearListSelection(ListView list)
        {
            var card = (list.SelectedItem as PlayingCard);
            if (card != null)
            {
                card.IsSelected = false;

                list.SelectedItem = null;
            }
        }

        private void SetStateDealtCardPiles()
        {
            string stateMessage = "";

            string empty = "empty";
            string pile = "Pile";
            string to = "to";
            string card = "card";
            string cards = "cards";
            string facedown = "face down";

            for (int i = 0; i < cCardPiles; i++)
            {
                stateMessage += pile + " " + (i + 1) + ", ";

                int cFaceDown = 0;
                int indexLastFaceUp = -1;

                ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

                var items = GetListSource(list);

                for (int j = items.Count - 1; j >= 0; j--)
                {
                    var playingcard = (items[j] as PlayingCard);

                    if (j == items.Count - 1)
                    {
                        if (playingcard.CardState == CardState.KingPlaceHolder)
                        {
                            stateMessage += empty;
                        }
                        else
                        {
                            stateMessage += (items[j] as PlayingCard).Card;
                        }
                    }
                    else
                    {
                        if (playingcard.FaceDown)
                        {
                            cFaceDown = playingcard.FaceDownCount;
                        }
                        else
                        {
                            indexLastFaceUp = j;
                        }
                    }
                }

                if ((indexLastFaceUp != -1) && (indexLastFaceUp != items.Count - 1))
                {
                    stateMessage += " " + to + " " + (items[indexLastFaceUp] as PlayingCard).Card;
                }

                stateMessage += ", ";

                if (cFaceDown > 0)
                {
                    stateMessage += cFaceDown + " " +
                        (cFaceDown > 1 ? cards : card) + " " + facedown + " , ";
                }

                // While we're here, set the row index for the cards, (1-based).
                for (int j = 0; j < items.Count; ++j)
                {
                    var playingcard = (items[j] as PlayingCard);
                    if (playingcard.FaceDown)
                    {
                        playingcard.VisibleRowIndex = 1;
                    }
                    else
                    {
                        playingcard.VisibleRowIndex = j + 1;
                    }
                }
            }

            AutomationProperties.SetHelpText(
                DealtCardPileState,
                stateMessage);
        }

        //private void SetStateDealtCardPiles()
        //{
        //    string stateMessage = "Top cards are, ";

        //    string empty = "empty";
        //    string pile = "Pile";

        //    for (int i = 0; i < cCardPiles; i++)
        //    {
        //        stateMessage += pile + " " + (i + 1) + ", ";

        //        ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (i + 1));

        //        var items = GetListSource(list);

        //        if (items.Count > 0)
        //        {
        //            if ((items[items.Count - 1] as PlayingCard).CardState == CardState.KingPlaceHolder)
        //            {
        //                stateMessage += empty;
        //            }
        //            else
        //            {
        //                stateMessage +=  
        //                    (items[items.Count - 1] as PlayingCard).Card;
        //            }

        //            stateMessage += ", ";
        //        }
        //    }

        //    Debug.Write("State: " + stateMessage);

        //    AutomationProperties.SetHelpText(
        //        CardPileGrid,
        //        stateMessage);
        //}
    }
}
