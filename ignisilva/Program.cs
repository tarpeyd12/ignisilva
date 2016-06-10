using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ignisilva
{
    class Program
    {
        static void Main( string[] args )
        {
            string folder = "../../../images/";
            string outputFolder = "../../../images/out/";
            string[] extensions = { ".jpg", ".png" };

            DirectoryInfo d = new DirectoryInfo( folder );//Assuming Test is your Folder
            List<FileInfo> Files = new List<FileInfo>();

            foreach( string ext in extensions )
            {
                string extension = ext;
                if( '.' == extension[0] )
                {
                    extension = "*" + extension;
                }
                else if( !(extension.StartsWith("*.")) )
                {
                    extension = "*." + extension;
                }
                Files.AddRange( d.GetFiles( extension ) );
            }
            
            ImageCodecInfo jpgCodec = ImageExtensions.GetEncoderInfoOrDefault("image/jpeg");
            EncoderParameters jpgQuality = new EncoderParameters( 1 );
            jpgQuality.Param[0] = new EncoderParameter( System.Drawing.Imaging.Encoder.Quality, 75L );

            bool progressPrint = false;

            //foreach( FileInfo fileinfo in Files )
            Parallel.ForEach( Files, (fileinfo) =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                string file = fileinfo.Name;

                if( !(file.EndsWith(".png") || file.EndsWith( ".jpg" ) ) )
                {
                    return;
                }

                Console.WriteLine( "Loading " + folder + file );
                Bitmap image = new Bitmap( Image.FromFile( folder + file ) );

                Bitmap gaussian = image;
                //gaussian = ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurKernal );
                gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurXKernal, false, progressPrint ), ImageProcessing.GaussianBlurYKernal, false, progressPrint );
                //gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurFastXKernal, false, progressPrint ), ImageProcessing.GaussianBlurFastYKernal, false, progressPrint );
                gaussian.Save( outputFolder + file + "_000_gaussian.jpg", jpgCodec, jpgQuality );

                ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelXKernal,  false, progressPrint ).Save( outputFolder + file + "_002_sobel_x.jpg",  jpgCodec, jpgQuality );
                ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelYKernal,  false, progressPrint ).Save( outputFolder + file + "_004_sobel_y.jpg",  jpgCodec, jpgQuality );
                ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelNXKernal, false, progressPrint ).Save( outputFolder + file + "_003_sobel_xn.jpg", jpgCodec, jpgQuality );
                ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelNYKernal, false, progressPrint ).Save( outputFolder + file + "_005_sobel_yn.jpg", jpgCodec, jpgQuality );

                Bitmap sharp = gaussian;
                sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.SharpenKernal, false, progressPrint );
                //sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.IdentityKernal );
                sharp.Save( outputFolder + file + "_001_sharp.jpg", jpgCodec, jpgQuality );

                {
                    ;
                }

                Bitmap final = gaussian;
                final = ImageProcessing.ImageKernal( final, ImageProcessing.EdgeFindKernal, false, progressPrint );
                final.Save( outputFolder + file + "_999_edge.jpg", jpgCodec, jpgQuality );

                stopwatch.Stop();
                Console.WriteLine( "Completed processing {0} in {1} seconds.", folder + file, ( float)stopwatch.ElapsedMilliseconds / 1000.0f );
                //GC.Collect();
            }
            );

            GC.Collect();
            Console.WriteLine( "Press [Return] to exit ..." );
            Console.ReadLine();
        }
    }
}
