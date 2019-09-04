using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xa11ytaire.Source.Classes;

namespace Xa11ytaire.Source.ViewModels
{
    public class PlayingCardViewModel : INotifyPropertyChanged
    {
        private bool _scanModeOn;

        public bool ScanModeOn
        {
            get
            {
                return _scanModeOn;
            }
            set
            {
                _scanModeOn = value;
                OnPropertyChanged("ScanModeOn");
            }
        }

        private bool _singleKeyToMove = false;

        public bool SingleKeyToMove
        {
            get
            {
                return _singleKeyToMove;
            }
            set
            {
                _singleKeyToMove = value;
                OnPropertyChanged("SingleKeyToMove");
            }
        }

        private bool _selectWithoutAltKey = false;

        public bool SelectWithoutAltKey
        {
            get
            {
                return _selectWithoutAltKey;
            }
            set
            {
                _selectWithoutAltKey = value;
                OnPropertyChanged("SelectWithoutAltKey");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName = null)
        {
            //Debug.WriteLine("PlayingCardViewModel PropertyChange:" + propertyName);

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal ObservableCollection<PlayingCard> PlayingCards1 { get => playingCards[0]; }
        internal ObservableCollection<PlayingCard> PlayingCards2 { get => playingCards[1]; }
        internal ObservableCollection<PlayingCard> PlayingCards3 { get => playingCards[2]; }
        internal ObservableCollection<PlayingCard> PlayingCards4 { get => playingCards[3]; }
        internal ObservableCollection<PlayingCard> PlayingCards5 { get => playingCards[4]; }
        internal ObservableCollection<PlayingCard> PlayingCards6 { get => playingCards[5]; }
        internal ObservableCollection<PlayingCard> PlayingCards7 { get => playingCards[6]; }

        private ObservableCollection<PlayingCard>[] playingCards;

        internal ObservableCollection<PlayingCard>[] PlayingCards { get => playingCards; set => playingCards = value; }

        public PlayingCardViewModel()
        {
            PlayingCards = new ObservableCollection<PlayingCard>[7];

            for (int i = 0; i < PlayingCards.Length; ++i)
            {
                PlayingCards[i] = new ObservableCollection<PlayingCard>();
            }

            PlayingCardsB = new ObservableCollection<PlayingCard>[7];

            for (int i = 0; i < PlayingCardsB.Length; ++i)
            {
                PlayingCardsB[i] = new ObservableCollection<PlayingCard>();
            }
        }

        private void PlayingCards1_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        // Barker: Test out having two sets of lists...
        internal ObservableCollection<PlayingCard> PlayingCardsB1 { get => playingCardsB[0]; }
        internal ObservableCollection<PlayingCard> PlayingCardsB2 { get => playingCardsB[1]; }
        internal ObservableCollection<PlayingCard> PlayingCardsB3 { get => playingCardsB[2]; }
        internal ObservableCollection<PlayingCard> PlayingCardsB4 { get => playingCardsB[3]; }
        internal ObservableCollection<PlayingCard> PlayingCardsB5 { get => playingCardsB[4]; }
        internal ObservableCollection<PlayingCard> PlayingCardsB6 { get => playingCardsB[5]; }
        internal ObservableCollection<PlayingCard> PlayingCardsB7 { get => playingCardsB[6]; }

        private ObservableCollection<PlayingCard>[] playingCardsB;

        internal ObservableCollection<PlayingCard>[] PlayingCardsB { get => playingCardsB; set => playingCardsB = value; }



    }
}
