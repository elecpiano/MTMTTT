using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Shared.Model
{
    public class Material
    {
        [XmlElement]
        public string themePackID { get; set; }

        [XmlElement(ElementName = "id")]
        public string type { get; set; }

        [XmlElement(ElementName = "name")]
        public string image { get; set; }

        [XmlElement(ElementName = "thumbnailname")]
        public string thumbnail { get; set; }

        [XmlElement]
        public bool visible { get; set; }
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

}
