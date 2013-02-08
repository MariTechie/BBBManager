using System;
using System.Drawing;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using BBBSysTrayManager.Helper;

namespace BBBSysTrayManager
{
    public class SysTray : Form
    {
        private NotifyIcon _trayIcon;
        private ContextMenu _trayMenu;
        private DateTime _stoppedTime;
        private ServiceControllerStatus _lastStatus;
        private int _notifyTimeInSeconds = 300;
        private int _verifyStatusInSeconds = 1;
        private int _verifyStoppedTimeInSeconds = 10;

        System.Windows.Forms.Timer verifyStatusProcessTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer verifyStoppedTimeProcessTimer = new System.Windows.Forms.Timer();

        #region Properties

        private ServiceControllerStatus CurrentStatus
        {
            get 
            {
                return ActivService.Status;
            }
        }

        private ServiceController ActivService
        {
            get {
                return new ServiceController("svctcom");
            }
        }

        #endregion

        #region Events

        void verifyStoppedTimeProcessTimer_Tick(object sender, EventArgs e)
        {
            NotifyStoppedTime();
        }

        void UpdateStatus_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        protected void TrayIcon_Click(object sender, EventArgs e)
        {
            var mouseEvent = e as MouseEventArgs;

            if (mouseEvent == null) return;

            if (mouseEvent.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (CurrentStatus == ServiceControllerStatus.Running)
                {
                    ActivService.Stop();
                }
                else if (CurrentStatus == ServiceControllerStatus.Stopped)
                {
                    ActivService.Start();
                }
            }
        }

        private void OnStart(object sender, EventArgs e)
        {
            if (CurrentStatus == ServiceControllerStatus.Stopped)
                ActivService.Start();
        }

        private void OnStop(object sender, EventArgs e)
        {
            if (CurrentStatus == ServiceControllerStatus.Running)
                ActivService.Stop();
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja realmente fechar o aplicativo?\nNão me responsabilizao se você não for produtivo!", "Alerta", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        #endregion

        #region Init

        private void Initialize()
        {
            verifyStatusProcessTimer.Interval = _verifyStatusInSeconds * 1000;
            verifyStatusProcessTimer.Tick += new EventHandler(UpdateStatus_Tick);
            verifyStatusProcessTimer.Start();

            verifyStoppedTimeProcessTimer.Interval = _verifyStoppedTimeInSeconds * 1000;
            verifyStoppedTimeProcessTimer.Tick += new EventHandler(verifyStoppedTimeProcessTimer_Tick);

            if (CurrentStatus == ServiceControllerStatus.Stopped)
            {
                _stoppedTime = DateTime.Now;
                verifyStoppedTimeProcessTimer.Start();
            }
        }

        private void InitSystrayForm()
        {
            // Create a simple tray menu with only one item.
            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add(GetCurrentStatus());
            _trayMenu.MenuItems.Add("-");
            _trayMenu.MenuItems.Add("Iniciar", OnStart);
            _trayMenu.MenuItems.Add("Parar", OnStop);
            _trayMenu.MenuItems.Add("-");
            _trayMenu.MenuItems.Add("Fechar", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "BBBManager " + GetCurrentStatus();
            _trayIcon.Icon = GetIconByEmbeddedResource();

            // Add menu to tray icon and show it.
            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;
            _trayMenu.MenuItems[0].Enabled = false;

            _trayIcon.Click += new EventHandler(TrayIcon_Click);
        }

        #endregion

        #region Behavior Status

        private void UpdateStatus()
        {
            if (CurrentStatus == _lastStatus) return;

            if (CurrentStatus == ServiceControllerStatus.Running)
            {
                ShowBalloonTipText("Status Processo", "Iniciado com sucesso");
                _stoppedTime = DateTime.MinValue;
                verifyStoppedTimeProcessTimer.Stop();
            }

            if (CurrentStatus == ServiceControllerStatus.Stopped)
            {
                ShowBalloonTipText("Status Processo", "Processo Encerrado");
                _stoppedTime = DateTime.Now;
                verifyStoppedTimeProcessTimer.Start();
            }

            _trayIcon.Text = "BBBManager " + GetCurrentStatus();
            _trayMenu.MenuItems[0].Text = GetCurrentStatus();
            _trayIcon.Icon = GetIconByEmbeddedResource();
            _lastStatus = CurrentStatus;
        }

        private void NotifyStoppedTime()
        {
            if (_stoppedTime > DateTime.MinValue)
            {
                TimeSpan ts = DateTime.Now.Subtract(_stoppedTime);

                if (ts.TotalSeconds > _notifyTimeInSeconds)
                {
                    _trayIcon.ShowBalloonTip(60000, "Aviso", "\n\n\n\tNão esqueça de ligar o BBB...\t \t \t \t \n\n\n ", ToolTipIcon.Warning);
                    _stoppedTime = DateTime.Now;
                }
            }
        }

        #endregion

        #region Util
        
        public Icon GetIconByEmbeddedResource()
        {
            Icon icon = null;

            var filename = CurrentStatus == ServiceControllerStatus.Running ? "on" : "off";

            var iconResource = string.Format("BBBSysTrayManager.icons.{0}.ico", filename);

            using (var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconResource))
            {
                icon = new Icon(iconStream);
            }

            return icon;
        }


        private string GetCurrentStatus()
        {
            const string status = "Status: {0}";

            return string.Format(status, CurrentStatus.ToStatusString());
        }

        #endregion

        #region BallonTip

        private void ShowBalloonTipText(string title, string content)
        {
            _trayIcon.ShowBalloonTip(3000, title, content, ToolTipIcon.Info);
        }

        #endregion

        public SysTray()
        {
            InitSystrayForm();

            _lastStatus = CurrentStatus;

            Initialize();
        }

        protected override void Dispose(bool isDisposing)
        {
            verifyStatusProcessTimer.Stop();
            verifyStoppedTimeProcessTimer.Stop();
            
            if (isDisposing)
            {
                _trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
