using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;

namespace JBoss
{
    public partial class JBossStandaloneService : ServiceBase
    {
        private const string JbossHome = "JBOSS_HOME";
        private const int WaitTimeInSeconds = 30;

        private readonly string _jbossBinFolderPath;
        private ServiceStatus _serviceStatus;

        public JBossStandaloneService()
        {
            _jbossBinFolderPath = Environment.GetEnvironmentVariable(JbossHome);
            _serviceStatus = new ServiceStatus()
            {
                dwWaitHint = TimeSpan.FromSeconds(WaitTimeInSeconds).Milliseconds
            };

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string standaloneBatFile = _jbossBinFolderPath + @"\bin\standalone.bat";

            try
            {
                SetServiceStatus(ServiceState.SERVICE_START_PENDING);

                var standaloneBatProcessInfo = new ProcessStartInfo(standaloneBatFile)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Process.Start(standaloneBatProcessInfo);

                SetServiceStatus(ServiceState.SERVICE_RUNNING);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Failed when attempting to start JBoss. Check the following message for more details: {e.Message}");

                SetServiceStatus(ServiceState.SERVICE_STOPPED);
            }

        }

        protected override void OnStop()
        {
            string jbossCliBatFile = _jbossBinFolderPath + @"\bin\jboss-cli.bat";
            string shutdownArgs = "--connect command=:shutdown";

            try
            {
                SetServiceStatus(ServiceState.SERVICE_STOP_PENDING);

                var jbossCliBatProcessInfo = new ProcessStartInfo(jbossCliBatFile, shutdownArgs)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Process process = Process.Start(jbossCliBatProcessInfo);

                process.WaitForExit(TimeSpan.FromSeconds(30).Milliseconds);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Failed when attempting to stop JBoss server. Check the following message for more details: {e.Message}");
            }
            finally
            {
                SetServiceStatus(ServiceState.SERVICE_STOPPED);
            }

        }

        private void SetServiceStatus(ServiceState state)
        {
            _serviceStatus.dwCurrentState = state;
            SetServiceStatus(this.ServiceHandle, ref _serviceStatus);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }

    enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

}