using System;
using System.Threading;

/*
NOTES FOR DEVS/MODDERS :)

THE BUG 1 AND 2 THAT I MENTION ON MY COMMENTS, ARE REFERENCE TO THE README FILE, ON THE "KNOWN BUGS" SECTION
FOR SOME REASON THAT I DON'T UNDERSTAND, THERE'S AN EXCEPTION THROWN (ArgumentOutOfRangeException) WHEN THE PLAYER MOVES TO THE LEFT CORNER OF THE SCREEN, OF COURSE I HANDLED IT, BUT IN A VERY UGLY WAY ( see more on PLAYERMOVEMENT() )
if you see any weird things happen to your character / to your ship when you move to the left corner, now you know why
FOR SOME REASON, I NEED DIFFERENT VALUES x VALUES WHEN MOVING TO THE LEFT OR RIGHT, THAT'S THE PURPOSE OF THE "leftDIRpressed" and rightDIRpressed" BOOLS ( you can see more on SHOOT()   and on PLAYERMOVEMENT() )
*/
sealed class Program
{
    public const string title = "Spacial_WAR_F A R E";

    static int maxX = 120;
    static int x = maxX / 2 - 4;
    static int previousX = x;
    static int maxY = 30;
    static int y = 25;
    static Time Time;

    static int enemyPOS;
    static (int Left, int Top) curPOS;
    //static bool leftDIRpressed;
    ////THESE TWO BOOLS EXIST BECAUSE IF YOU SHOOT WHILE MOVING TO THE LEFT, THE SHOT WILL BE MISALIGNED, SO THIS FIXES IT ( see how i use them on SHOOT() )

    static Thread spawnEnemyThread;
    static Thread playerMovementThread;
    static bool enemyKilled = false;

    public static bool bGameOver = false;

    static bool wakeThread = false;   //THIS BOOL IS HERE TO INCREASE THE PERFORMANCE IN A CLEVER WAY, IF I DINDT USE THIS BOOL, BUG 2 WOULD BE WAY TOO COMMON ( see more on SHOOT(),    see how i put it to use on CheckForKill() )

    static bool loopSwitch = false;
    static bool firstInit = true;
   internal  static bool? bEnd = null;
    //THIS BOOL IS HERE TO LOOP THE SWITCH STATEMENT


    static void Main()
    {
        SetNoResize();
        InitConsole();
     
        Time = new Time();
        StartGame();
     
    }

    public static void StartGame()
    {
        DisplayMessage("P R E S S  A N Y  K E Y  T O  S T A R T", true);
        InitGameVars();

        StartThreads();

        Time.RestartGameTimers();
        CheckForKill();
        if (CheckEnd())
        {
            StartGame();
        }
    }

    private static bool CheckEnd()
    {
        while (!bEnd.HasValue)
        {
          //wait
        }
        var tmpEndVal = bEnd.Value;
        bEnd = null;
        SCORE.ScoreToFile();
        return tmpEndVal;
    }

    private static void InitConsole()
    {
        Console.CursorVisible = false;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear(); //THIS IS HERE IF AN USER WANTS TO RUN THE GAME STRAIGHT THROUGH CMD, BASH, ETC.
        Console.ForegroundColor = ConsoleColor.White;
        Console.SetWindowSize(maxX, maxY);
        Console.SetBufferSize(maxX, maxY);
    }

    private static void StartThreads()
    {
        spawnEnemyThread = new Thread(SpawnEnemy);
        spawnEnemyThread.Start();
        Thread.Sleep(250);//THIS IS HERE TO ENSURE THAT THE ENEMY WILL SPAWN BEFORE THE PLAYER, AVOIDING BUGS
        playerMovementThread = new Thread(PlayerMovement);
        playerMovementThread.Start();
    }


    internal static ConsoleKeyInfo DisplayMessage(string message, bool clearConsoleFirst = false, bool readKey = true)
    {
        ConsoleKeyInfo keyInfo = default(ConsoleKeyInfo);
        if (clearConsoleFirst)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }
        Console.SetCursorPosition(38, 13);
        Console.WriteLine(message);
        if (readKey)
        {
            keyInfo = Console.ReadKey();
            Console.Clear();
        }
        
        return keyInfo;

    }

    private static void InitGameVars()
    {
        firstInit = true;
        bGameOver = false;
        SCORE.ScoreINT = 0;
        enemyKilled = false;
        loopSwitch = false;
        SetMaintitle(title);


    }

    private static void SetNoResize()
    {
        IntPtr handle = NoResizing.GetConsoleWindow();
        IntPtr sysMenu = NoResizing.GetSystemMenu(handle, false);


        //ALL THIS STUFF IS TO ENSURE THAT THE PLAYER WONT BE ABLE TO RESIZE THE WINDOW (see more on "NoResizing.cs")
        if (handle != IntPtr.Zero)
        {
            NoResizing.DeleteMenu(sysMenu, NoResizing.SC_CLOSE, NoResizing.MF_BYCOMMAND);
            NoResizing.DeleteMenu(sysMenu, NoResizing.SC_MINIMIZE, NoResizing.MF_BYCOMMAND);
            NoResizing.DeleteMenu(sysMenu, NoResizing.SC_MAXIMIZE, NoResizing.MF_BYCOMMAND);
            NoResizing.DeleteMenu(sysMenu, NoResizing.SC_SIZE, NoResizing.MF_BYCOMMAND);
        }
        //ALL THIS STUFF IS TO ENSURE THAT THE PLAYER WONT BE ABLE TO RESIZE THE WINDOW (see more on "NoResizing.cs")
    }

    public static void SetMaintitle(string text)
    {
        Console.Title = text;
    }

    static void Draw(int x, int y, ConsoleColor color, string stringToWrite)
    {

        if (x < 0) x = 0;
        if (x > maxX) x = maxX;
        Console.SetCursorPosition(x, y);
        Console.BackgroundColor = color;
        Console.Write(stringToWrite);

    }


    static void CheckForKill()
    {

        while (!bGameOver)
        {

            curPOS = Console.GetCursorPosition();
            if (curPOS.Top == 2)
            {
                if (curPOS.Left == enemyPOS || curPOS.Left == enemyPOS + 1)
                {

                    enemyKilled = true;
                    Time.ResetTimers(); //RESETS THE TIMER
                }
            }

            if (!wakeThread) { Thread.Sleep(50); } // THIS IS HERE TO INCREASE THE PERFORMANCE (however there's still cpu spikes) (this is what causes bug 2, but its necessary for the performance)
                                                   //HAD I JUST SIMPLY USED "Thread.Sleep(1);" CONSTANTLY (without the wakeTHREAD) THE BUG THAT THE ENEMY SURVIVES WOULD BE WAY TOO COMMON
        }

    }


    static void PlayerMovement()
    {


        while (!bGameOver)
        {
            switch (Console.ReadKey(true).Key)
            {

                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    x--;
                    EraseAndRedrawSpaceShip();

                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    x++;
                    EraseAndRedrawSpaceShip();

                    break;

                case ConsoleKey.Spacebar:
                case ConsoleKey.S:
                    Shoot();
                    break;

            }

        }
    }

    private static void EraseAndRedrawSpaceShip()
    {
        if (x > maxX - 3) x = 0;
        else if (x < 0) x = maxX - 3;
        DrawSpaceShip(previousX, ConsoleColor.Black);
        DrawSpaceShip(x);
    }


    private static void DrawSpaceShip(int posX, ConsoleColor color = ConsoleColor.White)
    {

        Draw(posX, y, color, GetSpaces(6));
        Draw(posX + 2, y - 1, color, GetSpaces(2)); //DRAWS THE "CANNON" OF YOUR SHIP
        previousX = posX;
    }


    static void Shoot()
    {

        for (int fall = maxY - 7; fall > 1; fall--)
        {

            if (bGameOver) break;

            if (fall == 3) { wakeThread = true; }   //THIS BOOL ONLY TURNS TRUE WHEN THE MISSILE IS ABOUT TO HIT THE ENEMY


            Draw(x + 2, fall, ConsoleColor.White, GetSpaces(2));
            Thread.Sleep(51);
            Draw(x + 2, fall, ConsoleColor.Black, GetSpaces(2));

        }

        wakeThread = false;   //AFTER ALL THE SHOOTING ENDS, IT TURNS FALSE AGAIN

    }


    static void SpawnEnemy()
    {

        while (!bGameOver)
        {

            Thread.Sleep(50); //THIS IS HERE TO INCREASE THE PERFORMANCE OF THE GAME

            if (enemyKilled || firstInit)
            {
                enemyKilled = false;
                Draw(enemyPOS - 2, 2, ConsoleColor.Black, GetSpaces(10));  //USED TO "ERASE" THE ENEMY

                Random randoM = new Random();
                enemyPOS = randoM.Next(5, maxX - 9);

                Draw(enemyPOS - 1, 2, ConsoleColor.White, GetSpaces(3));
                if (!firstInit)
                {
                    SCORE.ScoreINT++;
                }
                SCORE.ScoreToScreen();
                firstInit = false;
            }

        }
    }
    static string GetSpaces(int spacesCount)
    {
        return new string(' ', spacesCount);
    }
}
