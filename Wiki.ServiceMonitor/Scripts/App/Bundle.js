/// <reference path="../knockout-3.3.0.debug.js" />


ko.bindingHandlers.beforeUnloadText = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        if (window.onbeforeunload == null) {
            window.onbeforeunload = function () {
                var value = valueAccessor();
                var promptText = ko.utils.unwrapObservable(value);
                if (typeof promptText == "undefined" || promptText == null) {
                    
                } else {
                    if (promptText != null && typeof promptText != "string") {
                        var err = "Error: beforeUnloadText binding must be " +
                                  "against a string or string observable.  " +
                                  "Binding was done against a " + typeof promptText;
                        console.log(err);
                        console.log(promptText);
                        return err;
                    }
                    return promptText;
                }
            };

        }
    }
};