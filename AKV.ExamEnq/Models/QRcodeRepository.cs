using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class QRcodeRepository
    {
        public static QRcodeRepository Repository = new QRcodeRepository();

        public byte[] RenderQrCode(string qrtext)
        {
            string level = "L";
            QRCodeGenerator.ECCLevel eccLevel = (QRCodeGenerator.ECCLevel)(level == "L" ? 0 : level == "M" ? 1 : level == "Q" ? 2 : 3);
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrtext, eccLevel))
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                string filePath = HttpContext.Current.Server.MapPath("~/Logo/AU.ico");
                return ImageToByte(qrCode.GetGraphic(20, Color.Black, Color.White,
                      GetIconBitmap(filePath), 15));

              
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        private Bitmap GetIconBitmap(string path)
        {
            if (path.Length == 0)
            {
                return null;
            }
            try
            {
                return new Bitmap(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
        

    }
}