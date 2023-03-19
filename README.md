# EEGuildHuntScoreRecorder

Reads scores from Guild Hunt leaderboard. This code isn't pretty and I don't care.

To use it:

EEGuildHuntTool.exe setup <folder path>

This will build the folder structure for you to save your screenshots to. You should take screenshots of all scores (with the gray boxes FULLY VISIBLE for each score).
Screenshots MUST come from the Leaderboard screen, not the Challenge Record screen.

You can use this to reset the folders when you have recorded all your scores and copied the output CSV elsewhere.

Folder path can be absolute or relative to the executable.

EEGuildHuntTool.exe run <folder path>

This will parse all images for scores and save the result in a scores.csv file when finished. 

NOTE: the tesseract library is outputting warnings at the end. I also don't care about that.