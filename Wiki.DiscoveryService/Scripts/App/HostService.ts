/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout/knockout.d.ts" />
module ConfigApp {

    class HostsInfoModel {
        static model: HostsInfoModel;

        alerts = ko.observableArray<AlertModel>();
        msgSuccess = ko.observable();
        activeTab = ko.observable();


        static addAlert = (msg: string, priority: string) => {
            var model = new AlertModel(msg, priority);
            HostsInfoModel.model.alerts.push(model);
            window.setTimeout(() => {
                    HostsInfoModel.model.alerts.remove(model);
                },
                5000);
        };
        getPageModel = mode => {
            return {
                read: () => {
                    return this.activeTab() === mode;
                },
                write: value => {
                    if (value)
                        this.activeTab(mode);
                }
            };
        };
        hostsModel = ko.observable();
        serviceModel = ko.observable();
        networkSettingsmodel = ko.observable();
        logModel = ko.observable();

        isNetworkTab = ko.pureComputed(this.getPageModel("network"));
        isLogTab = ko.pureComputed(this.getPageModel("log"));
        isHostsTab = ko.pureComputed(this.getPageModel("hosts"));
        isServiceTab = ko.pureComputed(this.getPageModel("service"));


        constructor() {
            HostsInfoModel.model = this;

            this.isHostsTab.subscribe(val => {
                if (!val) {
                    this.hostsModel(null);
                } else {
                    this.hostsModel(new HostsModel());
                }
            });
            this.isNetworkTab.subscribe(val => {
                if (!val) {
                    this.networkSettingsmodel(null);
                } else {
                    this.networkSettingsmodel(new HostsNetworkModel());
                }
            });

            this.isLogTab.subscribe(val => {
                if (!val) {
                    this.logModel(null);
                } else {
                    this.logModel(new LogsModel());
                }
            });
            this.isServiceTab.subscribe(val => {
                if (!val) {
                    this.serviceModel(null);
                } else {
                    this.serviceModel(new ServicesModel());
                }
            });

            this.isHostsTab(true);
        }
    }

    class LogsModel {
        files = ko.observableArray<LogModel>();
        load =() => {
            ResourceService.getAllLogs()
                .done(data => {
                    var items = $.map(data,
                        (item: any) => {
                            var itm = new LogModel(item);
                            return itm;
                        });
                    this.files(items);
                })
                .fail((e) => {
                    HostsInfoModel.addAlert(`Logs is NOT loaded${e.responseText}`, AlertPriority.Error);
                });
        };

        constructor() {
            this.load();
        }
    }

    class LogModel {
        file = ko.observable();
        logName = ko.observable();

        getLog = model => {
            ResourceService.getLog(model.logName())
                .done(data => {
                    this.logName(model.logName());
                    this.file(data);
                    HostsInfoModel.addAlert(`Log ${this.logName()} is loaded`, AlertPriority.Success);
                })
                .fail((e) => {
                    HostsInfoModel.addAlert(`Log ${this
                        .logName()} is NOT loaded` +
                        e.responseText,
                        AlertPriority.Error);
                });
        };

        constructor(name: string) {
            this.logName(name);
        }
    }

    class ServicesModel {
        servicesArray = ko.observableArray<ServiceModel>();
        load = () => {
            ResourceService.getHostsGroupByService()
                .done((services) => {
                    for (let service in services) {
                        if (services.hasOwnProperty(service)) {
                            const obj = { serviceName: service, hosts: services[service] };
                            this.servicesArray.push(new ServiceModel(obj));
                        }
                    }
                })
                .fail((data) => {
                    console.log(data);
                });
        }
        constructor() {
            this.load();
        }
    }
    class HostsModel {
        hostsArray = ko.observableArray<HostModel>();

        load = () => {
            ResourceService.getHostsList()
                .done(data => {
                    var items = $.map(data,
                        (item: any) => {
                            var itm = new HostModel(item);
                            return itm;
                        });
                    this.hostsArray(items);
                })
                .fail((e) => {
                    HostsInfoModel.addAlert(`Log is NOT loaded ${e.responseText}`, AlertPriority.Error);
                });
        };

        constructor() {
            this.load();
        }
    }

    class HostsNetworkModel {
        networkArray = ko.observableArray<NetworkModel>();
        load = () => {
            ResourceService.getNetworkList()
                .done(data => {
                    var items = $.map(data,
                        (item: any) => {
                            var itm = new NetworkModel(item);
                            return itm;
                        });
                    this.networkArray(items);
                })
                .fail((e) => {
                    HostsInfoModel.addAlert(`Networks info is NOT loaded ${e.responseText}`, AlertPriority.Error);
                });
        };
        addHost = () => {
            var newHost = new NetworkModel("");
            this.networkArray.push(newHost);
            newHost.isVisibleSaveButton(true);
            newHost.isDisabled(false);
        };
        add = (data) => {
            ResourceService.addHost(data)
                .done(() => {
                    data.isDisabled(true);
                    HostsInfoModel.addAlert(`Adding host is succesfull`, AlertPriority.Success);
                })
                .fail((e) => {
                    HostsInfoModel.addAlert(`Adding host is failed ${e.responseText}`, AlertPriority.Error);
                });
        };
        remove = (data) => {
            ResourceService.removeHost(data)
                .done(() => {
                    HostsInfoModel.addAlert(`Removing host is succesfull`, AlertPriority.Success);
                    this.networkArray.remove(data);
                })
                .fail((e) => {
                    HostsInfoModel.addAlert(`Removing host is failed ${e.responseText}`, AlertPriority.Error);
                });
        };

        constructor() {
            this.load();
        }
    }

    class NetworkModel {
        isVisibleSaveButton = ko.observable(false);
        isVisisbleUpdateButton = ko.observable(false);
        isDisabled = ko.observable(true);

        isDisabledTrigger = () => {
            this.isDisabled(!this.isDisabled());
            this.isVisibleSaveButton(!this.isVisibleSaveButton());
        };
        hostName = ko.observable();
        remoteIp = ko.observable();
        monitorUrl = ko.observable();
        isDebug = ko.observable();
        isDirect = ko.observable();

        constructor(item) {
            item = item || {};
            this.hostName(item.HostName || "");
            this.remoteIp(item.RemoteIp || "");
            this.isDebug(item.IsDebug || false);
            this.monitorUrl(item.MonitorUrl || "");
            this.isDirect(item.IsDirect || false);
        }
    }

    class HostModel {
        isExpand = ko.observable(false);

        isExpandTrigger = () => {
            this.isExpand(!this.isExpand());
        };
        hostName = ko.observable();
        monitorUrl = ko.observable();
        remoteIp = ko.observable();
        lastBroadcast = ko.observable();
        lastUpdate = ko.observable();
        isActive = ko.observable();
        isDebug = ko.observable();
        isDirect = ko.observable();
        services = ko.observableArray();

        constructor(item) {
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
    }

    class ServiceModel {
        serviceName = ko.observable();
        hostsArray = ko.observable();
        isExpand = ko.observable(false);

        isExpandTrigger = () => {
            this.isExpand(!this.isExpand());
        };
        constructor(service) {
            this.serviceName(service.serviceName);
            this.hostsArray(service.hosts);
        }
    }

    class ServiceInfo {
        id = ko.observable("");
        url = ko.observable();
        version = ko.observable("");
        activeVersion = ko.observable("");
        processId = ko.observable();
        isRun = ko.observable();
        isAuto = ko.observable();
        isManualStop = ko.observable();


        constructor(item) {
            this.id(item.id);
            this.url(item.url);
            this.version(item.version);
            this.activeVersion(item.activeVersion);
            this.processId(item.processId);
            this.isRun(item.isRun);
            this.isAuto(item.isAuto);
            this.isManualStop(item.isManualStop);
        }
    }

    ko.applyBindings(new HostsInfoModel());
}