﻿using System;
using System.Threading;

namespace Communication.LogNet
{
    #region Log EventArgs


    /// <summary>
    /// 带有日志消息的事件
    /// </summary>
    public class HslEventArgs : EventArgs
    {
        /// <summary>
        /// 消息信息
        /// </summary>
        public HslMessageItem HslMessage { get; set; }

    }

    /// <summary>
    /// 日志存储回调的异常信息
    /// </summary>
    public class LogNetException : Exception
    {
        /// <summary>
        /// 使用其他的异常信息来初始化日志异常
        /// </summary>
        /// <param name="innerException">异常信息</param>
        public LogNetException(Exception innerException) : base(innerException.Message, innerException)
        {

        }
    }

    #endregion

    #region MyRegion

    /// <summary>
    /// 日志文件的存储模式
    /// </summary>
    public enum LogSaveMode
    {
        /// <summary>
        /// 单个文件的存储模式
        /// </summary>
        SingleFile = 1,

        /// <summary>
        /// 根据文件的大小来存储，固定一个大小，不停的生成文件
        /// </summary>
        FileFixedSize,

        /// <summary>
        /// 根据时间来存储，可以设置年，季，月，日，小时等等
        /// </summary>
        Time
    }

    #endregion

    #region Log Output Format

    /// <summary>
    /// 日志文件输出模式
    /// </summary>
    public enum GenerateMode
    {
        /// <summary>
        /// 按每分钟生成日志文件
        /// </summary>
        ByEveryMinute = 1,

        /// <summary>
        /// 按每个小时生成日志文件
        /// </summary>
        ByEveryHour = 2,

        /// <summary>
        /// 按每天生成日志文件
        /// </summary>
        ByEveryDay = 3,

        /// <summary>
        /// 按每个周生成日志文件
        /// </summary>
        ByEveryWeek = 4,

        /// <summary>
        /// 按每个月生成日志文件
        /// </summary>
        ByEveryMonth = 5,

        /// <summary>
        /// 按每季度生成日志文件
        /// </summary>
        ByEverySeason = 6,

        /// <summary>
        /// 按每年生成日志文件
        /// </summary>
        ByEveryYear = 7,
    }

    #endregion

    #region Message Degree

    /// <summary>
    /// 记录消息的等级
    /// </summary>
    public enum HslMessageDegree
    {
        /// <summary>
        /// 一条消息都不记录
        /// </summary>
        None = 1,
        /// <summary>
        /// 记录致命等级及以上日志的消息
        /// </summary>
        FATAL = 2,
        /// <summary>
        /// 记录异常等级及以上日志的消息
        /// </summary>
        ERROR = 3,
        /// <summary>
        /// 记录警告等级及以上日志的消息
        /// </summary>
        WARN = 4,
        /// <summary>
        /// 记录信息等级及以上日志的消息
        /// </summary>
        INFO = 5,
        /// <summary>
        /// 记录调试等级及以上日志的信息
        /// </summary>
        DEBUG = 6
    }

    #endregion

    #region LogMessage

    /// <summary>
    /// 单个日志的记录信息
    /// </summary>
    public class HslMessageItem
    {
        private static long IdNumber = 0;

        /// <summary>
        /// 默认的无参构造器
        /// </summary>
        public HslMessageItem()
        {
            Id = Interlocked.Increment(ref IdNumber);
            Time = DateTime.Now;
        }

        /// <summary>
        /// 单个记录信息的标识ID，程序重新运行时清空
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// 消息的等级
        /// </summary>
        public HslMessageDegree Degree { get; set; } = HslMessageDegree.DEBUG;

        /// <summary>
        /// 线程ID
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// 消息文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 消息发生的事件
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 消息的关键字
        /// </summary>
        public string KeyWord { get; set; }

        /// <summary>
        /// 是否取消写入到文件中去，在事件BeforeSaveToFile触发的时候捕获即可设置。
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>字符串信息</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(KeyWord))
            {
                return $"[{Degree}] {Time.ToString("yyyy-MM-dd HH:mm:ss.fff")} Thread [{ThreadId.ToString("D3")}] {Text}";
            }
            else
            {
                return $"[{Degree}] {Time.ToString("yyyy-MM-dd HH:mm:ss.fff")} Thread [{ThreadId.ToString("D3")}] {KeyWord} : {Text}";
            }
        }

        /// <summary>
        /// 返回表示当前对象的字符串，剔除了关键字
        /// </summary>
        /// <returns>字符串信息</returns>
        public string ToStringWithoutKeyword()
        {
            return $"[{Degree}] {Time.ToString("yyyy-MM-dd HH:mm:ss.fff")} Thread [{ThreadId.ToString("D3")}] {Text}";
        }
    }

    #endregion
}
