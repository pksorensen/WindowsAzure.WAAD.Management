define(["require", "exports", 'knockout', 'jquery', 'StorageAccountViewModel'], function(require, exports, __ko__, __$__, __StorageAccountViewModel__) {
    var ko = __ko__;
    var $ = __$__;
    var StorageAccountViewModel = __StorageAccountViewModel__;

    var AppDataModel = (function () {
        function AppDataModel() {
            this.hasAccessToken = ko.observable(false);
            var accessToken = sessionStorage["accessToken"] || localStorage["accessToken"];

            if (accessToken) {
                this.hasAccessToken(true);
            }
        }
        AppDataModel.prototype.getSecurityHeaders = function () {
            var accessToken = sessionStorage["accessToken"] || localStorage["accessToken"];

            if (accessToken) {
                return { "Authorization": "Bearer " + accessToken };
            }

            return {};
        };

        AppDataModel.prototype.getAuthorizationCode = function () {
            //   var authorizationUrl = "https://login.windows.net/802626c6-0f5c-4293-a8f5-198ecd481fe3/oauth2/authorize?api-version=1.0&response_type=code&client_id=f0fb2488-27e0-4d71-ab27-57d9a41024e0&resource=https://management.core.windows.net/&redirect_uri=https://localhost:44309/";
            $.ajax('/api/getAuthorizationUri', { type: "GET" }).done(function (authorizationUrl) {
                window.location.href = authorizationUrl;
            }).fail(function () {
                alert('Not logged in');
            });
        };
        AppDataModel.prototype.getUserInfo = function () {
            return $.ajax('/api/getUserInfo', { type: "GET" });
        };

        AppDataModel.prototype.getStorageAccounts = function (list) {
            return $.ajax('/api/getStorageAccounts', { type: "GET", headers: this.getSecurityHeaders() }).done(function (data) {
                for (var i = 0; i < data.length; ++i) {
                    list.push(new StorageAccountViewModel(data[i]));
                }
            });
        };
        AppDataModel.prototype.getAccesToken = function (code) {
            var request = $.ajax('/api/getAccessToken?code=' + code, { type: "GET" });
            request.done(function (token) {
                return sessionStorage["accessToken"] = token;
            });
            return request;
        };
        AppDataModel.prototype.getImageUrls = function (account) {
            return $.ajax('/api/getStorageAccountInfo?account=' + account, { type: "GET", headers: this.getSecurityHeaders() });
        };

        AppDataModel.prototype.cleanUpLocation = function () {
            window.location.search = null;

            if (typeof (history.pushState) !== "undefined") {
                history.pushState("", document.title, location.pathname);
            }
        };

        AppDataModel.prototype.getFragment = function () {
            if (window.location.search.indexOf("?") === 0) {
                return this.parseQueryString(window.location.search.substr(1));
            } else {
                return {};
            }
        };
        AppDataModel.prototype.parseQueryString = function (queryString) {
            var data = {}, pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;

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
        };
        return AppDataModel;
    })();

    
    return AppDataModel;
});
//# sourceMappingURL=AppDataModel.js.map
