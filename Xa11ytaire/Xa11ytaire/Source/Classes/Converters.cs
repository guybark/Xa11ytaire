using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Xa11ytaire.Source.Classes
{
    public class IsToggledToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isToggled = (bool)value;

            return (isToggled ? "Magenta" : "Black");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsSelectedToPaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isToggled = (bool)value;
            
            return (isToggled ? 
                new Thickness(4, 8, 4, 0) : new Thickness(0, 1, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UpturnedCardToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsObscuredToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isObscured = (bool)value;

            int height;

            if (isObscured)
            {
                height = MainPage.LastCardHeight / 5;
            }
            else
            {
                height = MainPage.LastCardHeight;

                // Debug.WriteLine("Last card height: " + height);
            }

            return height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsFaceDownToCountLabelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFaceDown = (bool)value;

            return isFaceDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsFaceDownToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFaceDown = (bool)value;

            return !isFaceDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsFaceDownToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFaceDown = (bool)value;

            return !isFaceDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsFaceDownToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFaceDown = (bool)value;

            return (isFaceDown ? Color.LightGreen : Color.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardToCardImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Card card = (Card)value;

            string cardAsset;

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

                case Suit.Spades:
                    cardAsset = "spades";
                    break;

                default:
                    cardAsset = "";
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

                case 13:
                    cardAsset += "king";
                    break;

                default:
                    cardAsset = "emptydealtcardpile";
                    break;
            }

            if (string.IsNullOrEmpty(cardAsset))
            {
                return null;
            }

            var image = Device.RuntimePlatform == Device.Android ?
                FileImageSource.FromFile(cardAsset + ".png") :
                FileImageSource.FromFile("Assets/Images/" + cardAsset + ".png");

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NextCardIsEmptyToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = (bool)value;

            string cardAsset = isEmpty ? "EmptyDealtCardPile" : "CardBack";

            var image = Device.RuntimePlatform == Device.Android ?
                FileImageSource.FromFile(cardAsset + ".png") :
                FileImageSource.FromFile("Assets/Images/" + cardAsset + ".png");

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
