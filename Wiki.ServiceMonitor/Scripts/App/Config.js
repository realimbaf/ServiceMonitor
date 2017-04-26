/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
var ConfigApp;
(function (ConfigApp) {
    var ConfigPage = (function () {
        function ConfigPage() {
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
            this.nugetModel = ko.observable();
            this.serviceModel = ko.observable();
            this.logModel = ko.observable();
            this.isServiceTab = ko.pureComputed(this.getPageModel("service"));
            this.isLogTab = ko.pureComputed(this.getPageModel("log"));
            this.isNugetTab = ko.pureComputed(this.getPageModel("nuget"));
            ConfigPage.model = this;
            this.isNugetTab.subscribe(function (val) {
                if (!val) {
                    _this.nugetModel(null);
                }
                else {
                    _this.nugetModel(new NugetListModel());
                }
            });
            this.isServiceTab.subscribe(function (val) {
                if (!val) {
                    _this.serviceModel(null);
                }
                else {
                    _this.serviceModel(new ServiceListModel());
                }
            });
            this.isLogTab.subscribe(function (val) {
                if (!val) {
                    _this.logModel(null);
                }
                else {
                    _this.logModel(new LogListModel());
                }
            });
            this.isNugetTab(true);
        }
        return ConfigPage;
    }());
    ConfigPage.addAlert = function (msg, priority) {
        var model = new ConfigApp.AlertModel(msg, priority);
        ConfigPage.model.alerts.push(model);
        window.setTimeout(function () {
            ConfigPage.model.alerts.remove(model);
        }, 5000);
    };
    var LogListModel = (function () {
        function LogListModel() {
            var _this = this;
            this.files = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getAllLog()
                    .done(function (data) {
                    var logsWithoutVersions = $.map(data, function (item) {
                        var index = item.indexOf("_");
                        return item.slice(0, index);
                    });
                    var array = [];
                    logsWithoutVersions.reduce(function (previous, current, index) {
                        if (array[array.length - 1] != null && array[array.length - 1].hasOwnProperty(current)) {
                            array[array.length - 1][current].push(data[index]);
                        }
                        else {
                            var object = {};
                            object[current] = [data[index]];
                            array.push(object);
                        }
                    }, array);
                    var observableArray = $.map(array, function (log) {
                        for (var item in log) {
                            var itm = new LogModel(item, log[item].reverse());
                        }
                        return itm;
                    });
                    _this.files(observableArray);
                });
            };
            this.load();
        }
        return LogListModel;
    }());
    var LogModel = (function () {
        function LogModel(name, children) {
            var _this = this;
            this.content = ko.observable();
            this.file = ko.observable();
            this.isExpand = ko.observable(false);
            this.isExpandTrigger = function () {
                _this.isExpand(!_this.isExpand());
            };
            this.nameLog = ko.observable();
            this.childrenLogs = ko.observableArray();
            this.getLog = function (file) {
                ConfigApp.ResourceService.getLog(file)
                    .done(function (data) {
                    _this.content(data);
                    _this.file(file);
                });
            };
            this.nameLog(name);
            this.childrenLogs(children);
        }
        return LogModel;
    }());
    var ServiceListModel = (function () {
        function ServiceListModel() {
            var _this = this;
            this.services = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getServiceList()
                    .done(function (data) {
                    var items = $.map(data, function (item) {
                        var itm = new ConfigApp.ServiceModel(item);
                        return itm;
                    });
                    _this.services(items);
                });
            };
            this.compareVersions = function (a, b) {
                if (a === b) {
                    return 0;
                }
                return -1;
            };
            this.clearAlert = function () {
                ConfigPage.model.alerts.removeAll();
            };
            this.stop = function (service) {
                ConfigApp.ResourceService.stopService(service.id())
                    .done(function (data) {
                    _this.load();
                    ConfigPage.addAlert("Service stoped", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e, a, b, c) {
                    ConfigPage.addAlert("error stop service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.start = function (service) {
                ConfigApp.ResourceService.startService(service.id(), service.version())
                    .done(function (data) {
                    _this.load();
                    ConfigPage.addAlert("Service started", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e) {
                    ConfigPage.addAlert("error start service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.setActive = function (service) {
                ConfigApp.ResourceService.setActive(service.id())
                    .done(function (data) {
                    _this.load();
                })
                    .fail(function (e, a, b, c) {
                    ConfigPage.addAlert("error set active service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.removeActive = function (service) {
                ConfigApp.ResourceService.removeActive(service.id())
                    .done(function (data) {
                    _this.load();
                })
                    .fail(function (e, a, b, c) {
                    ConfigPage.addAlert("error remove active service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.load();
        }
        return ServiceListModel;
    }());
    var NugetListModel = (function () {
        function NugetListModel() {
            var _this = this;
            this.nugetList = ko.observableArray();
            this.loadNugets = function () {
                ConfigApp.ResourceService.getNugetList()
                    .done(function (data) {
                    var items = $.map(data, function (item) {
                        var itm = new NugetModel(item);
                        return itm;
                    });
                    _this.nugetList(items);
                });
            };
            this.loadNugets();
        }
        return NugetListModel;
    }());
    var NugetModel = (function () {
        function NugetModel(name) {
            var _this = this;
            this.name = ko.observable('');
            this.isExpand = ko.observable(false);
            this.versions = ko.observableArray();
            this.isExpandTrigger = function () {
                _this.isExpand(!_this.isExpand());
            };
            this.start = function (ver) {
                ConfigApp.ResourceService.startService(_this.name(), ver)
                    .done(function (data) {
                    ConfigPage.addAlert("Service started", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e) {
                    ConfigPage.addAlert("error start service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
                ;
            };
            this.name(name);
            this.isExpand.subscribe(function (val) {
                if (val && _this.versions().length == 0) {
                    ConfigApp.ResourceService.getNugetVersions(_this.name())
                        .done(function (data) {
                        _this.versions(data);
                    });
                }
            });
        }
        return NugetModel;
    }());
    ko.applyBindings(ConfigPage);
})(ConfigApp || (ConfigApp = {}));
