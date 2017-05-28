// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines.Networking.Windows.RIO;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions;

namespace System.IO.Pipelines.Samples.Http
{
    public class RioHttpServer : ITransportFactory
    {
        private readonly Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory _pipeFactory = new Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.PipeFactory();

        public ITransport Create(IEndPointInformation endPointInformation, IConnectionHandler handler)
        {
            return new RioTransport(endPointInformation, handler, _pipeFactory);
        }
    }
}
