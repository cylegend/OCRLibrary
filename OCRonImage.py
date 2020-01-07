import cv2
import pytesseract
import numpy as np
from matplotlib import pyplot as plt
from PIL import Image
import sys

def OCRonImage():
    path = sys.argv[1]
    choose = "2"
    image = cv2.imread(path)
    # convert image to grayscale
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    #shows the original image
    #Image.open(path).show()
    if choose == "1":
         res,gray = cv2.threshold(gray, 133, 255, cv2.THRESH_BINARY)
    elif choose == "2":
         gray = cv2.adaptiveThreshold(gray, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY, 31, 14)
    elif choose == "3":
        #no need for this if the background is white
        # detect the edges
         edgePicture = cv2.Canny(gray, 50, 200)
         plt.imshow(cv2.cvtColor(edgePicture, cv2.COLOR_BGR2RGB))
         plt.show()
         #find the non-zero min-max coords of canny
         #0 is black so it detects wherever there is white and stores them in an array.
         #it then finds the minimum value to be used as the first values of coordinates (x,y) and the maximum acordingly.
         pts = np.argwhere(edgePicture>0)
         y1,x1 = pts.min(axis=0)
         y2,x2 = pts.max(axis=0)
         #cropping the original image to the edges found
         gray = gray[y1:y2, x1:x2]
         #applying the adaptive Threshold
         gray = cv2.adaptiveThreshold(gray, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY, 31, 14)

    pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'
    text = pytesseract.image_to_string(gray)
    #shows the converted image
    #plt.imshow(cv2.cvtColor(gray, cv2.COLOR_BGR2RGB))
    #plt.show()
    #print("\n",text)
    return text

try:
    text = OCRonImage()
    print(text)

except Exception as e:
    print(e.args)
    print(e.__cause__)
