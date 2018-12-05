#!/usr/bin/env python

import unittest
from time import sleep

from appium import webdriver
from appium.webdriver.common.touch_action import TouchAction

class SafariTests(unittest.TestCase):
    def setUp(self):
        desired_caps = {
            'app':'com.apple.mobilemail',
            'platformName':'iOS',
            'udid':'a28e52130a4ce5b3bdd13239d690f66a7f357820',
            'deviceName':'Batman',
            'deviceVersion':' 11.4.1',
            'automationName':'XCUITest',
            'teamId':'CB52FCDD4H',
            'signingId':'iPhone Developer',
            'showXcodeLog': True,
            'realDeviceLogger':'/usr/local/lib/node_modules/deviceconsole/deviceconsole',
            'bootstrapPath':'/usr/local/lib/node_modules/appium/node_modules/appium-xcuitest-driver/WebDriverAgent',
            'agentPath':'/usr/local/lib/node_modules/appium/node_modules/appium-xcuitest-driver/WebDriverAgent/WebDriverAgent.xcodeproj',
            'sessionTimeout':'6000',
            'startIWDP': True,
            'shouldUseSingletonTestManager': False,
            'shouldUseTestManagerForVisibilityDetection': True,
            'waitForQuiescence': False,
            'useNewWDA': True
        }
        self.driver = webdriver.Remote('http://mobile-manager.dsg-i.com:1234/wd/hub', desired_caps)


    def tearDown(self):
        self.driver.quit()

    def test_get(self):
        #self.driver.execute_script('mobile: activateApp', {'bundleId': 'com.apple.mobilemail'});

        sleep(30)

if __name__ == "__main__":
    suite = unittest.TestLoader().loadTestsFromTestCase(SafariTests)
    unittest.TextTestRunner(verbosity=2).run(suite)
