using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Shared.Utility
{
    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base(sb)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }

    public class XmlHelper
    {
        public static async void Serialize<T>(IStorageFile file, T obj)
        {
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (StreamWriter writer = new StreamWriter(stream.AsStreamForWrite(), Encoding.UTF8))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    xs.Serialize(writer, obj);
                }
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

        public static async Task<T> Deserialize<T>(string file)
        {
            T val;

            string content = await IsolatedStorageHelper.ReadFileAsync(file);
            using (StringReader sr = new StringReader(content))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                val = (T)xs.Deserialize(sr);
            }
            return val;
        }

        public static string SerializeToString<T>(T obj)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriterWithEncoding writer = new StringWriterWithEncoding(sb, Encoding.UTF8))
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

        public static async Task FastIterate(string file, Action<string, string> actionOnEachNode)
        {
            string content = await IsolatedStorageHelper.ReadFileAsync(file);

            if (content.Contains("?>"))
            {
                int from = content.IndexOf("?>");
                content = content.Substring(from + 2);
            }

            string nodeName = string.Empty;
            string nodeValue = string.Empty;
            int maxIndex = content.Length - 1;
            int i = 0;
            XmlParsingStatus status = XmlParsingStatus.NodeEnded;

            while (i < maxIndex)
            {
                switch (status)
                {
                    case XmlParsingStatus.BeginningOrEnd:
                        if (content[i] == '/')
                        {
                            status = XmlParsingStatus.EndOfNodeDetected;
                        }
                        else
                        {
                            status = XmlParsingStatus.BeginningOfNodeDetected;
                        }
                        break;
                    case XmlParsingStatus.BeginningOfNodeDetected:
                        if (content[i] == '>')
                        {
                            status = XmlParsingStatus.ValueCollecting;
                            nodeValue = string.Empty;
                        }
                        break;
                    case XmlParsingStatus.ValueCollecting:
                        if (content[i] == '<')
                        {
                            status = XmlParsingStatus.BeginningOrEnd;
                        }
                        else
                        {
                            nodeValue += content[i];
                        }
                        break;
                    case XmlParsingStatus.EndOfNodeDetected:
                        if (content[i] == '>')
                        {
                            status = XmlParsingStatus.NodeEnded;
                            actionOnEachNode(nodeName, nodeValue);
                            nodeName = string.Empty;
                            nodeValue = string.Empty;
                        }
                        else
                        {
                            nodeName += content[i];
                        }
                        break;
                    case XmlParsingStatus.NodeEnded:
                        if (content[i] == '<')
                        {
                            status = XmlParsingStatus.BeginningOrEnd;
                        }
                        break;
                    default:
                        break;
                }

                i++;
            }
        }

        public enum XmlParsingStatus
        {
            BeginningOrEnd,
            BeginningOfNodeDetected,
            ValueCollecting,
            EndOfNodeDetected,
            NodeEnded
        }

        /* incomplete version of using reflection
        
        public static void FastIterate<T>(string content, Action<T> actionOnEachItem) where T : class
        {
            if (content.Contains("?>"))
            {
                int from = content.IndexOf("?>");
                content = content.Substring(from);
            }

            var properties = typeof(T).GetRuntimeProperties();
            Dictionary<string, PropertyInfo> propertyDictionary = new Dictionary<string, PropertyInfo>();
            string className = (typeof(T)).ToString();
            bool beginningOfElementDetected = false;
            string elementName = string.Empty;
            int maxIndex = content.Length - 1;
            int i = 0;
            T obj = null;

            while (i < maxIndex)
            {
                if (beginningOfElementDetected)
                {
                    if (content[i] == '>')
                    {
                        if (elementName == className)
                        {
                            obj = Activator.CreateInstance<T>();
                        }
                        elementName = string.Empty;
                        beginningOfElementDetected = false;
                    }
                    else
                    {
                        elementName += content[i];
                    }
                    continue;
                }
                
                if (content[i] == '<')
                {
                    beginningOfElementDetected = true;
                    continue;
                }
            }
        }

        private void SetProperties(Dictionary<string, PropertyInfo> propertyDictionary, object obj, string propertyName, object propertyValue)
        {
            if (propertyDictionary.ContainsKey(propertyName))
            {
                propertyDictionary[propertyName].SetValue(obj, propertyValue);
            }
        }
    
        */
    }
}
