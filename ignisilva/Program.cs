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
            Stopwatch totalStopwatch = Stopwatch.StartNew();

            string folder = @"../../../images/";
            string outputFolder = @"../../../images/out/";
            
            string[] extensions = { @".jpg", @".png", @".bmp"/*, @".psd"*/ };

            int maxImageDimention = 1024;
            int HOG_block_size = 8;


            DirectoryInfo d = new DirectoryInfo( folder );
            List<FileInfo> Files = new List<FileInfo>();

            foreach( string ext in extensions )
            {
                if( ext.Length < 2 ) continue;
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
                //Files.AddRange( d.GetFiles( extension, SearchOption.AllDirectories ) );
            }

            Files.Sort( delegate ( FileInfo t1, FileInfo t2 ) { return ( t1.Length.CompareTo( t2.Length ) ); } );
            
            ImageCodecInfo jpgCodec = ImageExtensions.GetEncoderInfoOrDefault("image/jpeg");
            EncoderParameters jpgQuality = new EncoderParameters( 1 );
            jpgQuality.Param[0] = new EncoderParameter( System.Drawing.Imaging.Encoder.Quality, 100L );

            bool progressPrint = false;

            //foreach( FileInfo fileinfo in Files )
            Parallel.ForEach( Files, (fileinfo) =>
            {
                Stopwatch perStopwatch = Stopwatch.StartNew();

                string file = fileinfo.Name;
                string directory = fileinfo.DirectoryName;
                
                /*bool extcheck = false;
                foreach( string ext in extensions )
                {

                }
                if( !extcheck )
                {
                    return;
                }*/

                Console.WriteLine( "Loading {0} ({1} KBytes).", file, fileinfo.Length/1024 );
                Bitmap image = new Bitmap( Image.FromFile( fileinfo.FullName ) );

                double maxDimention = maxImageDimention;
                double scale = Math.Min( maxDimention / image.Width, maxDimention / image.Height );

                if( maxDimention > 0 && scale < 1.0 )
                {
                    image = new Bitmap( image, new Size( (int)( image.Width * scale ), (int)( image.Height * scale ) ) );
                }

                if( !true )
                {
                    image.Save( outputFolder + file + "_000_scaled.jpg", jpgCodec, jpgQuality );
                }

                
                Bitmap gaussian = image;
                //gaussian = ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurKernal );
                gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurXKernal, false, progressPrint ), ImageProcessing.GaussianBlurYKernal, false, progressPrint );
                //gaussian = ImageProcessing.ImageKernal( ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurFastXKernal, false, progressPrint ), ImageProcessing.GaussianBlurFastYKernal, false, progressPrint );
                if( !true )
                {
                    gaussian.Save( outputFolder + file + "_001_gaussian.jpg", jpgCodec, jpgQuality );
                }

                if( !true )
                {
                    Bitmap[] sobel = new Bitmap[4];

                    ( sobel[0] = ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelXKernal,  false, progressPrint ) ).Save( outputFolder + file + "_201_sobel_x.jpg",  jpgCodec, jpgQuality );
                    ( sobel[1] = ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelYKernal,  false, progressPrint ) ).Save( outputFolder + file + "_202_sobel_y.jpg",  jpgCodec, jpgQuality );
                    ( sobel[2] = ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelNXKernal, false, progressPrint ) ).Save( outputFolder + file + "_203_sobel_xn.jpg", jpgCodec, jpgQuality );
                    ( sobel[3] = ImageProcessing.ImageKernal( gaussian, ImageProcessing.SobelNYKernal, false, progressPrint ) ).Save( outputFolder + file + "_204_sobel_yn.jpg", jpgCodec, jpgQuality );
                    
                }

                if( !true )
                {
                    Bitmap sharp = image;
                    sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.SharpenKernal, false, progressPrint );
                    //sharp = ImageProcessing.ImageKernal( sharp, ImageProcessing.IdentityKernal );
                    sharp.Save( outputFolder + file + "_002_sharp.jpg", jpgCodec, jpgQuality );
                }

                if( true )
                {
                    ImageProcessing.MakeGrayscale3( image ).Save( outputFolder + file + "_003_gray.jpg", jpgCodec, jpgQuality );
                }

                if( true )
                {
                    //Hog.CalculateGradiantAnglesToBitmap( gaussian ).Save( outputFolder + file + "_101_hogA.jpg", jpgCodec, jpgQuality );
                    //Hog.CalculateGradiantMagnitudesToBitmap( gaussian ).Save( outputFolder + file + "_102_hogM.jpg", jpgCodec, jpgQuality );
                    //Hog.CalculateGradiantAngleMagnitudesToBitmap( gaussian ).Save( outputFolder + file + "_103_hogAM.jpg", jpgCodec, jpgQuality );

                    Bitmap hogInput = gaussian;
                    
                    Size hogBinSize = new Size( HOG_block_size, HOG_block_size );

                    float[,,] gradientAngles = Hog.CalculateGradiantAngles( hogInput );
                    float[,,] binGradients = Hog.BinGradients( gradientAngles, hogBinSize );
                    binGradients = Hog.NormalizeBinnedGradients2( binGradients, new Size( 2, 2 ) );
                     
                    Bitmap backdropImage = ImageProcessing.MakeGrayscale3( image );
                    backdropImage = ImageProcessing.ImageKernal( backdropImage, new float[,] { { 0.0f } } );

                    Bitmap hogOutput = Hog.BinGradientsToBitmap( binGradients, hogBinSize, backdropImage );
                    //hogOutput.Save( outputFolder + file + "_104_hogV.jpg", jpgCodec, jpgQuality );
                    hogOutput.Save( outputFolder + file + "_104_hogV.png" );
                }

                if( !true )
                {
                    ImageProcessing.ReduceImageColors( gaussian, 2, false, progressPrint ).Save( outputFolder + file + "_006_reduced_2.jpg", jpgCodec, jpgQuality );
                    ImageProcessing.ReduceImageColors( gaussian, 4, false, progressPrint ).Save( outputFolder + file + "_007_reduced_4.jpg", jpgCodec, jpgQuality );
                    ImageProcessing.ReduceImageColors( gaussian, 8, false, progressPrint ).Save( outputFolder + file + "_008_reduced_8.jpg", jpgCodec, jpgQuality );
                    ImageProcessing.ReduceImageColors( gaussian, 16, false, progressPrint ).Save( outputFolder + file + "_009_reduced_16.jpg", jpgCodec, jpgQuality );
                    ImageProcessing.ReduceImageColors( gaussian, 32, false, progressPrint ).Save( outputFolder + file + "_010_reduced_32.jpg", jpgCodec, jpgQuality );
                    ImageProcessing.ReduceImageColors( gaussian, 64, false, progressPrint ).Save( outputFolder + file + "_011_reduced_64.jpg", jpgCodec, jpgQuality );
                    ImageProcessing.ReduceImageColors( gaussian, 128, false, progressPrint ).Save( outputFolder + file + "_012_reduced_128.jpg", jpgCodec, jpgQuality );
                }

                if( !true )
                {
                    Bitmap final = gaussian;
                    final = ImageProcessing.ImageKernal( final, ImageProcessing.EdgeFindKernal, false, progressPrint );
                    final.Save( outputFolder + file + "_999_edge.jpg", jpgCodec, jpgQuality );
                }

                perStopwatch.Stop();
                Console.WriteLine( "Completed processing {0} in {1} seconds.", file, (float)perStopwatch.ElapsedMilliseconds / 1000.0f );
                //GC.Collect();
            }
            );

            GC.Collect();

            totalStopwatch.Stop();

            Console.WriteLine( "Completed Processing {0} images in {1} seconds.", Files.Count, (float)totalStopwatch.ElapsedMilliseconds / 1000.0f );

            Console.WriteLine( "Press [Return] to exit ..." );
            Console.ReadLine();
        }
    }
}
