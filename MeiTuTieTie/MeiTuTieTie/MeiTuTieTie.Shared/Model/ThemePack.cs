using Shared.Infrastructures;
using System.Runtime.Serialization;

namespace Shared.Model
{
    [DataContract]
    public class ThemePack : BindableBase
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string thumbnailUrl { get; set; }
        [DataMember]
        public string topThumbnailUrl { get; set; }
        [DataMember]
        public string previewUrl { get; set; }
        [DataMember]
        public string zipUrl { get; set; }
        [DataMember]
        public int zipSize { get; set; }
        [DataMember]
        public string count { get; set; }
        [DataMember]
        public string task { get; set; }
        [DataMember]
        public string compatible { get; set; }
        [DataMember]
        public int isPurchase { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string[] preview { get; set; }

        //extra members
        private bool _Downloaded = false;
        [IgnoreDataMember]
        public bool Downloaded
        {
            get { return _Downloaded; }
            set { base.SetProperty(ref _Downloaded, value); }
        }

        [IgnoreDataMember]
        public int Count
        {
            get
            {
                return int.Parse(count);
            }
        }
    }

    [DataContract]
    public class ThemePacksData
    {
        [DataMember]
        public ThemePack[] allThemePacks { get; set; }

        [DataMember]
        public ThemePack[] topThemePacks { get; set; }
    }


}
