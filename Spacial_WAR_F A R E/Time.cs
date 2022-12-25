using System;
using System.Timers;

internal class Time
{
    private const int gameDuration = 5500;
    private const int titleInterval = 100;
    private Timer mainTimer { get; set; }
    private Timer titleTimer { get; set; }

    private int timeRemaining { get; set; }
    public void RestartGameTimers()
    {
        timeRemaining = gameDuration;
        mainTimer = new Timer(gameDuration);
        mainTimer.Elapsed += (s, e) =>
        {
            StopTimers();
            Program.bGameOver = true;
            DisplayGameOver();
            var keyInfo = Program.DisplayMessage(@"D O  Y O U  W A N T  T O  P L A Y  A G A I N ?   Y \ N", true, true);

            var end = ActionRestart(keyInfo);
            while (!end.HasValue)
            {
                end = ActionRestart(keyInfo);
            }
            Program.bEnd = end;
        };
        titleTimer = new Timer(titleInterval);
        titleTimer.Elapsed += (s, e) =>
        {
            if (timeRemaining >= titleInterval) timeRemaining -= titleInterval;
            Program.SetMaintitle(Program.title + $" {timeRemaining} ms remaining");
        };
        StartTimers();
    }

    internal static bool? ActionRestart(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Y: return true;
            case ConsoleKey.N: return false;
        }
        return null;
    }
    public void ResetTimers()
    {
        StopTimers();
        StartTimers();
    }
    public void StartTimers()
    {
        timeRemaining = gameDuration;

        mainTimer.Start();
        titleTimer.Start();
    }
    public void StopTimers()
    {
        System.Threading.Thread.Sleep(200);  //THIS IS HERE TO ENSURE THAT EVERYTHING WILL "ABORT/END" PROPERLY"
        mainTimer.Stop();
        titleTimer.Stop();
    }
    private void DisplayGameOver(int repeatTimes = 4)
    {
        for (int i = 0; i < repeatTimes; i++)
        {
            Console.Clear();
            System.Threading.Thread.Sleep(150);
            Console.Write(GameOverString.GameOver);
            System.Threading.Thread.Sleep(150);
        }
    }
}