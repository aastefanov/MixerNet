using System;
using System.Threading;
using System.Threading.Tasks;
using LucHeart.CoreOSC;

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