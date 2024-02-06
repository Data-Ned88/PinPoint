using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OneNoteInterop = Microsoft.Office.Interop.OneNote;
using System.Xml;
using System.Xml.Linq;

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



        public static List<OneNoteSection> GetSectionsInNotebook(XmlDocument hier, XmlNamespaceManager nsmgr, string notebookname)
        {
            string selector = $"//one:Notebook[@name=\"{notebookname}\"]";
            XmlNode notebookXml = hier.SelectSingleNode(selector, nsmgr);
            return GetSectionsInNotebook(notebookXml);

        }
        public static List<OneNoteSection> GetSectionsInNotebook(XmlNode nbXml)
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
                        SectionListEntry.IsValidPinPointInstance = false; // Eventually replace the hardcode false with Valid pinpoint eval function call
                    }
                    //TODO Parser function to determine if it's a valid PinPoint Existing Section. Would we have this here??
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
                                SectionListEntry.IsValidPinPointInstance = false; // Eventually replace the hardcode false with Valid pinooint eval function call
                            }
                            output.Add(SectionListEntry);
                        }
                    }
                }
            }
            return output;
        }



    }
}
