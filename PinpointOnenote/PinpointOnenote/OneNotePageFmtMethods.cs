﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneNoteInterop = Microsoft.Office.Interop.OneNote;
using System.Xml;
using System.Xml.Linq;
using System.Security.Policy;
using System.Configuration;
using PinpointOnenote.OneNoteClasses;
using System.Net.Configuration;

namespace PinpointOnenote
{
    /// <summary>
    /// This contains all the methods to create and retrieve one note pages, and populate them with content.
    /// </summary>
    public static class OneNotePageFmtMethods
    {
        #region Page Creation, Check Existence, and other operations.

        public static string AddOneNoteNewPage(OneNoteInterop.Application app,
            string SectionID,
            string PageName,
            string PageLevel = "1")
        {
            //Adds a new page to the section with the title and level(default 1) you supply it. Returns the page ID as string.
            string pageID;
            app.CreateNewPage(SectionID, out pageID, OneNoteInterop.NewPageStyle.npsBlankPageNoTitle);
            string pageXML;

            app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
            XDocument PageResult = XDocument.Parse(pageXML);
            XNamespace ns = PageResult.Root.Name.Namespace;
            XElement pageEl = PageResult.Descendants(ns + "Page").First(); //search the first page element.
            pageEl.Add(new XElement(ns + "Title", //add the title
                new XElement(ns + "OE", //add the OE to the title
                    new XElement(ns + "T", // add the text holder to the OE
                        new XCData(PageName))))); //add the data to the text holder
            pageEl.SetAttributeValue("name", PageName); //set page name
            pageEl.SetAttributeValue("pageLevel", PageLevel); //set page level

            app.UpdatePageContent(PageResult.ToString());
            return pageID;
        }

        public static XDocument RenderOneNotePage(OneNoteInterop.Application app, string pageID, List<OneNoteOE> sectionsData, bool newPage = false)
        {
            //Params:
            //1. OneNoteInterop.Application app - should be quite obvious
            //2. string pageID - page ID you are adding content to.
            // 3. List<OneNoteOE> sectionsData. List of OE Class objects from OneNoteClasses tahta represent page sections. This has come from Data Parsers, and its the OE ojects not the prepared XML. (This functioon calls that).
            // 4. bool newPage = false - is the page being rendered for the first time??
            string pageXML;
            app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
            XDocument PageResult = XDocument.Parse(pageXML);

            List<OneNoteQuickStyleDef> requisiteDefs = new List<OneNoteQuickStyleDef>();
            Dictionary<string,string> quickStylesDict = new Dictionary<string, string>();
            List<string> quickStylesDictSectionSearcher = new List<string>();
            requisiteDefs.Add(new OneNoteQuickStyleDef("p"));
            requisiteDefs.Add(new OneNoteQuickStyleDef(new Dictionary<string, string> { {"name","PageTitle"},{"font", "Calibri Light"}, { "fontSize", "20.0"} }));

            List<OneNoteQuickStyleDef> addQuickStyles = new List<OneNoteQuickStyleDef>();
            List<string> sectionsListnonStandardFromData = new List<string>();
            foreach (OneNoteOE section in sectionsData)
            {
                string sectionQSI_ = section.quickStyleIndexName;
                if (sectionQSI_ != "p" & sectionQSI_ != "PageTitle")
                {
                    sectionsListnonStandardFromData.Add(sectionQSI_);
                }
            }
            if (sectionsListnonStandardFromData.Count != sectionsListnonStandardFromData.Distinct().Count())
            {
                throw new Exception($"RenderOneNotePage ERROR: Your List<OneNoteOE> sectionsData input param value has 2 or more sections that share a quickStyleIndexName. THis is not allowed.");
            }
            foreach (string dataSectionName in sectionsListnonStandardFromData)
            {
                addQuickStyles.Add(new OneNoteQuickStyleDef(dataSectionName));
            }


            XNamespace ns = PageResult.Root.Name.Namespace;
            XElement pageEl = PageResult.Elements(ns + "Page").First();
            IEnumerable<XElement> qsi = pageEl.Elements(ns + "QuickStyleDef");
            XElement qsiLast = qsi.LastOrDefault();
            if (qsiLast == null)
            {
                // No QSDefs - add both and anything user-specified.
                pageEl.AddFirst(new XElement(ns + "QuickStyleDef",
                                     new XAttribute("index", "0"), new XAttribute("name", requisiteDefs[0].name),
                                     new XAttribute("fontColor", requisiteDefs[0].fontColor), new XAttribute("highlightColor", requisiteDefs[0].highlightColor),
                                     new XAttribute("font", requisiteDefs[0].font), new XAttribute("fontSize", requisiteDefs[0].fontSize),
                                     new XAttribute("spaceBefore", requisiteDefs[0].spaceBefore), new XAttribute("spaceAfter", requisiteDefs[0].spaceAfter)
                                            ));
                XElement newQsi = pageEl.Elements(ns + "QuickStyleDef").Last();
                newQsi.AddAfterSelf(new XElement(ns + "QuickStyleDef",
                                     new XAttribute("index", "1"), new XAttribute("name", requisiteDefs[1].name),
                                     new XAttribute("fontColor", requisiteDefs[1].fontColor), new XAttribute("highlightColor", requisiteDefs[1].highlightColor),
                                     new XAttribute("font", requisiteDefs[1].font), new XAttribute("fontSize", requisiteDefs[1].fontSize),
                                     new XAttribute("spaceBefore", requisiteDefs[1].spaceBefore), new XAttribute("spaceAfter", requisiteDefs[1].spaceAfter)
                                            ));
                int startInc = 2;
                string IncIndex = startInc.ToString();
                newQsi = pageEl.Elements(ns + "QuickStyleDef").Last();
                foreach (OneNoteQuickStyleDef qsdef in addQuickStyles)
                {
                    newQsi.AddAfterSelf(new XElement(ns + "QuickStyleDef",
                     new XAttribute("index", IncIndex), new XAttribute("name", qsdef.name),
                     new XAttribute("fontColor", qsdef.fontColor), new XAttribute("highlightColor", qsdef.highlightColor),
                     new XAttribute("font", qsdef.font), new XAttribute("fontSize", qsdef.fontSize),
                     new XAttribute("spaceBefore", qsdef.spaceBefore), new XAttribute("spaceAfter", qsdef.spaceAfter)
                            ));
                    startInc++;
                    IncIndex = startInc.ToString();
                    newQsi = pageEl.Elements(ns + "QuickStyleDef").Last();
                }
            }
            else
            {
                // There is at least one QuickStyleDef. First join both lists, and find the highest index and all tag names from the existing qsi (already assigned)
                requisiteDefs.AddRange(addQuickStyles);
                int[] qsiIndexes = (from qsie in qsi select int.Parse(qsie.Attribute("index").Value)).ToArray();
                string[] qsiTags = (from qsie in qsi select qsie.Attribute("name").Value).ToArray();
                int startInc = qsiIndexes.Max() + 1;
                string IncIndex = startInc.ToString();

                foreach (OneNoteQuickStyleDef qsdef in requisiteDefs)
                {
                    //is it in there?
                    if (!qsiTags.Contains(qsdef.name))
                    {
                        qsiLast.AddAfterSelf(new XElement(ns + "QuickStyleDef",
                            new XAttribute("index", IncIndex), new XAttribute("name", qsdef.name),
                            new XAttribute("fontColor", qsdef.fontColor), new XAttribute("highlightColor", qsdef.highlightColor),
                            new XAttribute("font", qsdef.font), new XAttribute("fontSize", qsdef.fontSize),
                            new XAttribute("spaceBefore", qsdef.spaceBefore), new XAttribute("spaceAfter", qsdef.spaceAfter)
                               ));
                        startInc++;
                        IncIndex = startInc.ToString();
                        qsiLast = pageEl.Elements(ns + "QuickStyleDef").Last();
                    }
                }
            }

            qsi = pageEl.Elements(ns + "QuickStyleDef");
            
            foreach (XElement qsie in qsi)
            {
                quickStylesDict.Add(qsie.Attribute("name").Value, qsie.Attribute("index").Value);

                // we want a dict of quicksltyledef index/name for the sections in the data to use later to identify stuff that may or many not be in any existing outlines, if we are updating an existing page
                if (sectionsListnonStandardFromData.Contains(qsie.Attribute("name").Value))
                {
                    quickStylesDictSectionSearcher.Add(qsie.Attribute("index").Value);
                }
                
            }

            

            XElement titleEl = pageEl.Elements(ns + "Title").Last();

            // THe below is not finalised yet. At the moment it just puts one line of test data on. What we want it to do is recursively build from the data object and "quickStylesDict".

            if (newPage) {

                // Add the outline
                XElement outlineEl = new XElement(ns + "Outline");
                XElement outlineElChildrenWrapper = new XElement(ns + "OEChildren");

                foreach (OneNoteOE sectionLoop in sectionsData)
                {
                    XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns, quickStylesDict);
                    outlineElChildrenWrapper.Add(sectionEl);
                    // add blank line under section
                    outlineElChildrenWrapper.Add(new XElement(ns + "OE",
                                                new XAttribute("quickStyleIndex", quickStylesDict["p"]),
                                                new XElement(ns + "T",
                                                    new XCData(" "))));
                }
                outlineEl.Add(outlineElChildrenWrapper);
                titleEl.AddAfterSelf(outlineEl);
            }
            else
            {
                IEnumerable<XElement> outlines = pageEl.Elements(ns + "Outline"); // iterable of all outlines on existing page (if there are any)
                if (outlines.FirstOrDefault() == null)
                {
                    // You have an existing page with no outlines. Unusual, but no harm in handling it. Do the 'new' procedure.
                    // Add the outline
                    XElement outlineEl = new XElement(ns + "Outline");
                    XElement outlineElChildrenWrapper = new XElement(ns + "OEChildren");

                    foreach (OneNoteOE sectionLoop in sectionsData)
                    {
                        XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns, quickStylesDict);
                        
                        outlineElChildrenWrapper.Add(sectionEl);
                        // add blank line under section
                        outlineElChildrenWrapper.Add(new XElement(ns + "OE",
                                                    new XAttribute("quickStyleIndex", quickStylesDict["p"]),
                                                    new XElement(ns + "T",
                                                        new XCData(" "))));
                    }
                    outlineEl.Add(outlineElChildrenWrapper);
                    titleEl.AddAfterSelf(outlineEl);
                }
                else if (outlines.Count() == 1)
                {
                    //There is a single outline that we look through.
                    XElement SingleOutlineExisting = outlines.First(); // edit this
                    XElement outlineChildrenNew = new XElement(ns + "OEChildren");
                    foreach (OneNoteOE sectionLoop in sectionsData)
                    {
                        XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns, quickStylesDict);
                        
                        outlineChildrenNew.Add(sectionEl);
                        // add blank line under section
                        outlineChildrenNew.Add(new XElement(ns + "OE",
                                                    new XAttribute("quickStyleIndex", quickStylesDict["p"]),
                                                    new XElement(ns + "T",
                                                        new XCData(" "))));
                    }
                    SingleOutlineExisting.Element(ns + "OEChildren").ReplaceWith(outlineChildrenNew);
                }
                else
                {
                    bool outlineFound = false;
                    foreach (XElement outline in outlines)
                    {
                        
                        // A list of the OEs in this outline's OEChildren Descendants that have a quickstyle index that is in the data sectiosn quickstlye indexes.
                        IEnumerable<XElement> taggedOEs = outline.Element(ns + "OEChildren").Descendants(ns + "OE")
                            .Where(x => x.Attribute("quickStyleIndex") != null && quickStylesDictSectionSearcher.Contains(x.Attribute("quickStyleIndex").Value));
                        if (taggedOEs.Count() > 0)
                        {
                            outlineFound = true;
                            XElement outlineChildrenNew = new XElement(ns + "OEChildren");
                            foreach (OneNoteOE sectionLoop in sectionsData)
                            {
                                XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns, quickStylesDict);
                                
                                outlineChildrenNew.Add(sectionEl);
                                // add blank line under section
                                outlineChildrenNew.Add( new XElement(ns + "OE", 
                                                            new XAttribute("quickStyleIndex", quickStylesDict["p"]),
                                                            new XElement(ns + "T",
                                                                new XCData(" "))));
                            }
                            outline.Element(ns + "OEChildren").ReplaceWith(outlineChildrenNew);
                            break;
                        }

                    }
                    if (!outlineFound) {
                        XElement firstOutline = outlines.First(); // edit this
                        XElement outlineChildrenNew = new XElement(ns + "OEChildren");
                        foreach (OneNoteOE sectionLoop in sectionsData)
                        {
                            XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns, quickStylesDict);
                            
                            outlineChildrenNew.Add(sectionEl);
                            // add blank line under section
                            outlineChildrenNew.Add(new XElement(ns + "OE",
                                                    new XAttribute("quickStyleIndex", quickStylesDict["p"]),
                                                    new XElement(ns + "T",
                                                        new XCData(" "))));
                        }
                        firstOutline.Element(ns + "OEChildren").ReplaceWith(outlineChildrenNew);
                        
                    }

                    // There is more than one outline, we need to find the right one based on it containing sections we are looking for, and edit that.
                    // If we don't find any, just use the first one.
                }
            }

            app.UpdatePageContent(PageResult.ToString());
            pageXML = "";
            app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
            PageResult = XDocument.Parse(pageXML);
            return PageResult;

        }

        public static XDocument GetPageXmlLinq (OneNoteInterop.Application app, string PageId)
        {
            string pageXML;
            app.GetPageContent(PageId, out pageXML, OneNoteInterop.PageInfo.piAll);
            XDocument PageResult = XDocument.Parse(pageXML);
            return PageResult;
        }


        /// <summary>
        /// Overload where section can be standard XmLNode rather than LINQ XElement
        /// </summary>
        /// <param name="sectionXML"></param>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public static bool CheckPageExistsInSection(XmlNode sectionXML, string pageName)
        {
            bool output = false;
            XElement sectionXMLX = XElement.Load(sectionXML.CreateNavigator().ReadSubtree());
            output = CheckPageExistsInSection(sectionXMLX, pageName);
            return output;
        }

        /// <summary>
        /// Takes in a section as XElement (LINQ XML Node) and name of a page, and checks if it exists.
        /// </summary>
        /// <param name="sectionXML"></param>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public static bool CheckPageExistsInSection (XElement sectionXML, string pageName)
        {
            bool output = false;
            XElement page = sectionXML.Elements(sectionXML.Name.Namespace + "Page").
                    Where(x => x.Attribute("name").Value == pageName).FirstOrDefault();
            if (page != null)
            {
                output = true;
            }
            return output;
        }
        public static string GetPageIdInSection(XmlNode sectionXML, string pageName) {
            string output;
            XElement sectionXMLX = XElement.Load(sectionXML.CreateNavigator().ReadSubtree());
            output = GetPageIdInSection(sectionXMLX, pageName);
            return output;
        }
        public static string GetPageIdInSection (XElement sectionXML, string pageName)
        {
            string output;
            XElement page = sectionXML.Elements(sectionXML.Name.Namespace + "Page").
                    Where(x => x.Attribute("name").Value == pageName).FirstOrDefault();
            if (page == null)
            {
                output = "";
            }
            else
            {
                output = page.Attribute("ID").Value;
            }
            return output;
        }
        #endregion
        #region Granular Page Elements
        public static XElement GetFontSizeConversionTable ()
        {
            XElement resource = XElement.Parse(Properties.Resources.OneNotePageAndElementStyles);
            XElement sconv = resource.Elements("SizingConverters").First();
            return sconv;
        }

        public static string GetOneNoteHyperLinkHTML (string sectionId, string pageId, string pageName, string linkText = null)
        {
            // We're making a decision here that all pages have to be created before any rendering can be done that includes links to those pages. (becuase the linable page ID is a mandatory property).
            if (linkText == null)
            {
                // linkText parameter has not been supplied - default it to pageName.
                linkText = pageName;
            }
            string link = $"<a href=\"onenote:#{pageName}&amp;section-id={sectionId}&amp;page-id={pageId}\">{linkText}</a>";
            return link;
        }

        public static string GetExternalHyperLinkHTML(string hyperlink, string linkText)
        {
            string link = $"<a href=\"{hyperlink}\">{linkText}</a>";
            return link;

        }

        #endregion
    }
}
