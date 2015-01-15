using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shared.Model
{
    [DataContract]
    public class MyTheme
    {
        public MyTheme()
        {
            visible = true;
        }

        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string thumbnail { get; set; }
        [DataMember]
        public bool visible { get; set; }
    }

    [DataContract]
    public class MyThemeData
    {
        public MyThemeData()
        {
            myThemes = new List<MyTheme>();
        }

        [DataMember]
        public List<MyTheme> myThemes { get; set; }
    }


}
