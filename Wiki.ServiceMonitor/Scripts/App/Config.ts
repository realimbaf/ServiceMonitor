/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />



module ConfigApp {

    class ConfigPage {

        static model: ConfigPage;

        alerts = ko.observableArray<AlertModel>();

        msgSuccess = ko.observable();

        activeTab = ko.observable();

        

        getPageModel = mode => {
            return {
                read: () => {
                    return this.activeTab() === mode;
                },
                write: value => {
                    if (value)
                        this.activeTab(mode);
                }
            }
        }



        nugetModel = ko.observable();
        serviceModel = ko.observable();
        logModel = ko.observable();

        isServiceTab = ko.pureComputed(this.getPageModel("service"));
        isLogTab = ko.pureComputed(this.getPageModel("log"));
        isNugetTab = ko.pureComputed(this.getPageModel("nuget"));

        static addAlert = (msg: string, priority: string) => {
            var model = new AlertModel(msg, priority);
            ConfigPage.model.alerts.push(model);
            window.setTimeout(() => {
                ConfigPage.model.alerts.remove(model);
            }, 5000);
        }


        constructor() {
            ConfigPage.model = this;

            this.isNugetTab.subscribe(val => {
                if (!val) {
                    this.nugetModel(null);
                } else {
                    this.nugetModel(new NugetListModel());
                }
            });
            this.isServiceTab.subscribe(val => {
                if (!val) {
                    this.serviceModel(null);
                } else {
                    this.serviceModel(new ServiceListModel());
                }
            });

            this.isLogTab.subscribe(val => {
                if (!val) {
                    this.logModel(null);
                } else {
                    this.logModel(new LogListModel());
                }
            });

            this.isNugetTab(true);


        }


    }

    class LogListModel {
        files = ko.observableArray();

        load = () => {
            ResourceService.getAllLog()
                .done(data => {

                    var logsWithoutVersions = $.map(data, (item: any) => {
                        var index = item.indexOf("_");
                        return item.slice(0, index);
                    });

                    var array = [];

                    logsWithoutVersions.reduce((previous, current, index) => {
                        if (array[array.length - 1] != null && array[array.length - 1].hasOwnProperty(current)) {
                            array[array.length -1][current].push(data[index]);
                        } else {
                            var object = {};                           
                            object[current] = [data[index]];
                            array.push(object);
                        }
                    }, array);

                    var observableArray = $.map(array, (log: any) => {
                        for (var item in log) {
                            var itm = new LogModel(item, log[item].reverse());
                        }
                        return itm;
                    });

                    this.files(observableArray);
                });
        };           
        constructor() {
            this.load();
        }
    }
    class LogModel {
        content = ko.observable();
        file = ko.observable();
        isExpand = ko.observable(false);

        isExpandTrigger = () => {
            this.isExpand(!this.isExpand());
        }

        nameLog  = ko.observable();
        childrenLogs = ko.observableArray();

        getLog = file => {
            ResourceService.getLog(file)
                .done(data => {
                    this.content(data);
                    this.file(file);
                });
        }
        constructor(name: string, children: any) {
            this.nameLog(name);
            this.childrenLogs(children);
        }
    }

    class ServiceListModel {
        services = ko.observableArray();
        load = () => {
            ResourceService.getServiceList()
                .done(data => {

                    var items = $.map(data, (item: any) => {
                        var itm = new ServiceModel(item);
                        return itm;
                    });

                    this.services(items);
                });

        };
        compareVersions = (a, b) => {
            if (a === b) {
                return 0;
            }
            return - 1;
        }
        clearAlert = () => {
            ConfigPage.model.alerts.removeAll();
        }
        stop = (service: ServiceModel) => {
            ResourceService.stopService(service.id())
                .done(data => {
                    this.load();
                    ConfigPage.addAlert("Service stoped", AlertPriority.Success);
                })
                .fail((e, a, b, c) => {
                    ConfigPage.addAlert("error stop service:" + e.responseText, AlertPriority.Error);

                });
        }

        start = (service: ServiceModel) => {
            ResourceService.startService(service.id(), service.version())
                .done(data => {
                    this.load();
                    ConfigPage.addAlert("Service started", AlertPriority.Success);

                })
                .fail((e) => {
                    ConfigPage.addAlert("error start service:" + e.responseText, AlertPriority.Error);
                });
        }

        setActive = (service: ServiceModel) => {
            ResourceService.setActive(service.id())
                .done(data => {
                    this.load();
                })
                .fail((e, a, b, c) => {
                    ConfigPage.addAlert("error set active service:" + e.responseText, AlertPriority.Error);
                });
        }

        removeActive = (service: ServiceModel) => {
            ResourceService.removeActive(service.id())
                .done(data => {
                    this.load();
                })
                .fail((e, a, b, c) => {
                    ConfigPage.addAlert("error remove active service:" + e.responseText, AlertPriority.Error);
                });
        }


        constructor() {
            this.load();
        }

    }


    class NugetListModel {
        nugetList = ko.observableArray<NugetModel>();
        loadNugets = () => {
            ResourceService.getNugetList()
                .done(data => {
                    var items = $.map(data, (item: any) => {
                        var itm = new NugetModel(item);
                        return itm;
                    });
                    this.nugetList(items);
                });
        }
       
        constructor() {
            this.loadNugets();
        }
    }



    class NugetModel {
        name = ko.observable('');
        isExpand = ko.observable(false);
        versions = ko.observableArray();
        isExpandTrigger = () => {
            this.isExpand(!this.isExpand());
        }

        start = ver => {
            ResourceService.startService(this.name(), ver)
                .done(data => {
                    ConfigPage.addAlert("Service started", AlertPriority.Success);
                })
                .fail((e) => {
                    ConfigPage.addAlert("error start service:" + e.responseText, AlertPriority.Error);
                });

            ;
        }

        constructor(name: string) {
            this.name(name);
            this.isExpand.subscribe(val => {
                if (val && this.versions().length == 0) {
                    ResourceService.getNugetVersions(this.name())
                        .done(data => {
                            this.versions(data);
                        });

                }
            });

        }
    }



    ko.applyBindings(ConfigPage);
}


