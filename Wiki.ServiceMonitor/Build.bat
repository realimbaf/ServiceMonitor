
if %computername%==VSAVINOV (
net stop Wiki.ServiceMonitor
rem echo [%1]
	xcopy "%1*" "d:\Worck\ServiceMonitor\" /y
	rem del d:\Worck\ServiceMonitor\log /S /Q /F

	rem net start Wiki.ServiceMonitor
)

if %computername%==VNEDVIGIN (
net stop Wiki.Monitoring
rem echo [%1]
	xcopy "%1*" "D:\Infrastucture\Monitoring\" /y
	xcopy "D:\Infrastucture\Monitoring\backup\*" "D:\Infrastucture\Monitoring\" /y
	rem del d:\Worck\ServiceMonitor\log /S /Q /F

	net start Wiki.Monitoring
)

if %computername%==MISHENKO (
net stop Wiki.Monitoring
rem echo [%1]
	xcopy "%1*" "D:\Work\ServiceMonitor" /y
	xcopy "D:\Work\ServiceMonitor\backup\*" "D:\Work\ServiceMonitor" /y
	rem del d:\Worck\ServiceMonitor\log /S /Q /F

	rem net start Wiki.Monitoring
)
if %computername%==out4 (
net stop Wiki.Monitoring
rem echo [%1]
	xcopy "%1*" "C:\ServiceMonitor" /y
	xcopy "C:\ServiceMonitor\backup\*" "C:\ServiceMonitor" /y
	rem del d:\Worck\ServiceMonitor\log /S /Q /F

	rem net start Wiki.Monitoring
)
