using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Windows.Media.Control;

namespace PotatOSC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            UdpClient client = new UdpClient();
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 9000;
            IPEndPoint endPoint = new IPEndPoint(ip, port);

            var smtc = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var session = smtc.GetCurrentSession();

            if (session != null)
            {
                session.MediaPropertiesChanged += async (sender, args) =>
                {
                    var mediaProperties = await sender.TryGetMediaPropertiesAsync();
                    await SendOSCMessage(client, endPoint, mediaProperties);
                };

                var mediaProperties = await session.TryGetMediaPropertiesAsync();
                await SendOSCMessage(client, endPoint, mediaProperties);
            }

            Console.WriteLine("Press any key on this console window to quit...");
            Console.ReadLine();
        }

        private static async Task SendOSCMessage(UdpClient client, IPEndPoint endPoint, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            string address = "/chatbox/input";
            string typeTag = ",s";
            string argument = $"Now playing: {mediaProperties.Title} by {mediaProperties.Artist}";

            using (var ms = new MemoryStream())
            {
                WritePadded(ms, System.Text.Encoding.ASCII.GetBytes(address));
                WritePadded(ms, System.Text.Encoding.ASCII.GetBytes(typeTag));
                WritePadded(ms, System.Text.Encoding.ASCII.GetBytes(argument));

                await client.SendAsync(ms.ToArray(), (int)ms.Length, endPoint);
            }
        }

        private static void WritePadded(MemoryStream ms, byte[] input)
        {
            ms.Write(input, 0, input.Length);

            int padding = 4 - (input.Length % 4);
            if (padding == 4) return;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(padding);
            Array.Clear(buffer, 0, padding);
            ms.Write(buffer, 0, padding);
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}



//haha now it's 69 lines long