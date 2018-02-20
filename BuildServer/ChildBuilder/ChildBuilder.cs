/////////////////////////////////////////////////////////////////////
//  ChildBuilder.cs - This is the child builder file               //
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
Child Builder builds the child processes from the processes received from MotherBuild

Build Process:
==============
Required files

- MotherBuilder.cs


Maintenance History:
====================
ver 1.0

*/
using Navigator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuildServer;

namespace MessagePassingComm
{
    class ChildBuilder
    {
        public static int PortChildBuilder { get; set; } = 0;
        public static int PortMotherBuilder { get; set; } = 0;
        public static Comm childbuilder { get; set; } = null;
        public static string BuildRequestName { get; set; } = null;
        public static Thread ReceiverThread { get; set; } = null;
        public static List<string> Tested { get; set; } = new List<string>();
        public static string FolderPath { get; set; } = null;
        public static string Author { get; set; } = null;
        public static string logFile { get; set; } = null;
        public static string DateTime { get; set; } = null;
       
        public ChildBuilder()  
        {
            Console.WriteLine(" Requirement 1: Shall be prepared using C#, the .Net Frameowrk, and Visual Studio 2017");
        }
        public  static void ThreadProc()  // Creates the thread process used for communication with MotherBuilder
        {
           
            bool result = true;
            while (result)
            { 
                CommMessage MessageCommunication = childbuilder.getMessage();
                MessageCommunication.show();
                if (MessageCommunication.command == "BUILD")
                {
                    BuildRequestName = MessageCommunication.xmlString;
                    startBuildProcess(MessageCommunication.xmlString);
                }
                if(MessageCommunication.command=="Kill")
                {
                    childbuilder.closeConnection();
                    CommMessage ReceiverClose = new CommMessage(CommMessage.MessageType.closeReceiver);
                    ReceiverClose.from= "http://localhost:" + PortChildBuilder.ToString() + "/IMessagePassingComm";
                    ReceiverClose.to = MessageCommunication.from;
                    ReceiverClose.author = "Child" + PortChildBuilder.ToString();
                    childbuilder.postMessage(ReceiverClose);
                    result = false;
                }
                if(MessageCommunication.command== "SentFiles")
                {
                    ProcessBuild();
                }
            }
        }
        public static void startBuildProcess(string buildRequest)     // Initiating the thread process
        {         
            XmlParsing xml = new XmlParsing();
            xml.XMLLoad("../../../RepositoryStorage/" + buildRequest);
             Author = xml.Parse("author");
            DateTime = xml.Parse("dateTime");
            Tested = xml.ParseList("tested");
                String testDriver = xml.Parse("testDriver");      
                Tested.Add(testDriver);        
            CommMessage requestFiles = new CommMessage(CommMessage.MessageType.request);
            requestFiles.author = "ChildBuilder";
            requestFiles.command = "RequestParsedFiles";
            requestFiles.fileName = FolderPath + "/";
            requestFiles.from = "http://localhost:" + PortChildBuilder.ToString() + "/IMessagePassingComm";
            requestFiles.to = "http://localhost:8081/IMessagePassingComm";
            requestFiles.arguments = Tested;
            childbuilder.postMessage(requestFiles);
        }
        public static void ProcessBuild()                   // Building the new Process
        {
            try
            {
                List<string> wholePath = new List<string>();
                foreach (string f in Tested)
                {
                    string fullPath = Path.GetFullPath(FolderPath+"//" + f);
                    wholePath.Add(fullPath);
                }
                string parameters = "/Ccsc.exe /target:library ";
                foreach (string fin in wholePath)
                {
                    parameters += " " + fin;
                }
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = parameters;
                p.StartInfo.WorkingDirectory = FolderPath;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
                string errors = p.StandardError.ReadToEnd();
                string output = p.StandardOutput.ReadToEnd();
                int index = Tested[0].IndexOf('.');
                if(File.Exists(FolderPath+"/"+Tested[0].Substring(0,index)+".dll"))
                {
                    Console.WriteLine("SUCCESS");
                }

                else
                {
                    Console.WriteLine("FAIL");
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
            }
            ReadyMessage();
        }
        public static void ReadyMessage()
        {
            CommMessage send = new CommMessage(CommMessage.MessageType.request);
            send.from = "http://localhost:" + PortChildBuilder.ToString() + "/IMessagePassingComm";
            send.to = "http://localhost:8083/IMessagePassingComm";
            send.author = "Butchi Venkata Akhil Rao";
            send.command = "READY";
            send.fileName = FolderPath + "/";
            send.show();
            childbuilder.postMessage(send);
        }
        static void Main(string[] args)
        {


            ChildBuilder childBuilder = new ChildBuilder();
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting ChildBuilder", '=');

            int j = 0;
            string[] commandlineArgs = new string[3];
            foreach (string command in args)
            {

                commandlineArgs[j] = command;
                j++;
            }
            Console.Title = "ChildProc";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            Console.Write("\n  Child Builder Initialized");
            Console.Write("\n ====================");
            PortMotherBuilder = Convert.ToInt32(commandlineArgs[1]);
            PortChildBuilder = Convert.ToInt32(commandlineArgs[2]);
            Console.Write("\n  Hello from childbuilder #{0}\n\n", commandlineArgs[0]);
            FolderPath = "../../../BuildStorage/" + "buildstorage_" + commandlineArgs[0].ToString();
            
                Directory.CreateDirectory(FolderPath);
            
            Console.WriteLine(PortChildBuilder);
            childbuilder = new Comm("http://localhost", PortChildBuilder);

            ReadyMessage();
            ReceiverThread = new Thread(ThreadProc);
            ReceiverThread.Start();

        }
    }
}
