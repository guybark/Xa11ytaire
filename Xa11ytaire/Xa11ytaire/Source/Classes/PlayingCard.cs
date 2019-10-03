using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Xa11ytaire.Source.Classes
{
    // Barker: Remove use of UserControl.
    // Barker: DependencyProperty->BindableProperty, and DependencyObject->BindableObject.

    public class PlayingCard : /*UserControl,*/ BindableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private Settings settings;

        public PlayingCard(Settings settings)
        {
            this.settings = settings;
        }

        // Barker: The ListView items don't seem to have an IsSelected, so add one
        // in order to bind to it.
        public static readonly BindableProperty IsSelectedProperty =
            BindableProperty.Create(
            "IsSelected",
            typeof(bool),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty FaceDownProperty =
            BindableProperty.Create(
            "FaceDown",
            typeof(bool),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty FaceDownCountProperty =
            BindableProperty.Create(
            "FaceDownCount",
            typeof(int),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty IsCardVisibleProperty =
            BindableProperty.Create(
            "IsCardVisible",
            typeof(bool),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty CardStateProperty =
            BindableProperty.Create(
            "CardState",
            typeof(CardState),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty IsKingDropZoneProperty =
            BindableProperty.Create(
            "IsKingDropZone",
            typeof(bool),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty IsScannedProperty =
            BindableProperty.Create(
            "IsScanned",
            typeof(bool),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty IsObscuredProperty =
            BindableProperty.Create(
            "IsObscured",
            typeof(bool),
            typeof(PlayingCard),
            null
        );

        public static readonly BindableProperty CardProperty =
            BindableProperty.Create(
            "Card",
            typeof(Card),
            typeof(PlayingCard),
            null
        );

        public CardState CardState
        {
            get { return (CardState)GetValue(CardStateProperty); }
            set
            {
                SetValue(CardStateProperty, value);

                OnPropertyChanged("CardState");
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);

                OnPropertyChanged("IsSelected");
            }
        }

        public bool FaceDown
        {
            get { return (bool)GetValue(FaceDownProperty); }
            set
            {
                SetValue(FaceDownProperty, value);

                OnPropertyChanged("FaceDown");
            }
        }

        public int FaceDownCount
        {
            get { return (int)GetValue(FaceDownCountProperty); }
            set
            {
                SetValue(FaceDownCountProperty, value);

                OnPropertyChanged("FaceDownCount");

                // Other read-only properties change with this.
                OnPropertyChanged("Name");
                OnPropertyChanged("HelpText");
            }
        }

        public bool IsCardVisible
        {
            get { return (bool)GetValue(IsCardVisibleProperty); }
            set
            {
                SetValue(IsCardVisibleProperty, value);

                OnPropertyChanged("IsCardVisible");
            }
        }

        public bool IsScanned
        {
            get { return (bool)GetValue(IsScannedProperty); }
            set
            {
                SetValue(IsScannedProperty, value);
                OnPropertyChanged("IsScanned");
            }
        }

        public bool IsObscured
        {
            get { return (bool)GetValue(IsObscuredProperty); }
            set
            {
                SetValue(IsObscuredProperty, value);
                OnPropertyChanged("IsObscured");
            }
        }

        public bool IsKingDropZone
        {
            get { return (bool)GetValue(IsKingDropZoneProperty); }
            set
            {
                SetValue(IsKingDropZoneProperty, value);
                OnPropertyChanged("IsKingDropZone");

                // Other read-only properties change with this.
                OnPropertyChanged("Card");
                OnPropertyChanged("Name");
                OnPropertyChanged("HelpText");
            }
        }

        public Card Card
        {
            get { return (Card)GetValue(CardProperty); }
            set
            {
                SetValue(CardProperty, value);
                OnPropertyChanged("Card");

                // Other read-only properties change with this.
                OnPropertyChanged("Name");
                OnPropertyChanged("HelpText");
            }
        }

        public int InitialIndex { get; set; }

        public int ListIndex { get; set; }

        private int visibleRowIndex = 0;

        public int VisibleRowIndex
        {
            get { return visibleRowIndex; }
            set
            {
                visibleRowIndex = value;
                OnPropertyChanged("HelpText");
            }
        }

        public Suit Suit
        {
            get => Card.Suit;
        }

        public int Rank
        {
            get => Card.Rank;
        }

        public string Name
        {
            get
            {
                string name;

                if (this.FaceDown)
                {
                    name = this.FaceDownCount + " " + Resource1.FaceDown;
                }
                else if (this.Card.Rank != 0)
                {
                    name = this.Card.ToString();
                }
                else
                {
                    name = Resource1.Empty;
                }

                return name;
            }
        }

        public string HelpText
        {
            get
            {
                string helpText = "";

                if (!this.FaceDown && (this.Card.Rank == 0))
                {
                    helpText = Resource1.EmptyCardHelpText;
                }
                else
                {
                    if (settings.IncludeRowNumber)
                    {
                        helpText = "Row " + this.VisibleRowIndex;
                    }
                }

                return helpText;
            }
        }

        public void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
