using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRlib
{
    public class OcrResults
    {
        private bool imageAccepted;
        private bool testResult;
        private string pathOfImage;
        private List<OcrResultsPerLine> table;

        public OcrResults()
        {

        }

        public bool ImageAccepted { get => imageAccepted; set => imageAccepted = value; }
        public bool TestResult { get => testResult; set => testResult = value; }
        public string PathOfImage { get => pathOfImage; set => pathOfImage = value; }
        public List<OcrResultsPerLine> Table { get => table; set => table = value; }
    }
}
