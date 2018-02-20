/////////////////////////////////////////////////////////////////////
//  XMLParser.cs - Parses files                                    //
//  ver 1.0                                                        //
//  Language:      Visual C#  2017                                 //
//  Platform:      Windows 10 (used Virtual Box on mac)            //
//  IDE used:      Microsoft Visual Studio 2017, Community Edition //                                                               
//  Application:   Build Server , FALL 2017                        //
//  Author:        Butchi Venkata Akhil Rao,                       //
//                 Syracuse University brao01@syr.edu              //
/////////////////////////////////////////////////////////////////////

/*
Module Operations:
==================
This file parses the files sent from child builder

Build Process:
==============
Required files

- ChildBuilder.cs


Maintenance History:
====================
ver 1.0

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BuildServer
{
    public class XmlParsing
    {
        public string Author { get; set; } = "";
        public string DateTime { get; set; } = "";
        public string ToolChain { get; set; } = "";
        public string TestDriver { get; set; } = "";
        public List<string> TestedFiles { get; set; } = new List<string>();
        public XDocument Document { get; set; } = new XDocument();
                                                
        public bool XMLLoad(string path)    //Load xml file from the test request sent
        {
            try
            {
                Document = XDocument.Load(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }
        public string Parse(string propertyName)
        {

            string StringParse = Document.Descendants(propertyName).First().Value;
            if (StringParse.Length > 0)
            {
                switch (propertyName)
                {
                    case "author":
                        Author = StringParse;
                        break;
                    case "dateTime":
                        DateTime = StringParse;
                        break;
                    case "testDriver":
                        TestDriver = StringParse;
                        break;
                    case "language":
                        ToolChain = StringParse;
                        break;
                    default:
                        break;
                }
                return StringParse;
            }
            return "";
        }
                                               /*----< parse document for property list >---------------------*/
     
        public List<string> ParseList(string propertyName)
        {
            List<string> values = new List<string>();

            IEnumerable<XElement> parseElems = Document.Descendants(propertyName);

            if (parseElems.Count() > 0)
            {
                switch (propertyName)
                {
                    case "tested":
                        foreach (XElement elem in parseElems)
                        {
                            values.Add(elem.Value);
                        }
                        TestedFiles = values;
                        break;
                    default:
                        break;
                }
            }
            return values;
        }
    }
    class XMLParserExecution
    {
#if (TEST_XMLPARSER)
        static void Main(string[] args)
        {
            XmlParsing x = new XmlParsing();
            x.XMLLoad(@"..\..\..\BuildStorage\TestRequest.xml");
            x.Parse("author");
            x.Parse("dateTime");
            x.Parse("language");
            x.Parse("testDriver");
            x.ParseList("tested");
            Console.WriteLine("Name of Author " + x.Author);
            Console.WriteLine(" date and time " + x.DateTime);
            Console.WriteLine("Tool chain to be used: " + x.ToolChain);
            Console.WriteLine("Test Driver: " + x.TestDriver);
            foreach(string test in x.TestedFiles)
            {
                Console.WriteLine("Tested: " + test);
            }
            Console.ReadKey();
        }
    }
#endif
    }

