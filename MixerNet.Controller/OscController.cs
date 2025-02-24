using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LucHeart.CoreOSC;

namespace MixerNet.Controller
{
    public class OscController
    {
        private readonly OscMultiplexer mixer;

        private OscBundle info;

        public string Remote { get; private set; }

        internal OscController(OscMultiplexer multiplexer)
        {
            mixer = multiplexer;
        }

        public OscController(SerialPort port) : this(new OscMultiplexer(new OscSlipClient(port)))
        {
            Remote = port.PortName;
        }

        public OscController(IPEndPoint remote) : this(new OscMultiplexer(new OscUdpClient(remote)))
        {
            Remote = remote.ToString();
        }

        public List<string> Channels { get; private set; }
        public List<string> Buses { get; private set; }

        public async Task Setup()
        {
            info = await mixer.CommandAsync("/info") as OscBundle ?? throw new Exception();

            var channelCount = info.Find("/info/channels")?.As<int>() ?? 0;
            var busCount = info.Find("/info/buses")?.As<int>() ?? 0;

            Channels = Enumerable.Range(0, channelCount)
                .Select(x => info.Find($"/ch/{x}/config/name")?.As<string>() ?? "").ToList();
            Buses = Enumerable.Range(0, busCount).Select(x => info.Find($"/bus/{x}/config/name")?.As<string>() ?? "")
                .ToList();
        }

        private int ParseChannel(string name)
        {
            return Channels.IndexOf(name);
        }

        private int ParseBus(string name)
        {
            return Buses.IndexOf(name);
        }


        public async Task<float> GetGainAsync(int input, int output)
        {
            return (await mixer.CommandAsync($"/ch/{input}/mix/{output}/level")).As<float>();
        }

        public async Task<float> GetGainAsync(string input, string output)
        {
            return await GetGainAsync(ParseChannel(input), ParseBus(output));
        }

        public async Task SetGainAsync(int input, int output, float gain)
        {
            await mixer.CommandAsync($"/ch/{input}/mix/{output}/level", gain);
        }

        public async Task SetGainAsync(string input, string output, float gain)
        {
            await SetGainAsync(ParseChannel(input), ParseBus(output), gain);
        }

        public async Task SetMutedAsync(int input, int output, bool muted)
        {
            await mixer.CommandAsync($"/ch/{input}/mix/{output}/muted", muted);
        }

        public async Task SetMutedAsync(string input, string output, bool muted)
        {
            await SetMutedAsync(ParseChannel(input), ParseBus(output), muted);
        }

        public async Task<bool> IsMutedAsync(int input, int output)
        {
            return (await mixer.CommandAsync($"/ch/{input}/mix/{output}/level")).As<float>() == 0.0f;
        }

        public async Task<bool> IsMutedAsync(string input, string output)
        {
            return await IsMutedAsync(ParseChannel(input), ParseBus(output));
        }

        public async Task<float> GetInputMultiplier(int input)
        {
            return (await mixer.CommandAsync($"/ch/{input}/multiplier")).As<float>();
        }

        public async Task<float> GetInputMultiplier(string input)
        {
            return await GetInputMultiplier(ParseChannel(input));
        }

        public async Task<float> GetOutputMultiplier(int output)
        {
            return (await mixer.CommandAsync($"/ch/{output}/multiplier")).As<float>();
        }

        public async Task<float> GetOutputMultiplier(string output)
        {
            return await GetOutputMultiplier(ParseBus(output));
        }

        public async Task SetInputMultiplier(int input, float multiplier)
        {
            await mixer.CommandAsync($"/ch/{input}/multiplier", multiplier);
        }

        public async Task SetInputMultiplier(string input, float multiplier)
        {
            await SetInputMultiplier(ParseChannel(input), multiplier);
        }

        public async Task SetOutputMultiplier(int output, float multiplier)
        {
            await mixer.CommandAsync($"/bus/{output}/multiplier", multiplier);
        }

        public async Task SetOutputMultiplier(string output, float multiplier)
        {
            await SetOutputMultiplier(ParseBus(output), multiplier);
        }

        public async Task<Tuple<float, float, float>> GetInputLevels(int input)
        {
            var bundle = await mixer.CommandAsync($"/ch/{input}/levels") as OscBundle ?? throw new Exception();

            return new Tuple<float, float, float>(bundle.Messages[0].As<float>(), bundle.Messages[1].As<float>(),
                bundle.Messages[2].As<float>());
        }

        public async Task<Tuple<float, float, float>> GetOutputLevels(int output)
        {
            var bundle = await mixer.CommandAsync($"/bus/{output}/levels") as OscBundle ?? throw new Exception();

            return new Tuple<float, float, float>(bundle.Messages[0].As<float>(), bundle.Messages[1].As<float>(),
                bundle.Messages[2].As<float>());
        }
    }
}