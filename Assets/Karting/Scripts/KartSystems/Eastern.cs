using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Eastern
{
    private IPAddress HostAddress { get; set; }
    private int HostPort { get; set; }
    private Socket Socket { get; set; }

    private SocketAsyncEventArgs SocketEventArg { get; set; }
    private ManualResetEvent ClientDoneResetEvent { get; set; }

    public Eastern()
    {
        HostAddress = null;
        HostPort = 0;
        Socket = null;

        SocketEventArg = new SocketAsyncEventArgs();
        ClientDoneResetEvent = new ManualResetEvent(false);
    }

    public void Connect(string hostname, int port)
    {
        HostAddress = IPAddress.Parse(hostname);
        HostPort = port;

        //Socket = new Socket(HostAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        SocketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnCompleted);
        SocketEventArg.RemoteEndPoint = new IPEndPoint(HostAddress, HostPort);
        SocketEventArg.UserToken = Socket;

        byte[] buffer = new byte[1024];
        SocketEventArg.SetBuffer(buffer, 0, buffer.Length);

        Socket.ConnectAsync(SocketEventArg);
        //ClientDoneResetEvent.WaitOne();
    }

    private void OnCompleted(object sender, SocketAsyncEventArgs eventArgs)
    {
        switch (eventArgs.LastOperation)
        {
            case SocketAsyncOperation.Connect:
                ProcessConnect(eventArgs);
                break;
            case SocketAsyncOperation.Receive:
                ProcessReceive(eventArgs);
                break;
            case SocketAsyncOperation.Send:
                ProcessSend(eventArgs);
                break;
            default:
                throw new Exception("Invalid operation completed.");
        }
    }

    private void ProcessConnect(SocketAsyncEventArgs eventArgs)
    {
        if (eventArgs.SocketError == SocketError.Success)
        {
            // Send 'Hello World' to the server
            //byte[] buffer = Encoding.UTF8.GetBytes("Hello World");
            //eventArgs.SetBuffer(buffer, 0, buffer.Length);
            Socket socket = eventArgs.UserToken as Socket;
            bool willRaiseEvent = socket.ReceiveAsync(eventArgs);

            if (!willRaiseEvent)
            {
                ProcessReceive(eventArgs);
            }
        }
        else
        {
            throw new SocketException((int)eventArgs.SocketError);
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs eventArgs)
    {
        if (eventArgs.SocketError == SocketError.Success)
        {
            byte[] foo = eventArgs.Buffer.Take(2).ToArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(foo);
            }
            //Console.WriteLine("Received from server: {0}", Encoding.UTF8.GetString(eventArgs.Buffer, 0, eventArgs.BytesTransferred));
            Console.WriteLine("{0}", BitConverter.ToInt16(foo, 0));

            // Data has now been sent and received from the server. Disconnect from the server
            Socket socket = eventArgs.UserToken as Socket;
            socket.Shutdown(SocketShutdown.Send);
            socket.Close();
            //ClientDoneResetEvent.Set();
        }
        else
        {
            
            throw new SocketException((int)eventArgs.SocketError);
        }
    }

    private void ProcessSend(SocketAsyncEventArgs eventArgs)
    {
        if (eventArgs.SocketError == SocketError.Success)
        {
            Console.WriteLine("Sent 'Hello World' to the server");

            //Read data sent from the server
            Socket socket = eventArgs.UserToken as Socket;
            bool willRaiseEvent = socket.ReceiveAsync(eventArgs);

            if (!willRaiseEvent)
            {
                ProcessReceive(eventArgs);
            }
        }
        else
        {
            throw new SocketException((int)eventArgs.SocketError);
        }
    }
}