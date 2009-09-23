using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLib
{
    public class MediaClass
    {
        private WMPLib.WindowsMediaPlayer player;
        private String curFileName;                 //Current media file name.
        private bool isPaused;                      //Is current state pause?
        private double curPos;                      //Current time position, use to restore play from pause status.
        private double curLength;


        public MediaClass()
        {
            player = new WMPLib.WindowsMediaPlayer();
            player.uiMode = "None";
            player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
            player.MediaError += new WMPLib._WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
            curFileName = "";
            isPaused = false;
            curPos = 0.0;
        }


        private void Player_PlayStateChange(int NewState)
        {
            if (WMPLib.WMPPlayState.wmppsPaused == (WMPLib.WMPPlayState)NewState)
            {
                isPaused = true;
            }
            else if (WMPLib.WMPPlayState.wmppsPlaying == (WMPLib.WMPPlayState)NewState)
            {
                if (isPaused)
                {
                    isPaused = false;
                    player.controls.currentPosition += curPos;
                    curPos = 0;
                }
            }
            //else if(WMPLib.WMPPlayState.wmppsStopped == (WMPLib.WMPPlayState)NewState)
            //{
            //    isPaused = false;
            //}
        }


        private void Player_MediaError(object pMediaObject)
        {
            Console.WriteLine("play media error");
        }

        public int OpenMediaFile(String fileName)
        {
            int ret = 0;
            //Validate fileName

            //Filename legal
            curFileName = fileName;
            player.URL = curFileName;

            return ret;
        }


        public int SetVolume(int vol)
        {
            int ret = 0;
            //Validate vol
            if ((vol < 0) || (vol > 100))
            {
                //Volumn is highest or lowest.
                return ret;
            }
            //vol legal
            player.settings.volume = vol;


            return ret;
        }

        public int GetVolume()
        {
            int ret = 0;
            ret = player.settings.volume;
            return ret;
        }


        public void MediaPlay()
        {
            curLength = player.currentMedia.duration;
            player.controls.currentPosition = curPos;
            player.controls.play();
        }


        public void MediaPause()
        {
            curPos = player.controls.currentPosition;
            player.controls.pause();

        }


        public void MediaStop()
        {
            player.controls.currentPosition = 0;
            player.controls.stop();
        }

        public double MediaGetLength()
        {
            double ret = 0;

            if (WMPLib.WMPPlayState.wmppsPlaying != player.playState)
            {
                return ret;
            }

            curLength = player.currentMedia.duration;
            return curLength;
        }
        public string MediaGetLengthString()
        {
            string ret = "00:00";

            if (WMPLib.WMPPlayState.wmppsPlaying != player.playState)
            {
                return ret;
            }

            return player.currentMedia.durationString;

        }

        public void MediaSetPosition(double position)
        {
            player.controls.currentPosition = position;
        }
        public double MediaGetPosition()
        {
            double ret = 0;

            if (WMPLib.WMPPlayState.wmppsPlaying != player.playState)
            {
                return ret;
            }

            curPos = player.controls.currentPosition;
            return curPos;
        }
        public string MediaGetPositionString()
        {
            string ret = "00:00";

            if (WMPLib.WMPPlayState.wmppsPlaying != player.playState)
            {
                return ret;
            }

            return player.controls.currentPositionString;
        }

        public string MediaGetInfo()
        {
            string mediaInfo = "";
            //mediaInfo += player.currentMedia.name;
            //mediaInfo += "@";
            mediaInfo += "Title: " + player.currentMedia.getItemInfo("Title");
            mediaInfo += "@";
            mediaInfo += "Author: " + player.currentMedia.getItemInfo("Author");
            mediaInfo += "@";
            mediaInfo += "Description: " + player.currentMedia.getItemInfo("Description");
            mediaInfo += "@";
            mediaInfo += "Duration: " + player.currentMedia.getItemInfo("Duration");
            mediaInfo += "@";
            mediaInfo += "FileType: " + player.currentMedia.getItemInfo("FileType");
            mediaInfo += "@";
            mediaInfo += "FileSize: " + player.currentMedia.getItemInfo("FileSize");


            return mediaInfo;
        }


        public int MediaFastForward(int tick)
        {
            int ret = 0;
            //player.controls.fastForward();
            player.controls.currentPosition += tick;
            //MessageBox.Show("Don't support this function!");

            return ret;
        }


        public int MediaFastReverse(int tick)
        {
            int ret = 0;
            //player.controls.fastReverse();
            player.controls.currentPosition -= tick;
            //MessageBox.Show("Don't support this function!");
            return ret;
        }

        public string MediaGetFilePath()
        {
            return curFileName;
        }
    }
}
