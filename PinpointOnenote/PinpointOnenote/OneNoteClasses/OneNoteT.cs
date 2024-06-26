﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Constructor method to build OneNoteT from static XML.
        /// </summary>
        /// <param name="IhFW"></param>
        /// <param name="IhF"></param>
        /// <param name="IndentCt"></param>
        /// <param name="InputSpansXml"></param>
        /// <param name="linksLookup"></param>
        /// <param name="inputBullet"></param>
        /// <param name="defaultBold"></param>
        public OneNoteT(string IhFW, string IhF, int IndentCt, IEnumerable<XElement> InputSpansXml = null,
            Dictionary<string,Dictionary<string, object>> linksLookup = null,
            string inputBullet = null, bool defaultBold = false)
        {
            // Constructor for the OneNote T from page data as Static XML.
            List<OneNoteSpan> lineSpans = new List<OneNoteSpan>();
            InheritedFontWeight = IhFW;
            InheritedFont = IhF;
            indentCount = IndentCt;
            Bullet = inputBullet;
            if (InputSpansXml == null || InputSpansXml.FirstOrDefault() == null)
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

                    // First thing to do is to check for internal hyperlinking
                    bool hasInternalPageHL = span.Attribute("InternalLinkPageName") != null;
                    bool hasInternalSectionHL = span.Attribute("InternalLinkSectionName") != null;
                    bool existLinksLookup = linksLookup != null;
                    if (existLinksLookup & hasInternalSectionHL & hasInternalPageHL)
                    {
                        // The T constructor has been given a OneNote apge links lookup from the XML data parser, and the span object in XML has attributes for internal link section and page names.
                        // So attempt to bould a hyperlink and give it to the .HTML property
                        //spanObj.HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                        spanObj.rawText = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                        string pageNameLinkTo = span.Attribute("InternalLinkPageName").Value;
                        string sectNameLinkTo = span.Attribute("InternalLinkSectionName").Value;
                        if (linksLookup.ContainsKey(sectNameLinkTo)) 
                        {
                            string sectLinkToID = (string)linksLookup[sectNameLinkTo]["sectionId"];
                            Dictionary<string, object> linkToSectPagesDict = (Dictionary<string, object>)linksLookup[sectNameLinkTo]["pages"];

                            HashSet<string> linkToSectPagesDictUniqueValues = new HashSet<string>();
                            foreach (var pair in linkToSectPagesDict)
                            {
                                if (pair.Value != null)
                                {
                                    string stringValue = pair.Value.ToString();
                                    linkToSectPagesDictUniqueValues.Add(stringValue);
                                }
                            }
                            string[] uniquePageNames = linkToSectPagesDictUniqueValues.ToArray();



                            if (uniquePageNames.Contains(pageNameLinkTo))
                            {
                                // We've found a page name/linkID value and the section ID, so we can do the link
                                string RT = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                                string HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                                spanObj.rawText = RT;

                                string pageLinkToID = linkToSectPagesDict.First(x => x.Value.ToString() == pageNameLinkTo).Key; //(string)linkToSectPagesDict[pageNameLinkTo];
                                string embedLink = OneNotePageFmtMethods.GetOneNoteHyperLinkHTML(sectLinkToID, pageLinkToID, pageNameLinkTo, RT);
                                Regex rx = new Regex(@"<span\s+style\s*=\s*[""'][^""']*[""']\s*>");
                                Match match = rx.Match(HTML);
                                if (match.Success)
                                {
                                    spanObj.HTML = match.Value + embedLink + "</span>";
                                }
                                else
                                {
                                    spanObj.HTML = "<span>" + embedLink + "</span>";
                                }

                            }
                            else
                            {
                                //We found the section ID, but not an id for the page asked for, so don't attempt the hyperlink.
                                spanObj.HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                                spanObj.rawText = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                            }
                        }
                        else 
                        {
                            // The section Link to tag wasn't found in the lookup keys, so don't attempt the hyerlink
                            spanObj.HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                            spanObj.rawText = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                        }
                    }
                    else
                    {
                        // T constructor doesn't ahve enough info to build a hyperlink.
                        spanObj.HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                        spanObj.rawText = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                    }
                    
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


        /// <summary>
        /// Constructor method to build a header row cell or data row cell T text line from a password bank item.
        /// </summary>
        /// <param name="IhFW"></param>
        /// <param name="IhF"></param>
        /// <param name="spans"></param>
        /// <param name="defaultBold"></param>
        public OneNoteT(string IhFW, string IhF,List<Dictionary<string,string>> spans, int IndentCt = 0, string inputBullet = null)
        {
            List<OneNoteSpan> lineSpans = new List<OneNoteSpan>();
            InheritedFontWeight = IhFW;
            InheritedFont = IhF;
            indentCount = IndentCt;
            Bullet = inputBullet;
            if (spans.Count == 0)
            {
                //throw new Exception("Attempt to build a OneNoteT line from XML with no <span> elements in it.");
                // Give it one span with no text
                OneNoteSpan spanObj = new OneNoteSpan();
                string blankSpanEmptyText = "";
                spanObj.HTML = blankSpanEmptyText;
                spanObj.rawText = blankSpanEmptyText;
                spanObj.isBold = false;
                lineSpans.Add(spanObj);
            }
            else
            {
                foreach (Dictionary<string, string> inputSpan in spans)
                {
                    OneNoteSpan spanObj = new OneNoteSpan();
                    spanObj.HTML = inputSpan["HTML"];
                    spanObj.rawText = inputSpan["RawText"];
                    spanObj.isBold = bool.Parse(inputSpan["isBold"]);
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
