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
using static System.Net.Mime.MediaTypeNames;

namespace FirstProgram
{
    class DemoPrint
    {
        
        static string BOARD = "//*[@id=\"board-vs-personalities\"]";
        static string CLOSE_AD = "/html/body/div[24]/div[2]/div/div/button";
        static string COORDS = "/html/body/div[2]/div[2]/chess-board/svg[1]";
        static string MOVE_TABLE = "/html/body/div[3]/div/vertical-move-list";
        static int WAIT_TIME = 1000;
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

            string playerColor = getPlayerColor(board);
            string pieceColor = "piece " + playerColor[0];


            while (true) {
                play(driver, board, playerColor, pieceColor);
                Console.WriteLine("pos play");
            }
            
        }

        static void play(WebDriver driver, IWebElement board, string playerColor, string pieceColor) { 
            var pieces = FindChildrenByClass(board, pieceColor);

            IWebElement piece = (IWebElement)pieces[0];
            piece.Click();


            var possiblePositions = FindChildrenByClass(board, "hint");

            Actions action = new Actions(driver);
            IWebElement position1 = (IWebElement)possiblePositions[0];
            action.MoveToElement(position1).Click().Perform();


            IWebElement table = driver.FindElement(By.XPath(MOVE_TABLE));

            ArrayList moves = FindChildrenByClass(table, "move");

            bool isCurrent = false;

            do
            {
                isCurrent = isCurrentPlayer((IWebElement)moves[moves.Count - 1], playerColor);


                Console.WriteLine(isCurrent);

                System.Threading.Thread.Sleep(WAIT_TIME);
            } while (!isCurrent);

           
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

        static string getPlayerColor(IWebElement element) {
            ArrayList blackChildren = FindChildrenByClass(element, "square-88");
            IWebElement blackChild = (IWebElement)blackChildren[0];
            Console.WriteLine(blackChild.Location);

            ArrayList whiteChildren = FindChildrenByClass(element, "square-11");
            IWebElement whiteChild = (IWebElement)whiteChildren[0];
            Console.WriteLine(whiteChild.Location);

            return whiteChild.Location.X - blackChild.Location.X <= 0 ? "white" : "black";
        }

        static bool isCurrentPlayer(IWebElement element, string playerColor) {
            Console.WriteLine(element);
            Console.WriteLine(playerColor);
            var children = element.FindElements(By.XPath(".//*"));
            int counter = children.Count;
            //foreach (IWebElement child in children) { 
            //string className = child.GetAttribute("class");
            //    if (className != null && className.Contains(playerColor))
            //    {
            //        return false;
            //    }

            //}
            return (counter == 2 && playerColor.Contains("white")) || (counter == 1 && playerColor.Contains("black"));
        }
        
        //static void wait(int milliseconds)
        //{
        //    var timer1 = new System.Windows.Forms.Timer();
        //    if (milliseconds == 0 || milliseconds < 0) return;

        //    // Console.WriteLine("start wait timer");
        //    timer1.Interval = milliseconds;
        //    timer1.Enabled = true;
        //    timer1.Start();

        //    timer1.Tick += (s, e) =>
        //    {
        //        timer1.Enabled = false;
        //        timer1.Stop();
        //        // Console.WriteLine("stop wait timer");
        //    };

        //    while (timer1.Enabled)
        //    {
        //        Application.DoEvents();
        //    }
        //}



    }
}