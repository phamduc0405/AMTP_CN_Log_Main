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
        public static event UpdateLogEventDelegate DisplayLogDataEvent;
        #endregion

        #region Fields
        /// <summary>
        /// 
        /// </summary>
        private static string[] _pathData;

        /// <summary>
        /// 
        /// </summary>
        private static string[] _logPathData;

        /// <summary>
        /// Log list for updating.
        /// </summary>
        private static Queue<ListCellDatas>[] _logQueuesData;

        /// <summary>
        /// Work thread for updating logs.
        /// </summary>
        private static Thread[] _workersData;

        /// <summary>
        /// 
        /// </summary>
        private static ManualResetEvent[] _writeResetEventsData;

        /// <summary>
        /// Semaphore for controlling concurrent access for log updates.
        /// </summary>
        private static SemaphoreSlim[] _semaphoreSlimsData;

        /// <summary>
        /// 
        /// </summary>
        private static bool[] _isSaveData;
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public static void Start()
        {
            if (_workersData != null)
            {
                return;
            }
            var count = 1;

            _workersData = new Thread[count];
            _writeResetEventsData = new ManualResetEvent[count];
            _logQueuesData = new Queue<ListCellDatas>[count];
            _semaphoreSlimsData = new SemaphoreSlim[count];
            _pathData = new string[count];
            _logPathData = new string[count];
            _isSaveData = new bool[count];


            for (var i = 0; i < count; i++)
            {
                #region Set log storage location according to type.
                {
                    _logPathData[i] = string.Format(@"{0}\Setting", DefaultData.AppPath);
                }
                #endregion

                _logQueuesData[i] = new Queue<ListCellDatas>();
                _semaphoreSlimsData[i] = new SemaphoreSlim(1, 1);
                _writeResetEventsData[i] = new ManualResetEvent(false);

                _workersData[i] = new Thread(Work);
                _workersData[i].Start(i);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Stop()
        {
            var count = 1;
            Parallel.For(0, count, (i) =>
            {
                _isSaveData[i] = false;
                if (_writeResetEventsData[i] != null)
                    _writeResetEventsData[i].Dispose();

                _workersData[i]?.Abort();


                if (_writeResetEventsData[i] != null)
                    _writeResetEventsData[i].Dispose();
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
                var index = 0;
                ListCellDatas data = new ListCellDatas();
                data = datas;
                await _semaphoreSlimsData[index].WaitAsync();

                try
                {
                    _logQueuesData[index].Enqueue(data);
#pragma warning disable 4014
                    Task.Run(() =>
#pragma warning restore 4014
                    {

                        if (DisplayLogDataEvent != null)
                            DisplayLogDataEvent.Invoke(data);
                    });
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    _semaphoreSlimsData[index].Release();
                }

                _writeResetEventsData[index].Set();
            });
        }

        /// <summary>
        /// Method for saving the logs in the list.
        /// </summary>
        private static async void Work(object obj)
        {
            var index = (int)obj;
            _isSaveData[index] = true;
            while (_isSaveData[index])
            {
                _writeResetEventsData[index].WaitOne();
                await _semaphoreSlimsData[index].WaitAsync();
                try
                {
                    if (_logQueuesData[index].Count > 0)
                    {
                        var data = _logQueuesData[index].Dequeue();
                        // Create a directory corresponding to the current year and month.
                        CheckFolderData(_logPathData[index]);

                        var path = string.Format(@"{0}\DataStorage.setting", _logPathData[index]);
                        string str = XmlHelper<ListCellDatas>.SerializeToString(data);
                        // Add to String Builder to write data from the same time zone at once. 
                        var builder = new StringBuilder();
                        builder.Append(str);
                        // Write
                        File.WriteAllText(path, str);
                    }
                    else
                        _writeResetEventsData[index].Reset();
                }
                catch (Exception exception)
                {
                    // Debug.WriteLine(exception);
                }
                finally
                {
                    _semaphoreSlimsData[index].Release();
                }

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Method for storing logs.
        /// </summary>
        /// <param name="fileStream">Stream of the log file to be saved.</param>
        /// <param name="content">Content of the log.</param>
        internal static void WriteData(FileStream fileStream, string content)
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
        internal static void CheckFolderData(string name)
        {
            if (!Directory.Exists(name))
                Directory.CreateDirectory(name);
        }
        internal static void CheckRemoveData(string dir, DateTime time, int days)
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
