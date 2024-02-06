using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote
{
    public class OneNoteSpan
    {
        public string rawText { get; set; }
        public string customFontWeight { get; set; }
        public string customFont { get; set; }
        public bool isBold { get; set; } // boldness of text can only be set at the CDATA/span level.
        public string HTML { get; set; }
    }
    public class OneNoteT
    {
        public string InheritedFontWeight { get; set; }
        public string InheritedFont { get; set; }
        public List<OneNoteSpan> textSpans { get; set; }
        public string cDataValue 
        {
            get {
                StringBuilder allText = new StringBuilder();
                foreach (OneNoteSpan span in textSpans)
                {
                    allText.Append(span.HTML);
                }
                return allText.ToString();
            }            
        }
        public double widthNeeded { 
            get 
            {
                XElement convTableXml = OneNotePageFmtMethods.GetFontSizeConversionTable();
                string fontweight;
                string font;
                double charWeight;
                double result = 0.0;
                foreach (OneNoteSpan span in textSpans)
                {

                    if (span.customFont != null)
                    {
                        font = span.customFont;
                    }
                    else
                    {
                        font = InheritedFont;
                    }

                    if (span.customFontWeight != null)
                    {
                        fontweight = span.customFontWeight;
                    }
                    else
                    {
                        fontweight = InheritedFontWeight;
                    }

                    XElement conversionFontWeightXML = convTableXml.Elements("SizingConverter")
                        .Where(x => x.Attribute("fontSize").Value == fontweight).FirstOrDefault();
                    if (conversionFontWeightXML == null)
                    {
                        throw new Exception($"Font conversion value not found for fontsize {fontweight}");
                    }
                    else
                    {
                        XElement conversionFontXML = conversionFontWeightXML.Elements("SizingConverterFont")
                            .Where(x => x.Attribute("fontName").Value == font).FirstOrDefault();

                        if (conversionFontXML == null)
                        {
                            throw new Exception($"Font conversion value not found for fontsize {fontweight} with specific font {font}");
                        }
                        else
                        {
                            // we are now in a position to convert
                            if (span.isBold) 
                            {
                                charWeight = Convert.ToDouble(conversionFontXML.Attribute("charWidthBold").Value);
                            } 
                            else 
                            {
                                charWeight = Convert.ToDouble(conversionFontXML.Attribute("charWidth").Value);
                            }
                            result += charWeight * span.rawText.Length;
                        }
                    }

                }
                return result + 15; 
            } 
        }
    }
}
