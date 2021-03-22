using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using ImageMagick;

namespace StrataJPEGResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void fileDropStackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFileName = Path.GetFileName(files[0]);


                string filePath = Path.GetDirectoryName(files[0]);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[0]);

                string newPath = filePath + "\\" + fileNameWithoutExtension + ".jpg";

                fileNameLabel.Content = firstFileName;


                using (var image = new MagickImage())
                {
                    image.Read(files[0]);
                    image.Progress += Image_Progress;
                    
                    image.Format = MagickFormat.Jpeg;
                    image.Quality = 75;

                    image.Resize(new Percentage(50.0));
                    //image.Write(newPath);
                }

                ////using (var image = new MagickImage(files[0]) )
                ////{
                //    image.Format = MagickFormat.Jpeg;
                //    image.Quality = 75;
                //    image.Resize(new Percentage(50.0));

                //    image.Write(newPath);

                //}
            }
        }

        private void Image_Progress(object sender, ProgressEventArgs e)
        {
            Console.WriteLine(e.Progress.ToString());

        }

    }
}
