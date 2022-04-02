using System;
using System.Net.Sockets;
using System.Linq;
using System.Net;
using Prism.Logging;
using Mv.Core.Interfaces;
using Mv.Modules.P99.Service;

namespace Mv.Modules.P99
{

    public static class ByteHelper
    {
        public static byte[] AppendXor(this byte[] data)
        {
            var r = GetXor(data);
            var bs = new byte[data.Length + 1];
            Buffer.BlockCopy(data, 0, bs, 0, data.Length);
            bs[bs.Length - 1] = r;
            return bs;
        }
        /// <summary>
        /// 异或校验
        /// </summary>
        /// <param name="data">需要校验的数据包</param>
        /// <returns></returns>
        private static byte GetXor(byte[] data)
        {
            byte CheckCode = 0;
            int len = data.Length;
            for (int i = 0; i < len; i++)
            {
                CheckCode ^= data[i];
            }
            return CheckCode;
        }
    }
    public class OPTLight : IOPTLight
    {
        private readonly ILoggerFacade logger;
        TcpClient client = new TcpClient();
        P99Config config = new P99Config();
        public OPTLight(ILoggerFacade logger, IConfigureFile configureFile)
        {
            client.ReceiveTimeout = 1000;
            client.SendTimeout = 1000;
            this.logger = logger;
            config = configureFile.GetValue<P99Config>(nameof(P99Config))??new P99Config();
        }
        public short GetCurrent(int index)
        {
            try
            {
                if(!client.IsOnline())
                {
                    client.Connect(IPAddress.Parse(config.UvLightIp),config.UvLightPort);
                }
                var recv = new byte[6];
                var bs = new byte[] { 0xFF, 0x14, 0x01, 0x00, 0x01 };
                bs[2] =(byte) index;
                var m=bs.AppendXor();
                var stream = client.GetStream();
                stream.Write(m);
                int cnt = stream.Read(recv, 0, recv.Length);
                if (cnt == 6)
                {
                    if (recv[0] == 0xff && recv[1] == 0x14)
                        return (short)((recv[3] * 256) + recv[4]);
                    else
                        return 0;
                }
                else
                {
                    return (-2);
                }
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message, Category.Debug, Priority.High);
                return -1;
            }
        }


    }
}
