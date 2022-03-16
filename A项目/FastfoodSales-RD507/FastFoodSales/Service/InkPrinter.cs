using System.Threading.Tasks;
using System;
using System.Text;
using System.Net.Sockets;
using MaterialDesignThemes.Wpf;


namespace DAQ.Service
{
    public interface IInkPrinter
    {
        Task<bool> WritePrinterCodeAsync(string text);
        Task<bool> WritePrinterTextAsync(string text);
        Task<byte[]> ReadNfcCodeAsync();
    }
    public class InkPrinter : IInkPrinter
    {
        private readonly IConfigureFile configureFile;
        private readonly Config _config;

        public InkPrinter(IConfigureFile configureFile)
        {
            this.configureFile = configureFile;
            _config = configureFile.Load().GetValue<Config>(nameof(Config)) ?? new Config();
        }

        public async Task<bool> WritePrinterTextAsync(string text)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    string cmd = $"STM:0:2::1{text}:";
                    byte[] bs = new byte[cmd.Length + 2];
                    byte[] br = new byte[10];
                    await client.ConnectAsync(_config.PrinterIpAddress, _config.PrinterPort).ConfigureAwait(false);
                    var sour = Encoding.ASCII.GetBytes(cmd);
                    Buffer.BlockCopy(sour, 0, bs, 1, sour.Length);
                    bs[0] = 0x02;
                    bs[bs.Length - 1] = 0x03;
                    client.Client.Send(bs);
                    client.Client.Receive(br);
                    if (br[0] == 0x06)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> WritePrinterCodeAsync(string text)
        {
            try
            {
                using (TcpClient client = new TcpClient
                {
                    ReceiveTimeout = 500,

                    SendTimeout = 200
                })
                {
                    await client.ConnectAsync(_config.PrinterIpAddress, _config.PrinterPort).ConfigureAwait(false);
                    string cmd = $"S2M:2:1:::::1:0:1{text}:";
                    byte[] bs = new byte[cmd.Length + 2];
                    byte[] br = new byte[10];
                    var sour = Encoding.ASCII.GetBytes(cmd);
                    Buffer.BlockCopy(sour, 0, bs, 1, sour.Length);
                    bs[0] = 0x02;
                    bs[bs.Length - 1] = 0x03;
                    client.Client.Send(bs);
                    client.Client.Receive(br);
                    if (br[0] == 0x06)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
                // throw;
            }
        }

        public async Task<byte[]> ReadNfcCodeAsync()
        {
            try
            {
                using (TcpClient client = new TcpClient
                {
                    ReceiveTimeout = 1000,

                    SendTimeout = 1000
                })
                {
                    await client.ConnectAsync(_config.NFCIpAddress, _config.NFCPort).ConfigureAwait(false);
                    byte[] bw = new byte[20] {0x44,0x30,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x31,0x45};
                    byte[]br=new byte[20];
                    client.Client.Send(bw);
                    int ret = 0;
                    int recvsize = 0;
                    int dataSize = 20;
                    byte[] datas = new byte[dataSize];
                    while (recvsize < dataSize)
                    {
                        ret= client.Client.Receive(datas, recvsize, dataSize - recvsize, SocketFlags.None);
                        if (ret <= 0)
                        {
                            datas = null;
                            break;
                        }
                        recvsize += ret;
                        if (datas[recvsize - 1] == 0)
                        {
                            datas = null;
                            break;
                        }
                        if (recvsize >= dataSize)
                        {
                            break;
                        }
                    }
                    return datas;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }



    }
}
