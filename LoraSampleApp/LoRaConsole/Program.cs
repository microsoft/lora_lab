using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;
using Microsoft.Windows.AI.Text.Experimental;
using System.Diagnostics;

try
{
    if (LanguageModel.GetReadyState() == AIFeatureReadyState.NotReady)
    {
        var sw = new Stopwatch();
        sw.Start();
        var languageModelDeploymentOperation = LanguageModel.EnsureReadyAsync();
        languageModelDeploymentOperation.Progress = (p, delta) =>
        {
            Console.WriteLine($"Progress: {delta:F3}    Time: {sw.ElapsedMilliseconds}");
        };
        await languageModelDeploymentOperation;
        sw.Stop();
    }
    var session = await LanguageModel.CreateAsync();
     string systemPrompt = "You are a visual studio assistant providing answers to questions about visual studio";
    var context = session.CreateContext(systemPrompt);

    // Load the adapter
    string adapterFilePath = @"C:\Users\anarvekar\.aitk\models\wcr\phi-silica-adapter\3.6\aiacajobs7p3i-bpw2tma\1.4.0\job1VSdata.safetensors";//@"C:\Users\anarvekar\Downloads\Test 1.safetensors";

    var langModExp = new LanguageModelExperimental(session);
    LowRankAdaptation loraAdapter = langModExp.LoadAdapter(adapterFilePath);

    // Set the adapter in LanguageModelOptions
    LanguageModelOptionsExperimental options = new LanguageModelOptionsExperimental
    {
        LoraAdapter = loraAdapter
    };

    Console.WriteLine("With LoRa...");
    await GetResponse(context, langModExp, options);

    Console.WriteLine("Without LoRa...");
    await GetResponse(context, langModExp, new LanguageModelOptionsExperimental());
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.InnerException?.Message);
}

static async Task GetResponse(LanguageModelContext context, LanguageModelExperimental session, LanguageModelOptionsExperimental options)
{
    var op = session.GenerateResponseAsync(context, "In Visual Studio, what happens if you don't provide stub methods for either the setter or the getter of a property?", options);
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
    var response = await op;
    Console.WriteLine(response.Text);
    sw.Stop();
    Console.WriteLine();
    Console.WriteLine($"TTFT: {ttft}ms Tokens: {tokenCount}  Time: {sw.ElapsedMilliseconds}ms  TPS: {1000.0 * (tokenCount - 1) / sw.ElapsedMilliseconds:F2}");
    return;
}