using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote
{
    //The file is called OneNoteSpan.cs but contains all the granular classes needed to format onenote pages.  
    public class OneNoteSpan
    {
        public string rawText { get; set; } //MANDATORY - text to appear on the Page without HTML and CSS wrapping. For width calculation.
        //OPTIONAL - Populate the below if the span within a line has to differ from the rest of the line in font size. Must be in (9.0,10.0,10.5,11.0-16.0,18.0)
        public string customFontWeight { get; set; }
        //OPTIONAL - Same as above comment/property but for font family. Must be in (Arial,Calibri,Times New Roman).
        public string customFont { get; set; }
        public bool isBold { get; set; } // boldness of text can only be set at the CDATA/span level.
        public string HTML { get; set; } //MANDATORY - HTML formatted text to add to the XML.      
    }
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

                //Bullets and indents optional
                if (Bullet != null || indentCount > 0 )
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
    }

    public class OneNoteOE
    {
        public bool isHeaderless { get; set; } = false; //only for OE As Section
        public bool treatAsCell { get; set; } = false; //if true, need to read as a table cell (optional shading color override)
        public OneNoteT textLine { get; set; } // main text with optional bullet point
        //MANDATORY - give it the name of a custom tag you want to give it (section), or "p" for generic.
        //As a strategy, we're just using this for tagging. We'll override with style CSS proeprties on the OE at all times for ease of lineage tracking.
        public string quickStyleIndexName { get; set; }
        public string alignment { get; set; } = "left"; //(left/center/right)
        public string fontFamily { get; set; } //MANDATORY> This contributes to InheritedFont in textLine and to the CssStyle
        public string fontWeight { get; set; } //MANDATORY> This contributes to InheritedFontWeight in textLine and to the CssStyle
        public string fontColor { get; set; } = "black"; //MANDATORY> This contributes to the CssStyle
        public string CssStyle 
        { 
            get {
                return $"'font-family:{fontFamily};font-size:{fontWeight}pt;color:{fontColor}'";
                } 
         }
        public string cellShadingOverride { get; set; } //only looked at when treatAsCell is True

        public List<OneNoteOEChild> OEChildren = new List<OneNoteOEChild>();

    }

    /// <summary>
    /// The base OneNoteOE holds a <List> of these in its OEChildren property.
    /// It contains 3 props: a type (table or Base OE), and 2 lists - a list of OeTable and a list of BaseOE
    /// They both get instantiated as empty and should only hold 1 element, which is called in a .FirstOrDefault();
    /// The type property controls which one gets looked at. Wrapping them in lists makes it easier to be flexible.
    /// </summary>
    public class OneNoteOEChild
    {
        public OneNoteOEChildType Type { get; set; }
        public List<OneNoteOE> BaseOE { get; set; } = new List<OneNoteOE>();
        public List<OneNoteTable> TableOE { get; set; } = new List<OneNoteTable>();
    }


    public class OneNoteTable
    {
        // has Headers, data (List>List>OE), custom col widths (dictionary), table color theme (from XML), table sizing (from xml)
        public bool hasHeaders { get; set; } //does the table have a header row? Controls the use of font size and shading in xml params
        public List<List<OneNoteOE>> dataRows { get; set; } = new List<List<OneNoteOE>>(); //This includes headers in row 1 if they exist.
        // Below - for any columns that you want to fix or cap width.
        // Each key is the position of the column (start at 0), and the value is a dictionary of string/string, where the ...
        // ... key is "width" (+ number as text for value) or "type" (with value of either "fix"/ "cap_at").
        public Dictionary<int, Dictionary<string, string>> customColWidths { get; set; } = new Dictionary<int, Dictionary<string, string>>();
        public XElement colorXml { get; set; } //MANDATORY - colour patterns from the XML resource (ColorTheme)
        public XElement sizingXml { get; set; } //MANDATORY - font weights for the table from the XML resource (TableSizing)
        public bool bordersVisible { get; set; } = true;

    }
}
