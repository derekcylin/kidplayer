/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2009/9/21
 * Time: 21:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using ID3v1Lib;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace KIDPlayer
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private string mediaLibPath;
		
		Form subForm;

        IWavePlayer waveOut;
        string fileName = null;
        WaveStream mainOutputStream;
        WaveChannel32 volumeStream;

		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		
		
		void MainFormLoad(object sender, System.EventArgs e)
		{
			if (File.Exists("config"))
            {
                StreamReader sr = File.OpenText("config");
                mediaLibPath = sr.ReadLine();
                if (Directory.Exists(mediaLibPath))
                {
                    FillDirectoryTree(treeView1, mediaLibPath, true);
                }
                while (!sr.EndOfStream)
                {
                    string temp = sr.ReadLine();
                    if (File.Exists(temp))
                    {
                        listBox1.Items.Add(new Mp3TagID3V1(temp));
                    }
                }
                sr.Close();
            }
		}
		
		void MainFormFormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{

            CloseWaveOut();  


			StreamWriter sw = File.CreateText("config");
            sw.WriteLine(mediaLibPath);
            foreach (Mp3TagID3V1 m in listBox1.Items)
            {
                sw.WriteLine(m.GetMp3FilePath());
            }
            sw.Flush();
            sw.Close();

		}
		
		void MediaLibraryToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                mediaLibPath = folderBrowserDialog1.SelectedPath;

                FillDirectoryTree(treeView1, mediaLibPath, true);
            }
		}

		
		void LRCToolStripMenuItemCheckedChanged(object sender, EventArgs e)
		{
			if (lRCToolStripMenuItem.Checked==true)
            {
                FormLRC formLRC = new FormLRC();
                if (fileName!=null)
                {
                    Mp3TagID3V1 info = new Mp3TagID3V1(fileName);
                    ((TextBox)formLRC.Controls.Find("textBoxTitle", false)[0]).Text = info.Title;
                    ((TextBox)formLRC.Controls.Find("textBoxArtist", false)[0]).Text = info.Artist;
                }
                formLRC.Owner=this;
                subForm=formLRC;
                formLRC.Show();
            }
            else
            {
                subForm.Close();
            }
		}
		
		void TreeView1ItemDrag(object sender, ItemDragEventArgs e)
		{
			string strItem = ((TreeNode)(e.Item)).FullPath;
            DoDragDrop(strItem, DragDropEffects.Move);
		}
		
		void ListBox1DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
		}
		
		void ListBox1DragDrop(object sender, DragEventArgs e)
		{
			string temp = (string)e.Data.GetData(DataFormats.Text);
            if (!File.Exists(temp))
            {
                return;
            }
            Point Position=new Point();
            Position.X = e.X;
            Position.Y = e.Y;
            Position = listBox1.PointToClient(Position);

            listBox1.Items.Add(new Mp3TagID3V1(temp));
		}
		
		void AddToPlaylistToolStripMenuItemClick(object sender, EventArgs e)
		{
			List<TreeNode> checkedNodes = new List<TreeNode>();
            TreeNodeCollection rootNodes = treeView1.Nodes;
    
            foreach (TreeNode tn in rootNodes)
            {
                FindCheckedNodes(tn, checkedNodes);
            }
            
            foreach (TreeNode tn in checkedNodes)
            {
                listBox1.Items.Add(new Mp3TagID3V1(tn.FullPath));
            }
		}
		
		void TreeView1AfterCheck(object sender, TreeViewEventArgs e)
		{
			            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }
            }
		}
		
		void ClearCheckedToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach (TreeNode tn in treeView1.Nodes)
            {
                tn.Checked = true;
                tn.Checked = false;
            }
		}
		
		void ClearToolStripMenuItemClick(object sender, EventArgs e)
		{
			listBox1.Items.Clear();
		}
		
		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		void ListBox1DoubleClick(object sender, EventArgs e)
		{
			fileName=((Mp3TagID3V1)listBox1.SelectedItem).GetMp3FilePath();
            buttonStop_Click(this, null);
            buttonPlay_Click(this, null);

		}
		
		void AddToolStripMenuItemClick(object sender, EventArgs e)
		{
			openFileDialog1.ShowDialog();
            string[] mediaFiles = openFileDialog1.FileNames;

            if (mediaFiles[0] != "openFileDialog1")
            {
                foreach (string element in mediaFiles)
                {
                    listBox1.Items.Add(new Mp3TagID3V1(element));
                }
            }
		}
		
		void RemoveToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (this.listBox1.SelectedItems != null)
            {
                while (this.listBox1.SelectedItems.Count > 0)
                    this.listBox1.Items.Remove(this.listBox1.SelectedItems[0]);
            }
		}
		
		void buttonPlay_Click(object sender, EventArgs e)
		{
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    return;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    return;
                }
            }

            // we are in a stopped state
            // TODO: only re-initialise if necessary

            if (String.IsNullOrEmpty(fileName))
            {
                //no file load
                //do something to load file
                return;
            }
            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }

            try
            {
                CreateWaveOut();
            }
            catch (Exception driverCreateException)
            {
                MessageBox.Show(String.Format("{0}", driverCreateException.Message));
                return;
            }

            mainOutputStream = CreateInputStream(fileName);
            trackBarPosition.Maximum = (int)mainOutputStream.TotalTime.TotalSeconds;
            labelTotalTime.Text = String.Format("{0:00}:{1:00}", (int)mainOutputStream.TotalTime.TotalMinutes,
                mainOutputStream.TotalTime.Seconds);
            trackBarPosition.TickFrequency = trackBarPosition.Maximum / 30;

            try
            {
                waveOut.Init(mainOutputStream);
            }
            catch (Exception initException)
            {
                MessageBox.Show(String.Format("{0}", initException.Message), "Error Initializing Output");
                return;
            }

            // not doing Volume on IWavePlayer any more
            volumeStream.Volume = volumeSlider1.Volume;
            waveOut.Play();
		}
		
		void buttonPause_Click(object sender, EventArgs e)
		{
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                }
            }
		}

        void buttonStop_Click(object sender, EventArgs e)
		{
            if (waveOut != null)
            {
                waveOut.Stop();
                trackBarPosition.Value = 0;
            }
		}


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                if (mainOutputStream.Position >= mainOutputStream.Length)
                {
                    buttonStop_Click(sender, e);
                }
                else
                {
                    TimeSpan currentTime = mainOutputStream.CurrentTime;
                    trackBarPosition.Value = (int)currentTime.TotalSeconds;
                    labelCurrentTime.Text = String.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes,
                        currentTime.Seconds);
                }
            }
        }

		
		
		void OpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			openFileDialog1.ShowDialog();
            string[] mediaFiles = openFileDialog1.FileNames;

            if (mediaFiles[0] != "openFileDialog1")
            {
                foreach (string element in mediaFiles)
                {
                    listBox1.Items.Add(new Mp3TagID3V1(element));
                }
            }
		}
		
		
		void MainFormResize(object sender, EventArgs e)
		{
			if(this.WindowState == FormWindowState.Minimized) 
  			{ 
   				this.Visible = false;
  			} 
		}
		
		void NotifyIcon1Click(object sender, EventArgs e)
		{
			this.Visible = true;
  			this.WindowState = FormWindowState.Normal; 
		}
		
		void Timer2Tick(object sender, EventArgs e)
		{
			timer2.Stop();
			fileName=((Mp3TagID3V1)listBox1.SelectedItem).GetMp3FilePath();
		}




        private WaveStream CreateInputStream(string fileName)
        {
            WaveChannel32 inputStream;
            if (fileName.EndsWith(".wav"))
            {
                WaveStream readerStream = new WaveFileReader(fileName);
                if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                {
                    readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                    readerStream = new BlockAlignReductionStream(readerStream);
                }
                if (readerStream.WaveFormat.BitsPerSample != 16)
                {
                    var format = new WaveFormat(readerStream.WaveFormat.SampleRate,
                        16, readerStream.WaveFormat.Channels);
                    readerStream = new WaveFormatConversionStream(format, readerStream);
                }
                inputStream = new WaveChannel32(readerStream);
            }
            else if (fileName.EndsWith(".mp3"))
            {
                WaveStream mp3Reader = new Mp3FileReader(fileName);
                WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);
                WaveStream blockAlignedStream = new BlockAlignReductionStream(pcmStream);
                inputStream = new WaveChannel32(blockAlignedStream);
            }
            else
            {
                throw new InvalidOperationException("Unsupported extension");
            }
            // we are not going into a mixer so we don't need to zero pad
            //((WaveChannel32)inputStream).PadWithZeroes = false;
            volumeStream = inputStream;
            var meteringStream = new MeteringStream(inputStream, inputStream.WaveFormat.SampleRate / 10);
     

            return meteringStream;
        }

        private void CreateWaveOut()
        {
            CloseWaveOut();
            int latency = 300;

            WaveCallbackInfo callbackInfo = WaveCallbackInfo.NewWindow();
            WaveOut outputDevice = new WaveOut(callbackInfo);
            outputDevice.DesiredLatency = latency;
            waveOut = outputDevice;

            //if (radioButtonWaveOut.Checked)
            //{
            //    WaveCallbackInfo callbackInfo = checkBoxWaveOutWindow.Checked ?
            //        WaveCallbackInfo.NewWindow() : WaveCallbackInfo.FunctionCallback();
            //    WaveOut outputDevice = new WaveOut(callbackInfo);
            //    outputDevice.DesiredLatency = latency;
            //    waveOut = outputDevice;
            //}
            //else if (radioButtonDirectSound.Checked)
            //{
            //    waveOut = new DirectSoundOut(latency);
            //}
            //else if (radioButtonAsio.Checked)
            //{
            //    waveOut = new AsioOut((String)comboBoxAsioDriver.SelectedItem);
            //    buttonControlPanel.Enabled = true;
            //}
            //else
            //{
            //    waveOut = new WasapiOut(
            //        checkBoxWasapiExclusiveMode.Checked ?
            //            AudioClientShareMode.Exclusive :
            //            AudioClientShareMode.Shared,
            //        checkBoxWasapiEventCallback.Checked,
            //        latency);
            //}
        }

        private void CloseWaveOut()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }
            if (mainOutputStream != null)
            {
                // this one really closes the file and ACM conversion
                volumeStream.Close();
                volumeStream = null;
                // this one does the metering stream
                mainOutputStream.Close();
                mainOutputStream = null;
            }
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }

        private void volumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            if (mainOutputStream != null)
            {
                volumeStream.Volume = volumeSlider1.Volume;
            }
        }

        private void trackBarPosition_Scroll(object sender, EventArgs e)
        {
            if (waveOut != null)
            {

                mainOutputStream.CurrentTime = TimeSpan.FromSeconds(trackBarPosition.Value);
            }
        }
	}
}
