using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Xa11ytaire.Source.Classes
{
    public class NextCardPileButton : ImageButton, INotifyPropertyChanged
    {
        private bool isEmpty = false;

        public bool IsEmpty
        {
            get
            {
                return this.isEmpty;
            }
            set
            {
                this.isEmpty = value;

                this.OnPropertyChanged("IsEmpty");

                // Barker: Trying to bind the filesourceimage results
                // in a null reference exception. For now, manually 
                // set up the relevant data.
                this.SetRemainingCardPileData();
            }
        }

        private void SetRemainingCardPileData()
        {
            string cardAsset = this.isEmpty ? "emptydealtcardpile" : "cardback";

            string cardFileName = cardAsset + ".png";

            var image = (Device.RuntimePlatform == Device.Android ?
                FileImageSource.FromFile(cardFileName) :
                FileImageSource.FromFile("Assets/Images/" + cardFileName));

            this.Source = image as FileImageSource;

            var accessibleName = this.isEmpty ?
                Resource1.EmptyNextCard : Resource1.NextCard;

            AutomationProperties.SetName(this, accessibleName);
        }

        //public FileImageSource RemainingCardPileImage
        //{
        //    get
        //    {
        //        string cardAsset = "cardback";

        //        string cardFileName = cardAsset + ".png";

        //        var image = (Device.RuntimePlatform == Device.Android ?
        //            FileImageSource.FromFile(cardFileName) :
        //            FileImageSource.FromFile("Assets/Images/" + cardFileName));

        //        return image as FileImageSource;
        //    }
        //}


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName = null)
        {
            //Debug.WriteLine("NextCardPileButton PropertyChange:" + propertyName);

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
