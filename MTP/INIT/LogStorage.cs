using ACO2_App._0.Model;
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
    public class LogStorage
    {
        #region User define
      private  ListCellDatas _listCellDatas;
        #endregion
        #region Event
        /// <summary>
        /// Delegate for log update events.
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="content">Content of the log</param>
        public delegate void UpdateLogEventDelegate(ListCellDatas datas);

        /// <summary>
        /// Event for log update.
        /// </summary>
        public static event UpdateLogEventDelegate DisplayLogEvent;
        #endregion

        #region Fields
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
        private static Queue<ListCellDatas>[] _logQueues;

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
        /// <summary>
        /// 
        /// </summary>
        public static void Start()
        {
            if (_workers != null)
            {
                return;
            }
            var count = 2;

            _workers = new Thread[count];
            _writeResetEvents = new ManualResetEvent[count];
            _logQueues = new Queue<ListCellDatas>[count];
            _semaphoreSlims = new SemaphoreSlim[count];
            _path = new string[count];
            _logPath = new string[count];
            _isSave = new bool[count];


            for (var i = 1; i < count; i++)
            {
                #region Set log storage location according to type.
                {
                    _logPath[i] = string.Format(@"{0}\Setting", DefaultData.AppPath);
                }
                #endregion

                _logQueues[i] = new Queue<ListCellDatas>();
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
            var count = 0;
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
        public static void Add(ListCellDatas datas)
        {
            var value = Task.Run(async () =>
            {
                var index = 1;
                ListCellDatas data = new ListCellDatas();
                data = datas;
                await _semaphoreSlims[index].WaitAsync();

                try
                {
                    _logQueues[index].Enqueue(data);
#pragma warning disable 4014
                    Task.Run(() =>
#pragma warning restore 4014
                    {

                        if (DisplayLogEvent != null)
                            DisplayLogEvent.Invoke(data);
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
                        // Create a directory corresponding to the current year and month.
                        CheckFolder(_logPath[index]);

                        var path = string.Format(@"{0}\DataStorage.setting");
                        string str = XmlHelper<ListCellDatas>.SerializeToString(data);
                        // Add to String Builder to write data from the same time zone at once. 
                        var builder = new StringBuilder();
                        builder.Append(str);
                        // Write
                        File.WriteAllText(path, str);
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
