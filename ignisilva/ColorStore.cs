using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ignisilva
{
    class ColorStore : IXmlWritable
    {
        private UInt32 _maxNumColors;
        private UInt32 _colorlength;

        private Dictionary<UInt32, Tuple<Int32, byte[]>> hashToColor;
        private Dictionary<Int32, byte[]> idToColor;

        private HashTableHashing.IHashAlgorithm _hasher;

        public UInt32 MaxNumColors { get { return _maxNumColors; } }
        public UInt32 HistogramLength { get { return _maxNumColors; } }
        public Int32 NumColors { get { return hashToColor.Count; } }
        public UInt32 ColorLength { get { return _colorlength; } }


        ColorStore( UInt32 cololength, UInt32 maxnumcolors)
        {
            _maxNumColors = maxnumcolors;
            _colorlength = cololength;
            _hasher = new HashTableHashing.MurmurHash2Simple();
        }

        public byte[] ColorToHistogram( byte[] color )
        {
            UInt32 hash = _hasher.Hash( color );

            // if the color is not in the database
            if( !hashToColor.ContainsKey( hash ) )
            {
                if( NumColors >= MaxNumColors )
                {
                    return null;
                }

                // add it
                hashToColor.Add( hash, Tuple.Create( NumColors, color ) );
            }

            byte[] histogram = new byte[HistogramLength];

            for( UInt32 i = 0; i < histogram.Length; ++i )
            {
                histogram[i] = 0;
            }

            // set the index of the color in the histogram to max value
            histogram[hashToColor[hash].Item1] = 255;

            return histogram;
        }


        public byte[] HistogramToColor( byte[] histogram )
        {
            if( histogram.Length != HistogramLength )
            {
                return null;
            }

            if( null == idToColor )
            {
                idToColor = new Dictionary<Int32, byte[]>();

                foreach( KeyValuePair<UInt32, Tuple<Int32, byte[]>> hc in hashToColor )
                {
                    idToColor.Add( hc.Value.Item1, hc.Value.Item2 );
                }
            }
            
            double[] floatColor = new double[ColorLength];

            for( Int32 index = 0; index < HistogramLength; ++index )
            {
                double value = (double)histogram[index] / 255.0;

                byte[] colorAtIndex = idToColor[index];

                for( UInt32 i = 0; i < ColorLength; ++i )
                {
                    floatColor[i] += ((double)colorAtIndex[i]/255.0) * value;
                }
            }

            floatColor = Func.NormalizeDoubleArray( floatColor );

            byte[] color = new byte[ColorLength];

            for( UInt32 i = 0; i < color.Length; ++i )
            {
                color[i] = (byte)( Func.Clamp( floatColor[i] * 255.0, 0.0, 255.0 ) );
            }
            
            return color;
        }

        public XmlWriter WriteXml( XmlWriter xml, string fmt = "b64" )
        {
            xml.WriteStartElement( "color_store" );
            xml.WriteAttributeString( "color_len", ColorLength.ToString() );
            xml.WriteAttributeString( "hist_len", HistogramLength.ToString() );
            xml.WriteAttributeString( "num_colors", NumColors.ToString() );
            foreach( KeyValuePair<UInt32, Tuple<Int32, byte[]>> hc in hashToColor )
            { 
                xml.WriteStartElement( "color" );
                xml.WriteAttributeString( "id", hc.Value.Item1.ToString() );
                string format = XmlHelper.RecommendBinaryEncodingFormat( fmt, hc.Value.Item2 );
                xml.WriteAttributeString( "format", format );
                XmlHelper.WriteByteString( xml, hc.Value.Item2, format );
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            return xml;
        }

    }
}
