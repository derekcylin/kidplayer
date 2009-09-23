using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;


namespace Play
{
    public class APIClass
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(string lpszLongPath, string shortFile, int cchBuffer);
        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
    } 
    public class Sound
    {
        //播放
        public static void Play(string strFileName)
        {
            string buf = "";
            buf = buf.PadLeft(128, ' ');
            strFileName = "open " + Convert.ToChar(34) + strFileName + Convert.ToChar(34) + " alias media";
            APIClass.mciSendString(strFileName, buf, buf.Length, 0); //初始化音频文件 
            APIClass.mciSendString("play media", buf, buf.Length, 0); //播放音频文件 

        }

        //关闭
        public static void Stop()
        {
            APIClass.mciSendString("close media", "", 0, 0);
        }
        //暂停
        public static void Pause()
        {
            APIClass.mciSendString("pause media", "", 0, 0);
        }
   }
    public class Information
    {
        //总时间
        public static int GetMp3Lenth()
        {
            string durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            APIClass.mciSendString("status media length", durLength, durLength.Length, 0);
            durLength = durLength.Trim();

            if (string.IsNullOrEmpty(durLength))

                return 0;

            else
            {
                return Convert.ToInt32(durLength) / 1000;
            }
            //int h = s / 3600;

            //int m = (s - (h * 3600)) / 60;

            //s = s - (h * 3600 + m * 60);

            //vReturn = string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);  
        }
        //当前播放位置
        public static int CurrentPosition()
        {
            string buf = "";
            buf = buf.PadLeft(128, ' ');
            APIClass.mciSendString("status media position", buf, buf.Length, 0);
            buf = buf.Trim();
            if (string.IsNullOrEmpty(buf))
                return 0;
            else
                return (int)(Convert.ToDouble(buf)) / 1000;   
        }


        //进度控制
        public static bool SetProcess(int process)
        {
            bool result = false;
            string MciCommand = string.Format("seek media to {0}", process);
            int RefInt = APIClass.mciSendString(MciCommand, process.ToString(), 0, 0);
            APIClass.mciSendString("play media", null, 0, 0);
            if (RefInt == 0)
            {
                result = true;
            }

            return result;
        }   
        //声音控制
        public static bool SetValume(int Valume)
        {
            bool result = false;
            string MciCommand = string.Format("setaudio media volume to {0}", Valume);
            int RefInt = APIClass.mciSendString(MciCommand, null, 0, 0);
            if (RefInt == 0)
            {
                result = true;
            }

            return result;
        }
    }
}



