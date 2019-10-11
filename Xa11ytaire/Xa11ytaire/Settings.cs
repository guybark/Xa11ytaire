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
        static private bool hideUI = false;

        public bool ShowSuggestionsButton { get => showSuggestionsButton; set => showSuggestionsButton = value; }

        public bool TurnOverOneCard { get => turnOverOneCard; set => turnOverOneCard = value; }

        public bool IncludeRowNumber { get => includeRowNumber; set => includeRowNumber = value; }

        public static bool HideUI { get => hideUI; set => hideUI = value; }
    }
}
