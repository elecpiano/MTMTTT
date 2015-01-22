using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Shared.Utility
{
    public abstract class DataTemplateSelector : ContentControl
    {
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

    public class LoadMoreDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }

        public DataTemplate LoadMoreTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ILoadMoreItem itemdata = item as ILoadMoreItem;
            if (itemdata != null)
            {
                if (itemdata.IsLoadMore)
                {
                    return LoadMoreTemplate;
                }
                else
                {
                    return ItemTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }

    public interface ILoadMoreItem
    {
        bool IsLoadMore { get; set; }
    }

}
