define(["require", "exports", 'knockout'], function(require, exports, __ko__) {
    var ko = __ko__;
    function prittyBytes(bytes) {
        var i = 0;
        while ((bytes / 1024) >= 1) {
            bytes /= 1024;
            i++;
        }
        switch (i) {
            case 0:
                return bytes + " Bytes";
            case 1:
                return bytes + " KB";
            case 2:
                return bytes + " MB";
            case 3:
                return bytes + " MB";
        }
    }
    var StorageAccountViewModel = (function () {
        function StorageAccountViewModel(name) {
            var _this = this;
            this.accountname = ko.observable();
            this.bytes = ko.observable(0);
            this.prittySize = ko.computed(function () {
                return _this.bytes() == 0 ? '' : prittyBytes(_this.bytes());
            });
            this.accountname(name);
        }
        return StorageAccountViewModel;
    })();

    
    return StorageAccountViewModel;
});
//# sourceMappingURL=StorageAccountViewModel.js.map
