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

namespace JBoss
{
    public partial class JBossStandaloneService : ServiceBase
    {
        const string JbossHome = "JBOSS_HOME";

        private readonly string _jbossBinFolderPath;

        public JBossStandaloneService()
        {
            _jbossBinFolderPath = Environment.GetEnvironmentVariable(JbossHome);
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string standaloneBatFile = _jbossBinFolderPath + @"\bin\standalone.bat";

            try
            {
                var standaloneBatProcessInfo = new ProcessStartInfo(standaloneBatFile)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

                Process.Start(standaloneBatProcessInfo);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"Failed when attempting to start JBoss. Check the following message for more details: {e.Message}");
            }

        }

        protected override void OnStop()
        {
            string jbossCliBatFile = _jbossBinFolderPath + @"\bin\jboss-cli.bat";
            string shutdownArgs = "--connect command=:shutdown";

            try
            {
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

        }

    }
}
