using Prism.Logging;
using SimpleTCP;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mv.Modules.P99.Service
{
    public class CognexScanner
    {
        private readonly string ip;
        private readonly int port;
        private readonly ILoggerFacade logger;

        public int TimeOut { get; set; } = 1000;
        SimpleTcpClient tcpClient = new SimpleTcpClient();
        public CognexScanner(string ip, int port, ILoggerFacade logger)
        {
            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentException("message", nameof(ip));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.ip = ip;
            this.port = port;
            this.logger = logger;
            var unused = CheckConnectedAsync();
        }
        public bool IsConnected => !(tcpClient.TcpClient == null || !tcpClient.TcpClient.Connected);
        public async Task<bool> CheckConnectedAsync()
        {

            if (tcpClient.TcpClient.IsOnline())
            {
                return true;
            }
            return await Task.Run(() =>
            {
                try
                {

                    tcpClient.Connect(ip, port);
                    return true;
                }
                catch (Exception e)
                {
                    logger.Log($"连接扫码枪{ip}:{port},{e.Message}", Category.Exception, Priority.None);
                    return false;
                }
            }).ConfigureAwait(false);
        }

        public (bool, string) GetCodeAsync()
        {
            var result = RequestAsync("+");
            return result;
        }

        private (bool, string) RequestAsync(string cmd)
        {
            (bool, string) result = (false, "");
            try
            {
                tcpClient.TimeOut = TimeSpan.FromMilliseconds(TimeOut);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (!tcpClient.TcpClient.IsOnline())
                {
                    try
                    {
                        tcpClient.Connect(ip, port);
                    }
                    catch (Exception e)
                    {
                        logger.Log($"连接扫码枪{ip}:{port},{e.Message}", Category.Exception, Priority.None);
                        result.Item2 = "连接扫码枪失败";
                        return result;
                    }
                }
                logger.Log($"检查扫码枪连接{stopwatch.ElapsedMilliseconds}ms", Category.Debug, Priority.None);
                var start = stopwatch.ElapsedMilliseconds;
                var message = tcpClient.WriteAndGetReply(cmd);
                if (message != null)
                {
                    result.Item1 = true;
                    result.Item2 = message.MessageString.Trim('\r', '\n');
                }
                else
                {
                    result.Item1 = false;
                    result.Item2 = "扫码超时";
                }
                logger.Log($"命令返回时间{stopwatch.ElapsedMilliseconds}ms", Category.Debug, Priority.None);
            }
            catch (Exception EX)
            {
                result.Item2 = EX.Message;
            }
            return result;
        }
    }
}