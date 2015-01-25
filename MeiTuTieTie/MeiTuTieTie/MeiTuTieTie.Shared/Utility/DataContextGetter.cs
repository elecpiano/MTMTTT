using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Utility
{
    public static class Utils
    {
        #region DataContext

        public static object GetDataContext(this object obj)
        {
            return (obj as FrameworkElement).DataContext;
        }

        public static T GetDataContext<T>(this object obj) where T : class
        {
            T context = (obj as FrameworkElement).DataContext as T;
            return context;
        }

        #endregion

    }
}
