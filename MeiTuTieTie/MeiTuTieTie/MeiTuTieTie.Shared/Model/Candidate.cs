using Shared.Infrastructures;
using System.Runtime.Serialization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Model
{
    public class Candidate : BindableBase
    {
        public StorageFile File { get; set; }
        public BitmapSource Thumbnail { get; set; }

        private double _Selected = 0d;
        public double Selected
        {
            get { return _Selected; }
            set { SetProperty(ref _Selected, value); }
        }
    }

}
