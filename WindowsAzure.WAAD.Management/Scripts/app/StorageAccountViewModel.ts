
import ko = require('knockout');
function prittyBytes(bytes: number) {

    var i = 0;
    while ((bytes / 1024) >= 1)
    {
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
class StorageAccountViewModel {
    constructor(name: string) {
        this.accountname(name);
    }
    accountname = ko.observable();
    bytes = ko.observable(0);

    prittySize = ko.computed(() =>
        this.bytes() == 0 ? '' : prittyBytes(this.bytes()));
}

export =     StorageAccountViewModel;