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
        bool firsttime = true;
        bool listdone = false;
        bool streaming = true;

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
            streaming = false;

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
            aTimer.Enabled = true;
            axVLCPlugin21.playlist.play();
        }

        // Load File
        private void button6_Click(object sender, EventArgs e)
        {

            // https://isubtitles.in/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                axVLCPlugin21.playlist.add("file:///" + openFileDialog1.FileName, openFileDialog1.SafeFileName, null);
            }
        }

        // Play File
        private void button2_Click(object sender, EventArgs e)
        {
            // Start the timer
            streaming = true;
            aTimer.Enabled = true;
            axVLCPlugin21.playlist.play();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.togglePause();
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //textBox2.Text = axVLCPlugin21.input.time.ToString(); // in miliseconds
            textBox2.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");

            textBox4.Text = axVLCPlugin21.input.title.track.ToString();

            // due to a timing glitch in VLC, once we hit the title track, we need to clear the input time
            if (axVLCPlugin21.input.title.track > 0 && firsttime == true)
            {
                axVLCPlugin21.input.time = 0;
                firsttime = false;
            }
            
            if ((axVLCPlugin21.input.title.track > 0 || streaming == true) && listdone == false)
            {
                if (ActionList[listIndex] == "mute")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                        axVLCPlugin21.audio.mute = true;

                    if (axVLCPlugin21.input.time > Int32.Parse(EndList[listIndex]))
                    {
                        axVLCPlugin21.audio.mute = false;
                        listIndex++;
                        if (listIndex >= sizeoflist)
                        {
                            //aTimer.Enabled = false;
                            listdone = true;
                            textBox2.Text = "End of List";
                            return;
                        }
                    }
                }
                else if (ActionList[listIndex] == "skip")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                    {
                        axVLCPlugin21.input.time = Int32.Parse(EndList[listIndex]);
                        listIndex++;

                        if (listIndex >= sizeoflist)
                        {
                            //aTimer.Enabled = false;
                            listdone = true;
                            textBox2.Text = "End of List";
                            return;
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox4.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader myFilterFile = new StreamReader(openFileDialog1.FileName);
                do
                {
                    //mute;00:01:52,840 --> 00:01:54,888
                    string[] FilterFileEntry = myFilterFile.ReadLine().Split(';');
                    ActionList.Add(FilterFileEntry[0]);                    
                    string[] times = FilterFileEntry[1].Split(new [] { " --> " }, StringSplitOptions.None);

                    times[0] = times[0].Substring(0, times[0].Length - 4);
                    TimeSpan ts = TimeSpan.Parse(times[0]);
                    StartList.Add(ts.TotalMilliseconds.ToString());

                    times[1] = times[1].Substring(0, times[1].Length - 4);
                    ts = TimeSpan.Parse(times[1]);
                    EndList.Add(ts.TotalMilliseconds.ToString());

                    //StartList.Add(times[0].ToString("HH:mm:ss tt"));
                    //EndList.Add(times[1].ToString("HH:mm:ss tt"));



                    //StartList.Add(FilterFileEntry[0]);
                    //ActionList.Add(FilterFileEntry[1]);
                    //EndList.Add(FilterFileEntry[2]);

                } while (!myFilterFile.EndOfStream);
                myFilterFile.Close();

                sizeoflist = ActionList.Count;
            }
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
    }
}
