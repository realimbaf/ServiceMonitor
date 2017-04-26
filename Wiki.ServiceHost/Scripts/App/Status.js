var StatusApp;
(function (StatusApp) {
    var StatusPage = (function () {
        function StatusPage() {
            var request = $.ajax({
                url: "api/security/getaccesskeys",
                cache: false
            }).done(function (data) {
            });
        }
        return StatusPage;
    }());
    StatusApp.StatusPage = StatusPage;
})(StatusApp || (StatusApp = {}));
//# sourceMappingURL=Status.js.map