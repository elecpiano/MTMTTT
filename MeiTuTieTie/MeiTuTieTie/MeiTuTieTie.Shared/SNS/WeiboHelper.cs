using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Shared.SNS
{
    /// <summary>
    /// 唤起微博分享帮助类
    /// author:shuifeng
    /// date:2013-04-19
    /// note: do not modify this class or association won't work correctly
    /// </summary>
    public class WeiboHelper
    {
        /// <summary>
        /// 启动微博分享文本
        /// </summary>
        /// <param name="text">微博文本，少于140个汉字</param>
        public async static void ShareText(string text)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("sinaweibo:sendweibo?weibocontent=" + text));
        }

        /// <summary>
        /// 分享图片、地理位置
        /// <param name="imageStream">图片流（不能超过5M）</param>
        /// <param name="text">微博文本，少于140个汉字</param>
        /// <param name="latitude">纬度，长度不能超过50个字符</param>
        /// <param name="longtitude">经度，长度不能超过50个字符</param>
        /// </summary>
        public async static Task Share(Stream imageStream = null, string text = "", string latitude = "", string longitude = "")
        {
            if (imageStream == null && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(latitude) && string.IsNullOrEmpty(longitude))
            {
                throw new ArgumentException("param can not be empty,at least one param is needed.");
            }

            if (latitude.Length > 50 || longitude.Length > 50)
            {
                throw new ArgumentException("location string length can not be longer than 50!");
            }

            if ((string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude)) || (!string.IsNullOrEmpty(latitude) && string.IsNullOrEmpty(longitude)))
            {
                throw new ArgumentException("invaild location parameters");
            }

            string fileName = "share.weiboimage";

            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            IStorageFile storageFile = await applicationFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (Stream stream = await storageFile.OpenStreamForWriteAsync())
            {
                //存储微博文本，140*3
                byte[] buffer = new byte[140 * 3];
                byte[] bytes;
                if (!string.IsNullOrEmpty(text))
                {
                    bytes = UTF8Encoding.UTF8.GetBytes(text);
                    bytes.CopyTo(buffer, 0);
                }
                stream.Write(buffer, 0, buffer.Length);

                //存储经度
                buffer = new byte[50];
                if (!string.IsNullOrEmpty(latitude))
                {
                    bytes = UTF8Encoding.UTF8.GetBytes(latitude);
                    bytes.CopyTo(buffer, 0);
                }
                stream.Write(buffer, 0, buffer.Length);

                //存储纬度
                buffer = new byte[50];
                if (!string.IsNullOrEmpty(longitude))
                {
                    bytes = UTF8Encoding.UTF8.GetBytes(longitude);
                    bytes.CopyTo(buffer, 0);
                }
                stream.Write(buffer, 0, buffer.Length);

                //存储图片
                if (imageStream != null)
                {
                    int length = (int)imageStream.Length;
                    buffer = new byte[length];
                    await imageStream.ReadAsync(buffer, 0, length);
                    stream.Write(buffer, 0, buffer.Length);
                    imageStream.Dispose();
                }
            }

            StorageFile bgfile = await applicationFolder.GetFileAsync("share.weiboimage");
            await Windows.System.Launcher.LaunchFileAsync(bgfile);
        }

        /// <summary>
        /// 唤起微博进行SSO授权登录
        /// </summary>
        /// <param name="appkey">Open平台申请的应用Appkey</param>
        /// <param name="redirectUri">回调Uri，在Open平台中填写</param>
        public async static void SsoAuth(string appkey, string redirectUri)
        {
            string id = GetId().ToString();

            //just for test usage.
            id = "com.weibo.sdk.android.demo";

            await Windows.System.Launcher.LaunchUriAsync(new Uri("sinaweibo:sso?appkey=" + appkey + "&redirecturi=" + redirectUri + "&productid=" + id));
        }

        /// <summary>
        /// get your application id
        /// </summary>
        /// <returns></returns>
        private static Guid GetId()
        {
            Guid applicationId = Guid.Empty;

            var productId = XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("ProductID");

            if (productId != null && !string.IsNullOrEmpty(productId.Value))
                Guid.TryParse(productId.Value, out applicationId);

            return applicationId;
        }
    }
}
