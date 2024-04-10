using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PinpointOnenote.OneNoteClasses;
using PinpointOnenote.Properties;
using OneNoteInterop = Microsoft.Office.Interop.OneNote;


namespace PinpointOnenote
{
    /// <summary>
    /// Eventually I want to moave all the PinPoint-specific functionality out of here and into a separate class.
    /// </summary>
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

        public static List<OneNoteOE> BuildPageDataFromXml (XElement pageXML, XElement sizingOptions, XElement tableCol, AllowableFonts defaultFont,
            Dictionary<string,Dictionary<string, object>> linksLookup)
        //XElement param pageXML should be a "Page" element from StaticAndTestData.xml
        // The page is made up of sections.
        {
            List<OneNoteOE> output = new List<OneNoteOE>();
            XElement pageSections = pageXML.Element("Sections");
            IEnumerable<XElement> sectionsList = pageSections.Elements("Section");
            if (sectionsList.FirstOrDefault() == null)
            {
                string pageName = pageXML.Attribute("name").Value.ToString();
                throw new Exception($"BuildPageDataFromXml ERROR: The Xml data for your page {pageName} has no Section elements, or no Section wrapper element.");
            }
            foreach (XElement sectionXml in sectionsList)
            {
                output.Add(BuildOEWithChildrenFromXml(sectionXml,sizingOptions,tableCol,defaultFont, linksLookup));
            }
            return output;

        }

        public static OneNoteTable BuildTableFromXml(XElement nodeXml, XElement sizingOptions, XElement tableCol, AllowableFonts defaultFont, Dictionary<string, 
            Dictionary<string, object>> linksLookup)
        {
            // Params:
            // 1. XElement nodeXml - this is the <Table> XElement from a page xml from StaticAndTestData.xml or something with the same structure.
            // 2. sizingOptions - a <TableSizing> from PinpointOnenote.Properties.Resources.OneNotePageAndElementStyles
            // 3. tableCol - a <ColorTheme> from PinpointOnenote.Properties.Resources.OneNotePageAndElementStyles
            // 4. defaultFont - font which is either Arial,Calibri or Times New Roman
            // 5.Dictionary<string, Dictionary<string, object>> linksLookup - this is all the onenote section names/linkableIds + nested dict of ...
            // ... their pages name/linableId key/value pairs in play. THis is needed to build internal linking spans if needed.
            OneNoteTable output = new OneNoteTable();

            string defaultFontStr = GetAllowableFontAsStr(defaultFont);
            string defaultFontSize;
            string defaultFontColor;
            string defaultShadingColor;
            bool BoldRow = false;
            //string fontSizeTHead = sizingOptions.Attribute("fontSizeTableHead").Value;

            if (nodeXml.Attribute("ShowBorders").Value == "yes")
            {
                output.bordersVisible = true;
            }
            if (nodeXml.Attribute("HeaderRow").Value == "yes")
            {
                output.hasHeaders = true;
            }
            output.colorXml = tableCol;
            output.sizingXml = sizingOptions;

            // Need to handle the data before we can get to the headers.
            int rowIncr = 0;
            IEnumerable<XElement> rowsXml = nodeXml.Elements("Row");

            if (rowsXml.FirstOrDefault() == null)
            {
                throw new Exception("Attempt to build a OneNoteTable from XML with no <Row> elements in it.");
            }
            foreach(XElement row in rowsXml)
            {
                List<OneNoteTableCell> dataRow = new List<OneNoteTableCell>();
                if (output.hasHeaders & rowIncr==0)
                {
                    // the table has headers and we're on the header row.
                    defaultFontSize = sizingOptions.Attribute("fontSizeTableHead").Value;
                    defaultFontColor = tableCol.Attribute("titleColor").Value;
                    defaultShadingColor = tableCol.Attribute("titleShade").Value;
                    BoldRow = true;
                }
                else
                {
                    // alternate shading and font colouring
                    defaultFontSize = sizingOptions.Attribute("fontSizeText").Value;
                    defaultFontColor = "#000000"; //black
                    defaultShadingColor = tableCol.Attribute("alternateShade").Value;
                    BoldRow = false;
                    if (rowIncr % 2 == 0)
                    {
                        defaultShadingColor = tableCol.Attribute("alternateShade").Value;
                    }
                    else
                    {
                        defaultShadingColor = tableCol.Attribute("mainShade").Value;
                    }
                }
                IEnumerable<XElement> cellXml = row.Elements("Cell");
                if (cellXml.FirstOrDefault() == null)
                {
                    string faultrow = rowIncr.ToString();
                    throw new Exception($"Row {faultrow} from OneNoteTable from XML has no <Cell> elements in it.");
                }
                foreach (XElement cell in cellXml)
                {
                    OneNoteTableCell cellOE = new OneNoteTableCell();
                    //cellOE.oeType = OneNoteOEType.BaseOE;
                    if (cell.Attribute("shadingColor") != null)
                    {
                        cellOE.cellShading = cell.Attribute("shadingColor").Value;
                    }
                    else { cellOE.cellShading = defaultShadingColor;}

                    string cellFontColor;
                    if (cell.Attribute("fontColor") != null)
                    {
                        cellFontColor = cell.Attribute("fontColor").Value;
                    }
                    else { cellFontColor = defaultFontColor; }


                    // lines in Cell
                    IEnumerable<XElement> cellLinesXml = cell.Elements("Line");
                    if (cellXml.FirstOrDefault() == null)
                    {
                        // add a blank line if there are no lines in the cell.
                        OneNoteOE emptyOE = new OneNoteOE();
                        emptyOE.oeType = OneNoteOEType.BaseOE;
                        emptyOE.fontFamily = defaultFontStr;
                        emptyOE.fontWeight = sizingOptions.Attribute("fontSizeLineBreak").Value;
                        emptyOE.textLine = new OneNoteT(emptyOE.fontWeight, emptyOE.fontFamily, 0, cellLinesXml); // we can recycle cellLinesXml as the spansXml because they are both empty.

                        cellOE.cellLines.Add(emptyOE);
                    }
                    else
                    {
                        // there are lines to work throguh - so do the full line procedure and their children.
                        foreach (XElement cellLine in cellLinesXml)
                        {
                            OneNoteOE lineOE = BuildOEWithChildrenFromXml(cellLine, sizingOptions,tableCol, defaultFont, linksLookup,0, cellFontColor, BoldRow);
                            cellOE.cellLines.Add(lineOE);
                        }
                    }

                    dataRow.Add(cellOE);

                }
                rowIncr++;
                output.dataRows.Add(dataRow);
            }
            // Here we need to segment the output.dataRows into columns, build a list of oes for each column, and then build/run a recursive function to get all the widthNeeded from their OnNoteTs.
            // The recursive function is straight below - use it for each column by making a list of empty List<double>(); tempLists for each column.
            Dictionary<int, List<double>> columnLineLengths = new Dictionary<int, List<double>>();
            int colCount = output.dataRows.First().Count;
            
            for (int i = 0;i < colCount;i++)
            {
                columnLineLengths[i] = new List<double>();
            }
            foreach (List<OneNoteTableCell> dr in output.dataRows)
            {
                int incrCellInRow = 0;
                foreach (OneNoteTableCell c in dr)
                {
                    List<double> holdDictValue; // need a holding variable for the latest knwon list-value for the dictionary key where the key is the column we're on.
                    holdDictValue = columnLineLengths[incrCellInRow]; // assign it.
                    holdDictValue = GetWidthsNeeded(c.cellLines, ref holdDictValue); //ref it through the recusrive function on the lines within this cell.
                    columnLineLengths[incrCellInRow] = holdDictValue; //reassign the dict value to be the upldated line lenghts list of numbers.
                    incrCellInRow++;
                }
            }
            Dictionary<int, string> columnLineLengthMaximums = new Dictionary<int, string>();

            //fill the above by max-aggreagting columnLineLengths, looking for a value in the XML table <Columns> for that positional key, ...
            //... and taking the master of truth from the max agg or the set value. If there's a value from XML and its fix, that's the number.
            //else if theres a value from XML and it's cap_at, we take the SMALLER number of the XML value and the agg max. Else just take the agg max.

            IEnumerable<XElement> tableCols = nodeXml.Element("Columns").Elements("Column");
            for (int i = 0; i < colCount; i++)
            {
                string istring = i.ToString();
                XElement colXmlConfig = tableCols.Where(x => x.Attribute("index").Value.ToString() == istring).FirstOrDefault();
                if (colXmlConfig == null)
                {
                    throw new Exception($"Column with index {istring} is in your data but not in your Xml columns list.");
                }
                double maxWidthAtIndex = Math.Round(columnLineLengths[i].Max(),2);
                if (colXmlConfig.Attribute("customWidth") != null)
                {
                    if (colXmlConfig.Attribute("customWidthType") == null)
                    {
                        throw new Exception($"Column with index {istring} has a customWidth attribute in your Xml but no customWidthType.");
                    }
                    if (colXmlConfig.Attribute("customWidthType").Value.ToString() == "fix")
                    {
                        columnLineLengthMaximums[i] = colXmlConfig.Attribute("customWidth").Value.ToString();
                    }
                    else if (colXmlConfig.Attribute("customWidthType").Value.ToString() == "cap_at") 
                    {
                        double customWidthValue = double.Parse(colXmlConfig.Attribute("customWidth").Value.ToString());
                        double[] arrayCapAt = { customWidthValue, maxWidthAtIndex };
                        columnLineLengthMaximums[i] = arrayCapAt.Min().ToString();
                    }
                    else
                    {
                        throw new Exception($"Column with index {istring} has a customWidthType not in 'fix' or 'cap_at'.");
                    }
                }
                else
                {
                    columnLineLengthMaximums[i] = maxWidthAtIndex.ToString();
                }

                output.colWidths = columnLineLengthMaximums;

            }

                return output;
        }


        public static List<double> GetWidthsNeeded(List<OneNoteOE> oeList, ref List<double> dataList )
        {
            List<double> workinglist = dataList;

            foreach (OneNoteOE oeIter in oeList)
            {
                if (oeIter.oeType == OneNoteOEType.BaseOE)
                {
                    double lineWidth = oeIter.textLine.widthNeeded;
                    workinglist.Add(lineWidth);
                    if (oeIter.OEChildren.Count > 0)
                    {
                        workinglist = GetWidthsNeeded(oeIter.OEChildren, ref workinglist);
                    }
                }
            }
            return workinglist;
        }



        public static OneNoteOE BuildOEWithChildrenFromXml (XElement nodeXml, XElement sizingOptions, XElement tableCol, 
            AllowableFonts defaultFont, Dictionary<string, Dictionary<string, object>> linksLookup, int inheritedIndents = 0,
            string inheritedFontColor = "black", bool boldByDefault = false,string inheritedAlignment = "left")
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
            // 5.Dictionary<string, Dictionary<string, object>> linksLookup - this is all the onenote section names/linkableIds + nested dict of ...
            // ... their pages name/linableId key/value pairs in play. THis is needed to build internal linking spans if needed.
            // 5. inheritedIndents - default 0, incrementable for child sections
            // 6. inheritedFontColor - if this is populated, the OneNoteOE output's default font color of bklack will be overriden by this hexcode as string.
            // 7. boldByDefault - default false, give this a true if you want any Line elements to ahve all their spans set to bold by default.
            // 8. inheritedAlignment - default left - give this center or right if you want the items in an OE to be aligned that way.


            // THis works on the XML. Next trick is to write a pretty-much exact copy of this, but which takes in a OneNoteOE of type section, and builds it into OneNote pageContent Xml.

            OneNoteOE output = new OneNoteOE();

            AllowableFonts defaultFontSel = defaultFont;
            XElement sizingOptionsSel = sizingOptions;
            XElement tableColSel = tableCol;
            int inputIndents = inheritedIndents;
            string oeFontColor = inheritedFontColor;
            bool makeBold = boldByDefault;
            string oeAlignment = inheritedAlignment;

            string [] sectionChildrenOk = {"Line", "Table"};
            string[] lineChildrenOk = { "Line" }; // Lines can only contain lines as sub bullets. A line containing a table has to be a section.

            string defaultFontStr = GetAllowableFontAsStr(defaultFontSel);
            Dictionary<string, Dictionary<string, object>> linksLookupPassOn = linksLookup;
            output.fontFamily = defaultFontStr;
            output.alignment = oeAlignment;
            output.fontColor = oeFontColor;
            


            // Dealing with the input itself. 

            if (nodeXml.Name == "Section")
            {
                output.author = nodeXml.Attribute("name").Value;
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

                        OneNoteTable table = BuildTableFromXml(firstElement, sizingOptionsSel, tableColSel, defaultFontSel, linksLookupPassOn);
                        output.oeType = OneNoteOEType.Table;
                        output.table = table;
                    }
                    else
                    {
                        // It's a line.
                        //output.oeType = OneNoteOEType.BaseOE;
                        output.fontWeight = sizingOptionsSel.Attribute("fontSizeText").Value; //Since it's a single-line headerless section, size it back to the line size not the header size.
                        IEnumerable<XElement> spansXml = firstElement.Elements("span");
                        string lineBullet = null;
                        if (firstElement.Attribute("Bullet") != null)
                        {
                            lineBullet =  firstElement.Attribute("Bullet").Value;
                        }

                        OneNoteT lineData = new OneNoteT(output.fontWeight, output.fontFamily, inputIndents, spansXml, linksLookup, lineBullet,boldByDefault);
                        output.textLine = lineData;
                      
                        // DO the child procedure on the line (which can only apply to lines within lines. So this would be bullets within a line in a headerless section.).
                        children = firstElement.Elements().Where(x => lineChildrenOk.Contains(x.Name.ToString()));
                        foreach (XElement child in children)
                        {
                            OneNoteOE childOE = BuildOEWithChildrenFromXml(child,
                                                    sizingOptionsSel,
                                                    tableColSel, defaultFontSel, linksLookupPassOn, inputIndents +1, oeFontColor, makeBold, oeAlignment);
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
                                // Make the line input from the XElement. It has to be childless and bulletless, because its the header of a section so it cant contain bullet points. or be multi line.
                                IEnumerable<XElement> spansXml = child.Elements("span");
                                output.textLine = new OneNoteT(output.fontWeight, output.fontFamily, inputIndents, spansXml, linksLookup, null, boldByDefault);
                            }
                            else
                            {
                                // Its a line after lineInc 0, so it's text in the main section. We want this indented and OeCHildrenWrapped so that it can be collapsed.
                                output.OEChildren.Add(BuildOEWithChildrenFromXml(child,
                                                    sizingOptionsSel,
                                                    tableColSel, defaultFontSel, linksLookupPassOn, inputIndents + 1, oeFontColor, makeBold, oeAlignment));
                            }
                            lineInc++;
                        }
                        else
                        {
                            // Its a table, which again we want indented so that it can be collapsed. But let the recursive function call handle this.
                            output.OEChildren.Add(BuildOEWithChildrenFromXml(child,
                                                    sizingOptionsSel,
                                                    tableColSel, defaultFontSel, linksLookupPassOn, inputIndents + 1, oeFontColor, makeBold, oeAlignment));
                        }
                    }
                }

            }
            else if (nodeXml.Name == "Line")
            {
                // The line itself.
                output.oeType = OneNoteOEType.BaseOE;
                if (nodeXml.Attribute("subHead") != null && nodeXml.Attribute("subHead").Value == "true")
                {
                    output.fontWeight = sizingOptionsSel.Attribute("fontSizeSectionHead").Value;
                }
                else
                {
                    output.fontWeight = sizingOptionsSel.Attribute("fontSizeText").Value;
                }
                
                output.inheritedIndents = inputIndents;
                IEnumerable<XElement> spansXml = nodeXml.Elements("span");
                if (spansXml.FirstOrDefault() == null) // Make the node font weight the blankLine value if there are no spans.
                {
                    output.fontWeight = sizingOptionsSel.Attribute("fontSizeLineBreak").Value;
                }
                string lineBullet = null;
                if (nodeXml.Attribute("Bullet") != null)
                {
                    lineBullet = nodeXml.Attribute("Bullet").Value;
                }
                output.textLine = new OneNoteT(output.fontWeight, output.fontFamily, inputIndents, spansXml, linksLookup, lineBullet, boldByDefault);

                // Children.
                IEnumerable<XElement> children = nodeXml.Elements().Where(x => lineChildrenOk.Contains(x.Name.ToString()));
                foreach (XElement child in children)
                {
                    // The children of a line have to be lines, so this is simple.
                    output.OEChildren.Add(BuildOEWithChildrenFromXml(child,
                                                    sizingOptionsSel,
                                                    tableColSel, defaultFontSel, linksLookupPassOn, inputIndents + 1, oeFontColor, makeBold, oeAlignment));
                }




            }
            else if (nodeXml.Name == "Table")
            {
                //. You will need a table function to loop the columns and cells, called from here. This table function should produce lines in the cells, ...
                // ... which may have their own lines, which will therefore call this function.
                output.oeType = OneNoteOEType.Table;
                OneNoteTable table = BuildTableFromXml(nodeXml, sizingOptionsSel, tableColSel, defaultFontSel, linksLookupPassOn);
                output.table = table;
            }

            // Deal with its child nodes - a table has no permissible child nodes that can be dealt with by this function directly.

            return output;
        }


        public static XElement BuildOneNoteXmlOeFromClassObject(OneNoteOE OeClassObject, XNamespace nameSpaceInput)
        {
            XNamespace ns = nameSpaceInput;

            XElement output = new XElement(ns + "OE");
            // Make an OE in Onenote Xml with the OneNoteOE OeClassObject param, and if it has children, recursively add these to the OE node target, passing it down.

            // Start by setting all the attributes that can be set at the OE level from the OE Class object properties.
            output.SetAttributeValue("alignment", OeClassObject.alignment);
            if (OeClassObject.author != null)
            {

                output.SetAttributeValue("author", OeClassObject.author);
            }
            


            if (OeClassObject.oeType == OneNoteOEType.BaseOE || OeClassObject.oeType == OneNoteOEType.Section)
            {
                output.SetAttributeValue("style", OeClassObject.CssStyle);
            }

            //Line or Table Comes First.
            if (OeClassObject.table != null)
            {
                // BuildOneNoteXmlTableFromClassObject build this function. Add the return result to the output. TABLES DO NOT HAVE CHILDREN.
                XElement tableXml = BuildOneNoteXmlTableFromClassObject(OeClassObject.table, ns);
                output.Add(tableXml);
            }
            if (OeClassObject.textLine != null)
            {
                
                OneNoteT TObject = OeClassObject.textLine;
                // Check if it's a bullet point first.
                if (TObject.Bullet != null)
                {
                    output.Add(new XElement(ns + "List",
                                    new XElement(ns + "Bullet", 
                                        new XAttribute("bullet", TObject.Bullet), 
                                        new XAttribute("fontSize", TObject.InheritedFontWeight)
                                    )
                                )
                        );
                }
                // Add the text line as T
                output.Add(new XElement(ns + "T",
                                new XCData(TObject.cDataValue)
                        )
                    );
            }
            if (OeClassObject.OEChildren.Count > 0)
            {
                XElement OeChildrenWrapper = new XElement(ns + "OEChildren");
                
                foreach (OneNoteOE child in OeClassObject.OEChildren)
                {
                    XElement childXele = BuildOneNoteXmlOeFromClassObject(child, ns);
                    OeChildrenWrapper.Add(childXele);
                }
                output.Add(OeChildrenWrapper);
            }

            return output;
        }

        public static XElement BuildOneNoteXmlTableFromClassObject(OneNoteTable TableClassObject, XNamespace nameSpaceInput)
        {
            XNamespace ns = nameSpaceInput;

            XElement outputTable = new XElement(ns + "Table");

            // Set table-level attributes
            if(TableClassObject.hasHeaders)
            {
                outputTable.SetAttributeValue("hasHeaderRow", "true");
            }
            else
            {
                outputTable.SetAttributeValue("hasHeaderRow", "false");
            }
            if (TableClassObject.bordersVisible)
            {
                outputTable.SetAttributeValue("bordersVisible", "true");
            }
            else
            {
                outputTable.SetAttributeValue("bordersVisible", "false");
            }

            //Columns
            XElement columnsWrapper = new XElement(ns + "Columns");
            int nCols = TableClassObject.colWidths.Count;
            for (int i = 0; i < nCols; i++)
            {
                string istring = i.ToString();
                string iwidth = TableClassObject.colWidths[i];
                XElement colXml = new XElement(ns + "Column", new XAttribute("index", istring), new XAttribute("width", iwidth), new XAttribute("isLocked", "true"));
                columnsWrapper.Add(colXml);
            }
            outputTable.Add(columnsWrapper);

            //Rows
            List<List<OneNoteTableCell>> dataRows = TableClassObject.dataRows;
            foreach (List<OneNoteTableCell> row in dataRows)
            {
                XElement rowXml = new XElement(ns + "Row");

                //The Cell shading colours are all set within the cells that come in from the TableClassObject param input. IF your input data doesn not have this in the cells , you're out of luck.
                if (nCols != row.Count)
                {
                    string nColsPrint = nCols.ToString();
                    string rowsPrint = row.Count.ToString();
                    throw new Exception($"Error building OneNoteXml table pattern in BuildOneNoteXmlTableFromClassObject: you have hit a row with {rowsPrint} cells, which is more than the columns specified ({nColsPrint}).");
                }
                for (int i = 0; i < row.Count;i++)
                {
                    OneNoteTableCell cell = row[i];
                    string istringData = i.ToString();
                    XElement cellXml = new XElement(ns + "Cell");
                    cellXml.SetAttributeValue("shadingColor",cell.cellShading);
                    XElement cellChildrenWrapper = new XElement(ns + "OEChildren");
                    if (cell.cellLines.Count == 0)
                    {
                        throw new Exception($"Error building OneNoteXml table pattern in BuildOneNoteXmlTableFromClassObject: Your cell in column {istringData} has no lines. The input data for each cell needs at least 1 line, even if it's blank ''.");
                    }
                    foreach (OneNoteOE line in cell.cellLines)
                    {
                        XElement cellLineXml = BuildOneNoteXmlOeFromClassObject(line, ns);
                        cellChildrenWrapper.Add(cellLineXml);
                    }


                    cellXml.Add(cellChildrenWrapper);
                    rowXml.Add(cellXml);

                }
                outputTable.Add(rowXml);
            }
            return outputTable;
        }

        public static XElement ParseOeToNew (XElement OeInput, XNamespace ns)
        {
            XElement output = new XElement(ns + "OE");
            if (OeInput.Attribute("alignment") != null)
            {
                output.SetAttributeValue("alignment", OeInput.Attribute("alignment").Value);
            }
            else
            {
                output.SetAttributeValue("alignment", "left");
            }
            if (OeInput.Attribute("style") != null)
            {
                output.SetAttributeValue("style", OeInput.Attribute("style").Value);
            }
            IEnumerable<XElement> lists = OeInput.Elements(ns + "List");
            IEnumerable<XElement> Ts = OeInput.Elements(ns + "T");
            IEnumerable<XElement> Tables = OeInput.Elements(ns + "Table");
            IEnumerable<XElement> children = OeInput.Elements(ns + "OEChildren");
            if (lists.Any())
            {
                XElement list = lists.First();
                XElement newlist = new XElement(ns + "List");
                IEnumerable<XElement> numbers = list.Elements(ns + "Number");
                IEnumerable<XElement> bullets = list.Elements(ns + "Bullet");
                if (numbers.Any())
                {
                    foreach (XElement n in numbers)
                    {
                        XElement newNumber = new XElement(ns + "Number");
                        IEnumerable<XAttribute> attribs = n.Attributes();
                        foreach (XAttribute attr in attribs)
                        {
                            newNumber.SetAttributeValue(attr.Name, attr.Value);
                        }
                        newlist.Add(newNumber);
                    }
                }
                if (bullets.Any())
                {
                    foreach (XElement b in bullets)
                    {
                        XElement newBullet = new XElement(ns + "Bullet");
                        IEnumerable<XAttribute> attribs = b.Attributes();
                        foreach (XAttribute attr in attribs)
                        {
                            newBullet.SetAttributeValue(attr.Name, attr.Value);
                        }
                        newlist.Add(newBullet);
                    }
                }
                output.Add(newlist);
            }

            if (Ts.Any())
            {
                foreach (XElement tee in Ts)
                {
                    if (tee.Nodes().OfType<XCData>().FirstOrDefault() == null)
                    {
                        output.Add(
                            new XElement(ns + "T",
                                new XCData(" ")
                            )
                        );
                    }
                    else
                    {
                        output.Add(
                            new XElement(ns + "T",
                                new XCData(tee.Nodes().OfType<XCData>().First().Value)
                            )
                        );
                    }
                }
            }

            if (Tables.Any())
            {
                XElement newTable = new XElement(ns + "Table");
                
                XElement oldTable = Tables.First();
                newTable.SetAttributeValue("bordersVisible", oldTable.Attribute("bordersVisible").Value);
                newTable.SetAttributeValue("hasHeaderRow", oldTable.Attribute("hasHeaderRow").Value);
                XElement newColumns = new XElement(ns + "Columns");
                IEnumerable<XElement> oldColumns = oldTable.Element(ns + "Columns").Elements(ns + "Column");
                foreach (XElement column in oldColumns) 
                {
                    newColumns.Add(
                        new XElement(ns + "Column",
                            new XAttribute("index", column.Attribute("index").Value),
                            new XAttribute("width", column.Attribute("width").Value),
                            new XAttribute("isLocked", column.Attribute("isLocked").Value)
                            )
                        );
                }
                newTable.Add(newColumns);
                IEnumerable<XElement> rows = oldTable.Elements(ns + "Row");
                foreach (XElement row in rows)
                {
                    XElement newRow = new XElement(ns + "Row");
                    IEnumerable<XElement> oldRowCells = row.Elements(ns + "Cell");
                    foreach (XElement oldCell in oldRowCells)
                    {
                        XElement newCell = new XElement(ns + "Cell");
                        if (oldCell.Attribute("shadingColor") != null)
                        {
                            newCell.SetAttributeValue("shadingColor", oldCell.Attribute("shadingColor").Value);
                        }
                        IEnumerable<XElement> oldCellChildren = oldCell.Elements(ns + "OEChildren");
                        if (oldCellChildren.Any())
                        {
                            IEnumerable<XElement> oldCellOEs = oldCellChildren.First().Elements(ns + "OE");
                            XElement newCellChildren = new XElement(ns + "OEChildren");
                            foreach (XElement cellChild in oldCellOEs)
                            {
                                XElement newCellOE = ParseOeToNew(cellChild, ns);
                                newCellChildren.Add(newCellOE);
                            }
                            newCell.Add(newCellChildren);
                        }
                        newRow.Add(newCell);
                    }
                    newTable.Add(newRow);
                }
                output.Add(newTable);


            }

            if (children.Any())
            {
                XElement newChildren = new XElement(ns + "OEChildren");
                IEnumerable<XElement> oldchildren = children.First().Elements(ns + "OE");
                foreach (XElement oldchild in oldchildren)
                {
                    XElement newOE = ParseOeToNew(oldchild, ns);
                    newChildren.Add(newOE);
                }
                output.Add(newChildren);
            }

            return output;
        }


        public static string ParseTextFromOneNoteCData(string inputString)
        {

            string returnString = "";
            Regex rx = new Regex(@"(<span\s+style\s*=\s*[""'][^""']*[""']\s*>)(.*?)</span>"); // non greedy match for spans.
            
            List<Match> tries = rx.Matches(inputString).Cast<Match>().ToList();

            if (tries.Count == 0)
            {
                returnString = inputString.Trim();
            }
            else
            {
                StringBuilder spansSB = new StringBuilder();
                foreach (Match m in tries)
                {
                    spansSB.Append(m.Groups[2].Value.Trim());
                }
                if (spansSB.Length > 0)
                {
                    returnString = spansSB.ToString();
                }
                
            }
            return returnString;
        }


        public static bool TestTableIsValidPasswordTable (XElement table, XNamespace ns)
        {
            bool outputbool = false;
            List<string> expectedHeaders = new List<string> { "Description", "Type", "URL",
                    "Username", "Password/PIN", "2FA", "2FA Method", "Date Last Modified", "Last Modified Sort", "Strength"};
            
            
            XElement headerRow = table.Elements(ns + "Row").First();
            IEnumerable<XElement> hrCells = headerRow.Elements(ns + "Cell");
            HashSet<string> uniqueValuesFound = new HashSet<string>();
            int countFound = 0;
            foreach (XElement cell in hrCells)
            {
                XElement cellFirstOe = cell.Element(ns + "OEChildren").Elements(ns + "OE").First();
                XElement cfoeFirstT = cellFirstOe.Elements(ns + "T").FirstOrDefault();

                if (cfoeFirstT != null) //There is a T value in the Header Cell's first OE
                {
                    string headerValueFound = ParseTextFromOneNoteCData(cfoeFirstT.Nodes().OfType<XCData>().First().Value);
                    uniqueValuesFound.Add(headerValueFound);
                }
            }
            foreach (string f in uniqueValuesFound)
            {
                if (expectedHeaders.Contains(f))
                {
                    countFound++;
                }
            }
            if (countFound >= expectedHeaders.Count)
            {
                outputbool = true;
            }
            
            return outputbool;
        }



        /// <summary>
        /// Tests that a One Note page is valid Password Bank Page, based on:
        /// 1. It has an outline where the author attribute value is "PasswordBank"
        /// 2. This outline has a table where the first row contains all the expected headers for the Password bank table.
        /// No checks are done on the data rows - the GetPasswordsFromValidPage parser below does this.
        /// </summary>
        /// <param name="pageContent"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static bool TestOneNotePageValidPasswordBank(XDocument pageContent)
        {
            bool outputBool = false;
            XNamespace ns = pageContent.Root.Name.Namespace;
            XElement passwordBankOutline = pageContent.Element(ns + "Page").Elements(ns + "Outline").Where(x => x.Attribute("author").Value == "PasswordBank").FirstOrDefault();
            if (passwordBankOutline != null)
            {
                IEnumerable<XElement> tablesinOutline = passwordBankOutline.Descendants(ns + "Table");

                foreach (XElement vt in tablesinOutline)
                {
                    if (TestTableIsValidPasswordTable(vt,ns))
                    {
                        outputBool = true;
                        break;
                    }

                }

            }
            
            return outputBool;
        }

        /// <summary>
        /// Gets a PasswordBank (List<LoginEntry>) from a OneNote page.
        /// ONLY RUN THIS AFTER YOU@VE TESTED THE PAGE for having valid password bank table using TestOneNotePageValidPasswordBank.
        /// It doesn't hydrate the LoginEntry class instances with Strength or LastModifiedSort property values, defaults are left in for these.
        /// A different function should caclualte the stringths, once the algorithm has been decided.
        /// </summary>
        /// <param name="pageContent"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static List<LoginEntry> GetPasswordsFromValidPage (XDocument pageContent, XNamespace ns)
        {
            Dictionary<string, string> propsToHeaders = new Dictionary<string, string> { 
                {"LoginType","Type" },{ "LoginDescription", "Description"},
                {"LoginUrl","URL" },{ "LoginUsername", "Username"},
                {"LoginPass","Password/PIN" },{ "HasTwoFa", "2FA"},
                {"TwoFaMethod","2FA Method" },{ "LastModified", "Date Last Modified"}
            };
            List<string> headers = propsToHeaders.Values.ToList();
            // Above list is the headers of the columns we want to parse from the OneNote page data. (all bar last modified sort and strength - these will be recalculated.)

            List<LoginEntry> passwordBank = new List<LoginEntry>();
            XElement passwordBankOutline = pageContent.Element(ns + "Page").Elements(ns + "Outline").Where(x => x.Attribute("author").Value == "PasswordBank").First();
            XElement passwordTableInData = passwordBankOutline.Descendants(ns + "Table").Where(x => TestTableIsValidPasswordTable(x, ns)).First();

            Dictionary<string, int> headerColIndex = new Dictionary<string, int>();
            Dictionary<int,string> dataHeadersPositionsValues = new Dictionary<int, string>();
            XElement headerRow = passwordTableInData.Elements(ns + "Row").First();
            IEnumerable<XElement> hrCells = headerRow.Elements(ns + "Cell");
            int headerRowCellIncr = 0;
            //Loop through the cells in the headerRow (incrementing the counter by 1 each time),
            //and hydrate dataHeadersPositionsValues Dict with the counter and the text value found in CData.
            foreach (XElement cell in hrCells)
            {
                XElement cellFirstOe = cell.Element(ns + "OEChildren").Elements(ns + "OE").First();
                XElement cfoeFirstT = cellFirstOe.Elements(ns + "T").FirstOrDefault();
                string headerValueFound;
                if (cfoeFirstT != null) //There is a T value in the Header Cell's first OE
                {
                    headerValueFound = ParseTextFromOneNoteCData(cfoeFirstT.Nodes().OfType<XCData>().First().Value);
                    
                }
                else
                {
                    headerValueFound = "";
                }
                dataHeadersPositionsValues.Add(headerRowCellIncr, headerValueFound);
                headerRowCellIncr++;
            }
            //Loop through the headers we're interested in, and hydrate headerColIndex Dict with the headername and the first Index from dataHeadersPositionsValues Dict Keys where its value is the target header.
            foreach (string targHeader in headers)
            {
                int firstIndex = dataHeadersPositionsValues.Where(x => x.Value == targHeader).Select(y => y.Key).Min();
                headerColIndex.Add(targHeader, firstIndex);
            }
            // Now we have a dictionary of headers we're interested in, and the column indexes of the data table where they first occur (should only be once, but no harm.)
            // We use this to direct a loop through the rest of the table to exampine the data rows and hydrate the List<LoginEntry> passwordBank

            IEnumerable<XElement> dataRows = passwordTableInData.Elements(ns + "Row").Skip(1);

            foreach (XElement dataRow in dataRows)
            {
                LoginEntry login = new LoginEntry();
                List<XElement> cellsInRow = dataRow.Elements(ns + "Cell").ToList();

                foreach (var loginProp in login.GetType().GetProperties())
                {
                    string loginPropName = loginProp.Name;
                    if (propsToHeaders.ContainsKey(loginPropName)) // We're looking at a property that we actually want to set from data.
                    {
                        string headerTarget = propsToHeaders[loginPropName];
                        int cellIndex = headerColIndex[headerTarget];

                        XElement targetCell = cellsInRow[cellIndex];

                        // At this point, we have identified the cell in the row that contains the right data based on the LoginEntry proeprty name in the loop.

                        IEnumerable<XElement> OesWithT = targetCell.Elements(ns + "OEChildren").Elements(ns + "OE").Where(x => x.Elements(ns + "T").Any()); // OEs in the target cell with a T

                        StringBuilder allTextfromOEs = new StringBuilder();
                        string allTextConcatenated = "";

                        foreach (XElement oe in OesWithT)
                        {
                            allTextfromOEs.Append(
                                ParseTextFromOneNoteCData(oe.Elements(ns + "T").First().Nodes().OfType<XCData>().First().Value)
                                );
                        }
                        if (allTextfromOEs.Length > 0)
                        {
                            allTextConcatenated = allTextfromOEs.ToString();
                        }

                        // At this point, allTextConcatenated contains all the Cdata text in the OE>Ts of the cell.
                        // Now we decide what to do with it based on its data type/property name. booleans first, then dates, then enums, then standard strings.

                        if (loginPropName == "HasTwoFa")
                        {
                            if (allTextConcatenated.Replace("&nbsp;", " ").Trim() == "Y")
                            {
                                loginProp.SetValue(login, true);
                            }
                            else
                            {
                                loginProp.SetValue(login, false);
                            }
                        }
                        else if (loginPropName == "LastModified")
                        {
                            DateTime? dlm; //date last modified
                            if (DateTime.TryParseExact(allTextConcatenated.Replace("&nbsp;", " ").Trim(), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                            {
                                dlm = parsedDate;
                            }
                            else
                            {
                                dlm = null;
                            }
                            loginProp.SetValue(login, dlm);
                        }
                        else if (loginPropName == "LoginType")
                        {
                            if (allTextConcatenated.Replace("&nbsp;", " ").Trim() == "PIN (4)") { loginProp.SetValue(login, LoginTypes.PinFour); }
                            else if (allTextConcatenated.Replace("&nbsp;", " ").Trim() == "PIN (6)") { loginProp.SetValue(login, LoginTypes.PinSix); }
                            else if (allTextConcatenated.Replace("&nbsp;", " ").Trim() == "Password") { loginProp.SetValue(login, LoginTypes.Password); }
                            // no need for an else here as this property comes with a default of "not set".
                        }
                        else
                        {
                            // Bog standard string
                            loginProp.SetValue(login, allTextConcatenated.Trim());
                        }
                    }
                }
                passwordBank.Add(login);
            }

            return passwordBank;
        }



        public static bool TestOneNoteSectionValidPasswordBank(OneNoteInterop.Application app, XmlNode SectionXML)
        {
            XElement sectionXMLX = XElement.Load(SectionXML.CreateNavigator().ReadSubtree());
            return TestOneNoteSectionValidPasswordBank(app, sectionXMLX);
        }
        public static bool TestOneNoteSectionValidPasswordBank (OneNoteInterop.Application app, XElement SectionXML)
        {
            bool outputBool = false;
            XNamespace ns = SectionXML.Name.Namespace;
            List<XElement> pbPages = SectionXML.Elements(ns + "Page").Where( x => x.Attribute("name").Value == "Password Bank").ToList();
            
            if (pbPages.Count == 1) // There is one page in the section where the name is Password Bank. Try the content of this page against the tester.
            {
                string pageId = pbPages.First().Attribute("ID").Value;
                XDocument pbPageContent = OneNotePageFmtMethods.GetPageXmlLinq(app, pageId);
                outputBool = TestOneNotePageValidPasswordBank(pbPageContent);
            }
            return outputBool;
        }
        public static string InvalidPasswordBankReason(OneNoteInterop.Application app, XmlNode SectionXML)
        {
            XElement sectionXMLX = XElement.Load(SectionXML.CreateNavigator().ReadSubtree());
            return InvalidPasswordBankReason(app, sectionXMLX);
        }
        public static string InvalidPasswordBankReason(OneNoteInterop.Application app, XElement SectionXML)
        {
            string outputMessage;
            XNamespace ns = SectionXML.Name.Namespace;
            List<XElement> pbPages = SectionXML.Elements(ns + "Page").Where(x => x.Attribute("name").Value == "Password Bank").ToList();
            if (pbPages.Count == 1)
            {
                
                string pageId = pbPages.First().Attribute("ID").Value;
                XDocument pbPageContent = OneNotePageFmtMethods.GetPageXmlLinq(app, pageId);
                if (TestOneNotePageValidPasswordBank(pbPageContent))
                {
                    outputMessage = "Valid Password Bank Section";
                } 
                else
                {
                    outputMessage = "There is a page called Password Bank, but it does not have a password table with the correct columns." +
                        "\n(Description, Type, URL, Username, Password/PIN, 2FA, 2FA Method, Date Last Modified, Last Modified Sort, Strength)" +
                        "\n\n If this page has a password table, correct any accidental typos in the column headers on this page and refresh.";
                }
            }
            else if (pbPages.Count > 1)
            {
                outputMessage = "There is more than one page in this section called Password Bank.\n Delete or rename at least one so that only one page has this name." +
                    "\n(Don't delete/rename the page that has your password table!)";
            }
            else
            {
                outputMessage = "This section has no Password Bank page.";
            }


            return outputMessage;
        }

        public static List<LoginEntry> PasswordBankFromStaticXml (int nSkip = 0)
        {
            List<LoginEntry> pBank = new List<LoginEntry>();
            XElement xmlBank;
            XElement resource = XElement.Parse(Properties.Resources.StaticAndTestData);
            
            if (nSkip == 0 || resource.Elements("TestRecords").Count() == 1) // You are not electing to skip, or you only have 1 set of test records
            {
                xmlBank = resource.Elements("TestRecords").First();
            }
            else
            {
                xmlBank = resource.Elements("TestRecords").Skip(nSkip).First();
            }


            IEnumerable<XElement> enBank = xmlBank.Elements("LoginRecord");
            foreach (XElement lr in enBank)
            {
                LoginEntry login = new LoginEntry();
                login.LoginDescription = lr.Attribute("LoginDescription").Value;
                login.LoginPass = lr.Attribute("LoginPass").Value;
                login.LoginUsername = lr.Attribute("LoginUsername").Value;
                login.LoginUrl = lr.Attribute("LoginURL").Value;
                login.TwoFaMethod = lr.Attribute("TwoFaMethod").Value;
                login.HasTwoFa = bool.Parse(lr.Attribute("HasTwoFa").Value);

                string lType = lr.Attribute("LoginType").Value;
                if (lType == "Password")
                {
                    login.LoginType = LoginTypes.Password;
                }
                else if (lType == "PinSix")
                {
                    login.LoginType = LoginTypes.PinSix;
                }
                else if (lType == "PinFour")
                {
                    login.LoginType = LoginTypes.PinFour;
                }
                else
                {
                    login.LoginType = LoginTypes.NotSet;
                }
                DateTime? dlm; //date last modified
                if (DateTime.TryParseExact(lr.Attribute("LastModified").Value.Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    dlm = parsedDate;
                }
                else
                {
                    dlm = null;
                }
                login.LastModified = dlm;


                pBank.Add(login);
            }

            return pBank;
        }
    }
}
