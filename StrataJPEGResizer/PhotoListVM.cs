using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StrataJPEGResizer
{
    public class PhotoListVM : INotifyPropertyChanged
    {
        public BindingList<PhotoItem> photoList {get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoListVM()
        {
            photoList = new BindingList<PhotoItem>();
        }

        public void AddPhotoItem(PhotoItem photo )
        {
            if( !checkIfPhotoExists(photo) )
            {
                photoList.Add(photo);
                OnPropertyChanged();
            }
        }

        public void AddPhotoItems(List<PhotoItem> photos)
        {
            foreach( PhotoItem photo in photos)
            {
                if( !checkIfPhotoExists(photo))
                {
                    photoList.Add(photo);
                    OnPropertyChanged();
                }
            }
        }

        private bool checkIfPhotoExists(PhotoItem photo)
        {
            bool found = false;
            foreach(PhotoItem listItem in photoList )
            {
                string listItemPath = (listItem.FilePath + "//" + listItem.FileName + listItem.FileExt).ToLower();
                string photoPath = (photo.FilePath + "//" + listItem.FileName + listItem.FileExt).ToLower();

                if( listItemPath.Equals(photoPath) == true ) {
                    found = true;
                    //throw new Exception("exists");
                    break;
                }
            }

            return found;
        }


        public void OnPropertyChanged([CallerMemberName]string name = null )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
