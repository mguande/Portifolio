namespace PortfolioEditor;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args is ["--parse", var pdf])
        {
            var person = LinkedInCvParser.ParsePdf(pdf);
            Console.WriteLine(person.Name);
            Console.WriteLine(person.Role);
            Console.WriteLine(person.Email);
            Console.WriteLine(person.Linkedin);
            Console.WriteLine(person.Location);
            Console.WriteLine($"skills:{person.Skills.Count} [{string.Join("; ", person.Skills)}]");
            Console.WriteLine($"jobs:{person.Experience.Count} edu:{person.Education.Count}");
            foreach (var job in person.Experience)
                Console.WriteLine($"- {job.Period} | {job.Title} @ {job.Org}");
            return;
        }

        if (args is ["--dump", var dumpPdf])
        {
            foreach (var line in LinkedInCvParser.DebugLines(dumpPdf))
                Console.WriteLine(line);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(e.Exception.ToString(), "Erro no editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            MessageBox.Show(e.ExceptionObject.ToString(), "Erro no editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Run(new MainForm());
    }
}
