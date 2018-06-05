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
            aTimer.Enabled = true;
            axVLCPlugin21.playlist.play();
        }

        // Load File
        private void button6_Click(object sender, EventArgs e)
        {
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
            textBox2.Text = axVLCPlugin21.input.time.ToString(); // in miliseconds
            textBox4.Text = axVLCPlugin21.input.title.track.ToString();

            // due to a timing glitch in VLC, once we hit the title track, we need to clear the input time
            if (axVLCPlugin21.input.title.track > 0 && firsttime == true)
            {
                axVLCPlugin21.input.time = 0;
                firsttime = false;
            }
            
            if (axVLCPlugin21.input.title.track > 0 && listdone == false)
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
                    string[] FilterFileEntry = myFilterFile.ReadLine().Split(',');
                    StartList.Add(FilterFileEntry[0]);
                    ActionList.Add(FilterFileEntry[1]);
                    EndList.Add(FilterFileEntry[2]);

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
