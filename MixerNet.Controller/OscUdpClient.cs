using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LucHeart.CoreOSC;

namespace MixerNet.Controller
{
    public class OscUdpClient : IOscClient
    {
        private readonly UdpClient client;
        private readonly IPEndPoint remote;

        public OscUdpClient(IPEndPoint remote, CancellationToken? token = null)
        {
            client = new UdpClient();
            this.remote = remote;
        }

        public event EventHandler<IOscPacket>? PacketReceived;

        public void Open(CancellationToken? token = null)
        {
            client.Connect(remote);
            Task.Run(async () =>
            {
                while (true)
                    //while ((bool)!token?.IsCancellationRequested)
                {
                    var data = await client.ReceiveAsync();
                    PacketReceived?.Invoke(this, Helpers.ParseOsc(data.Buffer));
                }
            });
        }

        public void Send(IOscPacket data)
        {
            client.Send(data.GetBytes(), data.GetBytes().Length);
        }

        public async Task SendAsync(IOscPacket data)
        {
            await client.SendAsync(data.GetBytes(), data.GetBytes().Length);
        }

        public void Close()
        {
        }
    }
}