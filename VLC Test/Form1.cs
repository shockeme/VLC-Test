using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.IO;

namespace VLC_Test
{
    public partial class Form1 : Form
    {
        private static System.Timers.Timer aTimer;
        List<string> ActionList = new List<string>();
        List<string> StartList = new List<string>();
        List<string> EndList = new List<string>();
        int listIndex = 0;
        int sizeoflist = 0;
        int TitleTrack = 0;

        public Form1()
        {
            InitializeComponent();
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 20; //20 msecs

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.SynchronizingObject = this;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;
        }

        // Load and Play DVD 
        private void button1_Click(object sender, EventArgs e)
        {
            string temp; 
            string cddrive ="";
            int i;

            DriveInfo[] dr = System.IO.DriveInfo.GetDrives();
            for (i = 0; i < dr.Count(); i++)
            {
                if (dr[i].DriveType.ToString() == "CDRom")
                    cddrive = dr[i].ToString();
            }
            string[] temp1 = cddrive.Split('\\');

            temp = "dvd:///" + temp1[0].ToLower() + "/";
            //temp = "dvd:///e:/";
            axVLCPlugin21.playlist.add(temp, null);

            open_filter();

            aTimer.Enabled = true;
            axVLCPlugin21.playlist.play();
        }

        // Load File (*.avi, *.mkv, *.mp4, *.flv) - NOT DVDs
        private void button6_Click(object sender, EventArgs e)
        {

            // https://isubtitles.in/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "( *.avi;*.mkv;*.mp4;*.flv) |  *.avi;*.mkv;*.mp4;*.flv";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                axVLCPlugin21.playlist.add("file:///" + openFileDialog1.FileName, openFileDialog1.SafeFileName, null);
            }
            open_filter();
            TitleTrack = 0; // force title track to 0 for files (DVDs have their own title tracks)
            button12.Visible = false;
        }

        // Play File
        private void button2_Click(object sender, EventArgs e)
        {

            if (axVLCPlugin21.playlist.isPlaying)
            {
                axVLCPlugin21.playlist.togglePause();
                button2.Text = "Play";
            }
            else
            {
                // Start the timer
                button2.Text = "Pause";
                aTimer.Enabled = true;
                axVLCPlugin21.playlist.play();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.stop();
            button2.Text = "Play";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.togglePause();
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            textBox2.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");
            textBox1.Text = axVLCPlugin21.input.title.track.ToString();

            if (axVLCPlugin21.input.title.track == TitleTrack)
            {
                if (ActionList[listIndex] == "mute")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                        axVLCPlugin21.audio.mute = true;

                    if (axVLCPlugin21.input.time > Int32.Parse(EndList[listIndex]))
                    {
                        axVLCPlugin21.audio.mute = false;
                        listIndex++;
                    }
                }
                else if (ActionList[listIndex] == "skip")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                    {
                        axVLCPlugin21.input.time = Int32.Parse(EndList[listIndex]);
                        listIndex++;
                    }
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            open_filter();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time -= 5000;
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time += 5000;
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.video.toggleFullscreen();
        }

        private void open_filter()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "( *.txt) |  *.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader myFilterFile = new StreamReader(openFileDialog1.FileName);

                //string contents = myFilterFile.ReadToEnd();

                TitleTrack = Int32.Parse(myFilterFile.ReadLine());

                do
                {
                    //mute;00:01:52,840 --> 00:01:54,888
                    string[] FilterFileEntry = myFilterFile.ReadLine().Split(';');
                    ActionList.Add(FilterFileEntry[0]);
                    string[] times = FilterFileEntry[1].Split(new[] { " --> " }, StringSplitOptions.None);

                    times[0] = times[0].Substring(0, times[0].Length - 4);
                    TimeSpan ts = TimeSpan.Parse(times[0]);
                    StartList.Add(ts.TotalMilliseconds.ToString());

                    times[1] = times[1].Substring(0, times[1].Length - 4);
                    ts = TimeSpan.Parse(times[1]);
                    EndList.Add(ts.TotalMilliseconds.ToString());
                    
                } while (!myFilterFile.EndOfStream);
                myFilterFile.Close();

                sizeoflist = ActionList.Count;
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time -= 50000;
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time += 50000;
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (axVLCPlugin21.subtitle.count > 0)
                axVLCPlugin21.subtitle.track = 0;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            // Jump to title track 1 if pressed
            axVLCPlugin21.input.title.track = TitleTrack;
            axVLCPlugin21.input.time = 0;
        }
    }
}
