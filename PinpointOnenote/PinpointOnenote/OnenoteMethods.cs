using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OneNoteInterop = Microsoft.Office.Interop.OneNote;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace PinpointOnenote
{        
    /// <summary>
   /// This holds all the methods for interacting with the OneNote application, Notebooks, Sections, and Section Groups.
   /// </summary>

    public static class OnenoteMethods
    {

        public static OneNoteInterop.Application InstantiateOneNoteApp()
        {
            OneNoteInterop.Application app = new OneNoteInterop.Application();
            return app;

        }
        public static bool IsOnenoteOpen (OneNoteInterop.Application app)
        {
            //Microsoft.Office.Interop.OneNote.Application app = new Microsoft.Office.Interop.OneNote.Application();

            if (app.Windows.Count < 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static XmlDocument GetOneNoteHierarchy(OneNoteInterop.Application app)
        {
            String strXML;
            app.GetHierarchy(null,
                    OneNoteInterop.HierarchyScope.hsPages, out strXML);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXML);
            return xmlDoc;
        }

        public static XmlNamespaceManager GetOneNoteNSMGR (XmlDocument xmlDoc)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            string strNamespace = "http://schemas.microsoft.com/office/onenote/2013/onenote";
            nsmgr.AddNamespace("one", strNamespace);
            return nsmgr;
        }

        public static List<string> GetAvailableNotebookNames(XmlDocument xmlDoc, XmlNamespaceManager nsmgr)
        {
            /// <summary>
            /// Gets you the names of all open OneNote Notebooks as a List of Strings
            /// </summary>
            List<string> output = new List<string>();

            XmlNode xmlNode = xmlDoc.SelectSingleNode("//one:Notebooks", nsmgr);
            XmlNodeList notebookNodes = xmlNode.SelectNodes("//one:Notebook", nsmgr);
            foreach (XmlNode nb in notebookNodes)
            {
                output.Add(nb.Attributes["name"].Value);
            }


            //output.Add("First");
            return output;
        }

        public static List<XmlNode> GetAvailableNotebooks(XmlDocument xmlDoc, XmlNamespaceManager nsmgr)
        {
            List<XmlNode> output = new List<XmlNode>();
            XmlNode xmlNode = xmlDoc.SelectSingleNode("//one:Notebooks", nsmgr);
            XmlNodeList notebookNodes = xmlNode.SelectNodes("//one:Notebook", nsmgr);
            foreach(XmlNode nb in notebookNodes)
            {
                output.Add(nb);
            }
            return output;
        }
        public static string GetNotebookID(List<XmlNode> availableNotebooks,string notebookname)
        {
            string outstring = null;
            XmlNode targetNotebook = availableNotebooks.Where(x => x.Attributes["name"].Value == notebookname).FirstOrDefault();
            if (targetNotebook != null)
            {
                outstring = GetNotebookID(targetNotebook);
            }
            return outstring;
        }
        public static string GetNotebookID (XmlNode nbXml)
        {
            return nbXml.Attributes["ID"].Value;
        }

        private static bool IsLocked (XmlNode nbXml)
        {
            bool isLockedoutput = false;

            if (nbXml.Attributes["locked"] != null)
            {
                if (nbXml.Attributes["locked"].Value == "true")
                {
                    isLockedoutput = true;
                }
            }
            return isLockedoutput;
        }



        public static List<OneNoteSection> GetSectionsInNotebook(OneNoteInterop.Application app, XmlDocument hier, XmlNamespaceManager nsmgr, string notebookname)
        {
            string selector = $"//one:Notebook[@name=\"{notebookname}\"]";
            XmlNode notebookXml = hier.SelectSingleNode(selector, nsmgr);
            return GetSectionsInNotebook(app,notebookXml);

        }
        public static List<OneNoteSection> GetSectionsInNotebook(OneNoteInterop.Application app, XmlNode nbXml)
        { 
            List<OneNoteSection> output = new List<OneNoteSection>();
            foreach (XmlNode cn in nbXml.ChildNodes)
            {
                if (cn.Name == "one:Section")
                {
                    OneNoteSection SectionListEntry = new OneNoteSection();
                    SectionListEntry.Notebook = nbXml.Attributes["name"].Value;
                    SectionListEntry.SectionID = cn.Attributes["ID"].Value;
                    SectionListEntry.SectionName = cn.Attributes["name"].Value;
                    SectionListEntry.SectionXML = cn;
                    SectionListEntry.IsLocked = IsLocked(cn);
                    if (!IsLocked(cn))
                    {
                        SectionListEntry.IsValidPinPointInstance = DataParsers.TestOneNoteSectionValidPasswordBank(app, cn);
                        SectionListEntry.IsValidTooltip = DataParsers.InvalidPasswordBankReason(app, cn);
                    }
                    else
                    {
                        SectionListEntry.IsValidTooltip = "This section is locked.\n\nGo to OneNote and unlock this section,\nthen click 'Refresh Sections' below.";
                    }
                    output.Add(SectionListEntry);

                }
                else if ((cn.Name == "one:SectionGroup") && (cn.Attributes["name"].Value != "OneNote_RecycleBin"))
                {
                    foreach (XmlNode sg_s in cn.ChildNodes)
                    {
                        if (sg_s.Name == "one:Section")
                        {
                            OneNoteSection SectionListEntry = new OneNoteSection();
                            SectionListEntry.Notebook = nbXml.Attributes["name"].Value;
                            SectionListEntry.SectionGroupID = cn.Attributes["ID"].Value;
                            SectionListEntry.SectionGroup = cn.Attributes["name"].Value;
                            SectionListEntry.SectionID = sg_s.Attributes["ID"].Value;
                            SectionListEntry.SectionName = sg_s.Attributes["name"].Value;
                            SectionListEntry.SectionXML = sg_s;
                            SectionListEntry.IsLocked = IsLocked(sg_s);
                            if (!IsLocked(sg_s))
                            {
                                SectionListEntry.IsValidPinPointInstance = DataParsers.TestOneNoteSectionValidPasswordBank(app,sg_s);
                                SectionListEntry.IsValidTooltip = DataParsers.InvalidPasswordBankReason(app, sg_s);
                            }
                            else
                            {
                                SectionListEntry.IsValidTooltip = "This section is locked.\n\nGo to OneNote and unlock this section,\nthen click 'Refresh Sections' below.";
                            }
                            output.Add(SectionListEntry);
                        }
                    }
                }
            }
            return output;
        }

        public static bool CheckSectionExists(List<OneNoteSection> sectionsList, string sectionName)
        {
            bool output = false;
            OneNoteSection targetSection = sectionsList.Where(x => x.SectionName == sectionName).FirstOrDefault();
            if (targetSection != null)
            {
                output = true;
            }
            return output;
        }

        public static string GetSectionID (List<OneNoteSection> sectionsList, string sectionName)
        {
            OneNoteSection targetSection = sectionsList.Where(x => x.SectionName == sectionName).First();
            return targetSection.SectionID;
        }

        /// <summary>
        /// This adds a new section to a Notebook with the name provided in sectionName.
        /// It returns the section id of the new section, and updates the hierarchy you are working with for you.
        /// It does *not* support the adding of Sections to existing or new section groups within the notebook.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="hierarchy"></param>
        /// <param name="namespaceMgr"></param>
        /// <param name="sectionName"></param>
        /// <param name="existingSectionsList"></param>
        /// <returns></returns>
        public static string AddSectionToNotebook (
            OneNoteInterop.Application app,
            ref XmlDocument hierarchy,
            ref XmlNamespaceManager namespaceMgr,
            string sectionName,
            ref List<OneNoteSection> existingSectionsList,
            string notebookId,
            string sectionColor
            )
            
        {
            //Amend the name if it already exists.
            bool AlreadyExists = CheckSectionExists(existingSectionsList, sectionName);
            if (AlreadyExists)
            {
                sectionName = sectionName + " (1)";
            }
            XmlNode NotebookNodeInHierarchy = hierarchy.SelectSingleNode($"//one:Notebook[@ID=\"{notebookId}\"]", namespaceMgr);
            string NotebookPath = NotebookNodeInHierarchy.Attributes["path"].Value;
            string sectRelPath = sectionName + ".one";

            // Prepare a separate hierarchy xml document and namespace manager for just the Notebook we are adding the section to.
            string NoteBookXmlHier;
            app.GetHierarchy(notebookId, OneNoteInterop.HierarchyScope.hsPages, out NoteBookXmlHier);
            XmlDocument NotebookXmlDoc = new XmlDocument();
            NotebookXmlDoc.LoadXml(NoteBookXmlHier);
            XmlNamespaceManager notebookNSMGR = OnenoteMethods.GetOneNoteNSMGR(NotebookXmlDoc);
            XmlNode notebookNotebookNode = NotebookXmlDoc.SelectSingleNode("//one:Notebook", notebookNSMGR);
            string notebookName = notebookNotebookNode.Attributes["name"].Value;
            // Make the new section within the notebooks own heirarchy xml
            XmlElement newSection = NotebookXmlDoc.CreateElement("one", "Section", "http://schemas.microsoft.com/office/onenote/2013/onenote");
            newSection.SetAttribute("name", sectionName);
            newSection.SetAttribute("path", NotebookPath + sectRelPath);
            newSection.SetAttribute("color", sectionColor);

            //Find the first available section group in the notebook. If there is none, just add it, if there is, insert before it.
            XmlNodeList NotebookNodeSGL = NotebookXmlDoc.SelectNodes("//one:SectionGroup", notebookNSMGR);
            XmlNode NotebookNodeSGLFirst = NotebookNodeSGL.Cast<XmlNode>().FirstOrDefault();
            if (NotebookNodeSGLFirst == null)
            {
                notebookNotebookNode.AppendChild(newSection);
            }
            else
            {
                notebookNotebookNode.InsertBefore(newSection, NotebookNodeSGLFirst);
            }
            // update the hierarchy with the new notebook
            app.UpdateHierarchy(NotebookXmlDoc.OuterXml);
            // Refresh the working heirarchy (all notebooks) and return new section ID.
            hierarchy = GetOneNoteHierarchy(app);
            namespaceMgr = GetOneNoteNSMGR(hierarchy);
            existingSectionsList = GetSectionsInNotebook(app,hierarchy, namespaceMgr, notebookName);

            string output = GetSectionID(existingSectionsList, sectionName);
            return output;
        }


        public static Dictionary<string, Dictionary<string, object>> GetSectionPagesLookup(OneNoteInterop.Application app, XmlNode sectionXml)
        {
            XElement sectionXMLX = XElement.Load(sectionXml.CreateNavigator().ReadSubtree());
            Dictionary<string, Dictionary<string, object>> pagesLookup = GetSectionPagesLookup(app, sectionXMLX);
            return pagesLookup;
        }
        public static Dictionary<string, Dictionary<string, object>> GetSectionPagesLookup (OneNoteInterop.Application app,XElement sectionXml)
        {
            Dictionary<string, Dictionary<string, object>> pagesLookup = new Dictionary<string, Dictionary<string, object>>();
            string sectionName = sectionXml.Attribute("name").Value;
            string sectionID = sectionXml.Attribute("ID").Value;


            string sectionIDLink;
            string pageIDLink;

            app.GetHyperlinkToObject(sectionID, "", out sectionIDLink);
            Regex rxSect = new Regex(@"(section-id=)(\{[\-A-Za0z0-9]{0,65}\})");
            Regex rxPage = new Regex(@"(page-id=)(\{[\-A-Za0z0-9]{0,65}\})");

            Dictionary<string, object> SectionDict = new Dictionary<string, object>();
            SectionDict.Add("sectionId", rxSect.Match(sectionIDLink).Groups[2].Value);

            IEnumerable<XElement> pages = sectionXml.Elements(sectionXml.Name.Namespace + "Page");

            if (pages.FirstOrDefault() !=null)
            {
                Dictionary<string, object> pagesDict = new Dictionary<string, object>();
                foreach (XElement page in pages)
                {
                    app.GetHyperlinkToObject(page.Attribute("ID").Value, "", out pageIDLink);
                    pagesDict.Add(rxPage.Match(pageIDLink).Groups[2].Value, page.Attribute("name").Value);
                    pageIDLink = "";
                }
                SectionDict.Add("pages", pagesDict);
            }
            pagesLookup.Add(sectionName, SectionDict);
            return pagesLookup;
        }

        public static bool sectionIsLocked (OneNoteInterop.Application app, XmlDocument hier, XmlNamespaceManager nsmgr, string notebookName, string sectionId)
        {

            //to determine if a onenote section is locked in Xml

            bool returnable = false;

            List<OneNoteSection> sections = GetSectionsInNotebook(app, hier, nsmgr, notebookName);

            if (sections.Count > 0)
            {
                OneNoteSection targetSection = sections.Where(x => x.SectionID == sectionId).FirstOrDefault();
                if (targetSection != null && targetSection.IsLocked == true)
                {
                    returnable = true;
                }
            }
            return returnable;
        }
    }
}
