using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ID3v1Lib;


namespace KIDPlayer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
        }

        Form subForm;
        string mediaLibPath;
        int currentMediaIntex;
        WMPLib.WMPPlayState oldState;
        AxWMPLib.AxWindowsMediaPlayer media;



        private void MainForm_Load(object sender, EventArgs e)
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
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
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





        //menu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string mediaFile = openFileDialog1.FileName;
            if (mediaFile!="None Selected")
            {
                media.URL = mediaFile;
            }
        }


        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string[] mediaFiles = openFileDialog1.FileNames;

            if (mediaFiles[0] != "None Selected")
            {
                foreach (string element in mediaFiles)
                {
                    listBox1.Items.Add(new Mp3TagID3V1(element));

                }
            }

        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItems != null)
            {
                while (this.listBox1.SelectedItems.Count > 0)
                    this.listBox1.Items.Remove(this.listBox1.SelectedItems[0]);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            currentMediaIntex = listBox1.SelectedIndex;
            media.URL = ((Mp3TagID3V1)listBox1.SelectedItem).GetMp3FilePath();
        }


        private void lRCToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mediaLibPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                mediaLibPath = folderBrowserDialog1.SelectedPath;

                FillDirectoryTree(treeView1, mediaLibPath, true);
            }
        }



        //拖放操作
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string strItem = ((TreeNode)(e.Item)).FullPath;
            DoDragDrop(strItem, DragDropEffects.Move);

        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
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

        private void listBox1_DragDrop(object sender, DragEventArgs e)
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
        /********************************************************************/



        private void addToPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }
            }

        }



        private void media_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            WMPLib.WMPPlayState newState = (WMPLib.WMPPlayState)e.newState;
            switch (newState)
            {
                case WMPLib.WMPPlayState.wmppsMediaEnded:
                    {
                        if (currentMediaIntex + 1 < listBox1.Items.Count && oldState == WMPLib.WMPPlayState.wmppsPlaying)
                        {
                            media.URL = ((Mp3TagID3V1)listBox1.Items[++currentMediaIntex]).GetMp3FilePath();
                        }
                        else
                        {
                            currentMediaIntex = 0;
                        }
                        break;
                    }
                case WMPLib.WMPPlayState.wmppsReady:
                    {
                        media.Ctlcontrols.play();
                        break;
                    }

                default:
                    oldState = (WMPLib.WMPPlayState)e.newState;
                    break;
            }   
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void clearCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                tn.Checked = true;
                tn.Checked = false;
            }
        }

 




    }
}
