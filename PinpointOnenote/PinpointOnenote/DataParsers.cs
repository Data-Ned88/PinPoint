using System;
using System.Collections.Generic;
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
        
        public static List<OneNoteOE> BuildPageDataFromXml (XElement pageXML)
        //XElement param pageXML should be a "Page" element from StaticAndTestData.xml
        {
            List<OneNoteOE> output = new List<OneNoteOE>();

            return output;

        }
        public static OneNoteOE BuildOEWithChildrenFromXml (XElement nodeXml)
        {
            //TODO build out the 2 recursive functions to make class data from static XML, and onenote XML from class object.
            //This should be recursive and take in a Section element from a page from StaticAndTestData.xml, or anything more granular. //THIS IS UNFINISHED

            // XElement resource = XElement.Parse(PinpointOnenote.Properties.Resources.StaticAndTestData);  // gets the static and test data resource file.
            // XElement pageDataXml = resource.Descendants("Page").Where(x => x.Attribute("name").Value == "Notes and Instructions").First(); //Gets the first page from this
            // XElement firstSection = pageDataXml.Element("Sections").Elements("Section").First(); // gets the first section from this.


            OneNoteOE output = new OneNoteOE();


            // Dealing with the input itself. 

            if (nodeXml.Name == "Section")
            {
                string sectionQsDef = nodeXml.Attribute("name").Value;
                bool isHeaderless = bool.Parse(nodeXml.Attribute("headerless").Value);

                //1. Give it the qindex
                //2. Identify its child baseOEs (lines or sections) and child tables, and recurse them, passing on n indentations if or if not headerless.

            }
            else if (nodeXml.Name == "Line")
            {
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
