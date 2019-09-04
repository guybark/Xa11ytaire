using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xa11ytaire.Source;
using Xa11ytaire.Source.ViewModels;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Xa11ytaire.Source.Classes;

namespace Xa11ytaire
{
    public partial class MainPage : ContentPage
    {
        //private bool listeningForSpeech;
        //private Sa11ytaireSpeechService speechService;

        //private async void AzureToggleListeningForSpeech()
        //{
        //    if (speechService == null)
        //    {
        //        speechService = new Sa11ytaireSpeechService();
        //    }

        //    if (listeningForSpeech)
        //    {
        //        speechService.StopSpeechInputCustom(this);
        //    }
        //    else
        //    {
        //        listeningForSpeech = true;

        //        var resourceLoader = new ResourceLoader();

        //        // Let the player know that we're listening.
        //        SpeechInputStatus.Text = resourceLoader.GetString("WaitingForSpeech");

        //        // Get the recognized speech.

        //        // Note that we do not wait for this to complete!
        //        speechService.StartSpeechInputCustom(this);
        //    }
        //}

        //public void ReactToSpeechInput(string speechInput, LuisResult results)
        //{
        //    if (results == null)
        //    {
        //        return;
        //    }

        //    string intent = "";
        //    string entity = "";
        //    string entity2 = "";

        //    double? score = 0;

        //    var resourceLoader = new ResourceLoader();

        //    // Do we have an intent that we feel sufficiently confident in?
        //    var result = results.TopScoringIntent;
        //    if (result != null)
        //    {
        //        score = result.Score;
        //        if (score > 0.4)
        //        {
        //            // Seems good enough to me, so let's get the intent.
        //            intent = result.Intent;

        //            var entities = results.Entities;
        //            if ((entities != null) && (entities.Count() > 0))
        //            {
        //                if (intent == "MoveCard")
        //                {
        //                    if (entities.Count() >= 2)
        //                    {
        //                        for (int i = 0; i < entities.Count(); ++i)
        //                        {
        //                            var nextEntity = entities[i];

        //                            string type = nextEntity.Type;
        //                            if (type.Contains("FromLocation"))
        //                            {
        //                                entity = nextEntity.Entity;
        //                            }
        //                            else if (type.Contains("ToLocation"))
        //                            {
        //                                entity2 = nextEntity.Entity;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        intent = "";

        //                        Debug.WriteLine("Error: MoveCard with unexpected entity count, " + entities.Count());
        //                    }
        //                }
        //                else
        //                {
        //                    var firstEntity = entities.First<EntityModel>();

        //                    entity = firstEntity.Entity;
        //                }
        //            }

        //            Debug.WriteLine("Score: " + score + ", Intent \"" + intent +
        //                ", Entity \"" + entity + ", Entity2 \"" + entity2 + "\".");

        //            SpeechInputStatus.Text =
        //                resourceLoader.GetString("WaitingForSpeech") +
        //                " (Heard: \"" + speechInput +
        //                "\", Intent: " + intent +
        //                ", Score: " + score + ")";

        //            switch (intent)
        //            {
        //                case "Utilities.Confirm":

        //                    // Is an app dlg up?
        //                    if (openDlg != null)
        //                    {
        //                        openDlg.Hide();

        //                        RestartGame(true);
        //                    }

        //                    break;

        //                case "Utilities.Cancel":

        //                    // Is an app dlg up?
        //                    if (openDlg != null)
        //                    {
        //                        openDlg.Hide();
        //                    }
        //                    else
        //                    {
        //                        // Unselect and uncheck all cards.
        //                        ReactToCancel();
        //                    }

        //                    break;

        //                case "TurnOverNextCards":

        //                    // Get the AutomationPeer associated with the NextCard button,
        //                    // and programmatically invoke the button through UIA.
        //                    ButtonAutomationPeer peer =
        //                        FrameworkElementAutomationPeer.FromElement(NextCardDeck) as ButtonAutomationPeer;
        //                    if (peer != null)
        //                    {
        //                        peer.Invoke();
        //                    }

        //                    break;

        //                case "RestartGame":

        //                    RestartGame(true /* screenReaderAnnouncement */);

        //                    break;

        //                case "ShowTopOfCardLists":
        //                case "ShowBottomOfCardLists":

        //                    // Bring either the top or bottom of the area containing the 
        //                    // dealt card list into view.
        //                    CardPileGrid.ChangeView(
        //                        null,
        //                        (intent == "ShowTopOfCardLists" ? 0 : CardPileGrid.ScrollableHeight),
        //                        null);

        //                    break;

        //                case "SelectCard":

        //                    SelectCardByIntent(entity);

        //                    break;

        //                case "MoveCard":

        //                    SelectCardByIntent(entity);

        //                    SelectCardByIntent(entity2);

        //                    break;

        //                default:

        //                    break;
        //            }
        //        }
        //        //else
        //        //{
        //        //    string message = "";

        //        //    var code = results.stat["statusCode"];
        //        //    if ((code != null) && (code == "403"))
        //        //    {
        //        //        message = (string)results["message"];
        //        //    }

        //        //    SpeechInputStatus.Text =
        //        //        resourceLoader.GetString("WaitingForSpeech") +
        //        //        " (Call failed, status: " + code + ". Out of quota)";
        //        //}
        //    }
        //}

        //public void SpeechRecoStopped()
        //{
        //    SpeechInputStatus.Text = "";

        //    listeningForSpeech = false;
        //}

        public bool SelectCardByIntent(string entity)
        {
            bool cardIsSelected = false;

            // Is there a card upturned in the remaining card area?
            if (!cardIsSelected)
            {
                if (CardDeckUpturned.IsVisible)
                {
                    // Does the card spoken match the UIA Name of the upturned card?
                    string upturnedCardAutomationName =
                        AutomationProperties.GetName(CardDeckUpturned);
                    if (entity.ToLower() == upturnedCardAutomationName.ToLower())
                    {
                        // Check the upturned card.
                        CardDeckUpturned.IsToggled = true;

                        cardIsSelected = true;
                    }
                }
            }

            // Is the card at the top of one of the target card piles?
            if (!cardIsSelected)
            {
                string[] targetPileSuffix = { "C", "D", "H", "S" };

                for (int i = 0; i < cTargetPiles; i++)
                {
                    string targetButtonName = "TargetPile" + targetPileSuffix[i];

                    var targetButton = (CardPileToggleButton)TargetPiles.FindByName(targetButtonName);
                    string targetButtonAutomationName =
                        AutomationProperties.GetName(targetButton);

                    if (entity.ToLower() == targetButtonAutomationName.ToLower())
                    {
                        targetButton.IsToggled = true;

                        cardIsSelected = true;

                        break;
                    }
                }
            }

            if (!cardIsSelected)
            {
                // Is the card spoken available in the list of upturned dealt cards?
                for (int idxList = 0; idxList < cCardPiles; idxList++)
                {
                    // Check each dealt card list in turn.
                    ListView list = (ListView)CardPileGrid.FindByName("CardPile" + (idxList + 1));

                    var source = list.ItemsSource as ObservableCollection<PlayingCard>;

                    string listAutomationName = AutomationProperties.GetName(list);
                    if (entity.ToLower() == listAutomationName.ToLower())
                    {
                        // Select the last item in the list, which might be a card, 
                        // or it might be the slot on which a king 
                        if (source.Count > 0)
                        {
                            var item = source[source.Count - 1];

                            list.SelectedItem = item;
                        }

                        cardIsSelected = true;

                        break;
                    }

                    for (int idxCard = 0; idxCard < source.Count; idxCard++)
                    {
                        var item = source[idxCard];

                        PlayingCard cardInDealtCardPile = (item as PlayingCard);

                        // We're only interested in face-up cards here.
                        if (!cardInDealtCardPile.FaceDown)
                        {
                            string cardName = cardInDealtCardPile.Name.ToLower();
                            if (entity.ToLower() == cardName)
                            {
                                // Select the card of interest in the list.
                                list.SelectedItem = item;

                                cardIsSelected = true;

                                break;
                            }
                        }
                    }
                }
            }

            return cardIsSelected;
        }
    }
}