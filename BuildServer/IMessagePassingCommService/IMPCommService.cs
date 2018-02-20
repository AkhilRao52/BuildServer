////////////////////////////////////////////////////////////////////////////////////////////
//  IMPCommService.cs - Main communication package                                        //
//  ver 1.0                                                                               //
//  Language:      Visual C#  2017                                                        //
//  Platform:      Windows 10 (used Virtual Box on mac)                                   //
//  IDE used:      Microsoft Visual Studio 2017, Community Edition                        //                                                               
//  Application:   Build Server , FALL 2017                                               //
//  Author:        Butchi Venkata Akhil Rao,                                              //
//                 Syracuse University brao01@syr.edu                                     //
//  Reference:     Professor Jim Fawcett                                                  //
////////////////////////////////////////////////////////////////////////////////////////////

/*
Module Operations:
==================
It is resposible for interaction of Client with various functionalities of BuilServer

Build Process:
==============
Required files

- MainWindow.xaml


Maintenance History:
====================
ver 1.0

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;

namespace MessagePassingComm
{
  using Command = String;             // Command is key for message dispatching, e.g., Dictionary<Command, Func<bool>>
  using EndPoint = String;            // string is (ip address or machine name):(port number)
  using Argument = String;      
  using ErrorMessage = String;

  [ServiceContract(Namespace = "MessagePassingComm")]
  public interface IMessagePassingComm
  {
                                            /*----< support for message passing >--------------------------*/

    [OperationContract(IsOneWay = true)]
    void postMessage(CommMessage msg);

                                              // private to receiver so not an OperationContract
    CommMessage getMessage();

                                                 /*----< support for sending file in blocks >-------------------*/

    [OperationContract]
    bool openFileForWrite(string name,string path);

    [OperationContract]
    bool writeFileBlock(byte[] block);

    [OperationContract(IsOneWay = true)]
    void closeFile();
  }

  [DataContract]
  public class CommMessage
  {
    public enum MessageType
    {
      [EnumMember]
      connect,           // initial message sent on successfully connecting
      [EnumMember]
      request,           // request for action from receiver
      [EnumMember]
      reply,             // response to a request
      [EnumMember]
      closeSender,       // close down client
      [EnumMember]
      closeReceiver      // close down server for graceful termination
    }

                         /*----< constructor requires message type >--------------------*/

    public CommMessage(MessageType mt)
    {
      type = mt;
    }
                        /*----< data members - all serializable public properties >----*/

    [DataMember]
    public MessageType type { get; set; } = MessageType.connect;

    [DataMember]
    public string to { get; set; }

    [DataMember]
    public string from { get; set; }

    [DataMember]
    public string author { get; set; }
        [DataMember]
        public string fileName { get; set; }
        [DataMember]
        public string xmlString { get; set; }
        [DataMember]
        public Command command { get; set; }

    [DataMember]
    public List<Argument> arguments { get; set; } = new List<Argument>();

    [DataMember]
    public int threadId { get; set; } = Thread.CurrentThread.ManagedThreadId;

    [DataMember]
    public ErrorMessage errorMsg { get; set; } = "no error";

    public void show()
    {
      Console.Write("\n  CommMessage:");
      Console.Write("\n    MessageType : {0}", type.ToString());
      Console.Write("\n    to          : {0}", to);
      Console.Write("\n    from        : {0}", from);
      Console.Write("\n    author      : {0}", author);
      Console.Write("\n    command     : {0}", command);
       Console.Write("\n XML STRING: {0}", xmlString);
  Console.Write("\n File Name: {0}", fileName);
            Console.Write("\n    arguments   :");
            
      if (arguments.Count > 0)
        Console.Write("\n      ");
      foreach (string arg in arguments)
        Console.Write("{0} ", arg);
            Console.Write("\n    List of files   :",fileName);
            
             Console.Write("\n    ThreadId    : {0}", threadId);
      Console.Write("\n    errorMsg    : {0}\n", errorMsg);
    }

    public CommMessage clone()
    {
      CommMessage msg = new CommMessage(MessageType.request);
      msg.type = type;
      msg.to = to;
      msg.from = from;
      msg.author = author;
      msg.command = command;
      foreach (string arg in arguments)
        msg.arguments.Add(arg);
      return msg;
    }
  }
}
