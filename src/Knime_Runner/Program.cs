using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class KnimeRunner
{
    private static string KNIME_PATH = @"C:\Users\admin\AppData\Local\Programs\KNIME\knime.exe";
    private static string WORKFLOW_PATH = @"C:\Users\merkira\_Cell\knime-workspace\KNIME_ISI_Projeto";
    private static string LOG_PATH = @"C:\Discardable\log.txt";

    static void Main(string[] args)
    {
        Console.WriteLine("Sistema iniciado. O KNIME será executado agora e repetido a cada 24 horas.");

        // Garantir que a pasta dos logs existe
        Directory.CreateDirectory(Path.GetDirectoryName(LOG_PATH));

        while (true)
        {
            ExecutarKnime();

            Console.WriteLine("\nA aguardar 24 horas para a próxima execução...");
            Thread.Sleep(86400000); // 24 horas em milissegundos
        }
    }

    static void ExecutarKnime()
    {
        string dataHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        EscreverLog($"[{dataHora}] Iniciar execução do KNIME...");

        Process processo = new Process();
        processo.StartInfo.FileName = KNIME_PATH;

        // -nosave é adicionado para garantir saída limpa após execução
        processo.StartInfo.Arguments =
            $"-nosplash -reset -nosave -application org.knime.product.KNIME_BATCH_APPLICATION -workflowDir=\"{WORKFLOW_PATH}\"";

        processo.StartInfo.UseShellExecute = false;
        processo.StartInfo.RedirectStandardOutput = true;
        processo.StartInfo.RedirectStandardError = true;

        processo.Start();

        // Aguarda a conclusão sem limite de tempo
        processo.WaitForExit();

        if (processo.ExitCode == 0)
        {
            EscreverLog($"[{DateTime.Now}] KNIME terminou com sucesso.");
        }
        else
        {
            EscreverLog($"[{DateTime.Now}] Erro ao executar KNIME. Código: {processo.ExitCode}");
            string error = processo.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(error))
                EscreverLog($"Detalhes do erro: {error}");
        }
    }

    static void EscreverLog(string texto)
    {
        Console.WriteLine(texto);
        File.AppendAllText(LOG_PATH, texto + Environment.NewLine);
    }
}
