using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Xa11ytaire.Source.Classes
{
    public class CardPileToggleButton : ImageButton, INotifyPropertyChanged
    {
        // Barker: This first stuff turns a Button into a toggleable button.
        public event EventHandler<ToggledEventArgs> Toggled;

        public static BindableProperty IsToggledProperty =
            BindableProperty.Create("IsToggled", typeof(bool), typeof(CardPileToggleButton), false,
                                    propertyChanged: OnIsToggledChanged);

        public CardPileToggleButton()
        {
            Clicked += (sender, args) => IsToggled ^= true;

            this.BindingContext = this;
            this.SetBinding(NameProperty, "Name", BindingMode.TwoWay);
        }

        public bool IsToggled
        {
            set { SetValue(IsToggledProperty, value); }
            get { return (bool)GetValue(IsToggledProperty); }
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            VisualStateManager.GoToState(this, "ToggledOff");
        }

        static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            CardPileToggleButton toggleButton = (CardPileToggleButton)bindable;
            bool isToggled = (bool)newValue;

            // Fire event
            toggleButton.Toggled?.Invoke(toggleButton, new ToggledEventArgs(isToggled));

            // Set the visual state
            VisualStateManager.GoToState(toggleButton, isToggled ? "ToggledOn" : "ToggledOff");
        }

        public static readonly BindableProperty NameProperty = 
            BindableProperty.Create(
                "Name",
                typeof(string),
                typeof(CardPileToggleButton),
                null,
                BindingMode.OneWay
        );

        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);

                OnPropertyChanged("Name");
            }
        }


        // Barker: Now back to the original CardPileToggleButton stuff.

        private Card card;
        private Suit suit;

        public Card Card
        {
            get
            {
                return this.card;
            }
            set
            {
                this.card = value;
                this.OnPropertyChanged("Card");
                this.OnPropertyChanged("CardContent");
                this.OnPropertyChanged("CardPileAccessibleName");
                this.OnPropertyChanged("CardPileImage");

                if (this.Card != null)
                {
                    this.Name = this.Card.ToString();
                }
            }
        }

        public Suit Suit
        {
            get
            {
                return this.suit;
            }
            set
            {
                this.suit = value;
                this.OnPropertyChanged("Suit");
            }
        }

        public string CardPileAccessibleName
        {
            get
            {
                string cardPileAccessibleName;

                // Is this card pile empty?
                if (this.Card == null)
                {
                    // We'll load up a suit-specific localized string which indicates
                    // that no card is in this pile.

                    switch (this.Suit)
                    {
                        case Suit.Clubs:
                            cardPileAccessibleName = Resource1.ClubsPile;
                            break;

                        case Suit.Diamonds:
                            cardPileAccessibleName = Resource1.DiamondsPile;
                            break;

                        case Suit.Hearts:
                            cardPileAccessibleName = Resource1.HeartsPile;
                            break;

                        default:
                            cardPileAccessibleName = Resource1.SpadesPile;
                            break;
                    }
                }
                else
                {
                    // There is a card in this pile, so simply get the card's friendly name.
                    cardPileAccessibleName = this.Card.ToString();
                }

                return cardPileAccessibleName;
            }
        }

        public FileImageSource CardPileImage
        {
            get
            {
                string cardAsset;

                if (Settings.HideUI)
                {
                    cardAsset = "unknown";
                }
                else if (this.Card == null)
                {
                    // This card pile is empty.
                    switch (this.Suit)
                    {
                        case Suit.Clubs:
                            cardAsset = "emptytargetcardpileclubs";
                            break;

                        case Suit.Diamonds:
                            cardAsset = "emptytargetcardpilediamonds";
                            break;

                        case Suit.Hearts:
                            cardAsset = "emptytargetcardpilehearts";
                            break;

                        default:
                            cardAsset = "emptytargetcardpilespades";
                            break;
                    }
                }
                else
                {
                    switch (card.Suit)
                    {
                        case Suit.Clubs:
                            cardAsset = "clubs";
                            break;

                        case Suit.Diamonds:
                            cardAsset = "diamonds";
                            break;

                        case Suit.Hearts:
                            cardAsset = "hearts";
                            break;

                        default:
                            cardAsset = "spades";
                            break;
                    }

                    switch (card.Rank)
                    {
                        case 1:
                            cardAsset += "ace";
                            break;

                        case 2:
                            cardAsset += "two";
                            break;

                        case 3:
                            cardAsset += "three";
                            break;

                        case 4:
                            cardAsset += "four";
                            break;

                        case 5:
                            cardAsset += "five";
                            break;

                        case 6:
                            cardAsset += "six";
                            break;

                        case 7:
                            cardAsset += "seven";
                            break;

                        case 8:
                            cardAsset += "eight";
                            break;

                        case 9:
                            cardAsset += "nine";
                            break;

                        case 10:
                            cardAsset += "ten";
                            break;

                        case 11:
                            cardAsset += "jack";
                            break;

                        case 12:
                            cardAsset += "queen";
                            break;

                        default:
                            cardAsset += "king";
                            break;
                    }
                }

                string cardFileName = cardAsset + ".png";

                var image = (Device.RuntimePlatform == Device.Android ?
                    FileImageSource.FromFile(cardFileName) :
                    FileImageSource.FromFile("Assets/Images/" + cardFileName));

                return image as FileImageSource;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName = null)
        {
            //Debug.WriteLine("CardPileToggleButton PropertyChange:" + propertyName);

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
