using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingApplication.Model.Utilities
{
    public class CustomTraceListener : TraceListener
    {
        readonly StreamWriter _streamWriter;

        public CustomTraceListener(string filename = "ServerLog.txt")
        {
            FileStream fileStream = new FileStream(filename, FileMode.Append);
            _streamWriter = new StreamWriter(fileStream) { AutoFlush = true };
        }

        public override void Write(string message)
        {
            _streamWriter.WriteLine(DateTime.Now.ToString(CultureInfo.CurrentCulture) + ": " + message);
        }

        public override void WriteLine(string message)
        {
            _streamWriter.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + message);
            Console.WriteLine(message);
        }

        public static void AddCustomListener()
        {
            if (Trace.Listeners.Cast<object>().Any(obj => obj.GetType() == typeof(CustomTraceListener)))
                return;
            Trace.Listeners.Add(new CustomTraceListener());
        }
    }
}
