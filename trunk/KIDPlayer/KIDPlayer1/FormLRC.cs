using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TTLRC;
using System.Net;

namespace KIDPlayer
{
    public partial class FormLRC : Form
    {
        LRClib q = null;
        public FormLRC()
        {
            InitializeComponent();

            q = new LRClib();
            q.WebException+=new EventHandler(q_WebException);
            q.SelectSong+=new EventHandler(q_SelectSong);

        }


        void q_SelectSong(object sender, EventArgs e)
        {
            XmlNodeList list = sender as XmlNodeList;
            if (list != null)
            {
                FormSelect song = new FormSelect(list);
                song.DataGrid.RowHeaderMouseDoubleClick += new DataGridViewCellMouseEventHandler(delegate(object _sender, DataGridViewCellMouseEventArgs _e)
                {
                    int index = _e.RowIndex;
                    if (index >= 0 & index < list.Count)
                    {
                        q.CurrentSong = list[_e.RowIndex];
                    }
                    song.Close();
                });
                song.ShowDialog();
            }
        }

        void q_WebException(object sender, EventArgs e)
        {
            WebException ex = sender as WebException;
            MessageBox.Show(ex.Message);
        }


        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string singer = textBoxArtist.Text.Trim();
            string title = textBoxTitle.Text.Trim();
            richTextBoxLRC.Text = "";
            //richTextBoxLRC.Text += "歌手:" + singer + ",歌名:" + title + "\n";
            richTextBoxLRC.Text += q.DownloadLrc(singer, title);
        }

        private void FormLRC_FormClosing(object sender, FormClosingEventArgs e)
        {
            MenuStrip ms = (MenuStrip)(this.Owner.Controls.Find("menuStrip1", true)[0]);
            ToolStripMenuItem tsmi = (ToolStripMenuItem)(ms.Items.Find("lRCToolStripMenuItem", true)[0]);
            tsmi.Checked = false;
        }


    }
}
