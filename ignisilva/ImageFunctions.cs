using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace ignisilva
{
    static class ImageFunctions
    {
        public static float[,] IdentityKernal
        {
            get
            {
                return new float[,] { { 1 } };
            }
        }

        public static float[,] EdgeFindKernal
        {
            get
            {
                return new float[,]{
                { -1, -1, -1},
                { -1,  8, -1},
                { -1, -1, -1} };
            }
        }

        public static float[,] SharpenKernal
        {
            get
            {
                return new float[,]{
                {  0, -1,  0},
                { -1,  5, -1},
                {  0, -1,  0} };
            }
        }

        public static float[,] GaussianBlurFastKernal
        {
            get
            {
                return new float[,]{
                {1.0f*0.0625f,2.0f*0.0625f,1.0f*0.0625f},
                {2.0f*0.0625f,4.0f*0.0625f,2.0f*0.0625f},
                {1.0f*0.0625f,2.0f*0.0625f,1.0f*0.0625f}};
            }
        }

        public static float[,] GaussianBlurFastXKernal
        {
            get
            {
                return new float[,] { { 0.27901f, 0.44198f, 0.27901f } };
            }
        }

        public static float[,] GaussianBlurFastYKernal
        {
            get
            {
                return new float[,] { { 0.27901f }, { 0.44198f }, { 0.27901f } };
            }
        }

        public static float[,] GaussianBlurXKernal
        {
            get
            {
                return new float[,] { { 0.00598f, 0.060626f, 0.241843f, 0.383103f, 0.241843f, 0.060626f, 0.00598f } };
            }
        }

        public static float[,] GaussianBlurYKernal
        {
            get
            {
                return new float[,] { { 0.00598f }, { 0.060626f }, { 0.241843f }, { 0.383103f }, { 0.241843f }, { 0.060626f }, { 0.00598f } };
            }
        }

        public static float[,] GaussianBlurKernal
        {
            get
            {
                return new float[,]{
                    { 0.00000067f,  0.00002292f,  0.00019117f,  0.00038771f,  0.00019117f,  0.00002292f,  0.00000067f },
                    { 0.00002292f,  0.00078634f,  0.00655965f,  0.01330373f,  0.00655965f,  0.00078633f,  0.00002292f },
                    { 0.00019117f,  0.00655965f,  0.05472157f,  0.11098164f,  0.05472157f,  0.00655965f,  0.00019117f },
                    { 0.00038771f,  0.01330373f,  0.11098164f,  0.22508352f,  0.11098164f,  0.01330373f,  0.00038771f },
                    { 0.00019117f,  0.00655965f,  0.05472157f,  0.11098164f,  0.05472157f,  0.00655965f,  0.00019117f },
                    { 0.00002292f,  0.00078633f,  0.00655965f,  0.01330373f,  0.00655965f,  0.00078633f,  0.00002292f },
                    { 0.00000067f,  0.00002292f,  0.00019117f,  0.00038771f,  0.00019117f,  0.00002292f,  0.00000067f } };
            }
        }

        public static float[,] SobelXKernal
        {
            get
            {
                return new float[,]{
                {1,0,-1},
                {2,0,-2},
                {1,0,-1}};
            }
        }

        public static float[,] SobelYKernal
        {
            get
            {
                return new float[,]{
                {1,2,1},
                {0,0,0},
                {-1,-2,-1}};
            }
        }

        public static float[,] SobelNXKernal
        {
            get
            {
                return new float[,]{
                {-1,0,1},
                {-2,0,2},
                {-1,0,1}};
            }
        }

        public static float[,] SobelNYKernal
        {
            get
            {
                return new float[,]{
                {-1,-2,-1},
                {0,0,0},
                {1,2,1}};
            }
        }

        public static Bitmap ImageKernal( Bitmap input, float[,] matrix, bool alpha = false, bool printPercentage = false )
        {
            // lock input buffer for read only and copy to buffer
            Rectangle rect = new Rectangle( 0, 0, input.Width, input.Height );
            BitmapData inputDataRegion = input.LockBits( rect, ImageLockMode.ReadOnly, input.PixelFormat );
            Int32 pixelDepth = Bitmap.GetPixelFormatSize( inputDataRegion.PixelFormat ) / 8; //bytes per pixel
            byte[] inputBuffer = new byte[inputDataRegion.Width * inputDataRegion.Height * pixelDepth];
            //copy pixels to buffer
            Marshal.Copy( inputDataRegion.Scan0, inputBuffer, 0, inputBuffer.Length );
            
            byte[] outputBuffer = new byte[inputBuffer.Length];
            
            for( Int32 y = 0; y < input.Size.Height; ++y )
            {
                ImgaeKernal_ProcessScanline( y, inputBuffer, outputBuffer, pixelDepth, input.Size, matrix, alpha, printPercentage );
            }

            //Parallel.For( 0, input.Size.Height, y => { ImgaeKernal_ProcessScanline( y, inputBuffer, outputBuffer, pixelDepth, input.Size, matrix, alpha, printPercentage ); } );
            
            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height, input.PixelFormat );

            BitmapData outputDataRegion = output.LockBits( rect, ImageLockMode.WriteOnly, input.PixelFormat );

            //Copy the buffer back to image
            Marshal.Copy( outputBuffer, 0, outputDataRegion.Scan0, outputBuffer.Length );

            input.UnlockBits( inputDataRegion );
            output.UnlockBits( outputDataRegion );

            if( printPercentage )
            {
                Console.Write( "\rDone.                                         \n" );
            }

            return output;
        }


        private static void ImgaeKernal_ProcessScanline( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, float[,] matrix, bool alpha, bool printPercentage )
        {
            Size halfMatrixSize = new Size( matrix.GetLength( 0 ) / 2, matrix.GetLength( 1 ) / 2 );

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                float[] value = new float[pixelDepth];
                for( Int32 color = 0; color < pixelDepth; ++color ) { value[color] = 0.0f; }

                for( Int32 _y = -halfMatrixSize.Height; _y <= halfMatrixSize.Height; ++_y )
                {
                    for( Int32 _x = -halfMatrixSize.Width; _x <= halfMatrixSize.Width; ++_x )
                    {
                        Int32 px = Func.Clamp( x + _y, 0, imageSize.Width - 1 );
                        Int32 py = Func.Clamp( y + _x, 0, imageSize.Height - 1 );

                        Int32 inputBufferPixelIndex = GetPixelIndex( px, py, imageSize.Width, pixelDepth );

                        Int32 matrixIndexX = _x + halfMatrixSize.Width;
                        Int32 matrixIndexY = _y + halfMatrixSize.Height;
                        for( Int32 color = 0; color < pixelDepth; ++color )
                        {
                            value[color] +=
                                matrix[matrixIndexX, matrixIndexY] *
                                (float)inputBuffer[inputBufferPixelIndex + color];
                        }
                    }
                }

                Int32 outputBufferPixelIndex = GetPixelIndex( x, y, imageSize.Width, pixelDepth );
                if( !alpha && pixelDepth >= 4 )
                {
                    value[3] = 255.0f;
                }
                for( Int32 color = 0; color < pixelDepth; ++color )
                {
                    outputBuffer[outputBufferPixelIndex + color] = (byte)Func.Clamp( value[color], 0.0f, 255.0f );
                }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 100.0m ) );
                }
            }
            return;
        }

        public static Bitmap AddImages( Bitmap input1, Bitmap input2 )
        {
            Size outputSize = new Size( Math.Min( input1.Width, input2.Width ), Math.Min( input1.Height, input2.Height ) );

            byte[] imageData1 = ExtractImageData( input1 );
            byte[] imageData2 = ExtractImageData( input2 );

            int numColors1 = BytesPerPixelIn( input1 );
            int numColors2 = BytesPerPixelIn( input2 );

            int numColors = Math.Min( numColors1, numColors2 );
            
            byte[] outputData = new byte[imageData1.Length];

            for( Int32 x = 0; x < outputSize.Width; ++x )
            {
                for( Int32 y = 0; y < outputSize.Height; ++y )
                {
                    Int32 ipd1 = GetPixelIndex( x, y, input1.Width, numColors1 );
                    Int32 ipd2 = GetPixelIndex( x, y, input2.Width, numColors2 );
                    Int32 opd0 = GetPixelIndex( x, y, outputSize.Width, numColors );
                    for( Int32 color = 0; color < numColors; ++color )
                    {
                        int c1 = imageData1[ipd1 + color];
                        int c2 = imageData2[ipd2 + color];
                        outputData[opd0 + color] = (byte)Func.Clamp( (int)c1 + (int)c2, 0, 255 );
                    }
                }
            }

            PixelFormat pixelFormat = ( numColors1 < numColors2 ) ? input1.PixelFormat : input2.PixelFormat;
            
            return GenerateImageFromData( outputSize, outputData, pixelFormat );
        }

        public static Bitmap MultiplyImages( Bitmap input1, Bitmap input2 )
        {
            Size outputSize = new Size( Math.Min( input1.Width, input2.Width ), Math.Min( input1.Height, input2.Height ) );

            byte[] imageData1 = ExtractImageData( input1 );
            byte[] imageData2 = ExtractImageData( input2 );

            int numColors1 = BytesPerPixelIn( input1 );
            int numColors2 = BytesPerPixelIn( input2 );

            int numColors = Math.Min( numColors1, numColors2 );

            byte[] outputData = new byte[imageData1.Length];

            for( Int32 x = 0; x < outputSize.Width; ++x )
            {
                for( Int32 y = 0; y < outputSize.Height; ++y )
                {
                    Int32 ipd1 = GetPixelIndex( x, y, input1.Width, numColors1 );
                    Int32 ipd2 = GetPixelIndex( x, y, input2.Width, numColors2 );
                    Int32 opd0 = GetPixelIndex( x, y, outputSize.Width, numColors );
                    for( Int32 color = 0; color < numColors; ++color )
                    {
                        float c1 = imageData1[ipd1 + color] / 255.0f;
                        float c2 = imageData2[ipd2 + color] / 255.0f;
                        outputData[opd0 + color] = (byte)Func.Clamp( (c1 * c2)*255.0f, 0.0f, 255.0f );
                    }
                }
            }

            PixelFormat pixelFormat = ( numColors1 < numColors2 ) ? input1.PixelFormat : input2.PixelFormat;

            return GenerateImageFromData( outputSize, outputData, pixelFormat );
        }

        // TODO: make this function select actual average colors instead of most common colors
        public static Bitmap ReduceImageColors( Bitmap input, Int32 numDesiredColors = 256, bool alpha = false, bool printPercentage = false )
        {
            // lock input buffer for read only and copy to buffer
            Rectangle rect = new Rectangle( 0, 0, input.Width, input.Height );
            BitmapData inputDataRegion = input.LockBits( rect, ImageLockMode.ReadOnly, input.PixelFormat );
            Int32 pixelDepth = Bitmap.GetPixelFormatSize( inputDataRegion.PixelFormat ) / 8; //bytes per pixel
            byte[] inputBuffer = new byte[inputDataRegion.Width * inputDataRegion.Height * pixelDepth];
            //copy pixels to buffer
            Marshal.Copy( inputDataRegion.Scan0, inputBuffer, 0, inputBuffer.Length );

            byte[] outputBuffer = new byte[inputBuffer.Length];

            // do the processing
            {

                Dictionary<Int32, Int32> histogramDictionary = new Dictionary<Int32, Int32>();
                Dictionary<Int32, byte[]> colorDictionary = new Dictionary<Int32, byte[]>();

                // get the histogram data
                for( Int32 y = 0; y < input.Size.Height; ++y )
                {
                    ReduceImageColors_ProcessScanline_GenerateHistogram( y, inputBuffer, outputBuffer, pixelDepth, input.Size, histogramDictionary, colorDictionary, alpha, printPercentage );
                }

                // TODO: fix color selection
                // here we select the colors to be used in the output
                List<KeyValuePair<Int32, Int32>> histogram = histogramDictionary.ToList();

                histogram.Sort( delegate ( KeyValuePair<Int32, Int32> t1, KeyValuePair<Int32, Int32> t2 ) { return t1.Value - t2.Value; } );

                histogram = new List<KeyValuePair<Int32, Int32>>( histogram.Take( numDesiredColors ) );  // .Take(N) if N >= .Length, returns of size .Length

                List<byte[]> topColors = new List<byte[]>();
                foreach( KeyValuePair<Int32, Int32> k in histogram )
                {
                    topColors.Add( colorDictionary[ k.Key ] );
                }
                
                // get the outputImage
                for( Int32 y = 0; y < input.Size.Height; ++y )
                {
                    ReduceImageColors_ProcessScanline_ConsolidateColors( y, inputBuffer, outputBuffer, pixelDepth, input.Size, topColors, alpha, printPercentage );
                }
            }

            Bitmap output = new Bitmap( input.Size.Width, input.Size.Height, input.PixelFormat );

            BitmapData outputDataRegion = output.LockBits( rect, ImageLockMode.WriteOnly, input.PixelFormat );

            //Copy the buffer back to image
            Marshal.Copy( outputBuffer, 0, outputDataRegion.Scan0, outputBuffer.Length );

            input.UnlockBits( inputDataRegion );
            output.UnlockBits( outputDataRegion );

            if( printPercentage )
            {
                Console.Write( "\rDone.                                         \n" );
            }

            return output;
        }

        private static void ReduceImageColors_ProcessScanline_GenerateHistogram( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, Dictionary<Int32, Int32> histogram, Dictionary<Int32, byte[]> colorSet, bool alpha, bool printPercentage )
        {

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                Int32 pixelIndex = GetPixelIndex( x, y, imageSize.Width, pixelDepth );

                byte[] value = new byte[pixelDepth];
                for( Int32 color = 0; color < pixelDepth; ++color ) { value[color] = inputBuffer[pixelIndex + color]; }

                if( !alpha && pixelDepth >= 4 )
                {
                    value[3] = 255;
                }

                Int32 colorIndex = GetColorIndex( value );

                if( histogram.Keys.Contains( colorIndex ) )
                {
                    histogram[colorIndex]++;
                }
                else
                {
                    histogram.Add( colorIndex, 1 );
                    colorSet.Add( colorIndex, value );
                }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 50.0m ) );
                }
            }
            return;
        }


        private static void ReduceImageColors_ProcessScanline_ConsolidateColors( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, List<byte[]> colors, bool alpha, bool printPercentage )
        {

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                Int32 pixelIndex = GetPixelIndex( x, y, imageSize.Width, pixelDepth );

                byte[] value = new byte[pixelDepth];
                for( Int32 color = 0; color < pixelDepth; ++color ) { value[color] = inputBuffer[pixelIndex + color]; }

                if( !alpha && pixelDepth >= 4 )
                {
                    value[3] = 255;
                }

                Int32 bestColorIndex = 0;
                float bestColorDist = ColorValueDistance( colors[0], value );
                for( Int32 i = 1; i < colors.Count; ++i )
                {
                    float dist = ColorValueDistance( colors[i], value );
                    if( dist < bestColorDist )
                    {
                        bestColorDist = dist;
                        bestColorIndex = i;
                    }
                }

                for( Int32 color = 0; color < pixelDepth; ++color ) { outputBuffer[pixelIndex + color] = colors[bestColorIndex][color]; }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 50.0m + 50.0m ) );
                }
            }
            return;
        }


        public static byte[] GetRawPixelFromImageData( byte[] imageData, Int32 x, Int32 y, Int32 width, Int32 depth )
        {
            byte[] output = new byte[depth];

            Int32 index = GetPixelIndex( x, y, width, depth );

            for( Int32 color = 0; color < depth; ++color )
            {
                output[color] = imageData[index + color];
            }

            return output;
        }

        public static Int32 GetPixelIndex( Int32 x, Int32 y, Int32 width, Int32 depth )
        {
            return ( ( y * width ) + x ) * depth;
        }

        public static float ColorValueDistance( byte[] a, byte[] b )
        {
            float sum = 0.0f;

            for( Int32 i = 0; i < a.Length && i < b.Length; ++i )
            {
                float dif = (int)a[i] - (int)b[i];
                sum += dif * dif;
            }
            return (float)Math.Sqrt(sum);
        }

        public static Int32 GetColorIndex( byte[] color )
        {
            return BitConverter.ToInt32( color, 0 );
        }
        
        public static Int32 BytesPerPixelIn( Bitmap input )
        {
            return Bitmap.GetPixelFormatSize( input.PixelFormat ) / 8;
        }

        public static byte[] ExtractImageData( Bitmap input )
        {
            Rectangle rect = new Rectangle( 0, 0, input.Width, input.Height );
            BitmapData dataRegion = input.LockBits( rect, ImageLockMode.ReadOnly, input.PixelFormat );
            Int32 pixelDepth = Bitmap.GetPixelFormatSize( dataRegion.PixelFormat ) / 8; //bytes per pixel
            byte[] output = new byte[dataRegion.Width * dataRegion.Height * pixelDepth];
            //copy pixels to buffer
            Marshal.Copy( dataRegion.Scan0, output, 0, output.Length );

            input.UnlockBits( dataRegion );

            return output;
        }

        public static Bitmap GenerateImageFromData( Size size, byte[] data, PixelFormat pixelFormat )
        {
            Bitmap output = new Bitmap( size.Width, size.Height, pixelFormat );

            Rectangle rect = new Rectangle( 0, 0, size.Width, size.Height );

            BitmapData outputDataRegion = output.LockBits( rect, ImageLockMode.WriteOnly, pixelFormat );

            //Copy the buffer to image
            Marshal.Copy( data, 0, outputDataRegion.Scan0, data.Length );

            output.UnlockBits( outputDataRegion );

            return output;
        }


        // from http://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
        public static Bitmap MakeGrayscale3( Bitmap original )
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap( original.Width, original.Height );

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage( newBitmap );

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                } );

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix( colorMatrix );

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage( original, new Rectangle( 0, 0, original.Width, original.Height ), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes );

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static Bitmap ScaleDownImageNearest( Bitmap input, Size newSize )
        {
            Int32 pixelDepth = BytesPerPixelIn( input );
            byte[] inputData = ExtractImageData( input );
            byte[] outputData = new byte[ newSize.Width * newSize.Height * pixelDepth ];

            for( Int32 y = 0; y < newSize.Height; ++y )
            {
                for( Int32 x = 0; x < newSize.Width; ++x )
                {
                    Int32 outputPixelIndex = GetPixelIndex( x, y, newSize.Width, pixelDepth );
                    Int32 inputPixelIndex = GetPixelIndex( (Int32)Func.Clamp( (double)x/(double)newSize.Width*(double)input.Width, 0, input.Width - 1 ), (Int32)Func.Clamp( (double)y / (double)newSize.Height * (double)input.Height, 0, input.Height - 1 ), input.Width, pixelDepth );

                    for( Int32 color = 0; color < pixelDepth; ++color )
                    {
                        outputData[outputPixelIndex + color] = inputData[inputPixelIndex + color];
                    }
                }
            }

            return GenerateImageFromData( newSize, outputData, input.PixelFormat );
        }

    }
}
