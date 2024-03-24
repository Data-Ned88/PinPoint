using System;
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

            /*
             SYNOPSIS:
            This make an outline for each OneNoteOE in the sectionsData parameter. THe Outline has OECHildren has a first OE, which this 
            script tags the author attribute of, which has the effect of permanently tagging the auhtor property of the outline, which is much less
            at risk of erasure by users than the quickstyle defs, which we used before.

            It also allows the script on update (newPage param = false) to preserve other outlines not tagged by this script, which may contain custom user notes.
            On update, it deletes all outlines, saving the non-script tagged outlines and re-parsing them afresh.
            It then recreates the section-specific outlines from the input param afresh, adds them to the pageElement, 
                (setting the first one as Y-psition 90 px, 
                so ideally try and make a page with 1 outline section if you can as it will be more likely to stay front and centre around any user-added notes),
            then adds the user-specific notes, apart from any ink or images.

            This has been tested to work on page XML data with 2 sections, adding text-only user notes, then reupdating.
             
             */



            string pageXML;
            app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
            XDocument PageResult = XDocument.Parse(pageXML);

            List<string> sectionsListAuthorProps = new List<string>();
            foreach (OneNoteOE section in sectionsData)
            {
                
                if (section.author != null)
                {
                    sectionsListAuthorProps.Add(section.author);
                }
            }
            if (sectionsListAuthorProps.Count != sectionsListAuthorProps.Distinct().Count())
            {
                throw new Exception($"RenderOneNotePage ERROR: Your List<OneNoteOE> sectionsData input param value has 2 or more sections that share an author name. This is not allowed.");
            }


            XNamespace ns = PageResult.Root.Name.Namespace;
            XElement pageEl = PageResult.Elements(ns + "Page").First();

            if (newPage) {

                // Add the outline
                

                foreach (OneNoteOE sectionLoop in sectionsData)
                {
                    XElement outlineEl = new XElement(ns + "Outline");
                    XElement outlineElChildrenWrapper = new XElement(ns + "OEChildren");
                    XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns);
                    outlineElChildrenWrapper.Add(sectionEl);
                    outlineEl.Add(outlineElChildrenWrapper);
                    pageEl.Add(outlineEl);
                }
                app.UpdatePageContent(PageResult.ToString());
                pageXML = "";
                app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
                PageResult = XDocument.Parse(pageXML);


            }
            else
            {
                IEnumerable<XElement> outlines = pageEl.Elements(ns + "Outline"); // iterable of all outlines on existing page (if there are any)
                
                if (outlines.FirstOrDefault() == null)
                {
                    //Console.WriteLine("You have an existing page with no outlines. Unusual, but no harm in handling it.");
                    // You have an existing page with no outlines. Unusual, but no harm in handling it. Do the 'new' procedure.
                    // Add the outline
                    foreach (OneNoteOE sectionLoop in sectionsData)
                    {
                        XElement outlineEl = new XElement(ns + "Outline");
                        XElement outlineElChildrenWrapper = new XElement(ns + "OEChildren");
                        XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns);
                        outlineElChildrenWrapper.Add(sectionEl);
                        outlineEl.Add(outlineElChildrenWrapper);
                        pageEl.Add(outlineEl);
                    }

                    app.UpdatePageContent(PageResult.ToString());
                    pageXML = "";
                    app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
                    PageResult = XDocument.Parse(pageXML);
                }
                else
                {
                    //1. Remove everything from the outlines that matches one of the author names, and preserve in an array everything that doesn't.
                    List<XElement> userOutlines = new List<XElement>();
                    List<string> OutlinesToDelete = new List<string>();
                    foreach (XElement outline in outlines)
                    {
                        if (outline.Attribute("author") != null && sectionsListAuthorProps.Contains(outline.Attribute("author").Value))
                        {
                            
                            OutlinesToDelete.Add(outline.Attribute("objectID").Value);
                        }
                        else
                        {
                            userOutlines.Add(outline);
                            OutlinesToDelete.Add(outline.Attribute("objectID").Value);
                        }
                    }
                    foreach (string id in OutlinesToDelete)
                    {
                        app.DeletePageContent(pageID,id);
                    }
                    pageXML = "";
                    app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
                    PageResult = XDocument.Parse(pageXML);
                    ns = PageResult.Root.Name.Namespace;
                    pageEl = PageResult.Elements(ns + "Page").First();

                    // refill using the data
                    int sectionDataInc = 0;
                    foreach (OneNoteOE sectionLoop in sectionsData)
                    {
                        XElement outlineEl = new XElement(ns + "Outline");
                        if (sectionDataInc == 0)
                        {
                            outlineEl.Add(
                                new XElement(ns + "Position", new XAttribute("x", "36.0"), new XAttribute("y", "90.0"), new XAttribute("z", "0"))
                            );
                        }
                        XElement outlineElChildrenWrapper = new XElement(ns + "OEChildren");
                        XElement sectionEl = DataParsers.BuildOneNoteXmlOeFromClassObject(sectionLoop, ns);
                        outlineElChildrenWrapper.Add(sectionEl);
                        outlineEl.Add(outlineElChildrenWrapper);
                        pageEl.Add(outlineEl);
                        sectionDataInc++;
                    }
                    //reload the custom user outlines
                    foreach(XElement uo in userOutlines)
                    {
                        XElement newOutlineEl = new XElement(ns + "Outline");
                        string _x = uo.Element(ns + "Position").Attribute("x").Value;
                        string _y = uo.Element(ns + "Position").Attribute("y").Value;
                        string _width = uo.Element(ns + "Size").Attribute("width").Value;
                        string _height = uo.Element(ns + "Size").Attribute("height").Value;
                        newOutlineEl.Add(
                            new XElement(ns + "Position", new XAttribute("x", _x), new XAttribute("y", _y), new XAttribute("z", "2"))
                            );
                        newOutlineEl.Add(
                            new XElement(ns + "Size", new XAttribute("width", _width), new XAttribute("height", _height))
                               );
                        XElement newOutlineChildren = new XElement(ns + "OEChildren");

                        
                        IEnumerable<XElement> OEs = uo.Element(ns + "OEChildren").Elements(ns + "OE");
                        
                        foreach (XElement oec in OEs)
                        {
                            XElement newOE = DataParsers.ParseOeToNew(oec, ns);
                            newOutlineChildren.Add(newOE);
                        }
                        
                        newOutlineEl.Add(newOutlineChildren);
                        pageEl.Add(newOutlineEl);
                    }

                    app.UpdatePageContent(PageResult.ToString());
                    pageXML = "";
                    app.GetPageContent(pageID, out pageXML, OneNoteInterop.PageInfo.piAll);
                    PageResult = XDocument.Parse(pageXML);
                }
            }





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
