using ACO2_App._0.INIT;
using ExcelDataReader.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACO2.Data
{
    internal class DataLog
    {
        public struct Data
        {
            /// <summary>
            /// 
            /// </summary>
            public string Header { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public string Content { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime Time { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="header"></param>
            /// <param name="content"></param>
            public void Set(string header, string content)
            {
                Header = header;
                Content = content;

                Time = DateTime.Now;
            }
        }

        #region Fields
        /// <summary>
        /// Log list for updating.
        /// </summary>
        private readonly Queue<Data> _logQueue = new Queue<Data>();

        /// <summary>
        /// Work thread for updating logs.
        /// </summary>
        private Thread _worker;

        /// <summary>
        /// 
        /// </summary>
        private readonly ManualResetEvent _writeResetEvent = new ManualResetEvent(false);

        /// <summary>
        /// Semaphore for controlling concurrent access for log updates.
        /// </summary>
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 
        /// </summary>
        private bool _isSave;
        private string _eqpId;
        private int _monthToDelete;

        #endregion

        public DataLog(string eqpid,int del= 30)
        {
            _eqpId = eqpid;
            _monthToDelete = del;
        }

        public void Start()
        {
            _worker = new Thread(Work);
            _worker.Start();
        }

        public void Stop()
        {
            _isSave = false;
            _writeResetEvent.Dispose();

            if (_worker != null)
            {
                _worker.Abort();
                _worker.Join();
                _worker = null;
            }
        }

        /// <summary>
        /// Method for adding logs to the list for updating.
        /// </summary>
        /// <param name="content">Content of log.</param>
        public async Task Add(string header, string content)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var data = new Data();
                data.Set(header, content);
                _logQueue.Enqueue(data);
            }
            catch (Exception exception)
            {
                var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);
                LogTxt.Add(LogTxt.Type.Exception, "UnitLog Add: " + debug);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            _writeResetEvent.Set();
        }
        // Create a directory corresponding to the current year and month.
        private string folder = string.Empty;
        private string path = string.Empty;
        /// <summary>
        /// Method for saving the logs in the list.
        /// </summary>
        /// 
        private async void Work()
        {
            _isSave = true;
            while (_isSave )
            {
                _writeResetEvent.WaitOne();

                await _semaphoreSlim.WaitAsync();
                try
                {

                    if (_logQueue.Count > 0)
                    {
                        var data = _logQueue.Dequeue();
                        var time = data.Time;
                        if (string.IsNullOrEmpty(folder) || time.Hour > 7)
                        {
                            folder = string.Format(@"{0}\Data\{1:yyyy}\{2:MM}", DefaultData.LogPath, time, time);
                            CheckFolder(folder);
                        }
                        if (string.IsNullOrEmpty(path) || time.Hour > 7)
                        {
                            path = string.Format(@"{0}\{1}_{2:dd}.csv", folder,_eqpId, time);
                            Delete(_monthToDelete);
                        }

                        // Check the number of occurrences of data in the buffer queue at the same time.
                        var count = _logQueue.Count(x => x.Time.Day == time.Day);

                        // Add to String Builder to write data from the same time zone at once. 
                        var builder = new StringBuilder();
                        builder.AppendLine(data.Content);

                        for (var i = 0; i < count; i++)
                        {
                            builder.AppendLine(_logQueue.Dequeue().Content);
                        }

                        var isExistFile = File.Exists(path);

                        // Write
                        using (var fileStream = File.Open(path, FileMode.Append))
                        {
                            if (!isExistFile)
                                Write(fileStream, string.Format("{0}{1}", data.Header, Environment.NewLine));

                            Write(fileStream, builder.ToString());
                        }
                    }
                    else
                        _writeResetEvent.Reset();
                }
                catch (Exception exception)
                {
                    var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);
                    LogTxt.Add(LogTxt.Type.Exception, "UnitLog Work: " + debug);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Method for storing logs.
        /// </summary>
        /// <param name="fileStream">Stream of the log file to be saved.</param>
        /// <param name="content">Content of the log.</param>
        internal void Write(FileStream fileStream, string content)
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
        internal void CheckFolder(string name)
        {
            if (!Directory.Exists(name))
                Directory.CreateDirectory(name);
        }
        internal void Delete(int months)
        {
            // Lấy thời gian hiện tại
            DateTime currentDate = DateTime.Now;

            // Xác định tháng cần xóa
            DateTime monthToDelete = currentDate.AddMonths(-months);

            // Lấy danh sách các thư mục trong năm cần xóa
            string[] yearFolders = Directory.GetDirectories(Path.Combine(DefaultData.LogPath,"Data"));

            // Xóa thư mục đầu tiên của mỗi tháng
            foreach (var yearFolder in yearFolders)
            {
                string[] monthFolders = Directory.GetDirectories(yearFolder);
                foreach (var monthFolder in monthFolders.OrderBy(folder => folder)) // Sắp xếp theo thứ tự tăng dần
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(monthFolder);
                    DateTime creationTime = directoryInfo.CreationTime;
                    if (creationTime < monthToDelete)
                    {
                        // Xóa thư mục và nói cho người dùng biết
                        Directory.Delete(monthFolder, true);
                        Console.WriteLine($"Deleted folder: {monthFolder}");
                    }

                    break; // Chỉ xóa thư mục đầu tiên của mỗi tháng
                }
            }
        }
    }
}
