// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace System.IO.Pipelines.Samples
{
    public class RawSocketHttpServerSample : RawHttpServerSampleBase
    {
        public Socket listener { get; private set; }
        
        protected override Task Start(IPEndPoint ipEndpoint)
        {
            listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndpoint);
            return Task.CompletedTask;
        }

        protected override Task Stop()
        {
            listener.Dispose();
            return Task.CompletedTask;
        }
    }
}
