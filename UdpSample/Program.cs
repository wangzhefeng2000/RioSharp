﻿using RioSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;

namespace UdpSample
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            var sendPool = new RioFixedBufferPool(10, 256);
            var recivePool = new RioFixedBufferPool(10, 256);
            var pool = new RioConnectionlessSocketPool(sendPool, recivePool, ADDRESS_FAMILIES.AF_INET, SOCKET_TYPE.SOCK_DGRAM, PROTOCOL.IPPROTO_UDP);
            RioConnectionlessSocket sock = null;

            var multicastAdress = IPAddress.Parse("238.0.3.15");

            try
            {
                sock = pool.Bind(new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), 3000));
            }
            catch (Exception)
            {
                sock = pool.Bind();
            }

            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.Supports(NetworkInterfaceComponent.IPv4))
                .Select(n => new { n.GetIPProperties().GetIPv4Properties().Index });

            sock.JoinMulticastGroup(multicastAdress, 0);

            RioSegmentReader r = new RioSegmentReader(sock);
            r.OnIncommingSegment = segment => Console.WriteLine(Encoding.ASCII.GetString(segment.DataPointer, segment.CurrentContentLength));
            r.Start();
            
            while (true)
            {
                sock.Send(Encoding.ASCII.GetBytes("Hello, i'm process " + Process.GetCurrentProcess().Id), new IPEndPoint(multicastAdress, 3000));
                Thread.Sleep(1000);
            }
        }
    }
}
