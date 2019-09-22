using System;
using System.Collections.Generic;
using System.Text;

namespace Xa11ytaire
{
    public class Settings
    {
        private bool showSuggestionsButton = false;
        private bool turnOverOneCard = false;
        private bool includeRowNumber = false;

        public bool ShowSuggestionsButton { get => showSuggestionsButton; set => showSuggestionsButton = value; }

        public bool TurnOverOneCard { get => turnOverOneCard; set => turnOverOneCard = value; }

        public bool IncludeRowNumber { get => includeRowNumber; set => includeRowNumber = value; }
    }
}
