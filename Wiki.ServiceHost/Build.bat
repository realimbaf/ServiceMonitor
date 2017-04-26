
if %computername%==VSAVINOV (

rem echo [%1]
    copy %1\Wiki.ServiceHost.XML %2\App_Data\XmlDocument.xml

	rem xcopy "%1*" "d:\Worck\Services\Wiki.Service\" /y
	copy %1\Wiki.ServiceHost.exe "D:\Worck\ServiceMonitor"
	rem copy %1\Wiki.ServiceHost.pdb %1\test\
)
