using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TesteChessCom
{
    internal class Demo
    {
        IWebDriver driver = new FirefoxDriver("C:\\Vinicius\\Programas\\Selenium Drivers\\geckodriver");

        [SetUp]
        public void startBrowser()
        {
            Console.WriteLine("Sussy baka");
            driver.Navigate().GoToUrl("http://www.chess.com");
            driver.Manage().Window.Maximize();
            Console.WriteLine("End of sus");
        }

        [Test]
        public void test()
        {
            //driver.Url = "http://www.google.co.in";
        }

        //[TearDown]
        //public void closeBrowser()
        //{
        //    driver.Close();
        //}
    }
}
