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
using ID3;
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
		

		IWavePlayer waveOut;
		string filePath = null;
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
						//listBox1.Items.Add(new ID3Info(temp,true));
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
			//foreach (ID3Info m in listBox1.Items)
			//{
			//    sw.WriteLine(m.FilePath);
			//}
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

		
		void TreeView1ItemDrag(object sender, ItemDragEventArgs e)
		{
			string strItem = ((TreeNode)(e.Item)).FullPath;
			DoDragDrop(strItem, DragDropEffects.Move);
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
				//listBox1.Items.Add(new ID3Info(tn.FullPath,true));
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

		}
		
		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Application.Exit();
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

			if (String.IsNullOrEmpty(filePath))
			{
				//no file load
				//do something to load file
				return;
			}
			if (String.IsNullOrEmpty(filePath))
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

			mainOutputStream = CreateInputStream(filePath);
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

#region Choose audio api mode by system version
			//Get system version, if version>=6 then it is vista or above, use wasapi mode
			//if version<6, it is xp or below, use direct sound mode
			System.OperatingSystem osInfo = System.Environment.OSVersion;

			if (osInfo.Version.Major>=6)
			{
				waveOut = new WasapiOut(AudioClientShareMode.Shared, false, latency);
			}
			else
			{
				waveOut = new DirectSoundOut(latency);
			}
		  

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
#endregion

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

		private void listView1_DragEnter(object sender, DragEventArgs e)
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

		private void listView1_DragDrop(object sender, DragEventArgs e)
		{
			string tempFilePath = (string)e.Data.GetData(DataFormats.Text);
			if (!File.Exists(tempFilePath))
			{
				return;
			}
			Point Position = new Point();
			Position.X = e.X;
			Position.Y = e.Y;
			Position = listView1.PointToClient(Position);

			ID3Info tempID3Info=new ID3Info(tempFilePath,true);
			string[] tempStrings={tempID3Info.ID3v2Info.GetTextFrame("TRCK"),
								 tempID3Info.ID3v2Info.GetTextFrame("TIT2"),
								 tempID3Info.ID3v2Info.GetTextFrame("TPE1")};
			ListViewItem.ListViewSubItem tempSubItem1 = new ListViewItem.ListViewSubItem();
			tempSubItem1.Text = tempID3Info.ID3v2Info.GetTextFrame("TRCK");
			ListViewItem.ListViewSubItem tempSubItem2 = new ListViewItem.ListViewSubItem();
			tempSubItem2.Text = tempID3Info.ID3v2Info.GetTextFrame("TIT2");
			ListViewItem.ListViewSubItem tempSubItem3 = new ListViewItem.ListViewSubItem();
			tempSubItem3.Text = tempID3Info.ID3v2Info.GetTextFrame("TPE1");


			ListViewItem tempItem=new ListViewItem();
			tempItem.Tag = tempID3Info;
			tempItem.SubItems.Add(tempSubItem1);
			tempItem.SubItems.Add(tempSubItem2);
			tempItem.SubItems.Add(tempSubItem3);

			listView1.Items.Add(tempItem);
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			filePath = ((ID3Info)listView1.FocusedItem.Tag).FilePath;
			buttonStop_Click(this, null);
			buttonPlay_Click(this, null);
		}

		private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}
