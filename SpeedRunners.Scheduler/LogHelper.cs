using System;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SpeedRunners.Scheduler
{
    public class LogHelper
    {
        /// <summary>
        /// 日志记录
        /// </summary>
        public void Log(string content)
        {
            var filepath = Environment.CurrentDirectory + "\\";
            var filename = "log";
            Log(filepath, filename, content);
        }

        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="filepath">日志路径</param>
        /// <param name="filename">日志名称</param>
        /// <param name="content">日志内容</param>
        public void Log(string filepath, string filename, string content)
        {
            if (!File.Exists(filepath + filename + ".txt"))
            {
                FileStream fs1 = new FileStream(filepath + filename + ".txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1, Encoding.GetEncoding("gb2312"));
                sw.WriteLine(content);//开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(filepath + filename + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sr = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                sr.WriteLine(content);//开始写入值
                sr.Close();
                fs.Close();
            }
        }
        /// <summary>
        /// 日志组件实例
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// 请在要使用的类中调用并保存一个静态LogHelper的实例
        /// </summary>
        /// <returns>LogHelper实例</returns>
        public static LogHelper GetCurrentClassLogHelper()
        {
            return new LogHelper();
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private LogHelper()
        {
            _logger = LogManager.GetCurrentClassLogger();

        }

        #region Info
        /// <summary>
        /// 正常记录
        /// </summary>
        /// <param name="msg">日志内容</param>
        /// <param name="err">异常</param>
        public void Info(string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                var declaringType = st.GetFrame(1).GetMethod().DeclaringType;
                if (declaringType != null)
                    moduleName = declaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {
                // ignored
            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Info, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 正常记录
        /// </summary>
        /// <param name="msg">日志内容</param>
        /// <param name="err">异常</param>
        public void Info<T>(T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                var declaringType = st.GetFrame(1).GetMethod().DeclaringType;
                if (declaringType != null)
                    moduleName = declaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {
                // ignored
            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Info, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 正常记录
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Info(string title, string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Info, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 正常记录
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Info<T>(string title, T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Info, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }
        #endregion

        #region Warn
        /// <summary>
        /// 程序可能有问题、业务可能有问题
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Warn(string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Warn, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 程序可能有问题、业务可能有问题
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Warn<T>(T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Warn, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 程序可能有问题、业务可能有问题
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Warn(string title, string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Warn, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 程序可能有问题、业务可能有问题
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Warn<T>(string title, T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Warn, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }
        #endregion

        #region Error
        /// <summary>
        /// 代码错误、业务错误
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Error(string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 代码错误、业务错误
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Error<T>(T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Error, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }



        /// <summary>
        /// 代码错误、业务错误
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Error(string title, string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Error, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 代码错误、业务错误
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Error<T>(string title, T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Error, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }
        #endregion

        #region Fatal
        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Fatal(string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Fatal, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Fatal<T>(T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Fatal, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }


        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Fatal(string title, string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Fatal, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Fatal<T>(string title, T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Fatal, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }
        #endregion

        #region Debug

        /// <summary>
        /// 开发使用
        /// </summary>
        /// <param name="msg">日志内容</param> 
        public void Debug(string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Debug, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 开发使用
        /// </summary>
        /// <param name="msg">日志内容</param> 
        public void Debug<T>(T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Debug, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 开发使用
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Debug(string title, string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Debug, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }

        /// <summary>
        /// 开发使用
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Debug<T>(string title, T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }
            LogEventInfo ei = new LogEventInfo(LogLevel.Debug, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }
        #endregion

        #region Trace

        /// <summary>
        /// 异常调试
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Trace(string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }

            LogEventInfo ei = new LogEventInfo(LogLevel.Trace, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }


        /// <summary>
        /// 异常调试
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void Trace<T>(T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {
            }

            LogEventInfo ei = new LogEventInfo(LogLevel.Trace, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Exception = err;
            _logger.Log(ei);
        }


        /// <summary>
        /// 异常调试
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Trace(string title, string msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {

            }

            LogEventInfo ei = new LogEventInfo(LogLevel.Trace, _logger.Name, msg);
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }


        /// <summary>
        /// 异常调试
        /// </summary>
        /// <param name="title">日志标题</param>
        /// <param name="msg">日志内容</param>
        public void Trace<T>(string title, T msg, Exception err = default(Exception))
        {
            string moduleName = null;
            try
            {
                StackTrace st = new StackTrace(true);
                moduleName = st.GetFrame(1).GetMethod().DeclaringType.FullName + '.' + st.GetFrame(1).GetMethod().Name;
            }
            catch
            {
            }

            LogEventInfo ei = new LogEventInfo(LogLevel.Trace, _logger.Name, JsonConvert.SerializeObject(msg));
            ei.Properties["ModuleName"] = moduleName;
            ei.Properties["Title"] = title;
            ei.Exception = err;
            _logger.Log(ei);
        }
        #endregion

    }
}
