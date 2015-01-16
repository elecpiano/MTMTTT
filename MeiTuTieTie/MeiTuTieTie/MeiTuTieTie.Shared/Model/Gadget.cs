using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shared.Model
{
    [DataContract(Name="material")]
    public class Material
    {
        [DataMember]
        public string themePackID { get; set; }

        [DataMember(Name="id")]
        public string type { get; set; }
        
        [DataMember(Name = "name")]
        public string image { get; set; }
        
        [DataMember(Name = "thumbnailname")]
        public string thumbnail { get; set; }
        
        [DataMember]
        public bool visible { get; set; }
    }

    [DataContract]
    public class MaterialData
    {
        public MaterialData()
        {
            materials = new List<Material>();
        }

        [DataMember(Name = "materials")]
        public List<Material> materials { get; set; }
    }

}
