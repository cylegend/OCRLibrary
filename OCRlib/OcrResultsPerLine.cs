using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRlib
{
    public class OcrResultsPerLine
    {
        private int index = 0;
        private string actualText;
        private string expectedText;
        private string comment;
        private Enum.MistakeType category;
        private bool passedResult;

        public OcrResultsPerLine()
        {

        }

        /*
         * Getters and setters for all the variables.
         */
        public int Index { get => index; set => index = value; }
        public string ActualText { get => actualText; set => actualText = value; }
        public string ExpectedText { get => expectedText; set => expectedText = value; }
        public string Comment { get => comment; set => comment = value; }
        public Enum.MistakeType Category { get => category; set => category = value; }
        public bool PassedResult { get => passedResult; set => passedResult = value; }
    }
}
