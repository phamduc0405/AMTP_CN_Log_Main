using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACO2_App._0.INIT
{
    public class LogTxt
    {
        #region User define
        /// <summary>
        /// Type of the log
        /// </summary>
        public enum Type
        {
            Connect = 0,
            Exception,
            Status,
            PCSignalMess,
            PLCMess,
            Alarm,
            UI,
            FlowRun,
        }

        /// <summary>
        /// 
        /// </summary>
        public struct Data
        {
            public DateTime Time { get; private set; }
            public string Content { get; private set; }
            public string Unit { get; private set; }
            public string Eqp { get; private set; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="content"></param>
            public static Data Set(string content, string eqp, string unit)
            {
                var data = new Data()
                {
                    Time = DateTime.Now,
                    Content = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff},{1}\r\n", DateTime.Now, content),
                    Unit = unit,
                    Eqp = eqp
                };
                return data;
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Delegate for log update events.
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="content">Content of the log</param>
        public delegate void UpdateLogEventDelegate(Type type, string content, string unit);

        /// <summary>
        /// Event for log update.
        /// </summary>
        public static event UpdateLogEventDelegate DisplayLogEvent;
        #endregion

        #region Fields

        private static int _daysDel = 30;
        /// <summary>
        /// 
        /// </summary>
        private static string[] _path;

        /// <summary>
        /// 
        /// </summary>
        private static string[] _logPath;

        /// <summary>
        /// Log list for updating.
        /// </summary>
        private static Queue<Data>[] _logQueues;

        /// <summary>
        /// Work thread for updating logs.
        /// </summary>
        private static Thread[] _workers;

        /// <summary>
        /// 
        /// </summary>
        private static ManualResetEvent[] _writeResetEvents;

        /// <summary>
        /// Semaphore for controlling concurrent access for log updates.
        /// </summary>
        private static SemaphoreSlim[] _semaphoreSlims;

        /// <summary>
        /// 
        /// </summary>
        private static bool[] _isSave;
        #endregion

        public static int DaysDel
        {
            get { return _daysDel; }
            set { _daysDel = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Start()
        {
            if (_workers != null)
            {
                return;
            }
            var count = Enum.GetNames(typeof(Type)).Length;

            _workers = new Thread[count];
            _writeResetEvents = new ManualResetEvent[count];
            _logQueues = new Queue<Data>[count];
            _semaphoreSlims = new SemaphoreSlim[count];

            _path = new string[count];
            _logPath = new string[count];
            _isSave = new bool[count];


            for (var i = 0; i < count; i++)
            {
                #region Set log storage location according to type.
                switch ((Type)i)
                {
                    case Type.Connect: _logPath[i] = string.Format(@"{0}\log\Connect", DefaultData.LogPath); break;
                    case Type.Exception: _logPath[i] = string.Format(@"{0}\log\Exception", DefaultData.LogPath); break;
                    case Type.PCSignalMess: _logPath[i] = string.Format(@"{0}\log\PC", DefaultData.LogPath); break;
                    case Type.PLCMess: _logPath[i] = string.Format(@"{0}\log\PLC", DefaultData.LogPath); break;
                    case Type.FlowRun: _logPath[i] = string.Format(@"{0}\log\FlowRun", DefaultData.LogPath); break;
                    case Type.Status: _logPath[i] = string.Format(@"{0}\log\StatusMachine", DefaultData.LogPath); break;

                    case Type.UI: _logPath[i] = string.Format(@"{0}\log\UI", DefaultData.LogPath); break;
                    default: break;
                }
                #endregion

                _logQueues[i] = new Queue<Data>();
                _semaphoreSlims[i] = new SemaphoreSlim(1, 1);
                _writeResetEvents[i] = new ManualResetEvent(false);

                _workers[i] = new Thread(Work);
                _workers[i].Start(i);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Stop()
        {
            var count = Enum.GetNames(typeof(Type)).Length;
            Parallel.For(0, count, (i) =>
            {
                _isSave[i] = false;

                if (_writeResetEvents[i] != null)
                    _writeResetEvents[i].Dispose();

                _workers[i]?.Abort();


                if (_writeResetEvents[i] != null)
                    _writeResetEvents[i].Dispose();
            });
        }

        /// <summary>
        /// Method for adding logs to the list for updating.
        /// </summary>
        /// <param name="content">Content of log.</param>
        public static void Add(Type type, string content, string eqp = null, string unit = null)
        {
            var value = Task.Run(async () =>
            {
                var index = (int)type;
                await _semaphoreSlims[index].WaitAsync();

                try
                {
                    _logQueues[index].Enqueue(Data.Set(content, eqp, unit));
#pragma warning disable 4014
                    Task.Run(() =>
#pragma warning restore 4014
                    {

                        if (DisplayLogEvent != null)
                            DisplayLogEvent.Invoke(type, string.Format("{0:MM-dd HH:mm:ss.fff}  {1}", DateTime.Now, content), unit);
                    });
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    _semaphoreSlims[index].Release();
                }

                _writeResetEvents[index].Set();
            });
        }

        /// <summary>
        /// Method for saving the logs in the list.
        /// </summary>
        private static async void Work(object obj)
        {
            var index = (int)obj;
            _isSave[index] = true;
            while (_isSave[index])
            {
                _writeResetEvents[index].WaitOne();
                await _semaphoreSlims[index].WaitAsync();
                try
                {
                    if (_logQueues[index].Count > 0)
                    {
                        var data = _logQueues[index].Dequeue();
                        var time = data.Time;
                        var unit = data.Unit;
                        var eqp = data.Eqp;
                        // Create a directory corresponding to the current year and month.
                        CheckFolder(_logPath[index]);

                        var path = string.Format(@"{0}", _logPath[index]);
                        CheckFolder(path);
                        if (eqp != null)
                        {
                            path = string.Format(@"{0}\{1}", path, eqp);
                            CheckFolder(path);
                            if (unit != null)
                            {
                                path = string.Format(@"{0}\{1}", path, unit);
                                CheckFolder(path);
                            }
                        }

                        path = string.Format(@"{0}\{1:yyyy}", path, time);
                        CheckFolder(path);

                        path = string.Format(@"{0}\{1:MM}", path, time);
                        CheckFolder(path);

                        // Write log to the current month and day
                        path = string.Format(@"{0}\{1:MM-dd}.txt", path, time);

                        // Check the number of occurrences of data in the buffer queue at the same time.
                        var count = _logQueues[index].Count(x => x.Time.Day == time.Day);

                        // Add to String Builder to write data from the same time zone at once. 
                        var builder = new StringBuilder();
                        string str = data.Content;
                        str += "-------------\n";
                        builder.Append(str);
                        // Write
                        using (var fileStream = File.Open(path, FileMode.Append))
                        {
                            Write(fileStream, builder.ToString());
                        }

                        CheckRemove(_logPath[index], time, _daysDel);
                    }
                    else
                        _writeResetEvents[index].Reset();
                }
                catch (Exception exception)
                {
                    // Debug.WriteLine(exception);
                }
                finally
                {
                    _semaphoreSlims[index].Release();
                }

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Method for storing logs.
        /// </summary>
        /// <param name="fileStream">Stream of the log file to be saved.</param>
        /// <param name="content">Content of the log.</param>
        internal static void Write(FileStream fileStream, string content)
        {
            if (fileStream == null)
                return;

            var data = Encoding.Default.GetBytes(string.Format(@"{0}", content));
            fileStream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Method for creating a specified folder if it does not exist.
        /// </summary>
        /// <param name="name">Path of the directory</param>
        internal static void CheckFolder(string name)
        {
            if (!Directory.Exists(name))
                Directory.CreateDirectory(name);
        }
        internal static void CheckRemove(string dir, DateTime time, int days)
        {
            var directories = Directory.GetDirectories(dir);
            foreach (var direct in directories)
            {
                var dirMonth = Directory.GetDirectories(direct);
                foreach (var dirmonth in dirMonth)
                {
                    DateTime dtMonth = Directory.GetLastWriteTime(dirmonth);
                    if ((time - dtMonth).TotalDays > days)
                    {
                        Directory.Delete(dirmonth, true);
                    }
                }
                dirMonth = Directory.GetDirectories(direct);
                if (dirMonth.Length == 0)
                {
                    Directory.Delete(direct, true);
                }
            }
        }
    }
}
