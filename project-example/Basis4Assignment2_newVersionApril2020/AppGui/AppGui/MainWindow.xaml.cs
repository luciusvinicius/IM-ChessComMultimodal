using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

using OpenQA.Selenium.Interactions;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;

        //  new 16 april 2020
        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        static string BOARD = "//*[@id=\"board-vs-personalities\"]";
        static string CLOSE_AD = "/html/body/div[25]/div[2]/div/div/button";
        static string CLOSE_AD2 = "/html/body/div[26]/div[2]/div/div/button";

        static string COORDS = "/html/body/div[2]/div[2]/chess-board/svg[1]";
        static string MOVE_TABLE = "/html/body/div[3]/div/vertical-move-list";
        static int WAIT_TIME = 1000;
        

        private WebDriver driver;
        private IWebElement board;
        private string playerColor;
        private string pieceColor;
        private bool isCurrent;
        private IWebElement table;
        
        public MainWindow()
        {
            //InitializeComponent();

            FirefoxOptions options = new FirefoxOptions();
            options.BrowserExecutableLocation = ("C:\\Program Files\\Mozilla Firefox\\firefox.exe"); //location where Firefox is installed
            driver = new FirefoxDriver(options);

            driver.Navigate().GoToUrl("http://www.chess.com/play/computer");
            driver.Manage().Window.Maximize();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            try
            {
                IWebElement ad = driver.FindElement(By.XPath(CLOSE_AD));
                Console.WriteLine(ad);
                ad.Click();
            }
            catch (NoSuchElementException e)
            {
                IWebElement ad = driver.FindElement(By.XPath(CLOSE_AD2));
                Console.WriteLine(ad);
                ad.Click();
            }


            board = driver.FindElement(By.XPath(BOARD));

            // APAGAR AS COORDS
            //var coords = FindChildrenByClass(board, "coordinates");
            //var coord = (IWebElement)coords[0];
            //driver.ExecuteScript("arguments[0].style='display: none;'", coord);

            playerColor = getPlayerColor(board);
            pieceColor = "piece " + playerColor[0];

            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");

            //play();

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            //Console.WriteLine("Sussy message: " + e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            dynamic recognized = json.recognized;
            Console.WriteLine("JSON:");
            Console.WriteLine(json);

            switch ((string)recognized["Action"])
            {
                case "MOVE":
                    Console.WriteLine("MOVE");
                    string pieceName = recognized["MovingPiece"] != null ? (string)recognized["MovingPiece"] : null;
                    string from = recognized["PositionInitial"] != null ? (string)recognized["PositionInitial"] : null;
                    string to = recognized["PositionFinal"] != null ? (string)recognized["PositionFinal"] : null;
                    var possiblePieces = getPossiblePieces(
                        pieceName: pieceName,
                        from: from
                    );
                    Console.WriteLine("Possible pieces: " + possiblePieces.Count);
                    if (possiblePieces.Count == 1)
                    {
                        move((IWebElement)possiblePieces[0], to);
                    }
                    Console.WriteLine("SUSSY MOVE");
                    break;
            }

            Console.WriteLine("Sus");

            //Shape _s = null;
            //switch ((string)json.recognized[0].ToString())
            //{
            //    case "SQUARE": _s = rectangle;
            //        break;
            //    case "CIRCLE": _s = circle;
            //        break;
            //    case "TRIANGLE": _s = triangle;
            //        break;
            //}

            //App.Current.Dispatcher.Invoke(() =>
            //{
            //    string color = (string)json.recognized[1].ToString();
            //    switch (color)
            //    {
            //        case "GREEN":
            //            _s.Fill = Brushes.Green;
            //            table = driver.FindElement(By.XPath(MOVE_TABLE));
            //            if (isCurrentPlayerByTable(table)) {
            //                play();
            //            }
            //            break;
            //        case "BLUE":
            //            _s.Fill = Brushes.Blue;
            //            break;
            //        case "RED":
            //            _s.Fill = Brushes.Red;
            //            break;
            //    }
            //});

            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = ""; // "{ \"synthesize\": [";
            json2 += (string)json.recognized[0].ToString()+ " ";
            json2 += (string)json.recognized[1].ToString() + " DONE." ;
            //json2 += "] }";
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            */
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
            Console.WriteLine("bbbbbbbbbbbbbbbbbbbbbbbbb");
        }

        public void move(IWebElement piece, string to=null) {
            piece.Click();

            var possiblePositions = FindChildrenByClass(board, "hint");

            Actions action = new Actions(driver);
            IWebElement position1 = (IWebElement)possiblePositions[0];
            action.MoveToElement(position1).Click().Perform();
        }

        public ArrayList getPossiblePieces(String pieceName = null, String from = null, 
            String to = null, String direction = null)
        {
            /*
             * @parameter pieceName: name of the piece to move (KNIGHT, KING, etc)
             * @parameter from: a2, b3, c4, etc
             * @parameter to: a2, b3, c4, etc. 
             * This parameter will filter by the possible moves.
             * If just one piece can move to this position, it will be automatic
             * @parameter direction: up, down, left, right, etc
             */
            Console.WriteLine("Initus");
            string piece = pieceName == "KNIGHT" ? pieceColor + "n" : pieceColor + pieceName.ToLower()[0];
            Console.WriteLine("Sussy piece: " + piece);
            if (from != null)
            {
                piece += " square-" + getHorizontalNumber(from[0]) + from[1];
            }

            Console.WriteLine("FindByClass: " + piece);

            var possiblePieces = FindChildrenByClass(board, piece);

            if (possiblePieces.Count <= 1) {
                return possiblePieces;
            }

            


            return possiblePieces;

        }
        
        public int getHorizontalNumber(char letter) {
            Console.WriteLine("Sussy letter: " + (int)letter);
            return (int)letter - 64;
        }
        
        public void play()
        {
            var pieces = FindChildrenByClass(board, pieceColor);

            IWebElement piece = (IWebElement)pieces[0];
            piece.Click();


            var possiblePositions = FindChildrenByClass(board, "hint");

            Actions action = new Actions(driver);
            IWebElement position1 = (IWebElement)possiblePositions[0];
            action.MoveToElement(position1).Click().Perform();


            //table = driver.FindElement(By.XPath(MOVE_TABLE));

            //ArrayList moves = FindChildrenByClass(table, "move");
            
            

     
            //do
            //{
            //    isCurrent = isCurrentPlayer((IWebElement)moves[moves.Count - 1], playerColor);

            //    Console.WriteLine(isCurrent);

            //    System.Threading.Thread.Sleep(WAIT_TIME);
            //} while (!isCurrent);


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

        static string getPlayerColor(IWebElement element)
        {
            ArrayList blackChildren = FindChildrenByClass(element, "square-88");
            IWebElement blackChild = (IWebElement)blackChildren[0];
            Console.WriteLine(blackChild.Location);

            ArrayList whiteChildren = FindChildrenByClass(element, "square-11");
            IWebElement whiteChild = (IWebElement)whiteChildren[0];
            Console.WriteLine(whiteChild.Location);

            return whiteChild.Location.X - blackChild.Location.X <= 0 ? "white" : "black";
        }

        static bool isCurrentPlayer(IWebElement element, string playerColor)
        {
            Console.WriteLine(element);
            Console.WriteLine(playerColor);
            var children = element.FindElements(By.XPath(".//*"));
            int counter = children.Count;

            return (counter == 2 && playerColor.Contains("white")) || (counter == 1 && playerColor.Contains("black"));
        }

        public bool isCurrentPlayerByTable(IWebElement tab) {

            ArrayList moves = FindChildrenByClass(tab, "move");
            isCurrent = isCurrentPlayer((IWebElement)moves[moves.Count - 1], playerColor);
            Console.WriteLine("isCurrent: " + isCurrent);
            return isCurrent;
        }

    }
}
