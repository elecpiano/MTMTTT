using Windows.UI.Xaml;

namespace Shared.Utility
{
    public static class Utils
    {
        public static object GetDataContext(this object obj)
        {
            return (obj as FrameworkElement).DataContext;
        }

        public static T GetDataContext<T>(this object obj) where T : class
        {
            T context = (obj as FrameworkElement).DataContext as T;
            return context;
        }
    }
}
