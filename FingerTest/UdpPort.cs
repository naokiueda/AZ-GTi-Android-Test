using System;
using System.Net;
using System.Net.Sockets;

namespace SkyWatcherMotorMoveApp
{
    public class UdpPort
    {
        public UdpPort()
        {

        }

        ~UdpPort()
        {

        }

        public string sendRecvMsg(string sendmsg)
        {

            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.4.1"), 11880); // endpoint where server is listening
            try
            {
                client.Connect(ep);
            }catch(SocketException se)
            {
                System.Console.WriteLine(se.Message);
                return "";
            }

            int retry = 10;
            string rcvMsg = "";
            while (retry > 0)
            {
                try
                {
                    // send data
                    byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendmsg + "\r");
                    client.Send(sendBytes, sendBytes.Length);

                    // then receive data
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

                    var receivedData = client.Receive(ref ep);

                    rcvMsg = System.Text.Encoding.ASCII.GetString(receivedData);
                    if (rcvMsg.EndsWith("\r"))
                    {
                        if (rcvMsg.StartsWith("="))
                        {
                            rcvMsg = rcvMsg.Substring(0, rcvMsg.Length - 1);
                            break;
                        }
                    }
                }catch(SocketException se)
                {
                    //TImeout?
                    string error = se.Message.ToString();
                    System.Console.WriteLine(error);
                }
                System.Random r = new System.Random();
                int rnd = r.Next(10);
                System.Threading.Thread.Sleep(133 + rnd);
                retry--;
            }
            return rcvMsg;
        }
    }
}