@REM set PATH=C:\Program Files\7-Zip;%PATH%
@SETLOCAL
@set ZIPFILE=%1
@set BASEDIR=%~dp0
@set CONFIG=%2
@call %~dp0collect.cmd %CONFIG%
@pushd %BASEDIR%_collect\%CONFIG%
7z a %BASEDIR%%ZIPFILE% -r
@popd
@ENDLOCAL