using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCRlib
{
    public partial class CameraForm : Form
    {
        private Capture capture;
        public CameraForm(Capture capture)
        {
            InitializeComponent();
            this.capture = capture;
        }

        //Assigning event handler.
        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Streaming;
        }

        /*
         * Event handler for the webcam. 
         */
        private void Streaming(object sender, System.EventArgs e)
        {
            using (var image = capture.QueryFrame().ToImage<Bgr, byte>())
            {
                var bitmap = image.ToBitmap();
                cameraPicturebox.Image = bitmap;
            }
        }
    }
}
