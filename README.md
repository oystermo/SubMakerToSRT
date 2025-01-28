# SubMaker to SRT converter #

Subtitle format converter plugin[^1] for SubtitleEdit for .sub (json format) files created in SubMaker.
Files are converted into .srt format, empty (null) lines are removed. 

1. Compile plugin for Any CPU or download .dll file from https://drive.google.com/file/d/1jIb-iAcTCCqRRLQwlqZC4z0X5i1KsWyd/view?usp=drive_link
2. Copy dll file into SubtitleEdit plugins folder
3. Plugin should appear in "file" menu

[^1]: Plugin logic credits to Nikse (plugin based off of Haxor). Code includes MainForm, which appears crucial for compiling, though is not part of the converter logic (which is all under Plugin.cs)
