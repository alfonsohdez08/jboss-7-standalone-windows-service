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
using System.IO;

namespace JBoss
{
    /// <summary>
    /// Handles JBoss 7 Standalone server.
    /// </summary>
    public partial class JBossStandaloneService : ServiceBase
    {
        /// <summary>
        /// Environment variable that holds the home directory where JBoss is installed.
        /// </summary>
        private const string JbossHomeEnv = "JBOSS_HOME";
        
        /// <summary>
        /// Waiting time for fetch the service status during starting/stopping the service itself.
        /// </summary>
        private const int WaitTimeInSeconds = 30;

        /// <summary>
        /// JBoss home path obtained from looking up the environment variable held by JBossHomeEnv.
        /// </summary>
        private readonly string _jbossHomePath;

        /// <summary>
        /// Holds the service current status which would be sent to the SetServiceStatus function from Windows API.
        /// </summary>
        private ServiceStatus _serviceStatus;

        /// <summary>
        /// References the standalone.bat process running in the background.
        /// </summary>
        private Process _standaloneBatProcess;

        /// <summary>
        /// Creates an instance of <c>JBossStandaloneService</c>
        /// </summary>
        public JBossStandaloneService()
        {
            _jbossHomePath = Environment.GetEnvironmentVariable(JbossHomeEnv);
            _serviceStatus = new ServiceStatus()
            {
                dwWaitHint = TimeSpan.FromSeconds(WaitTimeInSeconds).Milliseconds
            };

            InitializeComponent();
        }

        /// <summary>
        /// Defines all the logic needed in order to start the JBoss server.
        /// </summary>
        /// <param name="args">Startup arguments.</param>
        protected override void OnStart(string[] args)
        {
            string standaloneBatFile = _jbossHomePath + @"\bin\standalone.bat";

            try
            {
                SetServiceStatus(ServiceState.SERVICE_START_PENDING);

                EventLog.WriteEntry("Starting JBoss server.", EventLogEntryType.Information);

                if (!File.Exists(standaloneBatFile))
                    throw new FileNotFoundException($"The standalone.bat file is not found in the given directory: {standaloneBatFile}");

                var standaloneBatProcessInfo = new ProcessStartInfo(standaloneBatFile)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                _standaloneBatProcess = Process.Start(standaloneBatProcessInfo);

                SetServiceStatus(ServiceState.SERVICE_RUNNING);

                EventLog.WriteEntry("JBoss server started.", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Failed when attempting to start JBoss. Check the following message for more details: {e.Message}", EventLogEntryType.Error);

                SetServiceStatus(ServiceState.SERVICE_STOPPED);
            }

        }

        /// <summary>
        /// Defines all the logic needed in order to stop the JBoss server.
        /// </summary>
        protected override void OnStop()
        {
            string jbossCliBatFile = _jbossHomePath + @"\bin\jboss-cli.bat";
            string shutdownArgs = "--connect command=:shutdown";

            try
            {
                SetServiceStatus(ServiceState.SERVICE_STOP_PENDING);

                if (!File.Exists(jbossCliBatFile))
                    throw new FileNotFoundException($"The jboss-cli.bat file is not found in the given directory: {jbossCliBatFile}");

                EventLog.WriteEntry("Stopping JBoss server.", EventLogEntryType.Information);

                var jbossCliBatProcessInfo = new ProcessStartInfo(jbossCliBatFile, shutdownArgs)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Process process = Process.Start(jbossCliBatProcessInfo);
                process.WaitForExit(TimeSpan.FromSeconds(30).Milliseconds);

                EventLog.WriteEntry("JBoss server stopped.", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Failed when attempting to stop JBoss server. Check the following message for more details: {e.Message}.", EventLogEntryType.Error);

                EventLog.WriteEntry("Terminating JBoss in hard manner.", EventLogEntryType.Warning);
                _standaloneBatProcess.Kill();
                _standaloneBatProcess = null;
            }
            finally
            {
                SetServiceStatus(ServiceState.SERVICE_STOPPED);
            }

        }

        /// <summary>
        /// Sets the service status.
        /// </summary>
        /// <param name="state">Service status/state.</param>
        private void SetServiceStatus(ServiceState state)
        {
            _serviceStatus.dwCurrentState = state;
            SetServiceStatus(this.ServiceHandle, ref _serviceStatus);
        }


        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }

    /// <summary>
    /// Represents the lifetime of a service.
    /// </summary>
    enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
    }

    /// <summary>
    /// Holds the configuration needed for set a service status.
    /// </summary>
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