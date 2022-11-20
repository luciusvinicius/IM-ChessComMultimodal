using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        static string BOARD_FRIEND = "//*[@id=\"board-single\"]";
        static string BOARD_FRIEND2 = "//*[@id=\"board\"]";
        static string GIVE_UP_BUTTON = "/html/body/div[4]/div/div[2]/div[2]/div[2]/a";


        static string CLOSE_AD = "/html/body/div[25]/div[2]/div/div/button";
        static string CLOSE_AD2 = "/html/body/div[27]/div[2]/div/div/button";


        static string COORDS = "/html/body/div[2]/div[2]/chess-board/svg[1]";
        static string MOVE_TABLE = "/html/body/div[3]/div/vertical-move-list";
        static string FRIENDS_LIST = "/html/body/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/div[3]";

        static string COMPUTER_START_BUTTON = "/html/body/div[4]/div/div[2]/button";
        static string FRIEND_START_BUTTON = "/html/body/div[4]/div/div[2]/div/div[2]/div[1]/button";
        static string FRIEND_AGREE_BUTTON = "/html/body/div[15]/div[2]/div/div/div/div[8]/button";


        // ------------------------ CONSTS
        static int WAIT_TIME = 1000;

        static string SITE_URL = "https://www.chess.com";
        static string LOGIN_URL = "https://www.chess.com/login_and_go?returnUrl=" + SITE_URL;
        static string COMPUTER_URL = "https://www.chess.com/play/computer";
        static string FRIENDS_URL = "https://www.chess.com/friends";
        static string VS_FRIENDS_URL = "https://www.chess.com/play/online/new?opponent=";

        // ------------------------ PHRASES
        static string FRIEND_CHOOSE = "Escolha um amigo dentre a lista de amigos";
        static string CONFIRM_CHOICE = "Você deseja";
        static string END_CONFIRM_CHOICE = "Por favor responda com sim ou não.";
        static string REPEAT_PHRASE = "Então poderia repetir a frase novamnte?";
        static string GAME_STARTED = "Jogo iniciado! Tenha um bom jogo!";
        static string GAME_ENDED = "Jogo finalizado!";
        static string FRIEND_WAIT_FOR_REQUEST = "Espere que o seu amigo aceite o pedido para começar a jogar.";
        static string LETS_PLAY = "Contra quem quer jogar? Computador ou amigos?";

        static string NO_KNOWN_PIECE_ERROR = "Não consegui identificar a peça, poderia indicá-la novamente?";
        static string NO_KNOWN_ACTION_ERROR = "Não consegui identificar a ação, poderia indicá-la novamente?";
        static string WRONG_MOVE_ERROR = "Possibilidade de movimento não existente, poderia indicá-lo novamente?";
        static string NO_POSSIBLE_MOVES_ERROR = "Não existem movimentos possíveis para essa peça";
        static string AMBIGUOS_MOVEMENT = "Existe mais de um movimento possível para essa peça, poderia indicar o destino?";
        static string AMBIGUOS_PIECE = "Existe mais de uma peça com essa descrição, poderia indicar a peça?";
        static string FRIEND_CHOOSE_COUNT_ERROR = "Amigo não encontrado, por favor, tente novamente";

        // ------------------------ CONFIRMATIONS

        
        float CONFIDENCE_BOTTOM_LIMIT = 0.1f;
        float CONFIDENCE_BOTTOM_UPPER_LIMIT = 0.59f;
        Dictionary<string, string> semanticDict = new Dictionary<string, string>()
        {
            ["MOVE"] = "mover",
            ["CAPTURE"] = "capturar",
            ["PLAY AGAINST"] = "jogar contra",
            ["END"] = "finalizar a partida",
            ["WHITE"] = "branco",
            ["BLACK"] = "preto",
            ["KING"] = "rei",
            ["QUEEN"] = "rainha",
            ["ROOK"] = "torre",
            ["BISHOP"] = "bispo",
            ["KNIGHT"] = "cavalo",
            ["PAWN"] = "peão",
            ["TO"] = "para",
            ["FROM"] = "de",
            ["RIGHT"] = "direita",
            ["LEFT"] = "esquerda",
            ["FRONT"] = "frente",
            ["BACK"] = "trás",
            ["FRIEND"] = "amigo",
            ["COMPUTER"] = "computador",
        };

        // ------------------------ VARS

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

        static string WAITING_CONFIRM = "WAITING_CONFIRM";

        public MainWindow()
        {

            //var amogus = new List<Class1>();
            //amogus.Add(new Class1("The Lord of the Rings", "J.R.R. Tolkien", 1178));
            //amogus.Add(new Class1("The Hobbit", "J.R.R. Tolkien", 310));
            //amogus.Add(new Class1("The Silmarillion", "J.R.R. Tolkien", 429));
            //amogus.Add(new Class1("The Children of Hurin", "J.R.R. Tolkien", 288));

            //amogus.Sort((x, y) => y.pagecount - x.pagecount);

            //for (int i = 0; i < amogus.Count; i++)
            //{
            //    Console.WriteLine(amogus[i].GetDescription());
            //}
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

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");

            sendMessage(LETS_PLAY);
            //play();

        }

        private void MmiC_Message(object sender, MmiEventArgs e) //Message spoken by user, convert to Json
        {
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            Console.WriteLine("JSON:");
            Console.WriteLine(json);
            var temp = json.recognized;
            Dictionary<string, string> recognized = JsonConvert.DeserializeObject<Dictionary<string, string>>(temp.ToString());
            Console.WriteLine("Recognized: ");


            foreach (KeyValuePair<string, string> kvp in recognized)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }

            performAction(recognized);


        }

        public void performAction(Dictionary<string, string> dict, bool ignoreConfidence = false)
        {
            float confidence = float.Parse(dict["Confidence"], CultureInfo.InvariantCulture);

            Console.WriteLine("Confidence: " + confidence);

            string entity = getFromRecognized(dict, "Entity");
            string action = getFromRecognized(dict, "Action", "");

            action = getCurrentOrUpdate(action, "action", "");

            bool isConfident = true;

            switch (action)
            {
                case "START":
                    Console.WriteLine("START");
                    if (driver.Url != COMPUTER_URL && !driver.Url.Contains(VS_FRIENDS_URL)) return;
                    startGame();
                    break;

                case "END":
                    Console.WriteLine("GIVE UP");
                    if (!ignoreConfidence) {
                        isConfident = generateConfidence(confidence, dict, forceConfidence: true);
                    }
                    if (isConfident)
                    {
                        giveUp();
                    }
                    break;
                    
                case "MOVE":
                    Console.WriteLine("MOVE");
                    string from = getFromRecognized(dict, "PositionInitial");
                    string to = getFromRecognized(dict, "PositionFinal");
                    int pieceNumber = dict.ContainsKey("NumberInitial") ? int.Parse(dict["NumberInitial"]) : 1;

                    var possiblePieces = getPossiblePieces(
                        pieceName: entity,
                        from: from,
                        number: pieceNumber
                    );

                    foreach (var piece in possiblePieces)
                    {
                        Console.WriteLine(piece.GetAttribute("class"));
                    }

                    int finalNumer = dict.ContainsKey("NumberFinal") ? int.Parse(dict["NumberFinal"]) : 1;
                    if (!ignoreConfidence) isConfident = generateConfidence(confidence, dict);
                    if (isConfident)
                    {
                        movePieces(
                            pieces: possiblePieces,
                            to: to,
                            number: finalNumer
                        );
                    }

                    break;

                case "PLAY AGAINST":
                    Console.WriteLine("PLAY AGAINST");

                    int friendNumber = dict.ContainsKey("Number") ? int.Parse(dict["Number"]) : -1;
                    if (!ignoreConfidence) isConfident = generateConfidence(confidence, dict);
                    if (isConfident)
                    {
                        opponentType(entity, friendNumber);
                        playAgainst(friendNumber);
                    }

                    break;

                case "SPECIAL":
                    String specialMove = getFromRecognized(dict, "SpecialMove");
                    if (specialMove == "ROQUE")
                    {
                        perfomRoque();
                    }
                    break;
                    
                case "ANSWER":
                    if (!context.ContainsKey(WAITING_CONFIRM) || !bool.Parse(context[WAITING_CONFIRM])) return;
                    bool answer = entity == "YES";
                    if (answer) performAction(context, true);
                    else
                    {
                        sendMessage(REPEAT_PHRASE);
                        context.Clear();
                    }
                    break;
                case "CAPTURE":
                    Console.WriteLine("CAPTURING");
                    string initialPos = getFromRecognized(dict, "PositionInitial");
                    string finalPos = getFromRecognized(dict, "PositionFinal");
                    pieceNumber = dict.ContainsKey("NumberInitial") ? int.Parse(dict["NumberInitial"]) : 1;

                    possiblePieces = getPossiblePiecesCapture(
                        pieceName: entity,
                        from: initialPos,
                        number: pieceNumber
                    );

                    Console.WriteLine("Possible pieces: " + possiblePieces.Count);
                    
                    foreach (var piece in possiblePieces)
                    {
                        Console.WriteLine(piece.GetAttribute("class"));
                    }

                    finalNumer = dict.ContainsKey("NumberFinal") ? int.Parse(dict["NumberFinal"]) : 1;
                    string target = getFromRecognized(dict, "Target");
                    capture(
                            pieces: possiblePieces,
                            to: finalPos,
                            number: finalNumer,
                            targetName: target
                        );

                    break;

                case "BACK":
                    driver.Navigate().Back();
                    break;

                default:
                    sendMessage(NO_KNOWN_ACTION_ERROR);
                    break;
            }
        }

        // ------------------------------ CAPTURE
        
        public List<IWebElement> getPossiblePiecesCapture(String pieceName = null, 
            String from = null, int number = 1)
        {

            var pieces = getPossiblePieces(pieceName: pieceName, from: from, number: number);
            


            Console.WriteLine("Possible pieces func: " + pieces.Count);

            foreach (var piece in pieces)
            {
                Console.WriteLine(piece.GetAttribute("class"));
            }

            if (pieces.Count == 0) {
                // pieces = every player is a possible piece
                pieces = FindChildrenByClass(board, pieceColor);
            }
            
            return pieces;
        }

        public void capture(List<IWebElement> pieces, string to = null, int number = -1, string targetName = "") {
            List<IWebElement> correctPieces = new List<IWebElement>();
            List<List<IWebElement>> possibleMovesList = new List<List<IWebElement>>();

            foreach (IWebElement piece in pieces)
            {
                var possibleMoves = findPossiblePositions(piece, to, number, target: targetName, isCapture: true);
                if (possibleMoves.Count >= 1)
                {
                    correctPieces.Add(piece);
                    possibleMovesList.Add(possibleMoves);
                }
            }

            Console.WriteLine("Correct pieces: " + correctPieces.Count);
            foreach (var piece in correctPieces)
            {
                Console.WriteLine(piece.GetAttribute("class"));
            }

            if (correctPieces.Count == 1)
            {
                var piece = correctPieces[0];
                string pieceName = Char.ToString(piece.GetAttribute("class")[7]);
                context["pieceName"] = pieceName;
                var possibleMoves = possibleMovesList[0];
                Console.WriteLine("Possible moves: " + possibleMoves.Count);
                foreach (var possibleMove in possibleMoves) { 
                    Console.WriteLine(possibleMove.GetAttribute("class"));
                }

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

        public List<IWebElement> getTargets(IWebElement piece, 
            string to = null, int number = -1, string targetName="") { 


            
            return null;
        }

        // ------------------------------ Rate confidence

        public bool generateConfidence(float confidence, Dictionary<string,string> recognized, bool forceConfidence = false)
        {
            bool isConfident = true;
            context[WAITING_CONFIRM] = "false";
            string phrase = CONFIRM_CHOICE + " " + getFromSemanticDict(recognized["Action"]);

            if (confidence < CONFIDENCE_BOTTOM_LIMIT)
            {
                isConfident = false;
            }
            else if (confidence < CONFIDENCE_BOTTOM_UPPER_LIMIT || forceConfidence)
            {
                isConfident = false;
                context[WAITING_CONFIRM] = "true";
                foreach (KeyValuePair<string, string> kvp in recognized)
                {
                    string key = kvp.Key;
                    string value = kvp.Value;
                    context[key] = value;
                    
                }

                foreach (KeyValuePair<string, string> kvp in context)
                {
                    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }


                switch (context["Action"]) {

                    case "MOVE":
                        phrase += getFromSemanticDict(context["Entity"]);
                        phrase += getFromSemanticDict("FROM") + getPhraseFromContext("NumberInitial", "ª") + getFromSemanticDict(context["PositionInitial"]);
                        phrase += getFromSemanticDict("TO") + getPhraseFromContext("NumberFinal", "ª") + getFromSemanticDict(context["PositionFinal"]);
                        break;

                    case "PLAY AGAINST":
                        if (context["Entity"] == "FRIEND") phrase += getPhraseFromContext("Number", "º");
                        phrase += getFromSemanticDict(context["Entity"]);
                        break;

                }


                phrase = phrase.Trim() + "?" + END_CONFIRM_CHOICE;
                Console.WriteLine("Confident Message: " + phrase);
                //Confident Message: Você deseja MOVE mover peão de esquerda para frente
                sendMessage(phrase);
            }

            return isConfident;
        }

        public string getFromSemanticDict(string key, string defaultOutput = "") {
            if (semanticDict.ContainsKey(key))
            {
                return " " + semanticDict[key];
            }
            return defaultOutput;
        }

        public string getPhraseFromContext(string key, string extra = "") {
            if (context.ContainsKey(key)) {
                return " " + context[key] + extra;
            }
            return "";
        }

        // ------------------------------ START GAME

        public void startGame() {
            Console.WriteLine("Start Game");
            IWebElement button;
            if (driver.Url == COMPUTER_URL)
            {
               button = driver.FindElement(By.XPath(COMPUTER_START_BUTTON));
            
            }
            else if (driver.Url.Contains(VS_FRIENDS_URL))
            {
                button = driver.FindElement(By.XPath(FRIEND_START_BUTTON));
            }
            else
            {
                return;
            }

            Console.WriteLine("Clicking start button");
            button.Click();

            if (driver.Url == COMPUTER_URL)
            {
                System.Threading.Thread.Sleep(WAIT_TIME);
                button = driver.FindElement(By.XPath(COMPUTER_START_BUTTON));
                button.Click();
                sendMessage(GAME_STARTED);

            }
            else if (driver.Url.Contains(VS_FRIENDS_URL)) {
                System.Threading.Thread.Sleep(WAIT_TIME);
                try
                {
                    button = driver.FindElement(By.XPath(FRIEND_AGREE_BUTTON));
                    button.Click();

                    System.Threading.Thread.Sleep(WAIT_TIME);
                    button = driver.FindElement(By.XPath(FRIEND_START_BUTTON));
                    button.Click();
                }
                catch (NoSuchElementException)
                {
                    
                }


                string friendName = driver.Url.Substring(driver.Url.LastIndexOf("=") + 1);
                Console.WriteLine("Friend name: " + friendName);
                sendMessage("Desafio enviado para " + friendName + ". " + FRIEND_WAIT_FOR_REQUEST);
                
            }


        }

        // ------------------------------ GIVE UP

        public void giveUp() {
            Console.WriteLine("Give Up sussy");
            IWebElement button = driver.FindElement(By.XPath(GIVE_UP_BUTTON));
            Console.WriteLine("button: " + button);
            Console.WriteLine("button class: " + button.GetAttribute("class"));
            button.Click();
            sendMessage(GAME_ENDED);
        }



        // ------------------------------ PLAY AGAINS PC OR HUMAN

        public void playAgainst(int friendID) {
            /*
             * @param entity: "1", "2", etc...
             */
            if (friendID == -1 || driver.Url != FRIENDS_URL) return;
            
            IWebElement friendsList = driver.FindElement(By.XPath(FRIENDS_LIST));
            List<IWebElement> friends;

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

            redirect(VS_FRIENDS_URL + friendName, hasBoard: true, boardName: BOARD_FRIEND2);


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
        
        public void redirect(String URL, bool hasBoard = false, bool hasAd = false, string boardName = null) {
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
                if (boardName == null)
                {
                    boardName = BOARD;
                }
                board = driver.FindElement(By.XPath(boardName));
                Console.WriteLine("gamer board: " + board);
                Console.WriteLine("gamer board: " + board.GetAttribute("class"));
                playerColor = getPlayerColor(board);
                pieceColor = "piece " + playerColor[0];
            }
        }

        // ------------------------------ MOVEMENT

        public void movePieces(List<IWebElement> pieces, string to = null, int number = -1) { //, string direction = null) {

            List<IWebElement> correctPieces = new List<IWebElement>();
            List<List<IWebElement>> possibleMovesList = new List<List<IWebElement>>();
            
            foreach (IWebElement piece in pieces)
            {
                var possibleMoves = findPossiblePositions(piece, to, number);//, direction);
                if (possibleMoves.Count >= 1) {
                    correctPieces.Add(piece);
                    possibleMovesList.Add(possibleMoves);
                }
            }
            
            Console.WriteLine("Correct pieces: " + correctPieces.Count);

            if (correctPieces.Count == 1)
            {
                var piece = correctPieces[0];
                string pieceName = Char.ToString(piece.GetAttribute("class")[7]);
                context["pieceName"] = pieceName;
                var possibleMoves = possibleMovesList[0];
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
        public List<IWebElement> findPossiblePositions(IWebElement piece, string to=null, int number = -1, 
            string target = "", bool isCapture = false) { //, string direction=null) {
            piece.Click();
            string hint = isCapture ? "capture-hint" : "hint";

            string enemyColor = playerColor == "white" ? "piece b" : "piece w";
            

            // Filter by To
            if (to != null && to.Length <= 2)
            {
                hint += " square-" + getHorizontalNumber(to[0]) + to[1];
            }

            var possiblePositions = FindChildrenByClass(board, hint);

            if (possiblePositions.Count <= 1) return possiblePositions;

            // Filter by pieceName
            if (target != "")
            {

                Console.WriteLine("targeeeet: " + target);
                string enemyPiece = target == "KNIGHT" ? enemyColor + "n" : enemyColor + target.ToLower()[0];
                var newPossiblePositions = new List<IWebElement>();
                var enemyPieces = FindChildrenByClass(board, enemyPiece);
                
                foreach (var enPiece in enemyPieces) {
                    var enemyPos = enPiece.GetAttribute("class").Substring(enPiece.GetAttribute("class").LastIndexOf(" ") + 1);
                    foreach (var possiblePosition in possiblePositions)
                    {
                        string possiblePositionClass = possiblePosition.GetAttribute("class");
                        if (possiblePositionClass.Contains(enemyPos))
                        {
                            newPossiblePositions.Add(possiblePosition);
                        }
                    }
                }
                
                
                possiblePositions = newPossiblePositions;
                Console.WriteLine("possiblePositions target: " + possiblePositions.Count);
                foreach (var possiblePosition in possiblePositions)
                {
                    Console.WriteLine(possiblePosition.GetAttribute("class"));
                }

            }

            // Filter by Direction
            if (to != null && to.Length >= 2 && possiblePositions.Count > 1)
            {
                var newPossiblePositions = new List<IWebElement>();

                foreach (IWebElement possiblePosition in possiblePositions)
                {
                    if (isOnDirection(piece, possiblePosition, to))
                    {
                        newPossiblePositions.Add(possiblePosition);
                    }
                }

                var possiblePositionsOnDirection = sortByDirection(newPossiblePositions, to, reverse: target == "");
                Console.WriteLine("possiblePositions sorted: " + possiblePositionsOnDirection.Count);
                foreach (var possiblePosition in possiblePositionsOnDirection)
                {
                    Console.WriteLine(possiblePosition.GetAttribute("class"));
                }


                var usedPos = new List<int>();
                var newPossiblePieces = new List<List<IWebElement>>();
                number--;
                
                foreach (IWebElement child in possiblePositionsOnDirection)
                {
                    int counter = newPossiblePieces.Count;
                    Console.WriteLine("counter: " + counter);

                    if (number > 0 && counter > number) { break; }

                    if (to == "LEFT" || to == "RIGHT")
                    {

                        if (usedPos.Contains(child.Location.X))
                        {
                            newPossiblePieces[counter - 1].Add(child);
                            Console.WriteLine("newPossiblePieces[counter - 1].Add(child): " + child.GetAttribute("class"));
                        }
                        else
                        {
                            newPossiblePieces.Add(new List<IWebElement>());
                            newPossiblePieces[counter].Add(child);
                            Console.WriteLine("newPossiblePieces[counter].Add(child): " + child.GetAttribute("class"));
                            usedPos.Add(child.Location.X);
                        }
                    }

                    else if (to == "BACK" || to == "FRONT")
                    {

                        if (usedPos.Contains(child.Location.Y))
                        {
                            newPossiblePieces[counter - 1].Add(child);
                        }
                        else
                        {
                            newPossiblePieces.Add(new List<IWebElement>());
                            newPossiblePieces[counter].Add(child);
                            usedPos.Add(child.Location.Y);
                        }
                    }
                }

                Console.WriteLine("newPossiblePieces (inside func): " + newPossiblePieces.Count);
                foreach (var newPossiblePiece in newPossiblePieces)
                {
                    Console.WriteLine("newPossiblePiece: " + newPossiblePiece.Count);
                }


                if (number >= 0) return newPossiblePieces[number];
                else return newPossiblePieces[newPossiblePieces.Count + ++number];

            }

            return possiblePositions;

        }

        public List<IWebElement> findPossibleCaptures(IWebElement piece, string to = null)
        { //, string direction=null) {
            piece.Click();
            string capture = "capture-hint";

            // Filter by To
            if (to != null && to.Length <= 2)
            {
                capture += " square-" + getHorizontalNumber(to[0]) + to[1];
            }

            var possiblePositions = FindChildrenByClass(board, capture);


            // Filter by Direction
            if (to != null && to.Length >= 2 && possiblePositions.Count > 1)
            {
                var newPossiblePositions = new List<IWebElement>();

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

        public List<IWebElement> getPossiblePieces(String pieceName = null, String from = null, 
            String to = null, int number=1)//, String direction = null)
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
            pieceName = getCurrentOrUpdate(pieceName ,"pieceName");

            // i have no idea what the piece can be
            if (pieceName == null && from == null)
            {
                return new List<IWebElement>();
            }

            string piece;

            if (from == null || from.Length > 2) { 
                piece = pieceName == "KNIGHT" ? pieceColor + "n" : pieceColor + pieceName.ToLower()[0];

            }

            // Exact location of piece
            else {
                piece = " square-" + getHorizontalNumber(from[0]) + from[1];
            }

            var possiblePieces = FindChildrenByClass(board, piece);

            if (possiblePieces.Count <= 1) {
                return possiblePieces;
            }
            


            // if there are more than one piece, filter by direction
            if ((from == null || from.Length <= 2) && number == 1) {
                return possiblePieces;
            }


            foreach (var possiblePiece in possiblePieces)
            {
                Console.WriteLine(possiblePiece.GetAttribute("class"));
            }

            var possiblePiecesOnDirection = sortByDirection(possiblePieces, from);

            var usedPos = new List<int>();
            var newPossiblePieces = new List<List<IWebElement>>();
            number--;

            foreach (IWebElement child in possiblePiecesOnDirection)
            {
                int counter = newPossiblePieces.Count;

                if (number > 0 && counter > number) { break; }
                
                if (from == "LEFT" || from == "RIGHT") {
                    
                    if (usedPos.Contains(child.Location.X))
                    {
                        newPossiblePieces[counter - 1].Add(child);
                    }
                    else {
                        newPossiblePieces.Add(new List<IWebElement>());
                        newPossiblePieces[counter].Add(child);
                        usedPos.Add(child.Location.X);
                    }
                }

                else if (from == "BACK" || from == "FRONT")
                {

                    if (usedPos.Contains(child.Location.Y))
                    {
                        newPossiblePieces[counter - 1].Add(child);
                    }
                    else
                    {
                        newPossiblePieces.Add(new List<IWebElement>());
                        newPossiblePieces[counter].Add(child);
                        usedPos.Add(child.Location.Y);
                    }
                }
            }

            if (number >= 0) return newPossiblePieces[number];
            else return newPossiblePieces[newPossiblePieces.Count + number];

        }

        public List<IWebElement> sortByDirection(List<IWebElement> list, string direction=null, bool reverse = false)
        {
            if (direction == null)
            {
                return list;
            }
            
            if (direction == "LEFT" && !reverse || direction == "RIGHT" && reverse)
            {
                Console.WriteLine("Sussy left");
                list.Sort((o1, o2) =>
                {
                    return o1.Location.X - o2.Location.X;
                });
                Console.WriteLine("list: " + list.Count);
                foreach (var l in list)
                {
                    Console.WriteLine("l: " + l.GetAttribute("class"));
                    Console.WriteLine("l: " + l.Location.X);
                }
            }

            else if (direction == "RIGHT" && !reverse || direction == "LEFT" && reverse) {
                list.Sort((o1, o2) =>
                {
                    return o2.Location.X - o1.Location.X;
                });
            }

            else if (direction == "FRONT" && !reverse || direction == "BACK" && reverse)
            {
                list.Sort((o1, o2) =>
                {
                    return o1.Location.Y - o2.Location.Y;
                });
            }

            else if (direction == "BACK" && !reverse || direction == "FRONT" && reverse)
            {
                list.Sort((o1, o2) =>
                {
                    return o2.Location.Y - o1.Location.Y;
                });
            }
            
            
            return list;
        }

        public int CalculateDistance(IWebElement p1, IWebElement p2)
        {
            int pos1 = Convert.ToInt32(getPiecePosition(p1));

            int pos2 = Convert.ToInt32(getPiecePosition(p2));
            int res = Math.Abs(pos2 - pos1);
            return res;
        }
        public void perfomRoque()
        {
            List<IWebElement> possiblePieces = getPossiblePieces(pieceName: "KING");
            //rei
            if (possiblePieces.Count == 1)
            {

                var possiblePositions = new List<IWebElement>();
                var possibleMovesList = findPossiblePositions(possiblePieces[0]);

                foreach (IWebElement move in possibleMovesList)
                {
                    if (CalculateDistance(possiblePieces[0], move) == 20)
                    {
                        //Fazer jogada
                        performMove(move);
                    }

                    //Arraylist dos Web elements, pegar nas classes deles e ver se estão 2 posicoes seguidas
                }
            }
        }

        //public void capture()
        //{
        //    //ArrayList possibleCaptures = new ArrayList();
        //    List<IWebElement> piecesFromUser = FindChildrenByClass(board, "piece " + playerColor[0]);
        //    //{ "capturator":"captures"}
        //    Dictionary<IWebElement, List<IWebElement>> possibleCapturesDict = new Dictionary<IWebElement, List<IWebElement>>();

        //    foreach (IWebElement piece in piecesFromUser)
        //    {
        //        var possibleCaptureByEachPiece = findPossibleCaptures(piece);

        //        if (possibleCaptureByEachPiece.Count >= 1)
        //        {
        //            possibleCapturesDict.Add(piece, possibleCaptureByEachPiece);
        //        }

        //    }
        //    // se so tiver uma key é essa q se move
        //    if (possibleCapturesDict.Count == 1)
        //    {
        //        var firstKey = possibleCapturesDict.Keys.FirstOrDefault();
        //        if (possibleCapturesDict[firstKey].Count == 1)
        //        {
        //            performMove((IWebElement)(possibleCapturesDict[firstKey][0]));
        //        }
        //    }
        //}

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

        public dynamic getFromRecognized(Dictionary<string, string> recognized, string key, string defaultValue = null)
        {
            return recognized.ContainsKey(key) ? recognized[key] : defaultValue;

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

        static List<IWebElement> FindChildrenByClass(IWebElement element, string className)
        {
            var children = element.FindElements(By.XPath(".//*"));
            var list = new List<IWebElement>();
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
            List<IWebElement> blackChildren = FindChildrenByClass(element, "square-88");
            IWebElement blackChild = (IWebElement)blackChildren[0];
            Console.WriteLine(blackChild.Location);

            List<IWebElement> whiteChildren = FindChildrenByClass(element, "square-11");
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

            List<IWebElement> moves = FindChildrenByClass(tab, "move");
            isCurrent = isCurrentPlayer((IWebElement)moves[moves.Count - 1], playerColor);
            Console.WriteLine("isCurrent: " + isCurrent);
            return isCurrent;
        }

    }
}
