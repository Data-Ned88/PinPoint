using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneNoteInterop = Microsoft.Office.Interop.OneNote;
using System.Xml;
using System.Xml.Linq;
using System.Security.Policy;

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
