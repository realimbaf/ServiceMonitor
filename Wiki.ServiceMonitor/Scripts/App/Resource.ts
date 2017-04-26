/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
module ConfigApp {

    export class ResourceService {

        static getServiceList() {
            return $.ajax({
                url: "services",
                cache: false
            });
        }
        static getConcreteService(id: string) {
            return $.ajax({
                url: "/services/status/" + id,
                cache: false
            });
        }

        static getNugetList() {
            return $.ajax({
                url: "/services/list",
                cache: false
            });
        }

        static getNugetVersions(name: string) {
            return $.ajax({
                url: "/services/list/" + name,
                cache: false
            });
        }

        static startService(name: string, ver: string) {
            return $.ajax({
                url: "/services/start/" + name + "?ver=" + ver,
                cache: false
            });
        }
        static stopService(name: string) {
            return $.ajax({
                url: "/services/stop/" + name,
                cache: false
            });
        }

        static getAllLog() {
            return $.ajax({
                url: "/log/all/",
                cache: false
            });
        }
        static getLogById(id) {
            return $.ajax({
                url: "/log/all/" + id
        });
        }

        static getLog(file) {
            return $.ajax({
                url: "/log/file/" + file,
                cache: false
            });
        }

        static setActive(id: string) {
            return $.ajax({
                url: "/services/setactive/" + id,
                cache: false
            });
        }

        static removeActive(id: string) {
            return $.ajax({
                url: "/services/removeactive/" + id,
                cache: false
            });
        }
        static getSettings(id: string) {
            return $.ajax({
                url: "/settings/" + id,
                cache: false
            });     
        }

        static saveSettings(data: any, id: string) {
            return $.ajax({
                type: "POST",
                data: JSON.stringify(data),
                url: "/settings/" + id,
                contentType: "application/json"
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


    export class ConcreteServiceModel {
        url = window.location.href.split[2];
        service = ko.observable();
        load = (id: string) => {
            ResourceService.getConcreteService(id)
                .done(data => {
                    this.service(data);
                });
        }
        constructor() {
            this.load(this.url);
        }
    }

   

    export class ServiceModel {

        id = ko.observable('');
        url = ko.observable();
        version = ko.observable('');
        activeVersion = ko.observable('');
        processId = ko.observable();
        isRun = ko.observable();
        isAuto = ko.observable();
        isManualStop = ko.observable();


        constructor(item) {
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


    }

    


}