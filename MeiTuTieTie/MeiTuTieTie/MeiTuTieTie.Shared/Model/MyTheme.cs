using Shared.Infrastructures;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [DataMember]
        public int materialCount { get; set; }

        private bool _visible = false;
        [DataMember]
        public bool visible
        {
            get { return _visible; }
            set { SetProperty(ref _visible, value); }
        }

        private bool _Selected = false;
        [IgnoreDataMember]
        public bool Selected
        {
            get { return _Selected; }
            set { SetProperty(ref _Selected, value); }
        }
    }

    [DataContract]
    public class MyThemeData
    {
        public MyThemeData()
        {
            myThemes = new ObservableCollection<MyTheme>();
        }

        [DataMember]
        public ObservableCollection<MyTheme> myThemes { get; set; }
    }


}
