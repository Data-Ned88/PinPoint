using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PinpointOnenote.OneNoteClasses;
using PinpointOnenote.Properties;


namespace PinpointOnenote
{
    public static class DataParsers
    {
        public static string GetAllowableFontAsStr(AllowableFonts font)
        {
            string output;

            switch(font)
            {
                case AllowableFonts.Arial:
                    output = "Arial";
                    break;
                case AllowableFonts.Calibri:
                    output = "Calibri";
                    break;
                case AllowableFonts.TimesNewRoman:
                    output = "Times New Roman";
                    break;
                default:
                    output = "Calibri";
                    break;
            }

            return output;
        }

        public static List<OneNoteOE> BuildPageDataFromXml (XElement pageXML)
        //XElement param pageXML should be a "Page" element from StaticAndTestData.xml
        // The page is made up of sections. THIS FUNCTION must pad a line break as OE between each section.
        {
            List<OneNoteOE> output = new List<OneNoteOE>();

            return output;

        }
        public static OneNoteOE BuildOEWithChildrenFromXml (XElement nodeXml, XElement sizingOptions, XElement tableCol, AllowableFonts defaultFont, int inheritedIndents = 0)
        {
            //TODO build out the 2 recursive functions to make class data from static XML, and onenote XML from class object.
            //This should be recursive and take in a Section element from a page from StaticAndTestData.xml, or anything more granular. //THIS IS UNFINISHED

            // XElement resource = XElement.Parse(PinpointOnenote.Properties.Resources.StaticAndTestData);  // gets the static and test data resource file.
            // XElement pageDataXml = resource.Descendants("Page").Where(x => x.Attribute("name").Value == "Notes and Instructions").First(); //Gets the first page from this
            // XElement firstSection = pageDataXml.Element("Sections").Elements("Section").First(); // gets the first section from this.

            // Params:
            // 1. XElement nodeXml - the XML page data section or below.
            // 2. sizingOptions - a <TableSizing> from PinpointOnenote.Properties.Resources.OneNotePageAndElementStyles
            // 3. tableCol - a <ColorTheme> from PinpointOnenote.Properties.Resources.OneNotePageAndElementStyles
            // 4. defaultFont - font which is either Arial,Calibri or Times New Roman
            // 5. inheritedIndents - default 0, incrementable for child sections

            OneNoteOE output = new OneNoteOE();

            AllowableFonts defaultFontSel = defaultFont;
            XElement sizingOptionsSel = sizingOptions;
            XElement tableColSel = tableCol;
            int inputIndents = inheritedIndents;

            string [] sectionChildrenOk = {"Line", "Table"};
            string[] lineChildrenOk = { "Line" }; // Lines can only contain lines as sub bullets. A line containing a table has to be a section.

            string defaultFontStr = GetAllowableFontAsStr(defaultFontSel);
            output.fontFamily = defaultFontStr;
            


            // Dealing with the input itself. 

            if (nodeXml.Name == "Section")
            {
                output.quickStyleIndexName = nodeXml.Attribute("name").Value;
                output.isHeaderless = bool.Parse(nodeXml.Attribute("headerless").Value); // script will fail if you don't have this attribute in your XML.
                output.inheritedIndents = inputIndents;
                output.fontWeight = sizingOptionsSel.Attribute("fontSizeSectionHead").Value;
                output.oeType = OneNoteOEType.Section;
                IEnumerable<XElement> children = nodeXml.Elements().Where(x => sectionChildrenOk.Contains(x.Name.ToString()));

                if (output.isHeaderless)
                {
                    // If a section is headerless, it has to be UNI-element. (Nothing if not a header can unite 2+ areas. So find the first child and process that.
                    // all lines  (if any found) are children, who are given 1 indent. Evaluate the children with this in mind.

                    XElement firstElement = children.FirstOrDefault();
                    if (firstElement == null)
                    {
                        throw new Exception("Headerless section with no elements.");
                    }
                    else if (firstElement.Name == "Table")
                    {
                        //TODO Table behaviour - the table is output.table 
                        output.oeType = OneNoteOEType.Table;
                    }
                    else
                    {
                        // It's a line.
                        output.oeType = OneNoteOEType.BaseOE;
                        output.fontWeight = sizingOptionsSel.Attribute("fontSizeText").Value; //Since it's a single-line headerless section, size it abck to the line size not the header size.
                        OneNoteT lineData = new OneNoteT();
                        lineData.indentCount = inputIndents;
                        lineData.InheritedFont = output.fontFamily;
                        lineData.InheritedFontWeight = output.fontWeight;
                        if (firstElement.Attribute("Bullet") != null)
                        {
                            lineData.Bullet = firstElement.Attribute("Bullet").Value;
                        }
                        List<OneNoteSpan> lineSpans = new List<OneNoteSpan>();
                        IEnumerable<XElement> spansXml = firstElement.Elements("span");
                        foreach (XElement span in spansXml)
                        {
                            OneNoteSpan spanObj = new OneNoteSpan();
                            spanObj.HTML = ((XCData)span.Element("HTML").FirstNode).Value.ToString();
                            spanObj.rawText = ((XCData)span.Element("RawText").FirstNode).Value.ToString();
                            if (span.Attribute("isBold") != null)
                            {
                                spanObj.isBold = bool.Parse(span.Attribute("isBold").Value);
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
                        output.textLine = lineData;
                        // DO the child procedure on the line.
                        children = firstElement.Elements().Where(x => lineChildrenOk.Contains(x.Name.ToString()));
                        foreach (XElement child in children)
                        {
                            OneNoteOE childOE = BuildOEWithChildrenFromXml(child,
                                                    sizingOptionsSel,
                                                    tableColSel, defaultFontSel, inputIndents+1);
                            output.OEChildren.Add(childOE);
                        }
                    }


                }
                else
                {
                    //The first line (if found) is the header, and any lines after (if any) are processed as children.
                    int lineInc = 0;
                    foreach (XElement child in children)
                    {
                        if (child.Name == "Line")
                        {
                            if (lineInc == 0)
                            {
                                // Make the line input from the XElement. IT has to be childless, because its the header of a section so it cant contain bullet points. or be multi line.
                                OneNoteT headerline = new OneNoteT();
                            }
                            else
                            {
                                // Its a line after lineInc 0, so it's text in the main section. We want this indented and OeCHildrenWrapped so that it can be collapsed.
                            }
                            lineInc++;
                        }
                        else
                        {
                            // Its a table, which again we want indented so that it can be collapsed.
                        }
                    }
                }


                //1. Give it the qindex
                //2. Identify its child baseOEs (lines or sections) and child tables, and recurse them, passing on n indentations if or if not headerless.

            }
            else if (nodeXml.Name == "Line")
            {
                // THIS NEEDS TO HANDLE BLANK LINES (empty line elmenets in the XML)
                //1. Deal with the line itself (fonts/bullets/indents/spans/hyperlinks - need a resource for this containing the relevant ids/ spans)
                //2. Identify its child BaseOEs/Tables and recurse them.
            }
            else if (nodeXml.Name == "Table")
            {
                //. You will need a table function to loop the columns and cells, called from here. This table function should produce lines in the cells, ...
                // ... which may have their own lines, which will therefore call this function.
            }

            // Deal with its child nodes

            return output;
        }



    }
}
