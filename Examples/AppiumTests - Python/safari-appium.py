#!/usr/bin/env python

import unittest
from time import sleep

from appium import webdriver
from appium.webdriver.common.touch_action import TouchAction

class SafariTests(unittest.TestCase):
    def setUp(self):
        desired_caps = {
            'browserName':'safari',
            'platformName':'iOS',
            'udid':'a28e52130a4ce5b3bdd13239d690f66a7f357820',
            'deviceName':'Batman',
            'deviceVersion':' 11.3.1',
            'automationName':'XCUITest',
            'teamId':'CB52FCDD4H',
            'signingId':'iPhone Developer',
            'showXcodeLog': True,
            'realDeviceLogger':'/usr/local/lib/node_modules/deviceconsole/deviceconsole',
            'bootstrapPath':'/usr/local/lib/node_modules/appium/node_modules/appium-xcuitest-driver/WebDriverAgent',
            'agentPath':'/usr/local/lib/node_modules/appium/node_modules/appium-xcuitest-driver/WebDriverAgent/WebDriverAgent.xcodeproj',
            'sessionTimeout':'6000',
            'startIWDP': True
        }
        self.driver = webdriver.Remote('http://mobile-manager.dsg-i.com:1234/wd/hub', desired_caps)

    def tearDown(self):
        self.driver.quit()

    def test_get(self):
        # open barcamp page
        self.driver.get("http://www.barcampbrno.cz/2018/index.html")
        self.assertEqual('Barcamp Brno 2018', self.driver.title)

        # find prednasky button and scroll to it
        sleep(5)
        prednaskyButton = self.driver.find_element_by_xpath("//a[@href='/2018/program.html' and @class]")

        actions = TouchAction(self.driver)
        actions.press(None, 0, 1334/4).move_to(None, 0, prednaskyButton.location["y"]+200).release().perform()

        # click on prednasky button
        sleep(5)
        prednaskyButton.click()

        # find my presentation and scroll to it
        sleep(5)
        myPresentation = self.driver.find_element_by_xpath("//a[@href='/2018/prednaska/6203620f.html']")
        actions.press(None, 0, 1334/4).move_to(None, 0, myPresentation.location["y"]+200).release().perform()

        # click on myPresentation button
        sleep(5)
        myPresentation.click()

        #assert the page title
        sleep(5)
        self.driver.get("https://media.makeameme.org/created/yes-it-works.jpg")
        sleep(10)

if __name__ == "__main__":
    suite = unittest.TestLoader().loadTestsFromTestCase(SafariTests)
    unittest.TextTestRunner(verbosity=2).run(suite)
