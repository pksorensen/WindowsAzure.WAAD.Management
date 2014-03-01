
import ko = require('knockout');
import $ = require('jquery');
import StorageAccountViewModel = require('StorageAccountViewModel');


class AppDataModel implements IAppDataModel {
    constructor() {
        var accessToken = sessionStorage["accessToken"] || localStorage["accessToken"];

        if (accessToken) {
            this.hasAccessToken(true);
        }
    }

    private getSecurityHeaders() {
        var accessToken = sessionStorage["accessToken"] || localStorage["accessToken"];

        if (accessToken) {
            return { "Authorization": "Bearer " + accessToken };
        }

        return {};
    }
    hasAccessToken = ko.observable(false);

    getAuthorizationCode(): void {
        //   var authorizationUrl = "https://login.windows.net/802626c6-0f5c-4293-a8f5-198ecd481fe3/oauth2/authorize?api-version=1.0&response_type=code&client_id=f0fb2488-27e0-4d71-ab27-57d9a41024e0&resource=https://management.core.windows.net/&redirect_uri=https://localhost:44309/";

        $.ajax('/api/getAuthorizationUri', { type: "GET" })
            .done((authorizationUrl) => {
                window.location.href = authorizationUrl;
            })
            .fail(() => {
                alert('Not logged in');
            });


    }
    getUserInfo() {
        return $.ajax('/api/getUserInfo', { type: "GET" });
    }

    getStorageAccounts(list: KnockoutObservableArray<StorageAccountViewModel>) {
        return $.ajax('/api/getStorageAccounts', { type: "GET", headers: this.getSecurityHeaders() })
            .done((data :Array<string>) => {
                for (var i = 0; i < data.length; ++i) {
                    list.push(new StorageAccountViewModel(data[i]));
                }
            });
    }
    getAccesToken(code:string) {
        var request = $.ajax('/api/getAccessToken?code=' + code, { type: "GET" });     
        request.done(token=> sessionStorage["accessToken"] = token);
        return request;             
    }
    getImageUrls(account) {
        return $.ajax('/api/getStorageAccountInfo?account=' + account, { type: "GET", headers: this.getSecurityHeaders() });
    }

    cleanUpLocation() :void {
        window.location.search = null;

        if (typeof (history.pushState) !== "undefined") {
            history.pushState("", document.title, location.pathname);
        }
    }

    getFragment(): { [s: string]: string } {
      
        if (window.location.search.indexOf("?") === 0) {
            return this.parseQueryString(window.location.search.substr(1));
        } else {
            return {};
        }
    }
    parseQueryString(queryString): { [s: string]: string } {
        var data: { [s: string]: string }= {},
            pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;

        if (queryString === null) {
            return data;
        }

        pairs = queryString.split("&");

        for (var i = 0; i < pairs.length; i++) {
            pair = pairs[i];
            separatorIndex = pair.indexOf("=");

            if (separatorIndex === -1) {
                escapedKey = pair;
                escapedValue = null;
            } else {
                escapedKey = pair.substr(0, separatorIndex);
                escapedValue = pair.substr(separatorIndex + 1);
            }

            key = decodeURIComponent(escapedKey);
            value = decodeURIComponent(escapedValue);

            data[key] = value;
        }

        return data;
    }
}

export = AppDataModel;