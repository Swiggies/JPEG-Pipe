using System;

namespace JPEGPipe
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new JPEGPipe())
                game.Run();
        }
    }
}
