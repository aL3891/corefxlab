// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;
using System.IO.Pipelines.Networking.Windows.RIO;
using System.Threading;
using System.Net;
using System.IO.Pipelines.Samples.Rio;

namespace System.IO.Pipelines.Samples.Http
{
    internal class RioTransport : ITransport
    {
        private IEndPointInformation endPointInformation;
        private IConnectionHandler handler;
        private readonly Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory _PipeFactory;
        private RioTcpServer _rioTcpServer;
        bool running = true;

        public RioTransport(IEndPointInformation endPointInformation, IConnectionHandler handler, Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory _pipeFactory)
        {
            this.endPointInformation = endPointInformation;
            this.handler = handler;
            _PipeFactory = _pipeFactory;
        }

        private static void GetIp(string url, out IPAddress ip, out int port)
        {
            ip = null;

            var address = ServerAddress.FromUrl(url);
            switch (address.Host)
            {
                case "localhost":
                    ip = IPAddress.Loopback;
                    break;
                case "*":
                    ip = IPAddress.Any;
                    break;
                default:
                    break;
            }
            ip = ip ?? IPAddress.Parse(address.Host);
            port = address.Port;
        }

        private void StartAccepting(IPAddress ip, int port)
        {
            Thread.CurrentThread.Name = "RIO Accept Thread";
            var addressBytes = ip.GetAddressBytes();

            try
            {
                _rioTcpServer = new RioTcpServer((ushort)port, addressBytes[0], addressBytes[1], addressBytes[2], addressBytes[3]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            while (running)
            {
                try
                {
                    var connection = _rioTcpServer.Accept();
                    handler.OnConnection(new RioConnectionContext(connection, _PipeFactory));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }
            }
        }

        public Task BindAsync()
        {
            return Task.Factory.StartNew(() => StartAccepting(endPointInformation.IPEndPoint.Address, endPointInformation.IPEndPoint.Port), TaskCreationOptions.LongRunning);
        }

        public Task StopAsync()
        {
            running = false;
            return Task.CompletedTask;
        }

        public Task UnbindAsync()
        {
            _rioTcpServer?.Stop();
            _rioTcpServer = null;
            return Task.CompletedTask;
        }
    }
}
