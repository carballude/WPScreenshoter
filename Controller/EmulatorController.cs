using Microsoft.Phone.Tools;
using Microsoft.SmartDevice.Connectivity.Interface;
using Microsoft.SmartDevice.MultiTargeting.Connectivity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPScreenShoter.Model;

namespace WPScreenShoter.Controller
{
    public class EmulatorController
    {
        private static EmulatorController _INSTANCE = null;

        private IDevice _device;

        private string _preferredDevice;
        public string PreferredDevice
        {
            get
            {
                if (String.IsNullOrEmpty(_preferredDevice))
                    return _multiTargetingConnectivity.GetDefaultDeviceId();
                return _preferredDevice;
            }
            set { _preferredDevice = value; }
        }
        private IRemoteApplication _lastApp;

        private MultiTargetingConnectivity _multiTargetingConnectivity;

        public event EventHandler EmulatorConnected;
        public event EventHandler XapInstalled;
        public event EventHandler SnapshotTaken;

        private EmulatorController() 
        {
            _multiTargetingConnectivity = new MultiTargetingConnectivity(CultureInfo.CurrentUICulture.LCID);
        }
        public static EmulatorController GetEmulatorController()
        {
            if (_INSTANCE == null)
                _INSTANCE = new EmulatorController();
            return _INSTANCE;
        }

        private void StartEmulatorSynchronous(object id)
        {
            ConnectableDevice connectableDevice = _multiTargetingConnectivity.GetConnectableDevice((string)id);
            _device = connectableDevice.Connect();
            EmulatorConnected(this, null);
        }

        private void StartEmulatorSynchronous()
        {
            StartEmulatorSynchronous(PreferredDevice);
        }

        public void StartEmulator(string id)
        {
            new Thread(new ParameterizedThreadStart(StartEmulatorSynchronous)).Start(id);
        }

        public List<Emulator> Emulators
        {
            get
            {
                return _multiTargetingConnectivity.GetConnectableDevices().Where(x => x.IsEmulator()).Select(x => new Emulator() { Device = x }).ToList();
            }
        }

        private void InstallXapSynchronous(object pathToXap)
        {
            Guid? guid;
            Version version;
            bool flag;
            _device.Activate();
            var tempFile = Path.GetTempFileName();
            File.Copy((string)pathToXap, tempFile, true);
            Utils.ReadWMAppManifestXaml(tempFile, out guid, out version, out flag);
            var iconFile = Utils.ExtractIconFile(tempFile);
            if(_device.GetInstalledApplications().Any(x => x.ProductID == guid.Value))
                _lastApp = _device.GetApplication(guid.Value);
            else
                _lastApp = _device.InstallApplication(guid.Value, guid.Value, "NormalApp", iconFile, tempFile);
            _lastApp.Launch();
            XapInstalled(this, null);
        }

        public void InstallAndSnap(string pathToXap)
        {
            if (_device == null) StartEmulatorSynchronous();
            InstallXapSynchronous(pathToXap);
            Thread.Sleep(1500);
            TakeSnapshot(pathToXap + ".png");
        }

        public void InstallXap(string pathToXap)
        {
            new Thread(new ParameterizedThreadStart(InstallXapSynchronous)).Start(pathToXap);
        }

        public void TakeSnapshot(string filePath)
        {
            var pointer = FindWindow(null, "XDE");
            if (pointer == null) return;
            PrintWindow(pointer, filePath);
        }

        private void PrintWindow(IntPtr hdlChild, string filePath)
        {
            RECT rct;
            GetWindowRect(hdlChild, out rct);
            Rectangle wndRct = rct;
            Graphics gr = Graphics.FromHwnd(hdlChild);
            Bitmap bmp = new Bitmap(wndRct.Width, wndRct.Height);
            Graphics grBmp = Graphics.FromImage(bmp);
            BitBlt(grBmp.GetHdc(), 0, 0, wndRct.Width, wndRct.Height, gr.GetHdc(), 0, 0, CAPTUREBLT | SRCCOPY);
            grBmp.ReleaseHdc();            
            bmp.Save(filePath, ImageFormat.Png);
            SnapshotTaken(bmp, null);
        }

        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public static implicit operator Rectangle(RECT rect)
            {
                return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            }
        }

        #region DllImports
        private const int SRCCOPY = 0x00CC0020;
        private const int CAPTUREBLT = 0x40000000;
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        #endregion
    }
}
