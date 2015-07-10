rmdir /S /Q "%UserProfile%\Desktop\SharpDX-Tests\"
xcopy ".\*.*" "%UserProfile%\Desktop\SharpDX-Tests\" /s /d /y /EXCLUDE:Exclude.txt
