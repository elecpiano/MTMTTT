using Shared.Infrastructures;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shared.Model
{
    [DataContract]
    public class MyTheme : BindableBase
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

        private bool _visible = false;
        [DataMember]
        public bool visible
        {
            get { return _visible; }
            set { SetProperty(ref _visible, value); }
        }
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
