using System;
using System.Collections;
using System.Collections.Generic;
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

        // ------------------------ LOGIN
        static string USERNAME_FIELD = "username";
        static string PASSWORD_FIELD = "password";
        static string LOGIN_BUTTON = "login";
        static string USERNAME = "projetoIM";
        static string PASSWORD = "SigaPara20";

        // ------------------------ XPATHS
        static string BOARD = "//*[@id=\"board-vs-personalities\"]";
        static string CLOSE_AD = "/html/body/div[25]/div[2]/div/div/button";
        static string CLOSE_AD2 = "/html/body/div[27]/div[2]/div/div/button";


        static string COORDS = "/html/body/div[2]/div[2]/chess-board/svg[1]";
        static string MOVE_TABLE = "/html/body/div[3]/div/vertical-move-list";
        static string FRIENDS_LIST = "/html/body/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/div[3]";

        // ------------------------ CONSTS
        static int WAIT_TIME = 1000;

        static string SITE_URL = "https://www.chess.com";
        static string LOGIN_URL = "https://www.chess.com/login_and_go?returnUrl=" + SITE_URL;
        static string COMPUTER_URL = "https://www.chess.com/play/computer";
        static string FRIENDS_URL = "https://www.chess.com/friends";
        static string VS_FRIENDS_URL = "https://www.chess.com/play/online/new?opponent=";

        // ------------------------ PHRASES
        static string FRIEND_CHOOSE = "Escolha um amigo dentre a lista de amigos";

        static string NO_KNOWN_PIECE_ERROR = "Não consegui identificar a peça, poderia indicá-la novamente?";
        static string NO_KNOWN_ACTION_ERROR = "Não consegui identificar a ação, poderia indicá-la novamente?";
        static string WRONG_MOVE_ERROR = "Possibilidade de movimento não existente, poderia indicá-lo novamente?";
        static string NO_POSSIBLE_MOVES_ERROR = "Não existem movimentos possíveis para essa peça";
        static string AMBIGUOS_MOVEMENT = "Existe mais de um movimento possível para essa peça, poderia indicar o destino?";
        static string AMBIGUOS_PIECE = "Existe mais de uma peça com essa descrição, poderia indicar a peça?";
        static string FRIEND_CHOOSE_COUNT_ERROR = "Amigo não encontrado, por favor, tente novamente";

        private WebDriver driver;
        private IWebElement board;
        private string playerColor;
        private string pieceColor;
        private bool isCurrent;
        private IWebElement table;

        // ------------------------ DICTS
        public Dictionary<string, string> context = new Dictionary<string, string>();
        public Dictionary<string, string> pieceDict = new Dictionary<string, string>() {
            {"p", "PAWN"}, {"k", "KING"}, {"q", "QUEEN"}, {"r", "ROOK"}, {"b", "BISHOP"}, {"n", "KNIGHT"}
        };

        public MainWindow()
        {
            //InitializeComponent();

            FirefoxOptions options = new FirefoxOptions();
            options.BrowserExecutableLocation = ("C:\\Program Files\\Mozilla Firefox\\firefox.exe"); //location where Firefox is installed
            driver = new FirefoxDriver(options);
            
            redirect(LOGIN_URL);
            
            IWebElement username_field = driver.FindElement(By.Id(USERNAME_FIELD));
            username_field.SendKeys(USERNAME);
            IWebElement password_field = driver.FindElement(By.Id(PASSWORD_FIELD));
            password_field.SendKeys(PASSWORD);
            IWebElement login_button = driver.FindElement(By.Id(LOGIN_BUTTON));
            login_button.Click();

            driver.Manage().Window.Maximize();

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

            string entity = recognized["Entity"] != null ? (string)recognized["Entity"] : null;
            string action = recognized["Action"] != null ? (string)recognized["Action"] : "";

            if (action == "" && context.ContainsKey("action"))
            {
                action = context["action"];
            }

            switch (action)
            {
                case "MOVE":
                    Console.WriteLine("MOVE");
                    string from = recognized["PositionInitial"] != null ? (string)recognized["PositionInitial"] : null;
                    string to = recognized["PositionFinal"] != null ? (string)recognized["PositionFinal"] : null;
                    //string directionInitial = recognized["DirectionInitial"] != null ? (string)recognized["DirectionInitial"] : null;
                    //string directionFinal = recognized["DirectionFinal"] != null ? (string)recognized["DirectionFinal"] : null;

                    var possiblePieces = getPossiblePieces(
                        pieceName: entity,
                        from: from
                        //direction: directionInitial
                    );
                    Console.WriteLine("Possible pieces: " + possiblePieces.Count);
                    var teste = (IWebElement)possiblePieces[0];
                    Console.WriteLine("teste: " + teste.GetAttribute("class"));
                    movePieces(
                        pieces: possiblePieces, 
                        to: to 
                        //direction: directionFinal
                    );
                    break;

                case "PLAY AGAINST":
                    Console.WriteLine("PLAY AGAINST");
                    int friendNumber = recognized["Number"] != null ? (int)recognized["Number"] : -1;
                    opponentType(entity, friendNumber);
                    playAgainst(friendNumber);
                    break;

                default:
                    sendMessage(NO_KNOWN_ACTION_ERROR);
                    break;
            }


        }
        
        // ------------------------------ PLAY AGAINS PC OR HUMAN
        
        public void playAgainst(int friendID) {
            /*
             * @param entity: "1", "2", etc...
             */
            if (friendID == -1 || driver.Url != FRIENDS_URL) return;
            
            IWebElement friendsList = driver.FindElement(By.XPath(FRIENDS_LIST));
            ArrayList friends;

            try
            {
                friends = FindChildrenByClass(friendsList, "friends-list-item");
            }
            catch (StaleElementReferenceException e) {
                System.Threading.Thread.Sleep(WAIT_TIME);
                friends = FindChildrenByClass(friendsList, "friends-list-item");
            }
            

            if (friendID > friends.Count) {
                sendMessage(FRIEND_CHOOSE_COUNT_ERROR);
                return;
            }

            var friend = (IWebElement)friends[friendID - 1];
            var teste = FindChildrenByClass(friend, "friends-list-details");
            var friendDetails = (IWebElement)teste[0];
            var friendData = (IWebElement)FindChildrenByClass(friendDetails, "friends-list-user-data")[0];
            var friendName = friendData.Text;

            redirect(VS_FRIENDS_URL + friendName);

            sendMessage("Desafio enviado para " + friendName);

            //var friendActions = (IWebElement)FindChildrenByClass(friend, "friends-list-find-actions")[0];
            //var memberActionsContainer = (IWebElement)FindChildrenByClass(friendActions, "member-actions-container")[0];


        }

        public void opponentType(String entity, int friendID)
        {
            if (entity == null) return;
            
            if (entity == "COMPUTER") {
                redirect(COMPUTER_URL, hasBoard: true, hasAd: true);
            }
            else if (entity == "FRIEND")
            {
                if (driver.Url != FRIENDS_URL)
                {
                    redirect(FRIENDS_URL);
                    if (friendID == -1) sendMessage(FRIEND_CHOOSE);
                }
            }
        }

        public void redirect(String URL, bool hasBoard = false, bool hasAd = false) {
            driver.Navigate().GoToUrl(URL);
            
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            System.Threading.Thread.Sleep(WAIT_TIME);
            if (hasAd) {
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
            }

            if (hasBoard) {
                board = driver.FindElement(By.XPath(BOARD));

                playerColor = getPlayerColor(board);
                pieceColor = "piece " + playerColor[0];
            }
        }

        // ------------------------------ MOVEMENT

        public void movePieces(ArrayList pieces, string to = null) { //, string direction = null) {
            
            ArrayList correctPieces = new ArrayList();
            ArrayList possibleMovesList = new ArrayList();
            
            foreach (IWebElement piece in pieces)
            {
                var possibleMoves = findPossiblePositions(piece, to);//, direction);
                if (possibleMoves.Count >= 1) {
                    correctPieces.Add(piece);
                    possibleMovesList.Add(possibleMoves);
                }
            }

            Console.WriteLine("Correct pieces: " + correctPieces.Count);

            if (correctPieces.Count == 1)
            {
                var piece = (IWebElement)correctPieces[0];
                string pieceName = Char.ToString(piece.GetAttribute("class")[7]);
                context["pieceName"] = pieceName;
                var possibleMoves = (ArrayList)possibleMovesList[0];
                if (possibleMoves.Count == 1)
                {
                    context["from"] = to;
                    performMove((IWebElement)possibleMoves[0]);

                }
                else
                {
                    context["from"] = getPiecePosition(piece);
                    sendMessage(AMBIGUOS_MOVEMENT);
                }
            }
            else if (correctPieces.Count > 1)
            {
                sendMessage(AMBIGUOS_PIECE);
            }
            else
            {
                sendMessage(NO_KNOWN_PIECE_ERROR);
            }

        }
        public ArrayList findPossiblePositions(IWebElement piece, string to=null) { //, string direction=null) {
            piece.Click();
            string hint = "hint";

            // Filter by To
            if (to != null && to.Length <= 2)
            {
                hint += " square-" + getHorizontalNumber(to[0]) + to[1];
            }

            var possiblePositions = FindChildrenByClass(board, hint);
            Console.WriteLine("to: " + to);
            Console.WriteLine("Hint: " + hint);
            Console.WriteLine("Possible positions (inside): " + possiblePositions.Count);

            // Filter by Direction
            if (to != null && to.Length >= 2 && possiblePositions.Count > 1)
            {
                var newPossiblePositions = new ArrayList();

                foreach (IWebElement possiblePosition in possiblePositions)
                {
                    if (isOnDirection(piece, possiblePosition, to))
                    {
                        newPossiblePositions.Add(possiblePosition);
                    }
                }

                Console.WriteLine("New possible positions (Inside): " + newPossiblePositions.Count);

                return newPossiblePositions;
            }

            return possiblePositions;
        }

        public void performMove(IWebElement position) {
            Actions action = new Actions(driver);
            action.MoveToElement(position).Click().Perform();
        }

        public ArrayList getPossiblePieces(String pieceName = null, String from = null, 
            String to = null)//, String direction = null)
        {
            /*
             * @parameter pieceName: name of the piece to move (KNIGHT, KING, etc)
             * @parameter from: a2, b3, c4, etc
             * @parameter to: a2, b3, c4, etc. 
             * This parameter will filter by the possible moves.
             * If just one piece can move to this position, it will be automatic
             * @parameter direction: up, down, left, right, etc
             */

            from = getCurrentOrUpdate(from, "from");
            //to = getCurrentOrUpdate(to ,"to");
            //direction = getCurrentOrUpdate(direction ,"direction");
            pieceName = getCurrentOrUpdate(pieceName ,"pieceName");
            
            //Console.WriteLine("Dictionary: ");
            //foreach (KeyValuePair<string, string> con in context)
            //{
            //    Console.WriteLine(con.Key + ": " + con.Value);
            //}

            if (pieceName == null && from == null)
            {
                //sendMessage(NO_KNOWN_PIECE_ERROR);
                return new ArrayList();
            }

            string piece;

            if (from == null || from.Length > 2) { 
                piece = pieceName == "KNIGHT" ? pieceColor + "n" : pieceColor + pieceName.ToLower()[0];

            }

            else {
                piece = " square-" + getHorizontalNumber(from[0]) + from[1];
            }


            var possiblePieces = FindChildrenByClass(board, piece);

            if (possiblePieces.Count <= 1) {
                return possiblePieces;
            }

            // if there are more than one piece, filter by direction
            if (from == null || from.Length <= 2) {
                return possiblePieces;
            }
            
            var currentPiece = (IWebElement)possiblePieces[0];
            var possiblePiecesOnDirection = new ArrayList();

            foreach (IWebElement child in possiblePieces) {
                if (isOnSamePositionInADirection(currentPiece, child, from))
                {
                    possiblePiecesOnDirection.Add(child);
                }
                else if (isOnDirection(currentPiece, child, from))
                {
                    currentPiece = child;
                    possiblePiecesOnDirection = new ArrayList() { child };
                }
                
            }
            
            return possiblePiecesOnDirection;

        }



        //public void play()
        //{
        //    var pieces = FindChildrenByClass(board, pieceColor);

        //    IWebElement piece = (IWebElement)pieces[0];
        //    piece.Click();


        //    var possiblePositions = FindChildrenByClass(board, "hint");

        //    Actions action = new Actions(driver);
        //    IWebElement position1 = (IWebElement)possiblePositions[0];
        //    action.MoveToElement(position1).Click().Perform();


        //    //table = driver.FindElement(By.XPath(MOVE_TABLE));

        //    //ArrayList moves = FindChildrenByClass(table, "move");




        //    //do
        //    //{
        //    //    isCurrent = isCurrentPlayer((IWebElement)moves[moves.Count - 1], playerColor);

        //    //    Console.WriteLine(isCurrent);

        //    //    System.Threading.Thread.Sleep(WAIT_TIME);
        //    //} while (!isCurrent);

        
        //    //driver.Close();
        //}

        // -------------------------------- EXTRAS

        public string getFromRecognized(dynamic recognized, string key, string defaultValue = null)
        {
            return recognized[key] != null ? (string)recognized[key] : defaultValue;

        }

        public int getHorizontalNumber(char letter)
        {
            return (int)letter - 64;
        }

        public string getPiecePosition(IWebElement piece) {
            var pieceClass = piece.GetAttribute("class");

            return pieceClass.Substring(pieceClass.Length - 2);
        }
        

        public bool isOnSamePositionInADirection(IWebElement element, IWebElement target, string direction)
        {
            var el = element.Location;
            var t = target.Location;

            switch (direction)
            {
                case ("LEFT"):
                    return t.X == el.X;

                case ("RIGHT"):
                    return t.X == el.X;

                case ("FRONT"):
                    return t.Y == el.Y;

                case ("BACK"):
                    return t.Y == el.Y;

                default:
                    return false;
            }
        }

        public bool isOnDirection(IWebElement element, IWebElement target, string direction) {

            var el = element.Location;
            var t = target.Location;

            switch (direction) {
                
                case ("LEFT"):
                    return t.X < el.X;

                case ("RIGHT"):
                    return t.X > el.X;

                case ("FRONT"):
                    return t.Y < el.Y;

                case ("BACK"):
                    return t.Y > el.Y;

                default:
                    return false;
            }
        }

        public string getCurrentOrUpdate(string variable, string key, string defaultVal = null) {

            if (variable == null)
            {
                return getFromContext(key, defaultVal);
            }

            context[key] = variable;

            return variable;
        }

        public string getFromContext(string key, string defaultVal = null) {
            if (context.ContainsKey(key)) {
                return context[key];
            }

            return defaultVal;
        }
        
        public void sendMessage(String message) {
            mmic.Send(lce.NewContextRequest());
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, message);
            mmic.Send(exNot);
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
