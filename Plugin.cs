using SubtitleEdit;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string DoAction(Form parentForm, string subRipText, double frameRate, string listViewLineSeparatorString, string srtFileName, string videoFileName, string rawText)
        {
            string text1 = "";
            string final1 = "";

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Open subtitle file...";
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "Subtitle files|*.sub";
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return string.Empty;
                }
                using (var reader = new StreamReader(openFileDialog1.FileName))
                {
                    text1 = reader.ReadToEnd();
                }

                string no_r = remove_r(text1);
                string filtered = filter_content(no_r);
                string[] subs = no_r.Split('\n');
                int k = 0;
                int count = 0;
                while (k < subs.Length - 3)
                {
                    if (subs[k].Contains("text"))
                    {
                        count++;
                        string current_r = remove_r(subs[k]);
                        string current_f = filter_content(current_r);
                        int curr_in = -1;
                        int curr_out = -1;
                        if ((subs[k + 1].Contains("time_in")) && (subs[k + 2].Contains("time_out")))
                        {
                            curr_in = filter_time(subs[k + 1]);
                            curr_out = filter_time(subs[k + 2]);
                            k += 2;
                        }
                        final1 += create_title(count, current_f, curr_in, curr_out)+"\n";
                    }
                    k++;
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
            while (seconds > 59)
            {
                seconds = seconds - 60;
            }
            while (minutes > 59)
            {
                minutes = minutes - 60;
            }
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
        public static string remove_r(string text)
        {
            string new_str = "";
            new_str = text.Replace("\\r\\n", "\\n");
            return new_str;
        }
        public static string[] separate_to_titles(string text)
        {
            return text.Split('\n');
        }
        public static string filter_content(string content)
        {
            int brackets = 0;
            int i = 0;
            string new_content = "";
            while (i < content.Length - 3)
            {
                if ((content[i].ToString() == "\"") && (brackets <= 2))
                {
                    brackets++;
                }
                else if (brackets == 3)
                {
                    if ((content[i].ToString() == "\\") && (content[i+1].ToString() == "\""))
                    {
                        i++;
                        continue;
                    }
                    else
                    {
                        new_content += content[i].ToString();
                    }
                }
                i++;
            }
            new_content = new_content.Replace("\\n", "\n");
            return new_content;
        }
        public static int filter_time(string content)
        {
            int space = 0;
            int i = 0;
            string new_content = "";
            while (i < content.Length - 2)
            {
                if ((content[i].ToString() == ":") && ((content[i + 1].ToString() == " ")))
                {
                    space++;
                }
                else if (space == 1)
                {
                    new_content += content[i].ToString();
                }
                i++;
            }
            int extracted;
            try {
                extracted = Int32.Parse(new_content);
            }
            catch (Exception E)
            {
                extracted = -1;
            }
            
            return extracted;
        }

        public static string create_title(int number, string content, int start, int end)
        {
            return number.ToString() + "\n" + create_timestamp(start, end) + "\n" + content + "\n";
        }
    }
}