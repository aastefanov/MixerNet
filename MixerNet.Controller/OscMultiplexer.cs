using LucHeart.CoreOSC;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixerNet.Controller
{
    internal class OscMultiplexer
    {
        private IOscClient client;

        private Queue<Tuple<IOscPacket, AsyncManualResetEvent>> commands = new Queue<Tuple<IOscPacket, AsyncManualResetEvent>>();
        private Queue<IOscPacket> responses = new Queue<IOscPacket>();

        private AsyncLock queueLock = new AsyncLock();

        public OscMultiplexer(IOscClient client)
        {
            this.client = client;

            client.PacketReceived += async (s, e) => await HandleResponseAsync(s, e);

            client.Open();
        }

        private async Task HandleResponseAsync(object sender, IOscPacket response)
        {
            using (await queueLock.LockAsync())
            {
                var command = commands.Dequeue();

                //Console.WriteLine("Cmd: {0}, Resp: {1}", command.Item1)

                responses.Enqueue(response);
                command.Item2.Set();
            }
        }

        public async Task<IOscPacket> CommandAsync(string address, params object?[] args)
        {
            return await SendCommandAsync(new OscMessage(address, args));
        }


        public async Task<IOscPacket> SendCommandAsync(IOscPacket command)
        {
            var @event = new AsyncManualResetEvent(false);
            using (await queueLock.LockAsync())
            {
                commands.Enqueue(new Tuple<IOscPacket, AsyncManualResetEvent>(command, @event));
                client.Send(command);
            }
            await @event.WaitAsync();

            using (await queueLock.LockAsync())
                return responses.Dequeue();
        }
    }
}
