rmdir /S /Q "%UserProfile%\SkyDrive\Documentos\Proyectos\Blender\"
xcopy ".\*.*" "%UserProfile%\SkyDrive\Documentos\Proyectos\Blender\" /s /d /y /EXCLUDE:Exclude.txt
