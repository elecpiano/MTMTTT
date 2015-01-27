using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;

namespace MeiTuTieTie.Pages
{
    public interface IFileOpenPickerPageBase
    {
        void PickPhotosContiue(FileOpenPickerContinuationEventArgs args);
    }
}
