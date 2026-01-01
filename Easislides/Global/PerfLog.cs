using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Easislides
{
    internal static class PerfLog
    {
        private static readonly object Sync = new object();
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "perf_gfLyrics.log");
        private static int _enabled = -1;

        internal static bool Enabled
        {
            get
            {
                if (_enabled < 0)
                {
                    _enabled = Environment.GetEnvironmentVariable("EASISLIDES_PERF") == "1" ? 1 : 0;
                }
                return _enabled == 1;
            }
        }

        internal static IDisposable Scope(string name, string meta)
        {
            if (!Enabled)
            {
                return null;
            }
            return new PerfScope(name, meta);
        }

        private sealed class PerfScope : IDisposable
        {
            private readonly string _name;
            private readonly string _meta;
            private readonly Stopwatch _sw;

            internal PerfScope(string name, string meta)
            {
                _name = name;
                _meta = meta;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                Write(_name, _sw.ElapsedMilliseconds, _meta);
            }
        }

        private static void Write(string name, long elapsedMs, string meta)
        {
            string line = $"{DateTime.UtcNow:O}	{name}	{elapsedMs}ms";
            if (!string.IsNullOrEmpty(meta))
            {
                line += "	" + meta;
            }
            lock (Sync)
            {
                File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
