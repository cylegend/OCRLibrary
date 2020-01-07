# OCRLibrary
# Getting Started:
Make sure that you have installed Python, Beyond compare and you have the 2 scripts   provided (python and .txt).

Add the OCRlib.dll library to your project by going to 
References -> Add reference -> Browse

When you first create an OCRlib object, make sure you follow the next steps:
1.	Call SetPythonPathAndScripts method to set path for python and the 2 scripts
2.	Call SetBeyondComparePath method to set path for beyond compare exe (BCompare.exe)
3.	Call SetCamera method to set which camera to use if you are going to use the camera (0 is internal camera, 1 is external camera, 2 is second external camera etc.)
If these methods are never called, you won’t be able to get any results from the other methods.

# Methods:
## void SetPythonPathAndScripts(string pythonPath, string pythonScript, string beyondCompareScript);
Sets up the paths for the python.exe, the python script and the Beyond Compare script

## void SetBeyondComparePath(string beyondComparePath)
Sets up the path for BCompare.exe

## void ShowCamera()
Displays the camera’s window

## void SetCamera(int cameraToUse)
Sets up which camera to use if you choose to use the camera. 0 is the internal camera, 1 is the external, 2 is the second external etc.

## void StartCameraCapture(string pathToSaveTo, int frequencyOfFrames)
Starts the capturing of frames from the set camera. Frames are saved to the location which is provided by the first parameter and the second parameter is for after how many frames the picture is going to be saved. The lower, the more pictures. Recommended 30. The camera automatically stops saving any frames after approximately 30 seconds

## void StopCameraCapture()
Stops the capturing of frames manually before 1 minute passed

## Image MergeImagesNoTimeout(string pathToUse)
Merges images in a folder to create a single panoramic image. First parameter is the folder that must contain only images that can be stitch together. This can be an intensive function and will take some time if there are a lot of images in the folder or something is stitched wrongly. You can use the MergeImagesWithTimeout method to add a timeout. Returns the panoramic image created. You must save the image created somewhere in storage in order to be able to pass it to the CompareOcrWithActual method, then you can delete it if you don’t want the image saved in your device

## Image MergeImagesWithTimeout(string pathToUse, int maxRunTimeSeconds)
Merges images in a folder to create a single panoramic image. First parameter is the folder that must contain only images that can be stitch together and the second parameter is the maximum time allowed for the panoramic function to be performed. Since the more images, the more intensive this function can be a maxRunTimeSeconds of 300 is recommended. If the time out passes, the function returns null, otherwise it returns the panoramic image created. You must save the image created somewhere in storage in order to be able to pass it to the CompareOcrWithActual method, then you can delete it if you don’t want the image saved in your device

## string PerformOcr(string imagePath)
Performs optical character recognition on the image provided with an absolute path. Returns a string with the text produced

## OcrResults CompareOcrWithActual(string fullPathOfOriginalTextFile, string ocrText, string fullPathOfOcrImage)
Performs comparison between the actual text that is saved in a txt file and the ocr text that is received and returns an OcrResults object. Takes as parameters the absolute path of the text file that contains the original text, the ocr text that was produced by the PerformOcr method and the absolute path of the Image produced when you saved the image from MergeImagesWithTimeout. It produces 2 new text files which are then automatically deleted after they are used. These files are saved into the temp folder (C:\Users\UserName\AppData\Local\Temp\) of the user

# OcrResults object attributes
•	Bool imageAccepted – if the image is shown correctly, true
•	Bool testResult – if all the actual lines match the expected lines, true
•	String pathOfImage – contains the absolute path to the image
•	List<OcrResultsPerLine> table – List of OcrResultsPerLine objects

# OcrResultsPerLine object attributes
•	Int index – the number of the line
•	String actualText – the actual text of that line, retrieved from the ocr on the image
•	String expectedText – the text that was expected of that line, retrieved from the txt file
•	String comment – comments you can add for this line
•	Enum.MistakeType category – type of mistake(MINOR, MODERATE, MAJOR, NO_MISTAKE) so you can categorize the line more easily.
•	Bool passedResult – if the actualText was the same as the expectedText, true 


