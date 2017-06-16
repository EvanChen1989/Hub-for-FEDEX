using System.Windows;
using System.Windows.Controls;
using Cassia;
using System.Windows.Forms;
using System.Drawing;
using System;

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowState ws;
        WindowState wsl;
        NotifyIcon notifyIcon;
        public static Window tempWin;

        public MainWindow()
        {
            InitializeComponent();
            icon();
            wsl = WindowState;

            Helper.Set_Bootup();
            Helper.LoadPreviousSession();
            Helper.GetOSInfo();
            Helper.GetBrowserVersion();          
            Helper.GetUFTVersion();

            this.OSInfo.Content = Helper.OS_Info_String;
            this.UFTInfo.Content = Helper.UFT_Info_String;
            this.BrowserInfo.Content = Helper.Browser_Info_String;
            this.current_user.Text = Helper.CurrentUser;
            this.Comment_textbox.Text = Helper.Comment;

            SignalRConnector connector = new SignalRConnector();
            SignalRConnector.UploadNode();

            Helper.StartTimers();

            System.Windows.Threading.DispatcherTimer displayUpdater = new System.Windows.Threading.DispatcherTimer();
            displayUpdater.Tick += new System.EventHandler(DisplayContentUpdate);
            displayUpdater.Interval = new System.TimeSpan(0, 0, 1);
            displayUpdater.Start();

            
            //System.Threading.TimerCallback timerDelegate = new System.Threading.TimerCallback(SignalRConnector.KeepMachineAlive);
            //System.Threading.Timer timer = new System.Threading.Timer(timerDelegate, new object(), 5000, 30000);
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;
                this.Topmost = true;
            }
            //WindowState = wsl;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }


        private void icon()
        {
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "Hub is Running";
            this.notifyIcon.Text = "Hub is Running!";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            this.notifyIcon.ShowBalloonTip(1000);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void release_button_Click(object sender, RoutedEventArgs e)
        {
            Helper.UserDisplay = Helper.CurrentUser + " (Temp Use)";
            this.current_user.Text = Helper.UserDisplay;
            this.Comment_textbox.Text = Helper.Comment = "Comment:";
            Helper.Occupied = false;
            Helper.SaveSessionStatus();
        }

        private void occupy_button_Click(object sender, RoutedEventArgs e)
        {   
            Helper.UserDisplay = Helper.CurrentUser + " (Locked)";
            this.current_user.Text = Helper.UserDisplay;
            Helper.Comment = this.Comment_textbox.Text;
            Helper.Occupied = true;
            Helper.SaveSessionStatus();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Enter_Mama(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("IEXPLORE.EXE", "http://mama.hpeswlab.net");
        }
        private void Enter_0271(object sender, RoutedEventArgs e) { 
           System.Diagnostics.Process.Start("IEXPLORE.EXE", "http://mydph0271.hpeswlab.net:8080/qcbin");
        }
        private void Enter_AgileManager(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("IEXPLORE.EXE", "https://login.saas.hpe.com/msg/actions/showLogin");
        }
        private void Enter_UFTAutomationPortal(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("IEXPLORE.EXE", "http://myd-vm06527.hpeswlab.net:1234/");
        }

        private void Edit_Comment(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(Comment_textbox.Text))
                Comment_textbox.Text = "Comment:";
        }

        private void DisplayContentUpdate(object sender, System.EventArgs e)
        {
            
            string user = new TerminalServicesManager().CurrentSession.ClientName;
            if (Helper.Occupied == false)
                Helper.UserDisplay = user + "(Temp Use)";              
            else
                Helper.UserDisplay = Helper.CurrentUser + " (Locked)";
            this.current_user.Text = Helper.UserDisplay;

            if (this.OSInfo.Content.ToString() != Helper.OS_Info_String)
                this.OSInfo.Content = Helper.OS_Info_String;
            if (this.UFTInfo.Content.ToString() != Helper.UFT_Info_String)
                this.UFTInfo.Content = Helper.UFT_Info_String;
            if (this.BrowserInfo.Content.ToString() != Helper.Browser_Info_String)
                this.BrowserInfo.Content = Helper.Browser_Info_String;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}