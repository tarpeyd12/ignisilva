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
    class ImageProcessing
    {
        private static T Clamp<T>( T val, T min, T max ) where T : IComparable<T>
        {
            if( val.CompareTo( min ) < 0 ) return min;
            else if( val.CompareTo( max ) > 0 ) return max;
            else return val;
        }

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
                ProcessScanline( y, inputBuffer, outputBuffer, pixelDepth, input.Size, matrix, alpha, printPercentage );
            }

            //Parallel.For( 0, input.Size.Height, y => { ProcessScanline( y, inputBuffer, outputBuffer, pixelDepth, input.Size, matrix, alpha, printPercentage ); } );
            
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


        private static void ProcessScanline( Int32 y, byte[] inputBuffer, byte[] outputBuffer, Int32 pixelDepth, Size imageSize, float[,] matrix, bool alpha, bool printPercentage )
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
                        Int32 px = Clamp( x + _y, 0, imageSize.Width - 1 );
                        Int32 py = Clamp( y + _x, 0, imageSize.Height - 1 );

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
                    outputBuffer[outputBufferPixelIndex + color] = (byte)Clamp( value[color], 0.0f, 255.0f );
                }

                int _c;
                if( printPercentage && ( _c = y * imageSize.Width + x ) % 1200 == 0 )
                {
                    Console.Write( "{0:F2}%\r", ( (decimal)_c / (decimal)( imageSize.Width * imageSize.Height ) * 100.0m ) );
                }
            }
            return;
        }

        private static Int32 GetPixelIndex( Int32 x, Int32 y, Int32 width, Int32 depth )
        {
            return ( ( y * width ) + x ) * depth;
        }
        
    }
}
