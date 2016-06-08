using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ignisilva
{

    

    class Program
    {
        static void Main( string[] args )
        {
            Bitmap image = new Bitmap( Image.FromFile( "../../../images/flower.jpg" ) );

            Bitmap gaussian = image;
            for( int i = 0; i < 1; ++i )
            {
                gaussian = ImageProcessing.ImageKernal( gaussian, ImageProcessing.GaussianBlurKernal );
            }
            
            gaussian.Save( "../../../images/output_gaussian.png" );

            Bitmap final = ImageProcessing.ImageKernal( gaussian, ImageProcessing.EdgeFindKernal );
            final.Save("../../../images/output_final.png");

        }
    }
}
