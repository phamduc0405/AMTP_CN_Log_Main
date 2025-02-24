using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABOOKFEEDER.INIT
{
    public class IniHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsDebuggerPresent();

        /// <summary>
        /// 프로세스 실행 유무 상태 확인 및 처리
        /// </summary>
        public static void ExistProcessProc()
        {
            int thisID = System.Diagnostics.Process.GetCurrentProcess().Id; // 현재 기동한 프로그램 id

            //실행중인 프로그램중 현재 기동한 프로그램과 같은 프로그램들 수집
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(System.Windows.Forms.Application.ProductName);

            if (p.Length > 1)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    if (p[i].Id == thisID)
                    {

                        System.Diagnostics.Process.GetCurrentProcess().Kill(); //이미 실행중이라면, 프로그램 실행 안함
                    }
                }
            }
        }
        /// <summary>
        /// 프로세스 실행 유무 상태 확인 및 처리
        /// </summary>
        public static bool ExistProcess()
        {
            int thisID = System.Diagnostics.Process.GetCurrentProcess().Id; // 현재 기동한 프로그램 id

            //실행중인 프로그램중 현재 기동한 프로그램과 같은 프로그램들 수집
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(System.Windows.Forms.Application.ProductName);

            if (p.Length > 1)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    if (p[i].Id == thisID)
                    {
                        // 프로세스 종료
                        // System.Windows.Forms.MessageBox.Show("이미 실행 중입니다.");
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tạo thư mục 
        /// </summary>
        /// <param name="pPath">Đường Dẫn</param>
        public static void CreateFolderPath(string pPath)
        {
            try
            {
                if (Directory.Exists(pPath) == false)
                {
                    int Index = pPath.IndexOf("\\");
                    string TempPath = pPath.Substring(0, Index + 1);

                    for (int i = 0; i < 100; i++)
                    {
                        if (Directory.Exists(TempPath) == false) //없으면 생성
                        {
                            Directory.CreateDirectory(TempPath);
                        }
                        else
                        {
                            if (TempPath == pPath)
                            {
                                break;
                            }
                            else
                            {
                                Index = pPath.IndexOf("\\", Index + 1);

                                if (Index == -1)
                                {
                                    TempPath = pPath;
                                }
                                else
                                {
                                    TempPath = pPath.Substring(0, Index + 1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteParameter.cs CreateFolderPath : " + ex.Message);
            }
        }

        //[DllImport("kernel32.dll")]
        //public extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        //[DllImport("kernel32.dll")]
        //public extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        //[DllImport("kernel32.dll")]
        //public extern static uint SetLocalTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32")]

        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32")]

        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);


        [DllImport("User32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        public static string READ(string szFile, string szSection, string szKey)
        {

            StringBuilder tmp = new StringBuilder(255);
            long i = GetPrivateProfileString(szSection, szKey, "", tmp, 255, szFile);
            return tmp.ToString();
        }
        public static void WRITE(string szFile, string szSection, string szKey, string szData)
        {
            WritePrivateProfileString(szSection, szKey, szData, szFile);
        }

        public static void SetFormDockStyle(Control _form, Panel _panel)
        {
            _form.Visible = true;
            _form.Dock = DockStyle.Fill;
            _panel.Controls.Add(_form);
        }

        public static void SetFormDockStyle(Form _form, Panel _panel)
        {
            _form.TopLevel = false;
            _form.Visible = true;
            _form.Dock = DockStyle.Fill;
            _panel.Controls.Add(_form);
        }
        public static void SetFormDockStyle(Form _form)
        {
            _form.TopLevel = false;
            _form.Visible = true;
            _form.Dock = DockStyle.Fill;
        }

        public static Thread StartThread(ThreadStart pMethod)
        {
            Thread trd = new Thread(pMethod);
            trd.IsBackground = true;
            trd.Name = pMethod.Method.Name;
            trd.Priority = ThreadPriority.Highest;
            trd.Start();

            return trd;
        }

        public static void AbortThread(Thread pThread)
        {
            if (pThread != null)
            {
                if (pThread.IsAlive)
                {
                    pThread.Join(100);
                }
            }
        }

        //180919 ksw Log 삭제 기능 추가 
        /// <summary>                   
        /// Log 삭제 기능
        /// </summary>
        /// <param name="pDirectory"></param>
        /// <param name="pStorageDasy"></param>
        public static void DeleteOldFile(string pDirectory, int pStorageDasy)
        {
            if (Directory.Exists(pDirectory))                                                                         //Kiểm tra 
            {
                string[] directorys = Directory.GetDirectories(pDirectory);

                if (directorys.Length > 0)
                {
                    foreach (string dir in directorys) DeleteOldFile(dir, pStorageDasy);
                }

                string[] fileNames = Directory.GetFiles(pDirectory);

                List<FileInfo> files = new List<FileInfo>();

                foreach (string path in fileNames)                                                                      //Quét Số File
                {
                    files.Add(new FileInfo(path));
                }

                DateTime limitTime = DateTime.Now.Subtract(new TimeSpan(pStorageDasy, 0, 0, 0));                     //Tìm Ngày Lùi Về Trước

                FileInfo[] sortedFiles = files.Where(p => p.CreationTime < limitTime).ToArray();

                foreach (FileInfo fileInfo in sortedFiles) fileInfo.Delete();                                           //Xóa File vs Ngày Tạo < Hơn
            }
        }

        /// <summary>
        /// 빈 폴더 삭제 기능
        /// </summary>
        /// <param name="pDirectory"></param>
        /// <returns></returns>
        public static bool DeleteEmptyFolder(string pDirectory)
        {
            if (Directory.Exists(pDirectory))
            {
                string[] fileNames = Directory.GetFiles(pDirectory);
                string[] directorys = Directory.GetDirectories(pDirectory);
                while (true)
                {
                    System.Diagnostics.Debug.WriteLine(pDirectory);
                    directorys = Directory.GetDirectories(pDirectory);

                    if (directorys.Length > 0)
                    {
                        bool existFile = false;
                        foreach (string dir in directorys)
                        {
                            if (DeleteEmptyFolder(dir) == false) existFile = true;
                        }
                        if (existFile == true) break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (fileNames.Length == 0 && directorys.Length == 0)
                {
                    Directory.Delete(pDirectory);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
