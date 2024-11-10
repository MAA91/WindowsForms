using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Clock.Properties;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using AxWMPLib;

namespace Clock
{
    public partial class MainForm : Form
    {
        ColorDialog backgroundColorDialog;
        ColorDialog foregroundColorDialog;
        ChooseFont chooseFontDialog;
        AlarmList alarmList;
        Alarm alarm;
        string FontFile {get; set;}

        public MainForm()
        {
            InitializeComponent();
            AllocConsole();
            SetFontDirectory();
            this.TransparencyKey = Color.Empty;
            backgroundColorDialog = new ColorDialog();
            foregroundColorDialog = new ColorDialog();
            alarmList = new AlarmList();

            chooseFontDialog = new ChooseFont();
            LoadSettings();

            labelTime.ForeColor = foregroundColorDialog.Color;
            SetVisibility(false);
            this.Location = new Point
                (
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.Width,
                    50
                );

            alarm = new Alarm();
            GetNextAlarm();
        }

        void SetFontDirectory()
        {
            string location = Assembly.GetEntryAssembly().Location;
            string path = Path.GetDirectoryName(location);         
            Directory.SetCurrentDirectory($"{path}\\..\\..\\Fonts");
        }

        public void GetNextAlarm()
        {
            List<Alarm> alarms = new List<Alarm>();
            foreach (Alarm item in alarmList.ListBoxAlarms.Items)
            {
                if(item.Time > DateTime.Now)alarms.Add(item); 
            }
            if(alarms.Min() != null)alarm = alarms.Min();
            //List<TimeSpan> intervals = new List<TimeSpan>();
            //foreach (Alarm item in alarmList.ListBoxAlarms.Items)
            //{
            //    TimeSpan min = new TimeSpan(24, 0, 0);
            //    if (DateTime.Now - item.Time < min)
            //        alarm = item;
            //}
            Console.WriteLine(alarm);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            labelTime.Text = DateTime.Now.ToString("HH.mm.ss");
            if (checkBoxShowDate.Checked)
                labelTime.Text += $"\n{DateTime.Today.ToString("yyyy.MM.dd")}";
            if (showWeekdayToolStripMenuItem.Checked)
                labelTime.Text += $"\n{DateTime.Now.DayOfWeek}";
            if (
                 alarm != null &&
                 alarm.Weekdays[((int)DateTime.Now.DayOfWeek - 1 < 0 ? 6 : (int)DateTime.Now.DayOfWeek - 1)] == true &&                 
                 DateTime.Now.Hour == alarm.Time.Hour &&
                 DateTime.Now.Minute == alarm.Time.Minute &&
                 DateTime.Now.Second == alarm.Time.Second
               )
            {
                PlayAlarm();
                //MessageBox.Show(alarm.Filename, "Alarm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GetNextAlarm();
            }
            if(DateTime.Now.Second == 0)
            {
                Console.WriteLine("Minute");
                GetNextAlarm();
            }
        }
        
        void PlayAlarm()
        {
            axWindowsMediaPlayer.URL = alarm.Filename;
            axWindowsMediaPlayer.settings.volume = 100;
            axWindowsMediaPlayer.Ctlcontrols.play();
            axWindowsMediaPlayer.Visible = true;
        }

        private void SetVisibility(bool visible)
        {
            this.TransparencyKey = visible ? Color.Empty : this.BackColor;
            this.FormBorderStyle = visible ? FormBorderStyle.Sizable : FormBorderStyle.None;
            this.ShowInTaskbar = visible;
            this.btnHideControls.Visible = visible;
            checkBoxShowDate.Visible = visible;
            labelTime.BackColor = visible ? Color.Empty : backgroundColorDialog.Color;
            this.axWindowsMediaPlayer.Visible = false;
        }

        private void btnHideControls_Click(object sender, EventArgs e)
        {
            showControlsToolStripMenuItem.Checked = false;
            notifyIconSystemTray.ShowBalloonTip
            (
                3, "Важная информация",
                "Для того чтобы вернуть элементы управления, дважды кликните по часам",
                ToolTipIcon.Info
            );
        }

        private void labelTime_DoubleClick(object sender, EventArgs e)
        {
            showControlsToolStripMenuItem.Checked = true;
        }

        private void notifyIconSystemTray_MouseMove(object sender, MouseEventArgs e)
        {
            notifyIconSystemTray.Text = "Current time:\n" + labelTime.Text;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void topmostToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = topmostToolStripMenuItem.Checked;
        }

        private void showControlsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            SetVisibility(((ToolStripMenuItem)sender).Checked);
        }

        private void showDateToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxShowDate.Checked = ((ToolStripMenuItem)sender).Checked;
        }

        private void checkBoxShowDate_CheckedChanged(object sender, EventArgs e)
        {
            showDateToolStripMenuItem.Checked = ((CheckBox)sender).Checked;
        }

        private void notifyIconSystemTray_DoubleClick(object sender, EventArgs e)
        {
            if(!this.TopMost)
            {
                this.TopMost = true;
                this.TopMost = false;
            }
        }

        private void foregorundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (foregroundColorDialog.ShowDialog(this) == DialogResult.OK)
                labelTime.ForeColor = foregroundColorDialog.Color;
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundColorDialog.ShowDialog(this) == DialogResult.OK)
                labelTime.BackColor = backgroundColorDialog.Color;
        }

        private void fontsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (chooseFontDialog.ShowDialog(this) == DialogResult.OK)
                labelTime.Font = chooseFontDialog.ChoosenFont;
        }

        void LoadSettings()
        {
            StreamReader sr = new StreamReader("settings.txt");
            List<string> settings = new List<string>();
            while (!sr.EndOfStream)
                settings.Add(sr.ReadLine());
            backgroundColorDialog.Color = Color.FromArgb(Convert.ToInt32(settings.ToArray()[0]));
            foregroundColorDialog.Color = Color.FromArgb(Convert.ToInt32(settings.ToArray()[1]));
            FontFile = settings.ToArray()[2];
            topmostToolStripMenuItem.Checked = bool.Parse(settings.ToArray()[3]);
            showDateToolStripMenuItem.Checked = bool.Parse(settings.ToArray()[4]);
            labelTime.Font = chooseFontDialog.SetFontFile(FontFile);
            chooseFontDialog.SetFontFile(FontFile);
            sr.Close();

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            object run = rk.GetValue("Clock");
            if (run != null) loadOnWindowsStartupToolStripMenuItem.Checked = true;
            rk.Dispose();
        }

        void SaveSettings()
        {
            StreamWriter sw = new StreamWriter("settings.txt");
            sw.WriteLine(backgroundColorDialog.Color.ToArgb());
            sw.WriteLine(foregroundColorDialog.Color.ToArgb());
            sw.WriteLine(chooseFontDialog.FontFile.Split('\\').Last());
            sw.WriteLine(topmostToolStripMenuItem.Checked);
            sw.WriteLine(showDateToolStripMenuItem.Checked);
            sw.Close();
            //Process.Start("notepad", "settings.txt");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            alarmList.SaveAlarmsToFile("alarms.csv");
        }

        private void loadOnWindowsStartupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (loadOnWindowsStartupToolStripMenuItem.Checked)
                rk.SetValue("Clock", Application.ExecutablePath);
            else
                rk.DeleteValue("Clock", false);
            rk.Dispose();
        }

        private void MainForm_FormClosing(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
            alarmList.SaveAlarmsToFile("alarm.csv");
        }

        private void alarmsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            alarmList.ShowDialog(this);
            GetNextAlarm();
        }

        void SetPlayerInvisible(object sender, _WMPOCXEvents_EndOfStreamEvent e)
        {
            axWindowsMediaPlayer.Visible = false;
        }

        void SetPlayerInvisible(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (axWindowsMediaPlayer.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
                axWindowsMediaPlayer.Visible = false;
        }

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
    }
}
