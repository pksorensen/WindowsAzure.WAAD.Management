WindowsAzure.WAAD.Management
============================

A multi tenant Windows Azure Active Directory application demo for management of Windows Azure subscriptions using Owin and Typescript

If you want to try the demo without giving access to your production subscriptions and have a test subscription already placed on a default, that is the easiest way. If you have a test subscription you can also just create a new WAAD and move the subscription to that WAAD and create a test organizational user (remember to give him access to the subscription also in settings on management portal) and try out the demo and move it back when done.

The easiest thing is ofcause just to sign in with your current organizational user and try it out. All the code is here on github if you want to check it out. But I take full responsiblity for any harm this demo (https://demo009.s-innovations.net/) might cause to your subscription. It is only reading meta data on your storage blobs.


###Yes the code is ugly and hacky and contains no error checking.

It was a quick demo seeing what could be achieved with typescript and then owin/waad for some azure management stuff.

https://onedrive.live.com/?gologin=1&mkt=en-US#cid=293B7B497810C12C&id=293B7B497810C12C%211958&v=3


Demo is running at : https://demo009.s-innovations.net/

Signup first for the application. This will add the applicatino to your WAAD (You need to be Administrator of the WAAD) and the app will only ask permission for Sign on. 

Login with your organizational account and follow the on screen instructions. Note you will need to have permissions from your organizational account to a azure subscription for this to work. 

It will list all your storage accounts and then when clicking them ony by one it will calculate the size used on each storage account. 


The purpose is to show how one can create a management web based tool like cerebrata windows client tools. 
