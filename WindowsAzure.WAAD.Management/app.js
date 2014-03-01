/// <reference path="Scripts/app/AppDataModel.d.ts" />
require.config({
    // Once you setup baseUrl
    // Relative urls continue to work normal (from source file).
    // However Non-relative URLs use this as base.
    // By default this is the location of requirejs.
    baseUrl: 'Scripts/',
    shim: {
        "bootstrap": {
            deps: ["jquery"]
        },
        "bootstrapDocs": {
            deps: ["bootstrap"]
        }
    },
    paths: {
        "knockout": 'knockout-3.0.0',
        "StorageAccountViewModel": 'app/StorageAccountViewModel',
        "bootstrapDocs": 'docs.min',
        "bootstrap": 'bootstrap',
        "jquery": 'https://ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min'
    }
});

require(['knockout', 'app/AppDataModel', 'bootstrapDocs', 'template!/views/loginView.html', 'template!/views/storageAccountsView.html'], function (ko, appDataModel) {
    var model = new appDataModel();
    var username = ko.observable(null);
    var vm = {
        name: username,
        pageTitle: ko.observable("Windows Azure Active Directory"),
        isAuthenticated: ko.computed(function () {
            return username() !== null;
        }),
        //   loginViewModel: { isAuthenticated: ko.observable(false), userName: ko.observable("pks") },
        code: ko.observable(null),
        hasAccessToken: model.hasAccessToken,
        getCode: function () {
            return model.getAuthorizationCode();
        },
        getStorageAccounts: function () {
            return model.getStorageAccounts(vm.storageAccounts);
        },
        storageAccounts: ko.observableArray([]),
        getAccessToken: function () {
            return model.getAccesToken(vm.code()).done(function () {
                return vm.hasAccessToken(true);
            });
        },
        showImagesFor: function (storageaccount) {
            return model.getImageUrls(storageaccount.accountname()).done(function (data) {
                return storageaccount.bytes(data.Bytes);
            });
        },
        images: ko.observableArray([])
    };

    model.getUserInfo().done(function (name) {
        return vm.name(name);
    }).fail(function () {
        return vm.name(null);
    });

    //model.getManagementApiToken();
    ko.applyBindings(vm);

    if (model.getFragment()["code"] !== undefined) {
        vm.code(model.getFragment()["code"]);
    }
});
//# sourceMappingURL=app.js.map
