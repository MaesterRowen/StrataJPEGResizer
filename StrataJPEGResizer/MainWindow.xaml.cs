using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.IO;
using ImageMagick;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace StrataJPEGResizer
{
    public class ListItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string NewName { get; set; }
        public string Path { get; set; }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get { return this._Visibility; }
            set { this._Visibility = value; OnPropertyChanged(); }
        }
        private BitmapSource _ImageData;
        public BitmapSource ImageData
        {
            get { return this._ImageData; }
            set { this._ImageData = value;  OnPropertyChanged();  }
        }

        private int _Progress = 0;
        public int Progress
        {
            get { return this._Progress; }
            set { this._Progress = value; OnPropertyChanged(); }
        }

        private float _Opacity = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public float Opacity
        {
            get { return this._Opacity; }
            set { this._Opacity = value; OnPropertyChanged(); }
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BindingList<ListItem> itemList { get; set; }

        //public PhotoListVM photoListVM = new PhotoListVM();

        public ReadOnlyCollection<string> supportedTypes { get; } = new ReadOnlyCollection<string>(
            new string[] { ".jpg", ".jpeg", ".png", ".heic" }
        );

        public MainWindow()
        {
            itemList = new BindingList<ListItem>();
            //photoList = new BindingList<PhotoItem>();
            InitializeComponent();

            // Set intial values
            txtCustH.Text = "";
            txtCustV.Text = "";
            cmbFormat.Text = "JPEG";
            sldQuality.Value = 90;
            rad1600x1200.IsChecked = true;
            chkAppend.IsChecked = true;
            chkKeepExif.IsChecked = true;

        }


        private void fileDropStackPanel_DragEnter(object sender, DragEventArgs e)
        {
            this.fileDropListView.Background = Brushes.Red;
        }

        private void fileDropStackPanel_DragLeave(object sender, DragEventArgs e)
        {
            this.fileDropListView.Background = Brushes.GhostWhite;
        }

        private void fileDropStackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.fileDropListView.Background = Brushes.GhostWhite;
                // Loop through each 
                foreach (string file in droppedFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string filePath = Path.GetDirectoryName(file);
                    string fileExt = Path.GetExtension(file);

                    string fileNameNoExt = Path.GetFileNameWithoutExtension(file);

                    if( supportedTypes.Contains(fileExt.ToLower()) == false )
                    {
                        MessageBox.Show("Unsupported image type added");
                        return;
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    string newPath = filePath + "\\" + fileNameWithoutExtension + ".jpg";


                    ListItem item = new ListItem();
                    item.Name = fileName;
                    item.ImageData = null;
                    item.NewName = fileNameNoExt;
                    item.Path = filePath;
                    item.Opacity = 0f;
                    item.Progress = 0;
                    itemList.Add(item);

                    Task.Run(() =>
                    {
                        using (var image = new MagickImage(item.Path + "//" + item.Name))
                        {

                            MagickGeometry geometry = new MagickGeometry(128, 128);
                            image.Format = MagickFormat.Bmp;
                            image.Quality = 50;
                            image.Resize(geometry);
                            var memStream = new MemoryStream();

                            image.Write(memStream);
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.StreamSource = memStream;
                            bi.EndInit();
                            bi.Freeze();
                            item.ImageData = bi;

                            item.Visibility = Visibility.Hidden;
                        }
                    });

                }
            }
        }

        private void toggleWindowState(bool value)
        {
            //fileDropListView.IsEnabled = value;
            radOriginal.IsEnabled = value;
            rad800x600.IsEnabled = value;
            rad1600x1200.IsEnabled = value;
            radCustom.IsEnabled = value;
            sldQuality.IsEnabled = value;
            chkAppend.IsEnabled = value;
            chkKeepExif.IsEnabled = value;
            cmbFormat.IsEnabled = value;

            if (radCustom.IsChecked == true && value == true)
            {
                txtCustH.IsEnabled = true;
                txtCustV.IsEnabled = true;
            }
            else
            {
                txtCustH.IsEnabled = false;
                txtCustV.IsEnabled = false;
            }
        }
        private bool state = true;
        private void Image_Progress(object sender, ProgressEventArgs e)
        {
            // Find this item in our list
            string originStr = e.Origin;

            // Find last '/' in string
            string command = e.Origin.Substring(0, originStr.LastIndexOf('/'));
            if (command == "Save/Image") return;
            string path = e.Origin.Substring(command.Length + 1).ToLower();

            foreach(ListItem item in itemList)
            {
                string itemPath = (item.Path + "\\" + item.Name).ToLower();
                if( itemPath == path )
                {
                    item.Progress = e.Progress.ToInt32();
                    break;
                }
            }
        }
        private async void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            // Disable the list view
            state = !state;
            toggleWindowState(state);

            // Store our settings


            MagickFormat nFormat = MagickFormat.Jpeg;
            if (cmbFormat.Text == "PNG")
            {
                nFormat = MagickFormat.Png;
            }
                
                
            bool bKeepExifData = true;
            int nResX = 1600;
            int nResY = 1200;
            int nQuality = (int)sldQuality.Value;
            bool bResize = true;
            bool bAppendRsz = chkAppend.IsChecked == true;
            string extension = ".jpg";

            if( nFormat == MagickFormat.Png )
            {
                extension = ".png";
            }

            if (radOriginal.IsChecked == true)
            {
                bResize = false;
            }
            else if (rad800x600.IsChecked == true)
            {
                nResX = 800;
                nResY = 600;
                bResize = true;
            }
            else if (rad1600x1200.IsChecked == true)
            {
                nResX = 1600;
                nResY = 1200;
                bResize = true;
            }
            else if (radCustom.IsChecked == true)
            {
                int.TryParse(txtCustH.Text, out nResX);
                int.TryParse(txtCustV.Text, out nResY);
                bResize = true;
            }

            await Task.Run(() =>
            {
                Parallel.ForEach(itemList, new ParallelOptions { MaxDegreeOfParallelism = 5},
                item =>
                {
                    item.Opacity = 0.5f;
                    if( bResize )
                    {
                        using (var image = new MagickImage())
                        {
                            image.Read(item.Path + "\\" + item.Name);
                            image.Progress += Image_Progress;

                            image.Format = nFormat;
                            image.Quality = nQuality;

                            // Create the subfolder
                            string resizeDir = item.Path + "\\Converted\\Resize";
                            Directory.CreateDirectory(resizeDir);
                            image.Resize(nResX, nResY);
                            image.Write(resizeDir + "\\" + item.NewName + (bAppendRsz ? "_rsz" : "") + extension);
                        }
                    }

                    using (var image = new MagickImage())
                    {
                        image.Read(item.Path + "\\" + item.Name);
                        image.Format = nFormat;
                        image.Quality = 100;
                        // Create the subfolder
                        string convert = item.Path + "\\Converted";
                        Directory.CreateDirectory(convert);
                        image.Write(convert + "\\" + item.NewName + extension);
                    }
                });
            });

            toggleWindowState(true);
            itemList.Clear();
        }

        private void radCustom_Checked(object sender, RoutedEventArgs e)
        {
            txtCustH.IsEnabled = true;
            txtCustV.IsEnabled = true;
        }

        private void radCustom_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustH.IsEnabled = false;
            txtCustV.IsEnabled = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<ListItem> items = new List<ListItem>();
            foreach (ListItem item in fileDropListView.SelectedItems)
            {
                items.Add(item);
            }

            foreach( ListItem i in items )
            {
                itemList.Remove(i);
            }
        }

        private void sldQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if( sender == sldQuality && txtQuality != null )
            {
                txtQuality.Text = ((int)(e.NewValue)).ToString() + "%";
            }
        }

        private void cmbFormat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if( sender == cmbFormat && sldQuality != null && chkKeepExif != null)
            {
                ComboBoxItem typeItem = (ComboBoxItem)e.AddedItems[0];
                if( typeItem != null)
                {
                    string contentText = typeItem.Content.ToString();

                    sldQuality.IsEnabled = contentText == "JPEG";
                    chkKeepExif.IsEnabled = contentText == "JPEG";
                }
            }
        }
    }
}
