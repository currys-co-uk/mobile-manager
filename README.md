# Mobile Manager

[![Build Status](https://travis-ci.com/xfoggi/MobileManager.svg?token=PhqzsFPZKMpYRG2NVwVr&branch=master)](https://travis-ci.com/xfoggi/MobileManager)

## About
MobileManager is an application used for automation testing of iOS and Android mobile devices.


## Documentaion
Documentation files can be found directly in Documentation folder or on github https://github.com/dixons/mobile-manager/wiki

It contains all neccessary info how to install/update this project and setup basic testing environemnt on Mac or Linux.


## How to install
First you need to install all neccessary dependencies - follow the installation guide.

https://github.com/dixons/mobile-manager/wiki/Installation


## How to start
We currently do not have a single file executable build, so in order to get started, you need to clone this repository.

    git clone https://github.com/dixons/mobile-manager.git

After that, go to the folder and follow the startup manual:

https://github.com/dixons/mobile-manager/wiki/Startup


## How to connect devices
After successfuly running MobileManager it is time to connect mobile devices. Follow the connect devices guide.

https://github.com/dixons/mobile-manager/wiki/Connect-devices-(android-&-ios)
    
To see if devices are succesfully connected, contact our Devices API and try to get all devices. This can be done via Swagger or any API client.

https://github.com/dixons/mobile-manager/wiki/API-documentation
    
## How to reserve a device and run selenium tests
When devices are connected and are visible in Mobile Manager, you can try to reserve a device and run selenium tests against created Appium endpoint.

https://github.com/dixons/mobile-manager/wiki/Reservation-flow
    
In the folder `AppiumTests` you can find simple examples of Python selenium tests. You can create your own test based on these or changing the `desired_caps` inside one tests to your own device and run it.
