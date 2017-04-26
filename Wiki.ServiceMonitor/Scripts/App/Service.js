/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
var ConfigApp;
(function (ConfigApp) {
    var ServiceInfoModel = (function () {
        function ServiceInfoModel() {
            var _this = this;
            this.id = ko.observable();
            this.activeTab = ko.observable();
            this.alerts = ko.observableArray();
            this.msgSuccess = ko.observable();
            this.getPageModel = function (mode) {
                return {
                    read: function () {
                        return _this.activeTab() === mode;
                    },
                    write: function (value) {
                        if (value && _this.activeTab() === "setting") {
                            if (_this.settingModel().beforeUnloadPrompt() === null)
                                _this.activeTab(mode);
                            else {
                                var r = confirm("Покинуть страницу c несохраннеными данными?");
                                if (r) {
                                    _this.settingModel().beforeUnloadPrompt(null);
                                    _this.activeTab(mode);
                                }
                            }
                        }
                        else if (value) {
                            _this.activeTab(mode);
                        }
                    }
                };
            };
            this.currentStateModel = ko.observable();
            this.versionModel = ko.observable();
            this.logModel = ko.observable();
            this.settingModel = ko.observable();
            this.isCurrentStateTab = ko.pureComputed(this.getPageModel("current"));
            this.isVersionTab = ko.pureComputed(this.getPageModel("version"));
            this.isLogTab = ko.pureComputed(this.getPageModel("log"));
            this.isSettingTab = ko.pureComputed(this.getPageModel("setting"));
            ServiceInfoModel.model = this;
            this.id(window.location.href.split("/").pop());
            this.isVersionTab.subscribe(function (val) {
                if (!val) {
                    _this.versionModel(null);
                }
                else {
                    _this.versionModel(new VersionsModel());
                }
            });
            this.isCurrentStateTab.subscribe(function (val) {
                if (!val) {
                    _this.currentStateModel(null);
                }
                else {
                    _this.currentStateModel(new ConcreteServiceModel());
                }
            });
            this.isLogTab.subscribe(function (val) {
                if (!val) {
                    _this.logModel(null);
                }
                else {
                    _this.logModel(new LogModel());
                }
            });
            this.isSettingTab.subscribe(function (val) {
                if (!val) {
                    _this.settingModel(null);
                }
                else {
                    _this.settingModel(new SettingsModel());
                }
            });
            this.isCurrentStateTab(true);
        }
        return ServiceInfoModel;
    }());
    ServiceInfoModel.addAlert = function (msg, priority) {
        var model = new ConfigApp.AlertModel(msg, priority);
        ServiceInfoModel.model.alerts.push(model);
        window.setTimeout(function () {
            ServiceInfoModel.model.alerts.remove(model);
        }, 5000);
    };
    var ConcreteServiceModel = (function () {
        function ConcreteServiceModel() {
            var _this = this;
            this.content = ko.observable();
            this.compareVersions = function (a, b) {
                if (a === b) {
                    return 0;
                }
                return -1;
            };
            this.load = function () {
                ConfigApp.ResourceService.getConcreteService(_this.id)
                    .done(function (data) {
                    console.log(data);
                    var observableData = new ConfigApp.ServiceModel(data);
                    _this.content(observableData);
                });
            };
            this.clearAlert = function () {
                ServiceInfoModel.model.alerts.removeAll();
            };
            this.stop = function (service) {
                ConfigApp.ResourceService.stopService(_this.content().id())
                    .done(function (data) {
                    _this.load();
                    ServiceInfoModel.addAlert("Service stoped", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e, a, b, c) {
                    ServiceInfoModel.addAlert("error stop service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.start = function (service) {
                ConfigApp.ResourceService.startService(_this.content().id(), _this.content().version())
                    .done(function (data) {
                    _this.load();
                    ServiceInfoModel.addAlert("Service started", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e) {
                    ServiceInfoModel.addAlert("error start service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.setActive = function (service) {
                ConfigApp.ResourceService.setActive(_this.content().id())
                    .done(function (data) {
                    _this.load();
                    ServiceInfoModel.addAlert("Service is active", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e, a, b, c) {
                    ServiceInfoModel.addAlert("error set active service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.removeActive = function (service) {
                ConfigApp.ResourceService.removeActive(_this.content().id())
                    .done(function (data) {
                    _this.load();
                    ServiceInfoModel.addAlert("Service is unactive", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e, a, b, c) {
                    ServiceInfoModel.addAlert("error remove active service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
            };
            this.id = window.location.href.split("/").pop();
            this.load();
        }
        return ConcreteServiceModel;
    }());
    var VersionsModel = (function () {
        function VersionsModel() {
            var _this = this;
            this.versions = ko.observableArray();
            this.load = function () {
                ConfigApp.ResourceService.getNugetVersions(_this.id)
                    .done(function (data) {
                    _this.versions(data);
                });
            };
            this.start = function (ver) {
                ConfigApp.ResourceService.startService(_this.id, ver)
                    .done(function (data) {
                    _this.load();
                    ServiceInfoModel.addAlert("Service started", ConfigApp.AlertPriority.Success);
                })
                    .fail(function (e) {
                    ServiceInfoModel.addAlert("error start service:" + e.responseText, ConfigApp.AlertPriority.Error);
                });
                ;
            };
            this.id = window.location.href.split("/").pop();
            this.load();
        }
        return VersionsModel;
    }());
    var SettingsModel = (function () {
        function SettingsModel() {
            var _this = this;
            this.settingArray = ko.observableArray();
            this.beforeUnloadPrompt = ko.observable(null);
            this.isEnableSave = ko.observable(false);
            this.isDisabled = ko.pureComputed(function () {
                return _this.isEnableSave() == false ? 'disabled' : null;
            });
            this.load = function () {
                ConfigApp.ResourceService.getSettings(_this.id)
                    .done(function (data) {
                    var observableModel = $.map(data, function (setting) {
                        var itm = new SettingModel(_this, setting['key'], setting['value']);
                        return itm;
                    });
                    _this.settingArray(observableModel);
                });
            };
            this.add = function () {
                var newSetting = new SettingModel(_this, "", "");
                _this.settingArray.push(newSetting);
                newSetting.isUpdateble(true);
            };
            this.remove = function (model) {
                _this.settingArray.remove(model);
                _this.beforeUnloadPrompt("Dont save");
                _this.isEnableSave(true);
            };
            this.save = function () {
                var resultObject = {};
                _this.settingArray().forEach(function (value) {
                    if (value.hasOwnProperty('key') && value.key() != "") {
                        resultObject[value.key()] = value.value();
                    }
                });
                ConfigApp.ResourceService.saveSettings(resultObject, _this.id)
                    .done(function (data) {
                    _this.beforeUnloadPrompt(null);
                    ServiceInfoModel.addAlert("Settings is saved", ConfigApp.AlertPriority.Success);
                });
            };
            this.id = window.location.href.split("/").pop();
            this.load();
        }
        return SettingsModel;
    }());
    var SettingModel = (function () {
        function SettingModel(parent, key, value) {
            var _this = this;
            this.key = ko.observable();
            this.value = ko.observable();
            this.isUpdateble = ko.observable(false);
            this.textAreaSize = ko.observable(1);
            this.parent = ko.observable();
            this.isDisabled = ko.pureComputed(function () {
                return _this.isUpdateble() == false ? 'disabled' : null;
            });
            this.isTriggerUpdate = function () {
                _this.isUpdateble(!_this.isUpdateble());
            };
            this.sizeUp = function () {
                _this.textAreaSize(_this.textAreaSize() + 10);
            };
            this.sizeDown = function () {
                _this.textAreaSize(_this.textAreaSize() + -10);
            };
            this.key(key);
            this.value(value);
            this.parent(parent);
            this.value.subscribe(function (val) {
                if (val) {
                    _this.parent().beforeUnloadPrompt("Dont save");
                    _this.parent().isEnableSave(true);
                }
                ;
            });
            this.key.subscribe(function (val) {
                if (val) {
                    _this.parent().beforeUnloadPrompt("Dont save");
                    _this.parent().isEnableSave(true);
                }
                ;
            });
        }
        return SettingModel;
    }());
    var LogModel = (function () {
        function LogModel() {
            var _this = this;
            this.files = ko.observableArray();
            this.content = ko.observable();
            this.file = ko.observable();
            this.load = function () {
                ConfigApp.ResourceService.getLogById(_this.id)
                    .done(function (data) {
                    _this.files(data);
                });
            };
            this.getLog = function (file) {
                ConfigApp.ResourceService.getLog(file)
                    .done(function (data) {
                    _this.content(data);
                    _this.file(file);
                });
            };
            this.id = window.location.href.split("/").pop();
            this.load();
        }
        return LogModel;
    }());
    ko.applyBindings(new ServiceInfoModel());
})(ConfigApp || (ConfigApp = {}));
