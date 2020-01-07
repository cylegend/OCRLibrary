using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;

namespace OCRlib
{
    public class MergeImages
    {
        private List<Bitmap> images = new List<Bitmap>();

        private IntPoint[] harrisPoints1;
        private IntPoint[] harrisPoints2;

        private IntPoint[] correlationPoints1;
        private IntPoint[] correlationPoints2;

        private MatrixH homography;
        private string path;

        /*
         * path variable does not need a / at the end
         */
        public MergeImages(string path)
        {
            this.path = path;
        }

        /*
         * Detects feature points using Harris Corners Detection.
         * Corners are a good feature to match on photos because they are stable and have 
         * large variations in their neighborhood.
         */

        private void HarrisCornersDetectorRecursive()
        {
            HarrisCornersDetector harris = new HarrisCornersDetector(0.04f, 1000f);
            harrisPoints1 = harris.ProcessImage(images[0]).ToArray();
            harrisPoints2 = harris.ProcessImage(images[1]).ToArray();
        }

        /*
         * Matches feature points using a correlation measure.
         */
        private void CorrelationMatchingRecursive()
        {
            CorrelationMatching matcher = new CorrelationMatching(9);
            IntPoint[][] matches = matcher.Match(images[0], images[1], harrisPoints1, harrisPoints2);
            // Get the two sets of points
            correlationPoints1 = matches[0];
            correlationPoints2 = matches[1];
        }

        /*
         * Creates the homography matrix using a robust estimator.
         */
        private void HomographyEstimator()
        {
            //First parameter is the threshold, second parameter the probability
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            homography = ransac.Estimate(correlationPoints1, correlationPoints2);
        }

        /*
         * Blends the second image on the first image using homography.
         */
        private void BlendRecursive()
        {
            Blend blend = new Blend(homography, images[0]);
            Bitmap panorama = blend.Apply(images[1]);
            images[0] = panorama;
            images.RemoveAt(1);
        }

        public Bitmap CreatePanoramaFromFolder()
        {
            // Do it all
            GetImages();
            return CreatePanoramaRecursion();
        }

        /*
         * Recursive function that applies stitching on the first two images
         * then saves the result and then applies stitching on the result and the
         * next image, until all the images are used
         */
        private Bitmap CreatePanoramaRecursion()
        {
            if (images.Count < 2)
            {
                return images[0];
            }
            HarrisCornersDetectorRecursive();
            CorrelationMatchingRecursive();
            HomographyEstimator();
            BlendRecursive();
            return CreatePanoramaRecursion();
        }

        /*
         * Gets all the files in a folder. (All must be png images for this to work)
         * 
         */
        private void GetImages()
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (directory != null)
            {
                FileInfo[] files = directory.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(files[i].FullName);
                    images.Add(new Bitmap(img));
                }
            }
        }
    }
}
