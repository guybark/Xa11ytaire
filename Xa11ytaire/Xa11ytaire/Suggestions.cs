using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Xamarin.Forms;

using Xa11ytaire.Source;
using Xa11ytaire.Source.Classes;

namespace Xa11ytaire
{
    public sealed partial class MainPage : ContentPage
    {

        private bool GetMoveSuggestion(out string suggestion)
        {
            // Important: When checking for a move between dealt card piles,
            // only check the lowest face-up card. We don't want to just be 
            // moving cards back and forth between piles.

            // Important: No suggestion is ever made relating to moving a 
            // card from a target pile down to the dealt card piles, even
            // though such a move may be the only way to win a game.

            // Todo:
            // - Use localized resources.
            // - Add helper functions to avoid the code duplication below.

            suggestion = "";

            bool canMoveCard = false;

            // First check whether the upturned card can be moved to 
            // a target card pile.
            if (_deckUpturned.Count > 0)
            {
                Card upturnedCard = _deckUpturned[_deckUpturned.Count - 1];

                canMoveCard = CanRemainingCardBeMoved(
                    true, upturnedCard, out suggestion);
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

            // If necessary, check if a card can be moved from the remaining card pile.
            if (!canMoveCard)
            {
                canMoveCard = CheckRemainingCardPile(false, out suggestion);
            }

            return canMoveCard;
        }

        private bool CheckRemainingCardPile(
            bool checkingTopmostUpturnedCard,
            out string suggestion)
        {
            suggestion = "";

            bool canMoveCard = false;

            // First check from where we currently are in the remaining card pile.
            // (Assume we've already checked the topmost upturned card.)

            int countCardsToExamineInRemainingCardPile = _deckRemaining.Count;

            // Can we turn over at least one card?
            while (countCardsToExamineInRemainingCardPile > 0)
            {
                // Yes, so how many cards can we turn over?
                int countCardsToTurn;

                if (this.settings.TurnOverOneCard)
                {
                    countCardsToTurn = 1;
                }
                else
                {
                    countCardsToTurn = (countCardsToExamineInRemainingCardPile >= 3 ? 
                        3 : countCardsToExamineInRemainingCardPile);
                }

                // Access what would be the topmost card when get get the next cards.
                Card card = _deckRemaining[countCardsToExamineInRemainingCardPile - countCardsToTurn];

                // Can this card go anywhere?
                canMoveCard = CanRemainingCardBeMoved(
                    checkingTopmostUpturnedCard, card, out suggestion);
                if (canMoveCard)
                {
                    break;
                }

                countCardsToExamineInRemainingCardPile -= countCardsToTurn;
            }

            // If necessary, move through the upturned cards from the bottom up.
            int indexCardToExamineInUpturnedCardPile = (this.settings.TurnOverOneCard ? 0 : 2);

            if (!canMoveCard)
            {
                while (indexCardToExamineInUpturnedCardPile < _deckUpturned.Count)
                {
                    Card card = _deckUpturned[indexCardToExamineInUpturnedCardPile];

                    // Can this card go anywhere?
                    canMoveCard = CanRemainingCardBeMoved(
                        checkingTopmostUpturnedCard, card, out suggestion);
                    if (canMoveCard)
                    {
                        break;
                    }

                    indexCardToExamineInUpturnedCardPile += (this.settings.TurnOverOneCard ? 1 : 3);
                }
            }

            // Do we need to move through the remaining card pile again now?
            if (!canMoveCard)
            {
                if (!this.settings.TurnOverOneCard)
                {
                    int offsetToNextCardInRemainingCardPile =
                        (indexCardToExamineInUpturnedCardPile + 1) % 3;

                    if (offsetToNextCardInRemainingCardPile > 0)
                    {
                        countCardsToExamineInRemainingCardPile =
                            _deckRemaining.Count - offsetToNextCardInRemainingCardPile;

                        // Can we turn over at least one card?
                        while (countCardsToExamineInRemainingCardPile > 0)
                        {
                            // Yes, so how many cards can we turn over?
                            int countCardsToTurn;

                            if (this.settings.TurnOverOneCard)
                            {
                                countCardsToTurn = 1;
                            }
                            else
                            {
                                countCardsToTurn = (countCardsToExamineInRemainingCardPile >= 3 ?
                                    3 : countCardsToExamineInRemainingCardPile);
                            }

                            Card card = _deckRemaining[countCardsToExamineInRemainingCardPile - countCardsToTurn];

                            // Can this card go anywhere?
                            canMoveCard = CanRemainingCardBeMoved(
                                checkingTopmostUpturnedCard, card, out suggestion);
                            if (canMoveCard)
                            {
                                break;
                            }

                            countCardsToExamineInRemainingCardPile -= countCardsToTurn;
                        }
                    }
                }
            }

            return canMoveCard;
        }

        private bool CanRemainingCardBeMoved(
            bool checkingTopmostUpturnedCard,
            Card card, 
            out string suggestion)
        {
            suggestion = "";

            bool canMoveCard = false;

            CardPileToggleButton targetCardButton = null;

            if (card.Suit == Suit.Clubs)
            {
                targetCardButton = TargetPileC;
            }
            else if (card.Suit == Suit.Diamonds)
            {
                targetCardButton = TargetPileD;
            }
            else if (card.Suit == Suit.Hearts)
            {
                targetCardButton = TargetPileH;
            }
            else if (card.Suit == Suit.Spades)
            {
                targetCardButton = TargetPileS;
            }

            if ((card != null) && (targetCardButton != null))
            {
                // Is there a card on this target card pile yet?
                if (targetCardButton.Card == null)
                {
                    // No, so a move is only possible if the upturned card is an ace.
                    if (card.Rank == 1)
                    {
                        canMoveCard = true;
                    }
                }
                else
                {
                    // Check if the upturned card can be moved on top of the card
                    // that's currently at the top of the target card pile.
                    if (card.Rank == targetCardButton.Card.Rank + 1)
                    {
                        canMoveCard = true;
                    }
                }

                if (canMoveCard)
                {
                    if (checkingTopmostUpturnedCard)
                    {
                        suggestion = "Consider moving the upturned " +
                            card.ToString() + " to the " +
                            targetCardButton.Suit + " pile.";
                    }
                    else
                    {
                        suggestion = "Consider moving the " +
                            card.ToString() + 
                            " from somewhere in the remaining card pile, to the " +
                            targetCardButton.Suit + " pile.";
                    }
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
                            if (card.Rank == 13)
                            {
                                canMoveCard = true;
                            }
                        }
                        else
                        {
                            var playingCardUpturned = CreatePlayingCard();
                            playingCardUpturned.Card = card;

                            if (CanMoveCard(topCardInDealtCardPile, playingCardUpturned))
                            {
                                canMoveCard = true;
                            }
                        }

                        if (canMoveCard)
                        {
                            if (checkingTopmostUpturnedCard)
                            {
                                suggestion = "Consider moving the upturned " +
                                    card.ToString() + " to the " +
                                    topCardInDealtCardPile.Name + " in pile " +
                                    (i + 1);
                            }
                            else
                            {
                                suggestion = "Consider moving the " +
                                    card.ToString() +
                                    " from somewhere in the remaining card pile, to the " +
                                    topCardInDealtCardPile.Name + " in pile " +
                                    (i + 1);
                            }

                            break;
                        }
                    }
                }
            }

            return canMoveCard;
        }
    }
}
