using log4net;

namespace Common
{

    public static class Log
    {
        private static ILog log;

        public static void Init(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public static void Info(object message)
        {
            log.Info(message);
        }

        public static void InfoFormat(string format, object arg0)
        {
            log.InfoFormat(format, arg0);
        }

        public static void InfoFormat(string format, object arg0, object arg1)
        {
            log.InfoFormat(format, arg0, arg1);
        }

        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            log.InfoFormat(format, arg0, arg1, arg2);
        }

        public static void InfoFormat(string format,params object[] args)
        {
            log.InfoFormat(format, args);
        }


        public static void Warning(object message)
        {
            log.Warn(message);
        }

        public static void WarningFormat(string format, object arg0)
        {
            log.WarnFormat(format, arg0);
        }

        public static void WarningFormat(string format, object arg0, object arg1)
        {
            log.WarnFormat(format, arg0, arg1);
        }

        public static void WarningFormat(string format, object arg0, object arg1, object arg2)
        {
            log.WarnFormat(format, arg0, arg1, arg2);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public static void Error(object message)
        {
            log.Error(message);
        }

        public static void ErrorFormat(string format, object arg0)
        {
            log.ErrorFormat(format, arg0);
        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            log.ErrorFormat(format, arg0, arg1);
        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            log.ErrorFormat(format, arg0, arg1, arg2);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public static void Fatal(object message)
        {
            log.Fatal(message);
        }

        public static void FatalFormat(string format, object arg0)
        {
            log.FatalFormat(format, arg0);
        }

        public static void FatalFormat(string format, object arg0, object arg1)
        {
            log.FatalFormat(format, arg0, arg1);
        }

        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            log.FatalFormat(format, arg0, arg1, arg2);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
    }
}
