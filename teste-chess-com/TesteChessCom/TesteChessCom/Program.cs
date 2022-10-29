using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Interactions;
using System.Collections;
using OpenQA.Selenium.Support.UI;

namespace FirstProgram
{
    class DemoPrint
    {
        
        static string BOARD = "//*[@id=\"board-vs-personalities\"]";
        static string CLOSE_AD = "/html/body/div[24]/div[2]/div/div/button";
        static string COORDS = "/html/body/div[2]/div[2]/chess-board/svg[1]";
        static void Main()
        {
            Console.WriteLine("Guru99");

            FirefoxOptions options = new FirefoxOptions();
            options.BrowserExecutableLocation = ("C:\\Program Files\\Mozilla Firefox\\firefox.exe"); //location where Firefox is installed
            WebDriver driver = new FirefoxDriver(options);

            driver.Navigate().GoToUrl("http://www.chess.com/play/computer");
            driver.Manage().Window.Maximize();

            Actions builder = new Actions(driver);
            //builder.MoveToElement(driver.FindElement(By.XPath(PLAY_URL))).Click().Perform();
            //driver.FindElement(By.XPath(PLAY_URL)).Click();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            IWebElement ad = driver.FindElement(By.XPath(CLOSE_AD));
            ad.Click();

            IWebElement board = driver.FindElement(By.XPath(BOARD));

            var coords = FindChildrenByClass(board, "coordinates");
            var coord = (IWebElement)coords[0];
            driver.ExecuteScript("arguments[0].style='display: none;'", coord);

            var list = FindChildrenByClass(board, "piece w");

            IWebElement piece = (IWebElement) list[0];
            piece.Click();

            Console.WriteLine("Beggining of wait");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Console.WriteLine("End of wait");


            var possiblePositions = FindChildrenByClass(board, "hint");


            //var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Actions action = new Actions(driver);
            IWebElement position1 = (IWebElement) possiblePositions[0];
            action.MoveToElement(position1).Click().Perform();

            Console.WriteLine("End of sus");

            //driver.Close();
        }

        static ArrayList FindChildrenByClass(IWebElement element, string className)
        {
            var children = element.FindElements(By.XPath(".//*"));
            var list = new ArrayList();
            foreach (IWebElement child in children)
            {
                string childClass = child.GetAttribute("class");
                if (childClass != null && childClass.Contains(className))
                {
                    list.Add(child);
                }
            }

            return list;
        }

        // string equal to the best music genre
        




    }
}