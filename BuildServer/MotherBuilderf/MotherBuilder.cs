/////////////////////////////////////////////////////////////////////
//  MotherBuilder.cs - This is the mother builder file             //
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
Creates parse requests and sends them to the child builder.

Build Process:
==============
Required files

- ChildBuilder.cs
- MessageCommunication.cs
- BlockingQueue.cs


Maintenance History:
====================
ver 1.0

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWTools;
using Navigator;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MessagePassingComm
{
    class MotherBuilder
    {
        public string from { get; set; } = "http://localhost:8083/IMessagePassingComm";
        public static Dictionary<string, int> PortsChildBuilder = new Dictionary<string, int>();
        private static BlockingQueue<String> BuildRequest = new BlockingQueue<String>();
        private static BlockingQueue<CommMessage> readyqueue = new BlockingQueue<CommMessage>();
        Comm motherbuilder { get; set; } = null;
        public static int numProcess = 0;
        public MotherBuilder()              // Process checking 
        {
            ClientEnvironment.verbose = true;
            TestUtilities.vbtitle("Starting MotherBuilder", '=');
            this.motherbuilder = new Comm("http://localhost", 8083);
            Thread receiverThread = new Thread(ThreadProcess);
            receiverThread.Start();
            Thread ThreadCheck = new Thread(ProcessCheck);
            ThreadCheck.Start();
        }
        void ProcessCheck()
        {
            while (true)
            {
                if (readyqueue.size() != 0 && BuildRequest.size() != 0)
                {
                    CommMessage MsgReady = readyqueue.deQ();
                    String Build = BuildRequest.deQ();
                    
                    CommMessage BuildMsg = new CommMessage(CommMessage.MessageType.reply);
                    BuildMsg.to = MsgReady.from;
                    BuildMsg.from = this.from;
                    BuildMsg.author = "MotherBuilder";
                    BuildMsg.command = "BUILD";
                    BuildMsg.xmlString = Build;
                    motherbuilder.postMessage(BuildMsg);
                    bool result = true;
                    do
                    {
                        result = motherbuilder.postFile(Build, "../../../RepositoryStorage/", MsgReady.fileName);
                    } while (!result);
                }
            }
        }
        void ThreadProcess()                    // Receiver methods are called
        {
            
            while (true)
            {
                CommMessage rcv = motherbuilder.getMessage();
                rcv.show();
                Console.WriteLine("Requirement 3: The Communication Service shall support accessing build requests by Pool Processes from the mother Builder process, sending and receiving build requests, and sending and receiving files");
                if (rcv.command == "startchildbuilder")
                {
                    Console.WriteLine(rcv.arguments[0]);
                    ChildCall(Convert.ToInt32(rcv.arguments[0]));
                }
                else if (rcv.command == "StoreBuildRequests")
                {
                    foreach (string f in rcv.arguments)
                    {
                        BuildRequest.enQ(f);
                    }
                }
                else if (rcv.command == "KillProcess")
                {
                    foreach (int ChildPort in PortsChildBuilder.Values)
                    {
                        Console.WriteLine(ChildPort);
                        CommMessage MessageKill = new CommMessage(CommMessage.MessageType.connect);
                        MessageKill.author = "MotherBuilder";
                        MessageKill.command = "Kill";
                        MessageKill.from = this.from;
                        MessageKill.to = "http://localhost:" + ChildPort + "/IMessagePassingComm";
                        motherbuilder.postMessage(MessageKill);
                    }

                }
                else if (rcv.command == "READY")
                {
                    readyqueue.enQ(rcv);
                }
            }
        }

        public static void ChildCall(int count)                 // Builds requests and sends to child builder
        {
            List<string> buildrequests = new List<string>();
            List<string> Run = new List<string>();
            Console.WriteLine("Requirement 5: Shall provide a Process Pool component that creates a specified number of processes on command");

            Console.WriteLine("Requirement 7: Each Pool Process shall attempt to build each library, cited in a retrieved build request, logging warnings and errors");
            for (int i = 1; i <= count; ++i)
            {
                if (ProcessCreate(i))
                {
                    Console.WriteLine(" - success");
                }
                else
                {
                    Console.WriteLine(" - fail");
                }
            }
            foreach (KeyValuePair<string, int> elem in PortsChildBuilder)
            {

                Console.WriteLine("Key = {0}, Value = {1}", elem.Key, elem.Value);
            }
            Console.Write("\n  Press key to exit");
            Console.Write("\n  ");
            return;
        }

        
        public static bool ProcessCreate(int i)                // Creation of new processes
        {
            Console.WriteLine("Requirement 6: Pool Processes shall use message-passing communication to access messages from the mother Builder process");
            try
            {
            Process process = new Process();
            process.StartInfo.FileName = @"..\..\..\ChildBuilder\bin\Debug\ChildBuilder.exe";
            string ChildProcessNumber = i.ToString();
            string PortChildBuilder = (8083 + i).ToString();
            string PortMotherBuilder = "8083";
            string[] args = { ChildProcessNumber, PortMotherBuilder, PortChildBuilder };
            PortsChildBuilder.Add("ChildBuilder" + i.ToString(), 8083 + i);
            string commandline = String.Join(" ", args);
                process.StartInfo.Arguments = commandline;
                process.Start();
                ClientEnvironment.verbose = true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }

            return true;
        }
      
        static void Main(string[] args)
        {
            MotherBuilder s = new MotherBuilder();
            
            
        }
    }
}
