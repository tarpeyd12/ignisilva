using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ignisilva
{

    // pulled from: https://msdn.microsoft.com/en-us/library/ytz20d80(v=vs.100).aspx
    public static class ImageExtensions
    {
        public static string GetMimeType( this System.Drawing.Image image )
        {
            string sReturn = string.Empty;
            if( image.RawFormat.Guid == System.Drawing.Imaging.ImageFormat.Bmp.Guid )
                sReturn = "image/bmp";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Emf.Guid ) )
                sReturn = "image/emf";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Exif.Guid ) )
                sReturn = "image/exif";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Gif.Guid ) )
                sReturn = "image/gif";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Icon.Guid ) )
                sReturn = "image/icon";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Jpeg.Guid ) )
                sReturn = "image/jpeg";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.MemoryBmp.Guid ) )
                sReturn = "image/membmp";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Png.Guid ) )
                sReturn = "image/png";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Tiff.Guid ) )
                sReturn = "image/tiff";
            else if( image.RawFormat.Guid.Equals( System.Drawing.Imaging.ImageFormat.Wmf.Guid ) )
                sReturn = "image/wmf";
            else
                sReturn = "image/jpeg";
            return sReturn;
        }



        public static ImageCodecInfo GetEncoderInfoOrDefault( this string mimeType )
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            var existingEnc = encoders.FirstOrDefault(enc => enc.MimeType.Equals(mimeType));
            // should the specific encoder not be found, then return the JPG encoder by default.
            if( existingEnc == null )
            {
                existingEnc = encoders.SingleOrDefault( enc => enc.MimeType.Equals( "image/jpeg" ) );
            }
            // should the default encoder not exist, then return any encoder or null if none is present.
            if( existingEnc == null )
            {
                existingEnc = ( encoders.Length > 0 ) ? encoders[0] : null;
            }
            return existingEnc;
        }


        public static ImageCodecInfo GetEncoderInfoOrDefault( this System.Drawing.Image image )
        {
            return GetEncoderInfoOrDefault( image.GetMimeType() );
        }
    }
}
