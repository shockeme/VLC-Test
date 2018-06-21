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

            button12.Visible = false;
            textBox1.Visible = false;
            label1.Visible = false;
            label2.Visible = false;

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
                if (dr[i].DriveType.ToString() == "CDRom") // find the drive that is the CDRom.
                    cddrive = dr[i].ToString();
            }
            string[] temp1 = cddrive.Split('\\');

            temp = "dvd:///" + temp1[0].ToLower() + "/";
            axVLCPlugin21.playlist.add(temp, null);

            open_filter();
            textBox1.Visible = true;
            label1.Visible = true;
            label2.Visible = true;
            button12.Visible = true;

            //aTimer.Enabled = true;
            //axVLCPlugin21.playlist.play();
        }

        // Load File (*.avi, *.mkv, *.mp4, *.flv) - NOT DVDs
        private void button6_Click(object sender, EventArgs e)
        {
            // https://isubtitles.in/ place to download the SRT files which contain the subtitles.  Use these files to create filter files
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "( *.avi;*.mkv;*.mp4;*.flv) |  *.avi;*.mkv;*.mp4;*.flv"; // grab only movie files
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                axVLCPlugin21.playlist.add("file:///" + openFileDialog1.FileName, openFileDialog1.SafeFileName, null);

            open_filter(); // open the filter file
            TitleTrack = 0; // force title track to 0 for files (DVDs have their own title tracks)
            button12.Visible = false;
            button1.Visible = false;
        }

        // Play File
        private void button2_Click(object sender, EventArgs e)
        {
            if (axVLCPlugin21.playlist.isPlaying) // if the video is already playing, pause the video (and change the label to Play)
            {
                axVLCPlugin21.playlist.togglePause();
                button2.Text = "Play";
            } 
            else  // if the movie isn't playing, start it up, or start it again from a pause. 
            {
                // Start the timer
                button2.Text = "Pause";
                aTimer.Enabled = true;
                axVLCPlugin21.playlist.play();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.stop(); // stop the video
            button2.Text = "Play";
            button1.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.togglePause(); // pause the video
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            textBox2.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");
            textBox1.Text = axVLCPlugin21.input.title.track.ToString(); // want to see which title track we are current on

            if (axVLCPlugin21.subtitle.count > 0) // some files have the subtitle turned on.
                axVLCPlugin21.subtitle.track = 0; // turn them off.            

            if (axVLCPlugin21.input.title.track == TitleTrack)
            {
                if (ActionList[listIndex] == "mute")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex])) // see if the current time is after the start time but less than the end time.
                        axVLCPlugin21.audio.mute = true;

                    if (axVLCPlugin21.input.time > Int32.Parse(EndList[listIndex]))
                    {
                        axVLCPlugin21.audio.mute = false;
                        if (listIndex < StartList.Count-1)
                            listIndex++;
                    }
                }
                else if (ActionList[listIndex] == "skip")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]))
                    {
                        axVLCPlugin21.input.time = Int32.Parse(EndList[listIndex]); // jump to the end of the time listed
                        if (listIndex < StartList.Count-1)
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
            if (axVLCPlugin21.input.time - 5000 < 0)
                axVLCPlugin21.input.time = 0;
            else
                axVLCPlugin21.input.time -= 5000; // rewind 50 seconds (need to clamp to beginning of file)
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time += 5000; // need to clamp this when going beyond the movie time
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
                // 1
                TitleTrack = Int32.Parse(myFilterFile.ReadLine()); // the first row of the filterfile is the title track number (find it by using VLC and playing the video.  Check the playback menu and pick the one that is the movie.

                do
                {
                    //mute;00:01:52,840 --> 00:01:54,888
                    string[] FilterFileEntry = myFilterFile.ReadLine().Split(';'); // split based on the ; to split the action and the times
                    ActionList.Add(FilterFileEntry[0]); // add the "mute" or "skip" to the list.
                    string[] times = FilterFileEntry[1].Split(new[] { " --> " }, StringSplitOptions.None); // split based on the --> to split the start and end times

                    times[0] = times[0].Substring(0, times[0].Length - 4); // remove the last 4 characters of the string as we don't really need them 
                    TimeSpan ts = TimeSpan.Parse(times[0]); // convert the numbers to a time
                    StartList.Add(ts.TotalMilliseconds.ToString()); // add the start time to the list

                    times[1] = times[1].Substring(0, times[1].Length - 4);
                    ts = TimeSpan.Parse(times[1]);
                    EndList.Add(ts.TotalMilliseconds.ToString()); // add the end time to the list
                    
                } while (!myFilterFile.EndOfStream);
                myFilterFile.Close();

                sizeoflist = ActionList.Count;
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (axVLCPlugin21.input.time - 50000 < 0)
                axVLCPlugin21.input.time = 0;
            else
                axVLCPlugin21.input.time -= 50000; // rewind 50 seconds (need to clamp to beginning of file)
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time += 50000; // forward wind 50 seconds (need to clamp to end of file)
            textBox2.Text = axVLCPlugin21.input.time.ToString();
        }

        private void button12_Click(object sender, EventArgs e) // skip to DVD title screen (title track is determined by the first line of the filter file
        {
            // Jump to title track 1 if pressed
            axVLCPlugin21.input.title.track = TitleTrack;
            axVLCPlugin21.input.time = 0;
        }
    }
}
