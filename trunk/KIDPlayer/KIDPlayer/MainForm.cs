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

namespace KIDPlayer
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private string mediaLibPath;
		private WMPLib.WindowsMediaPlayer media;
		int currentMediaIntex;
		Form subForm;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			media=new WMPLib.WindowsMediaPlayer();
			media.PlayStateChange+=new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(PlayStateChange);
			media.uiMode="none";
		}
		
		private void PlayStateChange(int newState)
		{
			if (media.playState==WMPLib.WMPPlayState.wmppsMediaEnded)
            {
				int temp=listBox1.Items.Count-1;
				if (currentMediaIntex!=temp)
				{
					currentMediaIntex++;
					listBox1.ClearSelected();
					//listBox1.SetSelected(currentMediaIntex-1,false);
					listBox1.SetSelected(currentMediaIntex,true);
					timer2.Start();
				}
				else
				{	
					currentMediaIntex=0;
					listBox1.ClearSelected();
					//listBox1.SetSelected(temp,false);
					listBox1.SetSelected(currentMediaIntex,true);
					timer2.Start();
				}
            }
       
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
                if (media.currentMedia!=null)
                {
                    Mp3TagID3V1 info = new Mp3TagID3V1(media.currentMedia.sourceURL);
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
			currentMediaIntex = listBox1.SelectedIndex;
			media.settings.volume=trackBar2.Value;
			media.URL=((Mp3TagID3V1)listBox1.SelectedItem).GetMp3FilePath();
			timer1.Start();
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
		
		void Button1Click(object sender, EventArgs e)
		{
			media.controls.play();
			timer1.Start();
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			media.controls.pause();
			timer1.Stop();
		}
		
		void Button3Click(object sender, EventArgs e)
		{
			media.controls.stop();
			label1.Text="00:00";
			trackBar1.Value=0;
			timer1.Stop();
		}
		
		void Timer1Tick(object sender, EventArgs e)
		{
//			if (media.controls.currentPositionString==null) 
//			{
//				label1.Text="00:00";	
//			}
//			else
//			{
//				label1.Text=media.controls.currentPositionString;
//			}
			
			label1.Text=media.controls.currentPositionString;
			label2.Text=media.currentMedia.durationString;
			trackBar1.Value=(int)(500.0*media.controls.currentPosition/media.currentMedia.duration);
		}
		
		
		void TrackBar1MouseCaptureChanged(object sender, EventArgs e)
		{
			media.controls.currentPosition=trackBar1.Value/500.0*media.currentMedia.duration;
			timer1.Start();
		}
		
		void TrackBar1MouseDown(object sender, MouseEventArgs e)
		{
			timer1.Stop();
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
		
		void TrackBar2ValueChanged(object sender, EventArgs e)
		{
			media.settings.volume=trackBar2.Value;
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
			media.URL=((Mp3TagID3V1)listBox1.SelectedItem).GetMp3FilePath();
		}
	}
}
