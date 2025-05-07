using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;
using Microsoft.Windows.AI.Text.Experimental;
using System.Diagnostics;

try
{
    if (LanguageModel.GetReadyState() == AIFeatureReadyState.NotReady)
    {
        var languageModelDeploymentOperation = LanguageModel.EnsureReadyAsync();
        await languageModelDeploymentOperation;
    }
    var session = await LanguageModel.CreateAsync();
    // Load the adapter
    //string adapterFilePath = @"C:\Users\brunosonnino\.aitk\models\wcr\phi-silica-adapter\3.6\aiacajobhvs77-apacp2a\1.4.0\PhiTuning4.safetensors";
    //string adapterFilePath = @"C:\Users\anarvekar\Downloads\PhiTuning4.safetensors";
    string adapterFilePath = @"C:\Users\anarvekar\.aitk\models\wcr\phi-silica-adapter\3.6\aiacajobs7p3i-ccvbz2z\1.4.0\job1.safetensors";

    var langModExp = new LanguageModelExperimental(session);
    LowRankAdaptation loraAdapter = langModExp.LoadAdapter(adapterFilePath);

    // Set the adapter in LanguageModelOptions
    LanguageModelOptionsExperimental options = new LanguageModelOptionsExperimental
    {
        LoraAdapter = loraAdapter
    };

    Console.WriteLine("With LoRa...");
    await GetResponse(langModExp, options);

    Console.WriteLine("Without LoRa...");
    await GetResponse(langModExp, new LanguageModelOptionsExperimental());
    Console.ReadLine();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.InnerException?.Message);
}

static async Task GetResponse(LanguageModelExperimental session, LanguageModelOptionsExperimental options)
{
    var op = session.GenerateResponseAsync("what is a mitochondria?", options);

    var sw = new Stopwatch();
    var isFirstToken = true;
    var tokenCount = 0;
    var ttft = 0L;

    op.Progress = (p, delta) =>
    {
        if (isFirstToken)
        {
            ttft = sw.ElapsedMilliseconds;
            sw.Restart();
            isFirstToken = false;
        }
        Console.Write(delta);
        tokenCount++;
    };

    sw.Start();
    var response  = await op;
    Console.WriteLine(response.Text);
    sw.Stop();
    Console.WriteLine();
    Console.WriteLine($"TTFT: {ttft}ms Tokens: {tokenCount}  Time: {sw.ElapsedMilliseconds}ms  TPS: {1000.0 * (tokenCount - 1) / sw.ElapsedMilliseconds:F2}");
    return;
}