using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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

        public static float[,] EdgeFindKernal
        {
            get
            {
                return new float[,]{
                {-1,-1,-1},
                {-1,8,-1},
                {-1,-1,-1}};
            }
        }

        public static float[,] GaussianBlurKernal
        {
            get
            {
                return new float[,]{
                {1.0f*0.0625f,2.0f*0.0625f,1.0f*0.0625f},
                {2.0f*0.0625f,4.0f*0.0625f,2.0f*0.0625f},
                {1.0f*0.0625f,2.0f*0.0625f,1.0f*0.0625f}};
            }
        }

        public static Bitmap ImageKernal( Bitmap input, float[,] matrix )
        {
            Size imageSize = input.Size;

            Bitmap output = new Bitmap( imageSize.Width, imageSize.Height );

            /*float[,] matrix = new float[,]{
                {-1,-1,-1},
                {-1,8,-1},
                {-1,-1,-1}};*/

            for( Int32 x = 0; x < imageSize.Width; ++x )
            {
                for( Int32 y = 0; y < imageSize.Height; ++y )
                {
                    float value = 0.0f;

                    for( Int32 _x = -1; _x <= 1; ++_x )
                    {
                        for( Int32 _y = -1; _y <= 1; ++_y )
                        {
                            Int32 px = Clamp( x + _x, 0, imageSize.Width-1 ); 
                            Int32 py = Clamp( y + _y, 0, imageSize.Height-1 );

                            value += matrix[_x + 1, _y + 1] * input.GetPixel( px, py ).GetBrightness();
                        }
                    }

                    value *= 255.0f;
                    if( value > 255.0f ) value = 255.0f;
                    if( value < 0.0f ) value = 0.0f;

                    Color outputColor = Color.FromArgb( (byte)value, (byte)value, (byte)value );
                    output.SetPixel( x, y, outputColor );
                }
            }

            return output;
        }
    }
}
