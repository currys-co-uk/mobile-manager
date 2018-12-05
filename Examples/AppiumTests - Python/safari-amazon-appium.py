#!/usr/bin/env python

import unittest
from time import sleep

from appium import webdriver
from appium.webdriver.common.touch_action import TouchAction
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC


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

        # open Amazon page
        self.driver.get("https://www.amazon.com/")
        self.assertEqual('Amazon.com: Online Shopping for Electronics, Apparel, Computers, Books, DVDs & more', self.driver.title)

        #print(self.driver.page_source)
        # find search element and send text to it
        sleep(10)
        searchInputElement = self.driver.find_element_by_id("nav-search-keywords")

        searchInputElement.send_keys("xbox x console")
        searchInputElement.send_keys(u'\ue007')

        # clicks on first found result
        sleep(10)

        firstFoundResult = self.driver.find_elements_by_xpath("//ul[@id='resultItems']//li/a")[0]

        firstFoundResult.click()

        # click on myPresentation button
        sleep(10)

        priceElement = self.driver.find_element_by_id("newPitchPriceWrapper_feature_div")
        print("Product price: "+priceElement.text+"\n")

        customizeNowButton = self.driver.find_elements_by_xpath("//button[contains(text(),'Customize Now')]")[0]
        print("Click on Customize Now.\n")
        customizeNowButton.click()
        sleep(5)

        while ("#amazoncustom-buybox-widget_" not in self.driver.current_url):
            customizeNowButton.click()
            sleep(5)

        sleep(5)

        selectElements = self.driver.find_elements_by_xpath("//div[@data-control-template='configuratorpicker']//select")

        fh = open("amazon_drop.html", "w")
        fh.write(self.driver.page_source)
        fh.close

        try:
            for select in selectElements:
                print("select: "+select.text+"\n")
                select.click()
                sleep(10)
                self.driver.find_elements_by_xpath("//a[contains(@id,'pcConfigurator')]")[1].click()
                sleep(10)
        finally:
                fh = open("amazon.html", "w")
                fh.write(self.driver.page_source)
                fh.close

        buyButton = self.driver.find_element_by_id("buybox-atc-btn")
        buyButton.click()

        fh = open("amazon_drop.html", "w")
        fh.write(self.driver.page_source)
        fh.close

        #assert the page title
        sleep(5)
        self.driver.get("https://media.makeameme.org/created/yes-it-works.jpg")
        sleep(10)

if __name__ == "__main__":
    suite = unittest.TestLoader().loadTestsFromTestCase(SafariTests)
    unittest.TextTestRunner(verbosity=2).run(suite)
