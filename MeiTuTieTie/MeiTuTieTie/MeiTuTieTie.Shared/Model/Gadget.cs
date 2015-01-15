using System.Runtime.Serialization;

namespace Shared.Model
{
    [DataContract]
    public class Gadget
    {
        [DataMember]
        public string themePackID { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string thumbnail { get; set; }
        [DataMember]
        public bool visible { get; set; }
    }

}
