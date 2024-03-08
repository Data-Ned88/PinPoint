using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote.OneNoteClasses
{
    /// <summary>
    /// This represents a line of text plus optional bullet point or number preceding it.
    /// It can calculate its own width by adding up the length of its spans and * by font size/family-adjusted px,
    /// and adding any bullet points or indents from bullet points.
    /// This *does not* support numbered bullet lists.
    /// </summary>
    public class OneNoteT
    {
        public string InheritedFontWeight { get; set; }
        public string InheritedFont { get; set; }
        public int indentCount { get; set; } //How many indents does it have if it's bulleted?
        /// <summary>
        /// This is the string denoting the (<ul>) bullet number stype:
        /// "2" is a filled round  bullet, "3" is a hollow round bullet. 
        /// if unassigned, it will default to null and the user of this class should assume no bullet and build the T without a preceding List>Bullet.
        /// </summary>
        public string Bullet { get; set; }
        public List<OneNoteSpan> textSpans { get; set; }
        public string cDataValue
        {
            get
            {
                StringBuilder allText = new StringBuilder();
                foreach (OneNoteSpan span in textSpans)
                {
                    allText.Append(span.HTML);
                }
                return allText.ToString();
            }
        }
        public double widthNeeded
        {
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

                //Bullets and indents optional
                if (Bullet != null || indentCount > 0)
                {
                    //Take the character measurements from the Inherited Font. Same pattern and validation checks.

                    XElement bulletIndentConversionFWXML = convTableXml.Elements("SizingConverter")
                            .Where(x => x.Attribute("fontSize").Value == InheritedFontWeight).FirstOrDefault();
                    if (bulletIndentConversionFWXML == null)
                    {
                        throw new Exception($"Font conversion value not found for fontsize {InheritedFontWeight}");
                    }
                    else
                    {
                        XElement bulletIndentConversionFontXML = bulletIndentConversionFWXML.Elements("SizingConverterFont")
                            .Where(x => x.Attribute("fontName").Value == InheritedFont).FirstOrDefault();
                        if (bulletIndentConversionFontXML == null)
                        {
                            throw new Exception($"Font conversion value not found for fontsize {InheritedFontWeight} with specific font {InheritedFont}");
                        }
                        else
                        {
                            double charWeightIndentBullet = Convert.ToDouble(bulletIndentConversionFontXML.Attribute("charWidth").Value);
                            // bullet = 2 (bullet then space) * sizing
                            if (Bullet != null)
                            {
                                result += charWeightIndentBullet * 2;
                            }
                            // indents = indent * 4 * sizing
                            if (indentCount > 0)
                            {
                                result += charWeightIndentBullet * 4 * indentCount;
                            }
                        }
                    }
                }

                return result + 15;
            }
        }

        public OneNoteT(string IhFW, string IhF, int IndentCt, IEnumerable<XElement> InputSpansXml, string inputBullet = null, bool defaultBold = false)
        {
            // Constructor for the OneNote T from page data as Static XML.
            List<OneNoteSpan> lineSpans = new List<OneNoteSpan>();
            InheritedFontWeight = IhFW;
            InheritedFont = IhF;
            indentCount = IndentCt;
            Bullet = inputBullet;
            if (InputSpansXml.FirstOrDefault() == null)
            {
                //throw new Exception("Attempt to build a OneNoteT line from XML with no <span> elements in it.");
                // Give it one span with no text
                OneNoteSpan spanObj = new OneNoteSpan();
                string blankSpanEmptyText = "";
                spanObj.HTML = blankSpanEmptyText;
                spanObj.rawText = blankSpanEmptyText;
                lineSpans.Add(spanObj);
            }
            else
            {

                foreach (XElement span in InputSpansXml)
                {
                    OneNoteSpan spanObj = new OneNoteSpan();
                    spanObj.HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                    spanObj.rawText = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                    if (span.Attribute("isBold") != null)
                    {
                        spanObj.isBold = bool.Parse(span.Attribute("isBold").Value);
                    }
                    if (spanObj.isBold == false & defaultBold)
                    {
                        spanObj.isBold = true;
                    }
                    if (span.Attribute("customFont") != null)
                    {
                        spanObj.customFont = span.Attribute("customFont").Value;
                    }
                    if (span.Attribute("customFontWeight") != null)
                    {
                        spanObj.customFontWeight = span.Attribute("customFontWeight").Value;
                    }
                    lineSpans.Add(spanObj);
                }


            }
            textSpans = lineSpans;

        }

        public OneNoteT()
        {
        }
    }
}
