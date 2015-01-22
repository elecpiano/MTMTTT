using Shared.Infrastructures;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Shared.Model
{
    public class Material : BindableBase
    {
        [XmlElement]
        public string themePackID { get; set; }

        [XmlElement(ElementName = "id")]
        public string type { get; set; }

        [XmlElement(ElementName = "name")]
        public string image { get; set; }

        [XmlElement(ElementName = "thumbnailname")]
        public string thumbnail { get; set; }

        private bool _visible = true;
        [XmlElement]
        public bool visible
        {
            get { return _visible; }
            set { SetProperty(ref _visible, value); }
        }

        private bool _themeEnabled = true;
        [XmlIgnore]
        [IgnoreDataMember]
        public bool ThemeEnabled
        {
            get { return _themeEnabled; }
            set { SetProperty(ref _themeEnabled, value); }
        }

        [IgnoreDataMember]
        public string TypeName
        {
            get
            {
                string typeName = string.Empty;
                switch (type)
                {
                    case "biankuang":
                        typeName = "边框";
                        break;
                    case "beijing":
                        typeName = "背景";
                        break;
                    default:
                        typeName = "饰品";
                        break;
                }
                return typeName;
            }
        }
    }

    [XmlRoot("materials")]
    public class MaterialGroup// : List<material>
    {
        public MaterialGroup()
        {
            Materials = new List<Material>();
        }

        [XmlElement("material")]
        public List<Material> Materials { get; set; }
    }

    public enum MaterialType
    {
        keai,
        wenzi,
        gaoxiaobiaoqing,
        zhedang,
        katongxingxiang,
        biankuang
    }

}
