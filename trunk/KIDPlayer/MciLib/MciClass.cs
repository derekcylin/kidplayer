using System;
using System.Collections.Generic;
using System.Text;
using System.Media;
using System.Runtime.InteropServices;
using System.IO;

namespace MciLib
{
    public class MciClass
    {
        public MciClass()
        {
        }
        public MciClass(string mediaFile)
        {
            this.OpenFile(mediaFile);
        }

        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        private static extern int mciSendString(
            string lpstrCommand,
            string lpstrReturnString,
            int uReturnLength,
            int hwndCallback
            );


        public bool OpenFile(string mediaFile)
        {
            this.mediaPath = mediaFile;
            bool result = false;
            int mciResult;

            buf = "";
            buf = buf.PadLeft(128, ' ');
            mciCommand = "open " + Convert.ToChar(34) + mediaFile + Convert.ToChar(34) + " alias media";

            mciSendString("close media", null, 0, 0);
            
            //初始化音频文件 
            mciResult = mciSendString(mciCommand, buf, buf.Length, 0);  
        
            if (mciResult==0)
            {
                result = true;

                mediaName = Path.GetFileName(mediaPath);
            }

            return result;
        }

        public bool PlayMusic()
        {
            buf = "";
            buf = buf.PadLeft(128, ' ');
            bool result = false;
            int mciResult = mciSendString("play media", buf, buf.Length, 0); //播放音频文件 
            if (mciResult == 0)
            {
                result = true;
                
                //SetVolume(volume);//当前音量大小  
                
            }
            return result;  
        }


        public bool SetVolume(int Valume)
        {
            bool result = false;
            string mciCommand = string.Format("setaudio media volume to {0}", Valume);
            int mciResult = mciSendString(mciCommand, null, 0, 0);
            if (mciResult == 0)
            {
                result = true;
            }

            return result;
        }


        public bool CloseMusic()
        {

            int mciResult = mciSendString("close media", null, 0, 0);

            if (mciResult == 0)
            {
                return true;
            }
            return false;

        }

        public bool StopMusic()
        {
            int mciResult = mciSendString("stop media", null, 0, 0);
            mciResult = mciSendString("seek media to start", null, 0, 0);
            if (mciResult==0)
            {
                return true;
            }
            return false;
        }

        public bool PauseMusic()
        {

            int mciResult = mciSendString("pause media", null, 0, 0);

            if (mciResult == 0)
            {
                return true;
            }
            return false;

        }  

        public int GetLength()
        {
            durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            mciSendString("status media length", durLength, durLength.Length, 0);
            durLength = durLength.Trim().Trim('\0');

            if (durLength=="")
            {
                return 0;
            }
            else
            {
                return (int)(Convert.ToDouble(durLength));
            }
        }

        public string GetLengthString()
        {

            durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            mciSendString("status media length", durLength, durLength.Length, 0);
            durLength = durLength.Trim().TrimEnd('\0');

            if (durLength == "")
            {
                return "00:00";
            }
            else
            {
                int s = Convert.ToInt32(durLength) / 1000;
                int h = s / 3600;
                int m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);

                return string.Format("{0:D2}:{1:D2}", m, s);

            }

        }


        public int GetPosition()
        {
            durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            mciSendString("status media position", durLength, durLength.Length, 0);
            durLength = durLength.Trim().TrimEnd('\0');

            if (durLength == "")
            {
                return 0;
            }
            else
            {
                return (int)(Convert.ToDouble(durLength));
            }
        }


        public string GetPositionString()
        {

            durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            mciSendString("status media position", durLength, durLength.Length, 0);
            durLength = durLength.Trim().TrimEnd('\0');

            if (durLength == "")
            {
                return "00:00";
            }
            else
            {
                int s = Convert.ToInt32(durLength) / 1000;
                int h = s / 3600;
                int m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);

                return string.Format("{0:D2}:{1:D2}", m, s);
            }

        }

        public bool SetPosition(int position)
        {

            string mciCommand = string.Format("seek media to {0}", position);
            int mciResult = mciSendString(mciCommand, position.ToString(), 0, 0);
            if (mciResult == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        public enum Playstate : byte
        {
            Stopped = 1,
            Playing = 2,
            Pause = 3
        }

        public enum PlayStyle : byte
        {
            顺序 = 1,
            随机 = 2,
            循环 = 3
        }

        private string mediaPath;

        public string GetMediaPath()
        {
            return mediaPath;
        }
        private string mediaName;

        public string GetMediaName()
        {
            return mediaName;
        }

        private string durLength = "";

        string buf = "";
        

        private string mciCommand;
    }
}
