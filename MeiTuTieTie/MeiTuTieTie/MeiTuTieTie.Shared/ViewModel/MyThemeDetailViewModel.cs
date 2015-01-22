using System.Collections;
using Windows.UI.Xaml.Data;
using Shared.Model;
using System.Collections.Generic;
using Shared.JumpList;

namespace Shared.ViewModel
{
    public class MyThemeDetailViewModel
    {
        private IList data;

        public IList Data
        {
            get
            {
                return data;
            }
        }
        private CollectionViewSource collection;

        public CollectionViewSource Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = new CollectionViewSource();
                    collection.Source = Data;
                    collection.IsSourceGrouped = true;
                }
                return collection;
            }
        }

        public void SetData(IEnumerable<Material> materials)
        {
            data = materials.ToGroups(x => x.themePackID, x => x.TypeName);
        }

    }

}
