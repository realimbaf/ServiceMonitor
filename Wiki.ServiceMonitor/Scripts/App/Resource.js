/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
var ConfigApp;
(function (ConfigApp) {
    var ResourceService = (function () {
        function ResourceService() {
        }
        ResourceService.getServiceList = function () {
            return $.ajax({
                url: "services",
                cache: false
            });
        };
        ResourceService.getConcreteService = function (id) {
            return $.ajax({
                url: "/services/status/" + id,
                cache: false
            });
        };
        ResourceService.getNugetList = function () {
            return $.ajax({
                url: "/services/list",
                cache: false
            });
        };
        ResourceService.getNugetVersions = function (name) {
            return $.ajax({
                url: "/services/list/" + name,
                cache: false
            });
        };
        ResourceService.startService = function (name, ver) {
            return $.ajax({
                url: "/services/start/" + name + "?ver=" + ver,
                cache: false
            });
        };
        ResourceService.stopService = function (name) {
            return $.ajax({
                url: "/services/stop/" + name,
                cache: false
            });
        };
        ResourceService.getAllLog = function () {
            return $.ajax({
                url: "/log/all/",
                cache: false
            });
        };
        ResourceService.getLogById = function (id) {
            return $.ajax({
                url: "/log/all/" + id
            });
        };
        ResourceService.getLog = function (file) {
            return $.ajax({
                url: "/log/file/" + file,
                cache: false
            });
        };
        ResourceService.setActive = function (id) {
            return $.ajax({
                url: "/services/setactive/" + id,
                cache: false
            });
        };
        ResourceService.removeActive = function (id) {
            return $.ajax({
                url: "/services/removeactive/" + id,
                cache: false
            });
        };
        ResourceService.getSettings = function (id) {
            return $.ajax({
                url: "/settings/" + id,
                cache: false
            });
        };
        ResourceService.saveSettings = function (data, id) {
            return $.ajax({
                type: "POST",
                data: JSON.stringify(data),
                url: "/settings/" + id,
                contentType: "application/json"
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
        return AlertPriority;
    }());
    AlertPriority.Error = 'danger';
    AlertPriority.Warning = 'warning';
    AlertPriority.Success = 'success';
    AlertPriority.Info = 'info';
    ConfigApp.AlertPriority = AlertPriority;
    var ConcreteServiceModel = (function () {
        function ConcreteServiceModel() {
            var _this = this;
            this.url = window.location.href.split[2];
            this.service = ko.observable();
            this.load = function (id) {
                ResourceService.getConcreteService(id)
                    .done(function (data) {
                    _this.service(data);
                });
            };
            this.load(this.url);
        }
        return ConcreteServiceModel;
    }());
    ConfigApp.ConcreteServiceModel = ConcreteServiceModel;
    var ServiceModel = (function () {
        function ServiceModel(item) {
            this.id = ko.observable('');
            this.url = ko.observable();
            this.version = ko.observable('');
            this.activeVersion = ko.observable('');
            this.processId = ko.observable();
            this.isRun = ko.observable();
            this.isAuto = ko.observable();
            this.isManualStop = ko.observable();
            item = item || {};
            this.id(item.id || '');
            this.url(item.url || '');
            this.version(item.version || '');
            this.activeVersion(item.activeVersion || '');
            this.processId(item.processId || 0);
            this.isRun(item.isRun || false);
            this.isAuto(item.isAuto || false);
            this.isManualStop(item.isManualStop || false);
        }
        return ServiceModel;
    }());
    ConfigApp.ServiceModel = ServiceModel;
})(ConfigApp || (ConfigApp = {}));
