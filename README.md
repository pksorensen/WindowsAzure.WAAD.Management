WindowsAzure.WAAD.Management
============================

A multi tenant Windows Azure Active Directory application demo for management of Windows Azure subscriptions using Owin and Typescript


###Yes the code is ugly and hacky and contains no error checking.

It was a quick demo seeing what could be achieved with typescript and then owin/waad for some azure management stuff.

https://onedrive.live.com/?gologin=1&mkt=en-US#cid=293B7B497810C12C&id=293B7B497810C12C%211958&v=3


Demo is running at : https://demo009.s-innovations.net/

Signup first for the application. This will add the applicatino to your WAAD (You need to be Administrator of the WAAD) and the app will only ask permission for Sign on. 

Login with your organizational account and follow the on screen instructions. Note you will need to have permissions from your organizational account to a azure subscription for this to work. 

It will list all your storage accounts and then when clicking them ony by one it will calculate the size used on each storage account. 


The purpose is to show how one can create a management web based tool like cerebrata windows client tools. 
