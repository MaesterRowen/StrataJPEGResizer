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
            sldQuality.Value = 100;
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
                            image.Quality = 100;
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
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            // Disable the list view
            state = !state;
            toggleWindowState(state);

            //Console.WriteLine(cmbFormat.SelectedValue);

            // Loop through each item in our list and convert
            //for( int i = 0; i < itemList.Count; i++ )
            //{
            //    itemList[i].Progress = 100;
            //    itemList[i].Opacity = 0.5f;
            //}
            var t = Task.Run(() =>
            {
                Parallel.ForEach(itemList, new ParallelOptions { MaxDegreeOfParallelism = 5},
                item =>
                {
                    item.Opacity = 0.5f;
                    using (var image = new MagickImage())
                    {
                        image.Read(item.Path + "\\" + item.Name);
                        image.Progress += Image_Progress;

                        image.Format = MagickFormat.Jpeg;
                        image.Quality = 100;

                        // Create the subfolder
                        string resizeDir = item.Path + "\\Converted\\Resize";
                        Directory.CreateDirectory(resizeDir);
                        image.Resize(1600, 1200);
                        image.Write(resizeDir + "\\" + item.NewName + "_rsz.jpg");
                    }

                    using (var image = new MagickImage())
                    {
                        image.Read(item.Path + "\\" + item.Name);
                        image.Format = MagickFormat.Jpeg;
                        image.Quality = 100;
                        // Create the subfolder
                        string convert = item.Path + "\\Converted";
                        Directory.CreateDirectory(convert);
                        image.Write(convert + "\\" + item.NewName + ".jpg");
                    }
                });
            });
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
    }
}
