/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout/knockout.d.ts" />
var ConfigApp;
(function (ConfigApp) {
    var HostsInfoModel = (function () {
        function HostsInfoModel() {
            var _this = this;
            this.alerts = ko.observableArray();
            this.msgSuccess = ko.observable();
            this.activeTab = ko.observable();
            this.getPageModel = function (mode) {
                return {
                    read: function () {
                        return _this.activeTab() === mode;
                    },
                    write: function (value) {
                        if (value)
                            _this.activeTab(mode);
                    }
                };
            };
            this.hostsModel = ko.observable();
            this.serviceModel = ko.observable();
            this.networkSettingsmodel = ko.observable();
            this.logModel = ko.observable();
            this.isNetworkTab = ko.pureComputed(this.getPageModel("network"));
            this.isLogTab = ko.pureComputed(this.getPageModel("log"));
            this.isHostsTab = ko.pureComputed(this.getPageModel("hosts"));
            this.isServiceTab = ko.pureComputed(this.getPageModel("service"));
            HostsInfoModel.model = this;
            this.isHostsTab.subscribe(function (val) {
                if (!val) {
                    _this.hostsModel(null);
                }
                else {
                    _this.hostsModel(new HostsModel());
                }
            });
            this.isNetworkTab.subscribe(function (val) {
                if (!val) {
                    _this.networkSettingsmodel(null);
                }
                else {
                    _this.networkSettingsmodel(new HostsNetworkModel());
                }
            });
            this.isLogTab.subscribe(function (val) {
                if (!val) {
                    _this.logModel(null);
                }
                else {
                    _this.logModel(new LogsModel());
                }
            });
            this.isServiceTab.subscribe(function (val) {
                if (!val) {
                    _this.serviceModel(null);
                }
                else {
                    _this.serviceModel(new ServicesModel());
                }
            });
            this.isHostsTab(true);
        }
        HostsInfoModel.addAlert = function (msg, priority) {
            var model = new ConfigApp.AlertModel(msg, priority);
            HostsInfoModel.model.alerts.push(model);
            window.setTimeout(function () {
                HostsInfoModel.model.alerts.remove(model);
            }, 5000);
        };
        return HostsInfoModel;
    }());
    var LogsModel = (function () {
        function LogsModel() {
            var _this = this;
            this.files = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getAllLogs()
                    .done(function (data) {
                    var items = $.map(data, function (item) {
                        var itm = new LogModel(item);
                        return itm;
                    });
                    _this.files(items);
                })
                    .fail(function (e) {
                    HostsInfoModel.addAlert("Logs is NOT loaded" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.load();
        }
        return LogsModel;
    }());
    var LogModel = (function () {
        function LogModel(name) {
            var _this = this;
            this.file = ko.observable();
            this.logName = ko.observable();
            this.getLog = function (model) {
                ConfigApp.ResourceService.getLog(model.logName())
                    .done(function (data) {
                    _this.logName(model.logName());
                    _this.file(data);
                    HostsInfoModel.addAlert("Log " + _this.logName() + " is loaded", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e) {
                    HostsInfoModel.addAlert(("Log " + _this
                        .logName() + " is NOT loaded") +
                        e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.logName(name);
        }
        return LogModel;
    }());
    var ServicesModel = (function () {
        function ServicesModel() {
            var _this = this;
            this.servicesArray = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getHostsGroupByService()
                    .done(function (services) {
                    for (var service in services) {
                        if (services.hasOwnProperty(service)) {
                            var obj = { serviceName: service, hosts: services[service] };
                            _this.servicesArray.push(new ServiceModel(obj));
                        }
                    }
                })
                    .fail(function (data) {
                    console.log(data);
                });
            };
            this.load();
        }
        return ServicesModel;
    }());
    var HostsModel = (function () {
        function HostsModel() {
            var _this = this;
            this.hostsArray = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getHostsList()
                    .done(function (data) {
                    var items = $.map(data, function (item) {
                        var itm = new HostModel(item);
                        return itm;
                    });
                    _this.hostsArray(items);
                })
                    .fail(function (e) {
                    HostsInfoModel.addAlert("Log is NOT loaded " + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.load();
        }
        return HostsModel;
    }());
    var HostsNetworkModel = (function () {
        function HostsNetworkModel() {
            var _this = this;
            this.networkArray = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getNetworkList()
                    .done(function (data) {
                    var items = $.map(data, function (item) {
                        var itm = new NetworkModel(item);
                        return itm;
                    });
                    _this.networkArray(items);
                })
                    .fail(function (e) {
                    HostsInfoModel.addAlert("Networks info is NOT loaded " + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.addHost = function () {
                var newHost = new NetworkModel("");
                _this.networkArray.push(newHost);
                newHost.isVisibleSaveButton(true);
                newHost.isDisabled(false);
            };
            this.add = function (data) {
                ConfigApp.ResourceService.addHost(data)
                    .done(function () {
                    data.isDisabled(true);
                    HostsInfoModel.addAlert("Adding host is succesfull", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e) {
                    HostsInfoModel.addAlert("Adding host is failed " + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.remove = function (data) {
                ConfigApp.ResourceService.removeHost(data)
                    .done(function () {
                    HostsInfoModel.addAlert("Removing host is succesfull", ConfigApp.AlertPriority.Success);
                    _this.networkArray.remove(data);
                })
                    .fail(function (e) {
                    HostsInfoModel.addAlert("Removing host is failed " + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.load();
        }
        return HostsNetworkModel;
    }());
    var NetworkModel = (function () {
        function NetworkModel(item) {
            var _this = this;
            this.isVisibleSaveButton = ko.observable(false);
            this.isVisisbleUpdateButton = ko.observable(false);
            this.isDisabled = ko.observable(true);
            this.isDisabledTrigger = function () {
                _this.isDisabled(!_this.isDisabled());
                _this.isVisibleSaveButton(!_this.isVisibleSaveButton());
            };
            this.hostName = ko.observable();
            this.remoteIp = ko.observable();
            this.monitorUrl = ko.observable();
            this.isDebug = ko.observable();
            this.isDirect = ko.observable();
            item = item || {};
            this.hostName(item.HostName || "");
            this.remoteIp(item.RemoteIp || "");
            this.isDebug(item.IsDebug || false);
            this.monitorUrl(item.MonitorUrl || "");
            this.isDirect(item.IsDirect || false);
        }
        return NetworkModel;
    }());
    var HostModel = (function () {
        function HostModel(item) {
            var _this = this;
            this.isExpand = ko.observable(false);
            this.isExpandTrigger = function () {
                _this.isExpand(!_this.isExpand());
            };
            this.hostName = ko.observable();
            this.monitorUrl = ko.observable();
            this.remoteIp = ko.observable();
            this.lastBroadcast = ko.observable();
            this.lastUpdate = ko.observable();
            this.isActive = ko.observable();
            this.isDebug = ko.observable();
            this.isDirect = ko.observable();
            this.services = ko.observableArray();
            item = item || {};
            this.hostName(item.HostDetail.Hostname || "");
            this.monitorUrl(item.HostDetail.MonitorUrl || "");
            this.remoteIp(item.HostDetail.RemoteIp || "");
            this.lastBroadcast(item.HostDetail.LastBroadcast || "");
            this.lastUpdate(item.HostDetail.LastUpdate || "");
            this.isActive(item.HostDetail.IsActive || false);
            this.services(item.Services || null);
            this.isDebug(item.HostDetail.IsDebug || false);
            this.isDirect(item.HostDetail.IsDirect || false);
        }
        return HostModel;
    }());
    var ServiceModel = (function () {
        function ServiceModel(service) {
            var _this = this;
            this.serviceName = ko.observable();
            this.hostsArray = ko.observable();
            this.isExpand = ko.observable(false);
            this.isExpandTrigger = function () {
                _this.isExpand(!_this.isExpand());
            };
            this.serviceName(service.serviceName);
            this.hostsArray(service.hosts);
        }
        return ServiceModel;
    }());
    var ServiceInfo = (function () {
        function ServiceInfo(item) {
            this.id = ko.observable("");
            this.url = ko.observable();
            this.version = ko.observable("");
            this.activeVersion = ko.observable("");
            this.processId = ko.observable();
            this.isRun = ko.observable();
            this.isAuto = ko.observable();
            this.isManualStop = ko.observable();
            this.id(item.id);
            this.url(item.url);
            this.version(item.version);
            this.activeVersion(item.activeVersion);
            this.processId(item.processId);
            this.isRun(item.isRun);
            this.isAuto(item.isAuto);
            this.isManualStop(item.isManualStop);
        }
        return ServiceInfo;
    }());
    ko.applyBindings(new HostsInfoModel());
})(ConfigApp || (ConfigApp = {}));
//# sourceMappingURL=HostService.js.map