using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Shared.Utility
{
    public class XmlHelper
    {
        public static async void Serialize<T>(IStorageFile file, T obj)
        {
            using(IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(stream.AsStreamForWrite(), obj);
            }
        }

        public static async Task<T> Deserialize<T>(IStorageFile file)
        {
            T val;
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                val = (T)xs.Deserialize(stream.AsStreamForRead());
            }
            return val;
        }

        public static string SerializeToString<T>(T obj)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter writer = new StringWriter(sb))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(writer, obj);
            }
            return sb.ToString();
        }

        public static T DeserializeFromString<T>(string data)
        {
            T val;
            using (StringReader sr = new StringReader(data))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                val = (T)xs.Deserialize(sr);
            }
            return val;

        }
    }
}
