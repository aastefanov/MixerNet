using MixerNet.Controller;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;

var serialPort = args.Length > 1 ? args[1] : "COM3";
var host = args.Length > 2 ? args[2] : "127.0.0.1:10024";

var osc = new OscMultiplexer(new OscSlipClient(new SerialPort(serialPort)));

var udp = new UdpClient(IPEndPoint.Parse(host));

while (true)
{
    var received = await udp.ReceiveAsync();
    var response = await osc.CommandAsync(Helpers.ParseOsc(received.Buffer));
    await udp.SendAsync(response.GetBytes(), received.RemoteEndPoint);
}
