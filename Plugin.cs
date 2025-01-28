using Newtonsoft.Json;
using SubtitleEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SubMakerToSRT : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name
        {
            get { return "SubMakerToSRT"; }
        }

        string IPlugin.Text
        {
            get { return "Submaker to SRT converter"; }
        }

        decimal IPlugin.Version
        {
            get { return 1.5M; }
        }

        string IPlugin.Description
        {
            get { return "Converts to SRT"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "file"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        public class Title
        {
            public string text { get; set; }
            public int time_in { get; set; }
            public int time_out { get; set; }
            public int align { get; set; }
        }

        public class VideoData
        {
            public string video { get; set; }
            public List<Title> titles { get; set; }
        }

        public string DoAction(Form parentForm, string subRipText, double frameRate, string listViewLineSeparatorString, string srtFileName, string videoFileName, string rawText)
        {
            string contents = "";
            string final1 = "";

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Open subtitle file...";
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "Subtitle files|*.sub";
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return string.Empty;
                }
                using (var reader = new StreamReader(openFileDialog1.FileName))
                {
                    contents = reader.ReadToEnd();
                }

                contents = contents.Replace("\\r\\n", "\\n");

                var videoData = JsonConvert.DeserializeObject<VideoData>(contents);

                int titleNum = 1;
                foreach (var title in videoData.titles)
                {
                    string titleText = title.text;
                    int titleIn = title.time_in;
                    int titleOut = title.time_out;
                    if ((titleIn == 0) && (titleOut == 0))
                    {
                        if (titleText == null)
                        {
                            continue;
                        }
                        else
                        {
                            titleIn = -1;
                            titleOut = -1;
                        }
                    }
                    final1 += create_title(titleNum++, titleText, titleIn, titleOut) + "\n";
                }
                
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "SRT files (*.srt)|*.srt";
                    sfd.FilterIndex = 1;
                    string orig_file_address = openFileDialog1.FileName;
                    string truncated = orig_file_address.Substring(0, orig_file_address.Length - 4);
                    string toSave = truncated.Split('\\')[truncated.Split('\\').Length - 1];
                    sfd.FileName = toSave;
                    sfd.DefaultExt = "srt";
                    sfd.AddExtension = true;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(sfd.FileName, final1, Encoding.UTF8);
                    }
                }
            }
            return string.Empty;
        }
        public static string timecode_change(int n)
        {
            if (n == -1)
            {
                return "99:59:59,999";
            }
            int frames = n % 25;
            int format_frames = frames * 40;
            int seconds = n / 25;
            int minutes = seconds / 60;
            int hours = minutes / 60;
            seconds = seconds % 60;
            minutes = minutes % 60;
            string to_string_format_frames = "";
            string to_string_seconds = "";
            string to_string_minutes = "";
            string to_string_hours = "";
            if (format_frames < 10)
            {
                to_string_format_frames += "0";
            }
            if (format_frames < 100)
            {
                to_string_format_frames += "0";
            }
            to_string_format_frames += format_frames.ToString();
            if (seconds < 10)
            {
                to_string_seconds += "0";
            }
            to_string_seconds += seconds.ToString();
            if (minutes < 10)
            {
                to_string_minutes += "0";
            }
            to_string_minutes += minutes.ToString();
            if (hours < 10)
            {
                to_string_hours += "0";
            }
            to_string_hours += hours.ToString();
            return to_string_hours + ":" + to_string_minutes + ":" + to_string_seconds + "," + to_string_format_frames;
        }
        public static string create_timestamp(int start, int end)
        {
            return timecode_change(start) + " --> " + timecode_change(end);
        }
        public static string create_title(int number, string content, int startFrame, int endFrame)
        {
            return number.ToString() + "\n" + create_timestamp(startFrame, endFrame) + "\n" + content + "\n";
        }
    }
}