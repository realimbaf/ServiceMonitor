
module ConfigApp {

    export class ResourceService {

        static getHostsList() {
            return $.ajax({
                url: "/api/hosts/",
                cache: false
            });
        }
        static getHostsGroupByService() {
            return $.ajax({
                url: "/api/hosts/groupByServices",
                cache: false
            });
        }
        static addHost(data: any) {
            var hostName = data.hostName();
            var sendData = {
                MonitorUrl : data.monitorUrl(),
                IsDebug: (data.isDebug() == 'true'),
                IsDirect: (data.isDirect() == 'true') 
            };
            return $.ajax({
                type: "POST",
                data: JSON.stringify(sendData),
                url: "/api/hosts/" + hostName + "/add",
                contentType: "application/json"
            });
        }
        static removeHost(data: any) {
            var hostName = data.hostName();
            var sendData = {
                MonitorUrl: data.monitorUrl(),
                RemoteIp : data.remoteIp(),
                IsDebug: (data.isDebug() == 'true'),
                IsDirect: (data.isDirect() == 'true')
            };
            return $.ajax({
                type: "POST",
                data: JSON.stringify(sendData),
                url: "/api/hosts/" + hostName + "/remove",
                contentType: "application/json"
            });
        }
        static getNetworkList() {
            return $.ajax({
                url: "/api/hosts/networkSettings",
                cache: false
            });
        }
        static getAllLogs() {
            return $.ajax({
                url: "/api/logs",
                cache: false
            });
        }
        static getLog(fileName) {
            return $.ajax({
                url: "/api/logs/" + fileName,
                cache: false
            });
        }
    }
    export class AlertModel {
        message: string;
        priority: string;
        constructor(message: string, priority: string) {
            this.message = message;
            this.priority = priority;
        }
    }
    export class AlertPriority {
        static Error = 'danger';
        static Warning = 'warning';
        static Success = 'success';
        static Info = 'info';
    }
}