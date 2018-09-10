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
        List<string> StartTime = new List<string>();
        List<string> EndTime = new List<string>();
        List<string> Action = new List<string>();
        int listIndex = 0;
        int sizeoflist = 0;
        int TitleTrack = 0;
        bool filtersOn = true;
        int delay = 0;

        public Form1()
        {
            InitializeComponent();
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 20; //20 msecs

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.SynchronizingObject = this; // synchronize the timer with this thread so we can display the timestamp from the movie

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            button12.Visible = false; // don't show any of the DVD buttons or titles yet
            textBox1.Visible = false;
            label1.Visible = false;
            label2.Visible = false;

            button7.Enabled = true;
            button11.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            button7.Visible = false;
            button11.Visible = false;
            button13.Visible = false;
            textBox7.Visible = false;
            textBox8.Visible = false;
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

            string val = "";
            ShowInputDialog(ref val);

            delay = Int32.Parse(val) * 1000;
            textBox9.Text = delay.ToString();

            textBox1.Visible = true; // only show title count if playing DVDs
            label1.Visible = true; // only show label for title count if playing DVDs
            label2.Visible = true; // only show skip to title if playing DVDs
            button12.Visible = true; // only allow skip to title if playing DVDs
            textBox9.Visible = true;
            label8.Visible = true;

            //delay = Int32.Parse(textBox9.Text) * 1000;
        }

        // Load File (*.avi, *.mkv, *.mp4, *.flv) - NOT DVDs
        private void button6_Click(object sender, EventArgs e)
        {
            // if a video is not already loaded (ie. not the first time), then we want to clear out the filters and the movie.
            if (axVLCPlugin21.playlist.itemCount > 0)
            {
                axVLCPlugin21.playlist.stop();
                axVLCPlugin21.playlist.items.clear();
                ActionList.Clear();
                StartList.Clear();
                EndList.Clear();                
            }

            // https://isubtitles.in/ place to download the SRT files which contain the subtitles.  Use these files to create filter files
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "( *.avi;*.mkv;*.mp4;*.flv,*.m4v) |  *.avi;*.mkv;*.mp4;*.flv;*.m4v"; // grab only movie files
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                axVLCPlugin21.playlist.add("file:///" + openFileDialog1.FileName, openFileDialog1.SafeFileName, null);

            open_filter(); // open the filter file
            TitleTrack = 0; // force title track to 0 for files (DVDs have their own title tracks)
            button12.Visible = false; // don't show DVD buttons when playing a file 
            button1.Visible = false;
            textBox9.Visible = false;
            label8.Visible = false;

        }

        // Play File
        private void button2_Click(object sender, EventArgs e)
        {
            if (axVLCPlugin21.playlist.isPlaying) // if the video is already playing, pause the video (and change the label to Play)
            {
                axVLCPlugin21.playlist.pause();
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
            axVLCPlugin21.playlist.stop(); // stop the video - for some reason when I use .stop(), the system crashes on a 2nd video. 
            button2.Text = "Play";
            button1.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.playlist.pause(); // pause the video
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e) // this is the routine that gets called every 20 msecs
        {
            textBox2.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");
            textBox1.Text = axVLCPlugin21.input.title.track.ToString(); // want to see which title track we are current on

            if (axVLCPlugin21.subtitle.count > 0) // some files have the subtitle turned on.
                axVLCPlugin21.subtitle.track = 0; // turn them off.            

            if (axVLCPlugin21.input.title.track == TitleTrack)
            {
                textBox3.Text = ActionList[listIndex];
                textBox4.Text = TimeSpan.FromMilliseconds(Int32.Parse(StartList[listIndex]) + delay).ToString(@"hh\:mm\:ss\.fff"); // show the starting timestamp
                textBox5.Text = TimeSpan.FromMilliseconds(Int32.Parse(EndList[listIndex]) + delay).ToString(@"hh\:mm\:ss\.fff"); // show the ending timestamp
                textBox6.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.length - axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");

                if (ActionList[listIndex] == "mute")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) + delay && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]) + delay) // see if the current time is after the start time but less than the end time.
                        axVLCPlugin21.audio.mute = true;
                }
                else if (ActionList[listIndex] == "skip")
                {
                    if (axVLCPlugin21.input.time > Int32.Parse(StartList[listIndex]) + delay && axVLCPlugin21.input.time < Int32.Parse(EndList[listIndex]) + delay)
                    {
                        axVLCPlugin21.input.time = Int32.Parse(EndList[listIndex]); // jump to the end of the time listed
                        listIndex++;
                        return;
                    }
                }

                if (axVLCPlugin21.input.time > Int32.Parse(EndList[listIndex]) + delay) // update to the next list if we skipped over it (unmute it even if not muted)
                {
                    axVLCPlugin21.audio.mute = false;
                    if (listIndex < StartList.Count - 1)
                        listIndex++;
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
            listIndex = 0; // reset filter index
        }

        private void button9_Click(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time += 5000; // need to clamp this when going beyond the movie time
            textBox2.Text = axVLCPlugin21.input.time.ToString();
            listIndex = 0; // reset filter index
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
                    TimeSpan ts1 = TimeSpan.Parse(times[0]); // convert the numbers to a time
                    StartList.Add(ts1.TotalMilliseconds.ToString()); // add the start time to the list

                    times[1] = times[1].Substring(0, times[1].Length - 4);
                    TimeSpan ts2 = TimeSpan.Parse(times[1]);
                    EndList.Add(ts2.TotalMilliseconds.ToString()); // add the end time to the list
                    
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
            listIndex = 0; // reset filter index
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            axVLCPlugin21.input.time += 50000; // forward wind 50 seconds (need to clamp to end of file)
            textBox2.Text = axVLCPlugin21.input.time.ToString();
            listIndex = 0; // reset filter index
        }

        private void button12_Click(object sender, EventArgs e) // skip to DVD title screen (title track is determined by the first line of the filter file
        {
            // Jump to title track 1 if pressed
            axVLCPlugin21.input.title.track = TitleTrack;
            axVLCPlugin21.input.time = 0;
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            StartTime.Add(TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff"));  
            textBox7.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");
            button11.Enabled = true;
            button7.Enabled = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            textBox8.Text = TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff");
            EndTime.Add(TimeSpan.FromMilliseconds(axVLCPlugin21.input.time).ToString(@"hh\:mm\:ss\.fff"));
            if (checkBox1.Checked == true)
                Action.Add("mute");
            else
                Action.Add("skip");
            button7.Enabled = true;
            button11.Enabled = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            int i;
            String Message = null;
            for (i = 0; i < EndTime.Count; i++)
            {
                Message += Action[i] + ";" + StartTime[i] + " --> " + EndTime[i] + "\r\n";
            }
            
            //Please edit filter file and put values in time order!
            Form2 form2 = new Form2(Message);
            form2.Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                checkBox2.Checked = false;
            else
                checkBox2.Checked = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
                checkBox1.Checked = false;
            else
                checkBox1.Checked = true;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (filtersOn == true) // show filter editing panel
            {
                checkBox1.Visible = true;
                checkBox2.Visible = true;
                button7.Visible = true;
                button11.Visible = true;
                button13.Visible = true;
                textBox7.Visible = true;
                textBox8.Visible = true;
                button14.Text = "Close Editor";
                filtersOn = false;
                textBox9.Visible = true;
                label8.Visible = true;
            }
            else { // close filter editing panel
                checkBox1.Visible = false;
                checkBox2.Visible = false;
                button7.Visible = false;
                button11.Visible = false;
                button13.Visible = false;
                textBox7.Visible = false;
                textBox8.Visible = false;
                button14.Text = "Create Filters";
                filtersOn = true;
                textBox9.Visible = false;
                label8.Visible = false;
             }
        }

        private static DialogResult ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "DVD Delay Time (in seconds)";

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }
    }
}
