using LucHeart.CoreOSC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixerNet.Controller
{
    public interface IOscClient
    {
        event EventHandler<IOscPacket> PacketReceived;
        void Send(IOscPacket data);
        Task SendAsync(IOscPacket data);

        void Open(CancellationToken? token = null);
    }
}
