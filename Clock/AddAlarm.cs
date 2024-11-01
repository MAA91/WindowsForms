using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clock
{
    public partial class AddAlarm : Form
    {
        public Alarm Alarm { get; set; }
        public AddAlarm()
        {
            InitializeComponent();
            Alarm = new Alarm();
            labelFilename.MaximumSize = new Size(this.Width - 25, 70);
            openFileDialogSound.Filter = "MP-3 (*.mp3)|*.mp3|Flac (*.flac)|*.flac| All Audio| *mp3; *.flac";
            //labelFilename.SetBounds(labelFilename.Location.X, labelFilename.Location.Y, this.Width - 10, 75);
        }

        public AddAlarm(Alarm alarm):this()
        {
            Alarm = alarm;
            InitWindowFromAlarm();
        }

        void InitWindowFromAlarm()
        {
            if(Alarm.Date != DateTime.MinValue)this.dateTimePickerDate.Value = Alarm.Date;
            this.dateTimePickerTime.Value = Alarm.Time;
            this.labelFilename.Text = Alarm.Filename;
            for (int i = 0; i < Alarm.Weekdays.Length; i++)
                checkedListBoxWeek.SetItemChecked(i, Alarm.Weekdays[i]);
        }

        void InitAlarm()
        {
            Alarm.Date = dateTimePickerDate.Enabled ? dateTimePickerDate.Value : DateTime.MinValue;
            Alarm.Time = dateTimePickerTime.Value;
            Alarm.Filename = labelFilename.Text;
            for (int i = 0; i < Alarm.Weekdays.Length; i++) Alarm.Weekdays[i] = false;
            for (int i = 0; i < checkedListBoxWeek.CheckedIndices.Count; i++)
            {
                //Alarm.Weekdays[i] = (checkedListBoxWeek.Items[i] as CheckBox).Checked; 
                Alarm.Weekdays[checkedListBoxWeek.CheckedIndices[i]] = true;
                Console.Write(checkedListBoxWeek.CheckedIndices[i] + "\t");
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            InitAlarm();
            this.Close();
        }

        private void checkBoxExectDate_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePickerDate.Enabled = ((CheckBox)sender).Checked;
        }

        private void labelFilename_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = true;
        }

        private void buttonChooseFile_Click(object sender, EventArgs e)
        {
            if(openFileDialogSound.ShowDialog() == DialogResult.OK)
            {
               Alarm.Filename =  labelFilename.Text = openFileDialogSound.FileName;
            }
        }
    }
}
