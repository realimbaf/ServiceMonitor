var ConfigApp;
(function (ConfigApp) {
    var ResourceService = (function () {
        function ResourceService() {
        }
        ResourceService.getHostsList = function () {
            return $.ajax({
                url: "/api/hosts/",
                cache: false
            });
        };
        ResourceService.getHostsGroupByService = function () {
            return $.ajax({
                url: "/api/hosts/groupByServices",
                cache: false
            });
        };
        ResourceService.addHost = function (data) {
            var hostName = data.hostName();
            var sendData = {
                MonitorUrl: data.monitorUrl(),
                IsDebug: (data.isDebug() == 'true'),
                IsDirect: (data.isDirect() == 'true')
            };
            return $.ajax({
                type: "POST",
                data: JSON.stringify(sendData),
                url: "/api/hosts/" + hostName + "/add",
                contentType: "application/json"
            });
        };
        ResourceService.removeHost = function (data) {
            var hostName = data.hostName();
            var sendData = {
                MonitorUrl: data.monitorUrl(),
                RemoteIp: data.remoteIp(),
                IsDebug: (data.isDebug() == 'true'),
                IsDirect: (data.isDirect() == 'true')
            };
            return $.ajax({
                type: "POST",
                data: JSON.stringify(sendData),
                url: "/api/hosts/" + hostName + "/remove",
                contentType: "application/json"
            });
        };
        ResourceService.getNetworkList = function () {
            return $.ajax({
                url: "/api/hosts/networkSettings",
                cache: false
            });
        };
        ResourceService.getAllLogs = function () {
            return $.ajax({
                url: "/api/logs",
                cache: false
            });
        };
        ResourceService.getLog = function (fileName) {
            return $.ajax({
                url: "/api/logs/" + fileName,
                cache: false
            });
        };
        return ResourceService;
    }());
    ConfigApp.ResourceService = ResourceService;
    var AlertModel = (function () {
        function AlertModel(message, priority) {
            this.message = message;
            this.priority = priority;
        }
        return AlertModel;
    }());
    ConfigApp.AlertModel = AlertModel;
    var AlertPriority = (function () {
        function AlertPriority() {
        }
        AlertPriority.Error = 'danger';
        AlertPriority.Warning = 'warning';
        AlertPriority.Success = 'success';
        AlertPriority.Info = 'info';
        return AlertPriority;
    }());
    ConfigApp.AlertPriority = AlertPriority;
})(ConfigApp || (ConfigApp = {}));
//# sourceMappingURL=Resource.js.map