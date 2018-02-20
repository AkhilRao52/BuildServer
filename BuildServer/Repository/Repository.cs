/////////////////////////////////////////////////////////////////////
//  Repository.cs - This is the repository file                    //
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
Stores the source code information

Build Process:
==============
Required files

- ChildBuilder.cs

Maintenance History:
====================
ver 1.0

*/
using Navigator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace MessagePassingComm
{
    class Repository
    {
        Comm Repo { get; set; } = null;
        public string from { get; set; } = "http://localhost:8081/IMessagePassingComm";
        Thread ThreadReceive = null;
        List<String> ListOfFiles { get; set; } = new List<string>();
        List<String> XMLFiles { get; set; } = new List<string>();
        public Repository()
        {
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting Repository server on 8081 port number", '=');
            Repo = new Comm("http://localhost", 8081);
            ThreadReceive = new Thread(ThreadProcessReceive);
            ThreadReceive.Start();

        }
        private void FilesToChild(List<string> files, string path)              // Sending files to child builder
        {
            foreach(string s in files)
            {
                bool tester = true;
                do
                {
                    tester = Repo.postFile(s, "../../../RepositoryStorage/", path);
                } while (!tester);
            }
            
        }

        void ThreadProcessReceive()
        {
            while (true)
            {
                CommMessage RepositoryMessage = Repo.getMessage();
                RepositoryMessage.show();
                if (RepositoryMessage.command == null)
                    continue;
                if (RepositoryMessage.command == "RequestRepositoryFiles")
                {
                    CommMessage RepoFilesReply = new CommMessage(CommMessage.MessageType.reply);
                    RepoFilesReply.command = "getRepositoryFiles";
                    RepoFilesReply.from = "http://localhost:8081/IMessagePassingComm";
                    RepoFilesReply.to = RepositoryMessage.from;
                    RepoFilesReply.author = "repository";
                    RepoFilesReply.arguments = RepoFiles();
                    Repo.postMessage(RepoFilesReply);
                }
                if (RepositoryMessage.command == "RequestRepositoryXMLFiles")
                {
                    CommMessage XMLFilesReply = new CommMessage(CommMessage.MessageType.reply);
                    XMLFilesReply.command = "getRepositoryXMLFiles";
                    XMLFilesReply.from = "http://localhost:8081/IMessagePassingComm";
                    XMLFilesReply.to = RepositoryMessage.from;
                    XMLFilesReply.author = "repository";
                    XMLFilesReply.arguments = getRepoXMLFiles();
                    Repo.postMessage(XMLFilesReply);
                }
                if (RepositoryMessage.command == "sendBuildRequest")
                {

                    BuildRequestToQueue(RepositoryMessage.arguments);
                }
                if (RepositoryMessage.command == "BuildRequest")
                {

                    XMLFileGeneration(RepositoryMessage.arguments);
                }
                if (RepositoryMessage.command == "RequestParsedFiles")
                {
                    FilesToChild(RepositoryMessage.arguments, RepositoryMessage.fileName);
                    CommMessage MessageReply = new CommMessage(CommMessage.MessageType.reply);
                    MessageReply.from = RepositoryMessage.to;
                    MessageReply.to = RepositoryMessage.from;
                    MessageReply.author = "Repository";
                    MessageReply.command = "SentFiles";
                    Repo.postMessage(MessageReply);
                }
            }
        }
        private void XMLFileGeneration(List<String> selectedFiles)
        { 

            XDocument XML = new XDocument();
            if (selectedFiles.Count > 0)
            {

                XML.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                XComment comment = new XComment("CreatedBuild request from selected files");
                XML.Add(comment);
                XElement testRequest = new XElement("testRequest");
                XML.Add(testRequest);
                XElement child1 = new XElement("author", "Butchi Venkata Akhil Rao");
                XElement child2 = new XElement("dateTime", DateTime.Now.ToString());
                XElement child3 = new XElement("test");
                XElement Subchild1 = new XElement("testDriver", selectedFiles[0]);
                child3.Add(Subchild1);
                for (int i = 1; i < selectedFiles.Count(); i++)
                {
                    XElement SubChild2 = new XElement("tested", selectedFiles[i]);
                    child3.Add(SubChild2);
                }
                testRequest.Add(child1);
                testRequest.Add(child2);
                testRequest.Add(child3);
                String path = @"../../../RepositoryStorage/" + "BuildRequest" + ".xml";
                int count = 1;
                while (File.Exists(path))
                {
                    path = @"../../../RepositoryStorage/" + "BuildRequest" + count.ToString() + ".xml";
                    count++;
                }
                XML.Save(path);
                
            }
        }
        public void BuildRequestToQueue(List<String> selectedXmlFiles)          // Builds Request
        {
            Console.WriteLine("Requirement 4:Shall provide a Repository server that supports client browsing to find files to build, builds an XML build request string and sends that and the cited files to the Build Server");
                CommMessage BuildRequestName = new CommMessage(CommMessage.MessageType.request);
                BuildRequestName.to = "http://localhost:8083/IMessagePassingComm";
                BuildRequestName.from = from;
                BuildRequestName.arguments = selectedXmlFiles;             
                BuildRequestName.author = "Repository";
                BuildRequestName.command = "StoreBuildRequests";
                Repo.postMessage(BuildRequestName);
           
           
        }
        public List<String> RepoFiles()
        {
            if(ListOfFiles!=null)
            {
                ListOfFiles.Clear();
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(@"../../../RepositoryStorage");
            foreach (FileInfo f in directoryInfo.GetFiles())
            {
                ListOfFiles.Add(f.ToString());

            }
            return ListOfFiles;
        }
        public List<String> getRepoXMLFiles()
        {
            if (XMLFiles != null)
            {
                XMLFiles.Clear();
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(@"../../../RepositoryStorage");
            foreach (FileInfo f in directoryInfo.GetFiles("*.xml"))
            {
                XMLFiles.Add(f.ToString());

            }
            return XMLFiles;
        }

        static void Main(string[] args)
        {
            Repository repo = new Repository();
        }
    }
}
