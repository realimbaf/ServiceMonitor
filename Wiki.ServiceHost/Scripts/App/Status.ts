module StatusApp {
    export class StatusPage {
        constructor() {
            var request = $.ajax({
                url: "api/security/getaccesskeys",
                cache: false
            }).done(data=> {

            });
        }
    }

}
 