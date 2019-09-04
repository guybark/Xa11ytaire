using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace Xa11ytaire.Source
{
    public sealed class Card
    {
        public Suit Suit;
        public int Rank;

        public override string ToString()
        {
            string rank;

            switch (Rank)
            {
                case 1:
                    {
                        rank = Resource1.Ace;
                        break;
                    }
                case 2:
                    {
                        rank = Resource1.Two;
                        break;
                    }
                case 3:
                    {
                        rank = Resource1.Three;
                        break;
                    }
                case 4:
                    {
                        rank = Resource1.Four;
                        break;
                    }
                case 5:
                    {
                        rank = Resource1.Five;
                        break;
                    }
                case 6:
                    {
                        rank = Resource1.Six;
                        break;
                    }
                case 7:
                    {
                        rank = Resource1.Seven;
                        break;
                    }
                case 8:
                    {
                        rank = Resource1.Eight;
                        break;
                    }
                case 9:
                    {
                        rank = Resource1.Nine;
                        break;
                    }
                case 10:
                    {
                        rank = Resource1.Ten;
                        break;
                    }
                case 11:
                    {
                        rank = Resource1.Jack;
                        break;
                    }
                case 12:
                    {
                        rank = Resource1.Queen;
                        break;
                    }
                case 13:
                    {
                        rank = Resource1.King;
                        break;
                    }
                default:
                    {
                        rank = "";

                        break;
                    }
            }

            string ofText = Resource1.Of;
            string formattedString = "{0}" + " " + ofText + " " + "{1}";

            string suitString;

            switch (Suit)
            {
                case Suit.Clubs:
                    suitString = Resource1.Clubs;
                    break;
                case Suit.Diamonds:
                    suitString = Resource1.Diamonds;
                    break;
                case Suit.Hearts:
                    suitString = Resource1.Hearts;
                    break;
                default:
                    suitString = Resource1.Spades;
                    break;
            }

            return string.Format(formattedString, rank, suitString);
        }
    }
}
