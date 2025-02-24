using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2_App._0.Model
{
    /// <summary>
    /// Provides a set of methods and properties that you can use to get or check elapsed time.
    /// </summary>
    public class StopWatch
    {
        #region Fields
        /// <summary>
        /// Object for measuring time.
        /// </summary>
        private readonly Stopwatch _stopWatch;
        #endregion

        #region Properties

        /// <summary>
        /// The elapsed time from the start to the present.
        /// </summary>
        /// <returns>Elapsed time.(ms)</returns>
        public long ElapsedTime
        {
            get
            {
                return _stopWatch == null ? 0 : _stopWatch.ElapsedMilliseconds;
            }
        }

        #endregion

        #region Constructor & Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        public StopWatch()
        {
            _stopWatch = new Stopwatch();
        }

        /// <summary>
        /// Start measuring time.
        /// </summary>
        public void Start()
        {
            if (_stopWatch == null)
                return;

            _stopWatch.Reset();
            _stopWatch.Start();
        }
        public bool IsRunning { get { return _stopWatch.IsRunning; } }
        #endregion

        #region Public method
        /// <summary>
        /// Stop measuring time.
        /// </summary>
        public void Stop()
        {
            if (_stopWatch == null)
                return;

            _stopWatch.Stop();
        }
        public void Reset()
        {
            if (_stopWatch == null)
                return;

            _stopWatch.Reset();
        }
        /// <summary>
        /// Method to check if the specified time has elapsed.
        /// </summary>
        /// <param name="ms">Specified time.</param>
        /// <returns>Returns <c>true</c> if elapsed and <c>false</c> if not.</returns>
        public bool CheckElapsedTime(long ms)
        {
            return _stopWatch != null && _stopWatch.ElapsedMilliseconds >= ms;
        }
        #endregion
    }
}
