/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
module ConfigApp {

    class ServiceInfoModel {
        static model: ServiceInfoModel;
        id = ko.observable();
        activeTab = ko.observable();
        alerts = ko.observableArray<AlertModel>();     
        msgSuccess = ko.observable(); 
         

        getPageModel = mode => {
            return {
                read: () => {
                    return this.activeTab() === mode;
                },
                write: value => {
                    if (value && this.activeTab() === "setting") {
                        if (this.settingModel().beforeUnloadPrompt() === null)
                            this.activeTab(mode);
                        else {
                            var r = confirm("Покинуть страницу c несохраннеными данными?");
                            if (r) {
                                this.settingModel().beforeUnloadPrompt(null);
                                this.activeTab(mode);
                            }
                        }
                    }
                    else if (value){
                        this.activeTab(mode);
                    }
                }
            }
        }

        currentStateModel = ko.observable();
        versionModel = ko.observable();
        logModel = ko.observable();
        settingModel : any = ko.observable();


        isCurrentStateTab = ko.pureComputed(this.getPageModel("current"));
        isVersionTab = ko.pureComputed(this.getPageModel("version"));
        isLogTab = ko.pureComputed(this.getPageModel("log"));
        isSettingTab = ko.pureComputed(this.getPageModel("setting"));

        static addAlert = (msg: string, priority: string) => {
            var model = new AlertModel(msg, priority);
            ServiceInfoModel.model.alerts.push(model);
            window.setTimeout(() => {
                ServiceInfoModel.model.alerts.remove(model);
            }, 5000);
        }


        constructor() {
            ServiceInfoModel.model = this;
            this.id(window.location.href.split("/").pop());
            this.isVersionTab.subscribe(val => {
                if (!val) {
                    this.versionModel(null);
                } else {
                    this.versionModel(new VersionsModel());
                }
            });
            this.isCurrentStateTab.subscribe(val => {
                if (!val) {
                    this.currentStateModel(null);
                } else {
                    this.currentStateModel(new ConcreteServiceModel());
                }
            });
            this.isLogTab.subscribe(val => {
                if (!val) {
                    this.logModel(null);
                } else {
                    this.logModel(new LogModel());
                }
            });
            this.isSettingTab.subscribe(val => {
                if (!val) {
                    this.settingModel(null);
                } else {
                    this.settingModel(new SettingsModel());
                }
            });
            this.isCurrentStateTab(true);
        }

        
    }

    class ConcreteServiceModel {
        id : string;
        content = ko.observable<any>();

        compareVersions = (a, b) => {
            if (a === b) {
                return 0;
            }
            return - 1;
        }


        load = () => {
            ResourceService.getConcreteService(this.id)
                .done(data => {
                    console.log(data);
                    var observableData = new ServiceModel(data);
                    this.content(observableData);
                });
        }
        clearAlert = () => {
            ServiceInfoModel.model.alerts.removeAll();
        }


        stop = (service: ServiceModel) => {
            ResourceService.stopService(this.content().id())
                .done(data => {
                    this.load();
                    ServiceInfoModel.addAlert("Service stoped", AlertPriority.Success);
                })
                .fail((e, a, b, c) => {
                    ServiceInfoModel.addAlert("error stop service:" + e.responseText, AlertPriority.Error);

                });
        }

        start = (service: ServiceModel) => {
            ResourceService.startService(this.content().id(), this.content().version())
                .done(data => {
                    this.load();
                    ServiceInfoModel.addAlert("Service started", AlertPriority.Success);

                })
                .fail((e) => {
                    ServiceInfoModel.addAlert("error start service:" + e.responseText, AlertPriority.Error);
                });
        }

        setActive = (service: ServiceModel) => {
            ResourceService.setActive(this.content().id())
                .done(data => {
                    this.load();
                    ServiceInfoModel.addAlert("Service is active", AlertPriority.Success);
                })
                .fail((e, a, b, c) => {
                    ServiceInfoModel.addAlert("error set active service:" + e.responseText, AlertPriority.Error);
                });
        }

        removeActive = (service: ServiceModel) => {
            ResourceService.removeActive(this.content().id())
                .done(data => {                
                    this.load();
                    ServiceInfoModel.addAlert("Service is unactive", AlertPriority.Success);
                })
                .fail((e, a, b, c) => {
                    ServiceInfoModel.addAlert("error remove active service:" + e.responseText, AlertPriority.Error);
                });
        }

        constructor() {
            this.id = window.location.href.split("/").pop();
            this.load();
            
        }
    }

    class VersionsModel {
        id: string;
        versions = ko.observableArray();
        load = () => {
            ResourceService.getNugetVersions(this.id)
                .done(data => {
                    this.versions(data);
                });
        }
        start = (ver) => {
            ResourceService.startService(this.id, ver)
                .done(data => {
                    this.load();
                    ServiceInfoModel.addAlert("Service started", AlertPriority.Success);
                })
                .fail((e) => {
                    ServiceInfoModel.addAlert("error start service:" + e.responseText, AlertPriority.Error);
                });
            ;

        }
        constructor() {
            this.id = window.location.href.split("/").pop();
            this.load();
        }
    }

    class SettingsModel {
        id: string;
        settingArray = ko.observableArray<SettingModel>();     
        beforeUnloadPrompt = ko.observable(null);
        isEnableSave = ko.observable(false);
        isDisabled = ko.pureComputed(() => {
            return this.isEnableSave() == false ? 'disabled' : null;
        });
        load = () => {
            ResourceService.getSettings(this.id)
                .done(data => {
                    var observableModel = $.map(data,(setting:any) => {
                        var itm = new SettingModel(this,setting['key'], setting['value']);
                        return itm;
                    });
                    this.settingArray(observableModel);             
                });
        }
        constructor() {
            this.id = window.location.href.split("/").pop();
            this.load();

            
        }
        add = () => {
            var newSetting = new SettingModel(this,"", "");
            this.settingArray.push(newSetting);
            newSetting.isUpdateble(true);

        }
        remove = (model: SettingModel) => {
            this.settingArray.remove(model);
            this.beforeUnloadPrompt("Dont save");
            this.isEnableSave(true);
        }

        save = () => {
            var resultObject = {}
            this.settingArray().forEach((value) => { 
                if (value.hasOwnProperty('key') && value.key() != "") {
                    resultObject[value.key()] = value.value();
                }                           
            });

            ResourceService.saveSettings(resultObject, this.id)
                .done(data => {
                    this.beforeUnloadPrompt(null);
                    ServiceInfoModel.addAlert("Settings is saved", AlertPriority.Success);
                });
        }
    }

    class SettingModel {
        key : any = ko.observable();
        value : any = ko.observable();
        isUpdateble = ko.observable(false);
        textAreaSize = ko.observable(1);
        parent: any = ko.observable();

        isDisabled = ko.pureComputed(() => {
            return this.isUpdateble() == false ? 'disabled' : null;
        });

        isTriggerUpdate = () => {
            this.isUpdateble(!this.isUpdateble());
        }
        sizeUp = () => {
             this.textAreaSize(this.textAreaSize() + 10);
        }
        sizeDown = () => {
             this.textAreaSize(this.textAreaSize() + -10);
        }      

        constructor(parent: SettingsModel, key: string, value: string) {

            this.key(key);
            this.value(value);
            this.parent(parent);

            this.value.subscribe(val => {
                if (val) {
                    this.parent().beforeUnloadPrompt("Dont save");
                    this.parent().isEnableSave(true);
                };
            });
            this.key.subscribe(val => {
                if (val) {
                    this.parent().beforeUnloadPrompt("Dont save");
                    this.parent().isEnableSave(true);
                };
            });
        }
    }


    class LogModel {
        files = ko.observableArray();
        id: string;
        content = ko.observable();
        file = ko.observable();

        load = () => {

            ResourceService.getLogById(this.id)
                .done(data => {
                    this.files(data);
                });
        };
        
        getLog = file => {
            ResourceService.getLog(file)
                .done(data => {
                    this.content(data);
                    this.file(file);
                });
        }

        constructor() {
            this.id = window.location.href.split("/").pop();
            this.load();
        }
    }
     
    ko.applyBindings(new ServiceInfoModel());
}
