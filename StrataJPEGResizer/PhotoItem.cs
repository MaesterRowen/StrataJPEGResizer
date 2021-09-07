using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageMagick;

namespace StrataJPEGResizer
{
    public class PhotoItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileExt { get; set; }
        public MagickFormat PhotoFormat { get; set; }
        public int ResolutionX { get; set; }
        public int ResolutionY { get; set; }
        public ImageSource Thumbnail { get; set; }
    }
}
