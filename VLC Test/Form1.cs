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
        private bool mute;
        List<string> ActionList = new List<string>();
        List<string> StartList = new List<string>();
        List<string> EndList = new List<string>();
        int listIndex = 0;

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
            mute = false;

        }

        // Load and Play DVD 
        private void button1_Click(object sender, EventArgs e)
        {
            string temp;
            temp = "dvd:///e:/";
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

            /*if (axVLCPlugin21.input.title.track > 0)
            {
                if (ActionList[listIndex] == "mute")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                        axVLCPlugin21.audio.mute = true;
                    else
                        axVLCPlugin21.audio.mute = false;
                }
                else
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                        axVLCPlugin21.input.time = Int32.Parse(EndList[listIndex]);
                    listIndex++;
                }
            }*/
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
                    ActionList.Add(FilterFileEntry[0]);
                    StartList.Add(FilterFileEntry[1]);
                    EndList.Add(FilterFileEntry[2]);

                } while (!myFilterFile.EndOfStream);
                myFilterFile.Close();
            }
        }
    }
}
