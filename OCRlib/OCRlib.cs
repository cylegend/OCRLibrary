using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace OCRlib
{
    public class OCRlib
    {
        Thread saveFramesThread;
        private Capture capture;
        private string pathToBeyondCompare;
        private string pythonPath;
        private string pythonScript;
        private string beyondCompareScript;

        public OCRlib()
        {

        }

        /// <summary>
        /// Starts the capturing of frames.
        /// </summary>
        /// <param name="pathToSaveTo"></param>
        /// <param name="frequencyOfFrames"></param>
        /// <param name="cameraToUse"></param>
        public void StartCameraCapture(string pathToSaveTo, uint frequencyOfFrames)
        {
            if (saveFramesThread == null)
            {
                saveFramesThread = new Thread(() => SaveFrames(pathToSaveTo, frequencyOfFrames));
            }
            if (!saveFramesThread.IsAlive)
            {
                saveFramesThread = new Thread(() => SaveFrames(pathToSaveTo, frequencyOfFrames));
                saveFramesThread.Start();
            }
        }

        /// <summary>
        /// Sets which camera the device will use for the capturing of frames.
        /// </summary>
        /// <param name="cameraToUse"></param>
        public void SetCamera(int cameraToUse)
        {
            if (capture == null)
            {
                capture = new Capture(cameraToUse);
            }
        }
        
        /// <summary>
        /// Displays the camera window.
        /// </summary>
        public void ShowCamera()
        {
            if (capture != null)
            {
                CameraForm form = new CameraForm(capture);
                form.ShowDialog();
            }
        }

        /// <summary>
        /// Sets the python path and the script's path
        /// </summary>
        /// <param name="pythonPath"></param>
        /// <param name="pythonScript"></param>
        /// <param name="beyondCompareScript"></param>
        public void SetPythonPathAndScripts(string pythonPath, string pythonScript, string beyondCompareScript)
        {
            this.pythonPath = pythonPath;
            this.pythonScript = pythonScript;
            this.beyondCompareScript = beyondCompareScript;

        }

        /// <summary>
        /// Saving every frequencyOfFrames frame of the video to be able to create the panorama.
        /// Running for 30 seconds if it is not stopped by the user.
        /// pathToSaveTo variable needs to end with /
        /// </summary>
        /// <param name="pathToSaveTo"></param>
        /// <param name="frequencyOfFrames"></param>
        private void SaveFrames(string pathToSaveTo, uint frequencyOfFrames)
        {
            int frame = 0;
            string framePicture = "";
            while (capture.Grab() && frame < 900)
            {
                frame++;
                var image = capture.QueryFrame().ToImage<Bgr, byte>();
                var bitmap = image.ToBitmap();
                // Formating the picure as day month year hour minute seconds.
                string timestamp = DateTime.Now.ToString("dd MM yyyy HH mm ss");
                //Taking every frequencyOfFrames frame.
                if (frame % frequencyOfFrames == 0)
                {
                    // Saving the path.
                    framePicture = pathToSaveTo + timestamp + " " + frame + ".png";
                    bitmap.Save(framePicture, ImageFormat.Png);
                }
                bitmap.Dispose();
                image.Dispose();
            }
            saveFramesThread.Abort();
        }

        /// <summary>
        /// Stops the camera recording.
        /// </summary>
        public void StopCameraCapture()
        {
            if (saveFramesThread != null)
            {
                saveFramesThread.Abort();
                saveFramesThread = null;
            }
        }

        /// <summary>
        /// Merges the image in a folder to create a single image. Timeouts after a specific amount of
        /// time has passed, defined by the user.
        /// </summary>
        /// <param name="pathToUse"></param>
        /// <param name="maxRunTimeSeconds"></param>
        /// <returns>Panorama Image</returns>
        public Image MergeImagesWithTimeout(string pathToUse, uint maxRunTimeSeconds)
        {
            //Runs asynchronously.
            var task = Task.Run(() =>
            {
                MergeImages mergeImages = new MergeImages(pathToUse);
                Image panoramaPicture;
                return panoramaPicture = mergeImages.CreatePanoramaFromFolder();
            });
            //Waits for the task to be finished in maxRunTimeSeconds. 
            bool isCompletedSuccessfully = task.Wait(TimeSpan.FromSeconds(maxRunTimeSeconds));

            if (isCompletedSuccessfully)
            {
                return task.Result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Merges the image in a folder to create a single image. Algorithm will run until
        /// the photographs in the folder are merged.
        /// </summary>
        /// <param name="pathToUse"></param>
        /// <returns></returns>
        public Image MergeImagesNoTimeout(string pathToUse)
        {
            MergeImages mergeImages = new MergeImages(pathToUse);
            Image panoramaPicture;
            return panoramaPicture = mergeImages.CreatePanoramaFromFolder();
        }

        /// <summary>
        /// Performs ocr on the path of the image given.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public string PerformOcr(string imagePath)
        {
            string ocrText;
            //Starting the python process
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonPath;
            //Passing the arguments which is the python file and the path of the image taken.
            start.Arguments = string.Format("{0} {1}", pythonScript + " ", "\"" + imagePath + "\"");
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    ocrText = reader.ReadToEnd();
                }
            }
            return ocrText;
        }

        /// <summary>
        /// Sets path to the Beyond Compare exe (BCompare.exe)
        /// </summary>
        /// <param name="path"></param>
        public void SetBeyondComparePath(string path)
        {
            this.pathToBeyondCompare = path;
        }

        /// <summary>
        /// Compares the text of the test case to the 
        /// text produced by the ocr.
        /// </summary>
        /// <param name="fullPathOfOriginalTextFile"></param>
        /// <param name="fullPathOfOcrTextFile"></param>
        /// <param name="fullPathOfOcrImage"></param>
        /// <returns></returns>
        public OcrResults CompareOcrWithActual(string fullPathOfOriginalTextFile, string ocrText, string fullPathOfOcrImage)
        {
            OcrResults ocrResults = new OcrResults();
            //compares a temporary txt file to store the ocrText found.
            string fullPathOfOcrTextFile = Path.GetTempPath() + "temp.txt";
            File.WriteAllText(fullPathOfOcrTextFile, ocrText);
            string beyondCompareReport = CompareTextFilesWithBeyondCompare(fullPathOfOriginalTextFile, fullPathOfOcrTextFile);
            //after the comparison of the two text files is done the temp txt file is deleted.
            File.Delete(fullPathOfOcrTextFile);
            XmlNodeList allRows = GetAllRowElements(beyondCompareReport);
            bool allLinesCorrect = true;
            List<OcrResultsPerLine> tableRows = new List<OcrResultsPerLine>();
            (tableRows, allLinesCorrect) = PopulateOcrResultsArray(allRows);
            ocrResults.Table = tableRows;
            if (allLinesCorrect)
            {
                ocrResults.TestResult = true;
                ocrResults.ImageAccepted = true;
            }
            else
            {
                ocrResults.TestResult = false;
            }
            if (ocrResults.Table.Count < 1)
            {
                ocrResults.ImageAccepted = false;
            }
            ocrResults.PathOfImage = fullPathOfOcrImage;
            return ocrResults;
        }

        /// <summary>
        /// Inserts into the list all the results of the 
        /// comparison of the two text files, line by line
        /// </summary>
        /// <param name="allRows"></param>
        /// <returns></returns>
        private (List<OcrResultsPerLine>, bool) PopulateOcrResultsArray(XmlNodeList allRows)
        {
            bool allLinesCorrect = true;
            List<OcrResultsPerLine> ocrResultsPerLineList = new List<OcrResultsPerLine>();
            int index = 0;
            foreach (XmlNode node in allRows)
            {
                OcrResultsPerLine line = new OcrResultsPerLine();
                if (node.OuterXml.Contains("rightorphan"))
                {
                    line.Index = index;
                    line.ActualText = node.InnerText;
                    line.ExpectedText = "";
                    line.Category = Enum.MistakeType.MINOR;
                    line.Comment = "";
                    line.PassedResult = false;
                    allLinesCorrect = false;
                }
                else if (node.OuterXml.Contains("leftorphan"))
                {
                    line.Index = index;
                    line.ActualText = "";
                    line.ExpectedText = node.InnerText;
                    line.Category = Enum.MistakeType.MINOR;
                    line.Comment = "";
                    line.PassedResult = false;
                    allLinesCorrect = false;
                }
                else if (node.OuterXml.Contains("similar"))
                {
                    //This means that they are exactly equal and similar which means whitespace so ignore the line
                    if (node.LastChild.InnerText.Trim().Equals(node.FirstChild.InnerText.Trim()))
                    {
                        continue;
                    }
                    line.Index = index;
                    line.ActualText = node.LastChild.InnerText;
                    line.ExpectedText = node.FirstChild.InnerText;
                    line.Category = Enum.MistakeType.MINOR;
                    line.Comment = "";
                    line.PassedResult = false;
                    allLinesCorrect = false;
                }
                else if (node.OuterXml.Contains("different"))
                {
                    string actual = node.LastChild.InnerText;
                    string expected = node.FirstChild.InnerText;
                    line.Index = index;
                    line.ActualText = actual;
                    line.ExpectedText = expected;
                    line.Comment = "";
                    line.PassedResult = false;
                    allLinesCorrect = false;
                    Enum.MistakeType type = IdentifyTypeOfMistake(actual, expected);
                    if (type.Equals(Enum.MistakeType.MAJOR))
                    {
                        line.Category = Enum.MistakeType.MAJOR;
                    }
                    else if (type.Equals(Enum.MistakeType.MODERATE))
                    {
                        line.Category = Enum.MistakeType.MODERATE;
                    }
                    else if (type.Equals(Enum.MistakeType.MINOR))
                    {
                        line.Category = Enum.MistakeType.MINOR;
                    }
                }
                else if (node.OuterXml.Contains("same") && node.InnerText.Trim().Length > 0)
                {
                    string actual = node.InnerText;
                    string expected = node.InnerText;
                    line.Index = index;
                    line.ActualText = actual;
                    line.ExpectedText = expected;
                    line.Comment = "";
                    line.PassedResult = true;
                    line.Category = Enum.MistakeType.NO_MISTAKE;
                }
                //means line is empty.
                else
                {
                    continue;
                }
                ocrResultsPerLineList.Add(line);
                index++;
            }

            return (ocrResultsPerLineList, allLinesCorrect);
        }

        /// <summary>
        /// Identifies what type of error the two lines have
        /// based on the different words, letters and symbols found.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        private Enum.MistakeType IdentifyTypeOfMistake(string actual, string expected)
        {
            Enum.MistakeType type = Enum.MistakeType.MAJOR;
            //Checking if it is identified wrongly as different instead of orphan
            if (actual.Equals("") || expected.Equals(""))
            {
                return Enum.MistakeType.MINOR;
            }
            List<Diff> difference = GetWrongWordsDifferentArray(actual, expected);
            /*
                  * Categorizes differences as minor or major
                  * Minor mistakes examples: ; instead of : or . instead of ,
                  * Moderate mistakes examples: one letter classified wrongly
                  * Major mistakes examples: more than one letter wrong, gibberish or junk
                  */
            if (difference.Count < 2)
            {
                //Checks if there is only 1 difference, how many letters it is and if it more than 5 categorize it as moderate.
                if (difference[0].text.Length > 2)
                {
                    type = Enum.MistakeType.MODERATE;
                }
                else
                {
                    type = Enum.MistakeType.MINOR;
                }
            }
            //Checks if the error is just 1 character changes and the character is either .,;~-
            else if (difference.Count == 2 && difference[0].text.Length < 2 && difference[1].text.Length < 2)
            {
                if (difference[0].text.Equals(".") || difference[0].text.Equals(",") || difference[1].text.Equals(".") ||
                    difference[1].text.Equals(",") || difference[0].text.Equals(":") || difference[1].text.Equals(":") ||
                    difference[0].text.Equals(";") || difference[1].text.Equals(";") || difference[0].text.Equals("~") ||
                    difference[1].text.Equals("~") || difference[0].text.Equals("-") || difference[1].text.Equals("-") ||
                    difference[0].text.Equals(" ") || difference[1].text.Equals(" "))
                {
                    type = Enum.MistakeType.MINOR;
                }
                else
                {
                    type = Enum.MistakeType.MODERATE;
                }
            }
            else if (difference.Count < 5)
            {
                //If there are more than 4 characters wrong identify it as major.
                int numberOfWrongCharacters = 0;
                foreach (Diff word in difference)
                {
                    numberOfWrongCharacters += word.text.Length;
                }
                if (numberOfWrongCharacters < 5)
                {
                    type = Enum.MistakeType.MODERATE;
                }
                else
                {
                    type = Enum.MistakeType.MAJOR;
                }
            }
            else
            {
                type = Enum.MistakeType.MAJOR;
            }
            return type;
        }

        /// <summary>
        /// Gets a list of the different words found under the "Different" operation.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        private List<Diff> GetWrongWordsDifferentArray(string actual, string expected)
        {
            //Getting the original text string
            string originalText = expected;
            //Getting the ocr text string
            string ocrText = actual;
            diff_match_patch diff_match_patchObject = new diff_match_patch();
            //Waits for comparison to finish without timeouts
            diff_match_patchObject.Diff_Timeout = 0;
            //Compares the original text of the txt file to the one produced by the OCR algorithm
            List<Diff> difference = diff_match_patchObject.diff_main(ocrText, originalText);
            //Deletes all the occurances that are the same so only the differences remain
            for (int i = 0; i < difference.Count; i++)
            {
                Diff diff = difference[i];
                difference[i].text = difference[i].text.Trim();
                if (diff.operation.Equals(Operation.EQUAL) || diff.text.Trim().Equals(""))
                {
                    difference.RemoveAt(i);
                }
            }
            return difference;
        }

        /// <summary>
        /// Compares the 2 files given using
        /// Beyond Compare and returns a string in XML form.
        /// If the user copies something to the clipboard,
        /// this will probably crash because the results from
        /// the comparison are stored in the clipboard.
        /// </summary>
        /// <param name="originalTextFile"></param>
        /// <param name="ocrTextFile"></param>
        /// <returns></returns>
        private string CompareTextFilesWithBeyondCompare(string originalTextFile, string ocrTextFile)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //TODO: Change the scriptPath in API.
            string scriptPath = beyondCompareScript;
            string outputFilePath = Path.GetTempPath() + "temporary.txt";
            startInfo.FileName = pathToBeyondCompare;
            //startInfo.Arguments = "@\"" + scriptPath + "\" \"" + originalTextFile + "\" \"" + ocrTextFile + "\"";
            startInfo.Arguments = "@\"" + scriptPath + "\" \"" + originalTextFile + "\" \"" + ocrTextFile + "\" \"" + outputFilePath + "\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            //Retrieves results from the clipboard.
            string ocrText = File.ReadAllText(Path.GetTempPath() + "temporary.txt");
            File.Delete(Path.GetTempPath() + "temporary.txt");
            return ocrText;
        }

        private XmlNodeList GetAllRowElements(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            //gets all data from the xml file
            return document.GetElementsByTagName("linecomp");
        }
    }
}
