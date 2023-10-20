using System;
using System.Timers;

namespace Mappy.Controllers; 

public class Timers {
    private static Timers? _instance;
    public static Timers Instance => _instance ??= new Timers();
    
    public int Counter60Hz;
    private readonly Timer timer;

    protected Timers() {
        timer = new Timer(TimeSpan.FromMilliseconds(1.0f / 60.0f * 1000.0f));
        timer.AutoReset = true;
        timer.Elapsed += (_, _) => Counter60Hz++;
        timer.Start();
    }
}