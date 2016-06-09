using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace ignisilva
{
    class Program
    {
        static void Main( string[] args )
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string folder = "../../../images/";
            string outputFolder = "../../../images/out/";
            string extension = ".jpg";

            DirectoryInfo d = new DirectoryInfo( folder );//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles( "*"+extension ); //Getting files

            foreach( FileInfo fileinfo in Files )
            {
                string file = fileinfo.Name;

                Console.WriteLine( "Loading " + folder + file );
                Bitmap image = new Bitmap( Image.FromFile( folder + file ) );
                Console.WriteLine( "Done." );

                Bitmap gaussian = image;
                //gaussian = ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurKernal );
                gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurXKernal, false, true ), ImageProcessing.GaussianBlurYKernal, false, true );
                //gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurFastXKernal, false, true ), ImageProcessing.GaussianBlurFastYKernal, false, true );
                gaussian.Save( outputFolder + file + "_gaussian.png" );

                ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelXKernal, false, true ).Save( outputFolder + file + "_sobel.jpg" );

                Bitmap sharp = gaussian;
                sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.SharpenKernal, false, true );
                //sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.IdentityKernal );
                sharp.Save( outputFolder + file + "_sharp.png" );

                Bitmap final = gaussian;
                final = ImageProcessing.ImageKernal( final, ImageProcessing.EdgeFindKernal, false, true );
                final.Save( outputFolder + file + "_final.png", System.Drawing.Imaging.ImageFormat.Png );

                stopwatch.Stop();
                Console.WriteLine( (float)stopwatch.ElapsedMilliseconds / 1000.0f );
            }

            Console.ReadLine();
        }
    }
}
