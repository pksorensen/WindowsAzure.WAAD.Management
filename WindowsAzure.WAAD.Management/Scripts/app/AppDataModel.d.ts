


interface IAppDataModel {

    getAuthorizationCode(): void;
    getUserInfo(): JQueryXHR;
    getStorageAccounts(list : KnockoutObservableArray<any>): void;
    getAccesToken(code: string): JQueryXHR;
    parseQueryString(queryString: string): { [s: string]: string };
    getFragment(): { [s: string]: string };
    cleanUpLocation(): void;
    hasAccessToken: KnockoutObservable<boolean>
    getImageUrls(account:string) : JQueryXHR;
}
interface IAppDataModelConstructable extends IAppDataModel {
    new (): IAppDataModel;
}
