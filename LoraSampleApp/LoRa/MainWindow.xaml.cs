using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AI.Text;
using Microsoft.Windows.AI.Text.Experimental;
using Windows.Storage.Pickers;

namespace LoRa
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private LanguageModelContext context;
        private LanguageModelExperimental? _loraModel;

        string prompt = "# The Fact\r\n\r\nThe equivalence between matter and energy is expressed in the equation E = mc\\u00b2.\r\n\r\n## Additional context\r\n \r\n16.2 Mass, Energy, and the Theory of Relativity\r\n\r\n16.2 Mass, Energy, and the Theory of Relativity\r\n\r\nAs we have seen, energy cannot be created or destroyed, but only converted from one form to another.  One of the remarkable conclusions derived by Albert Einstein (see Albert Einstein ) when he developed his theory of relativity is that matter can be considered a form of energy too and can be converted into energy.  Furthermore, energy can also be converted into matter.  This seemed to contradict what humans had learned over thousands of years by studying nature.  Matter is something we can see and touch, whereas energy is something objects have when they do things like move or heat up.  The idea that matter or energy can be converted into each other seemed as outrageous as saying you could accelerate a car by turning the bumper into more speed, or that you could create a bigger front seat by slowing down your car.  That would be pretty difficult to believe; yet, the universe actually works somewhat like that.\\n Converting Matter into Energy\r\n\r\nThe remarkable equivalence between matter and energy is given in one of the most famous equations:\r\n\r\nE = m c 2\r\n ";

        public MainWindow()
        {
            this.InitializeComponent();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("AdapterFilePath", out var value))
            {
                AdapterTextBox.Text = value as string ?? string.Empty;
            }
            MainGrid.Loaded += MainGrid_Loaded;
            PromptTextBox.Text = prompt;
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1290));
        }

        private async void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var readyState = LanguageModel.GetReadyState();
                if (readyState == Microsoft.Windows.AI.AIFeatureReadyState.NotSupportedOnCurrentSystem || readyState == Microsoft.Windows.AI.AIFeatureReadyState.DisabledByUser)
                {
                    InitTxt.Text = "Phi Silica not available in this system";
                    return;
                }
                if (readyState == Microsoft.Windows.AI.AIFeatureReadyState.NotReady)
                {
                    InitTxt.Text = "Installing Phi Silica";
                    var installTask = LanguageModel.EnsureReadyAsync();

                    installTask.Progress = (installResult, progress) => Console.WriteLine($"Progress: {progress * 100:F1}");

                    var result = await installTask;
                    InitTxt.Text = "Done: " + result.Status.ToString();
                }
                var languageModel = await LanguageModel.CreateAsync();
                string systemPrompt = "You will be given a fact and some additional context. Respond with a relevant question, one correct answer and some incorrect answers. Reply with a strict JSON string for class {question: string, answers: [{answer: string, correct: bool}], gettyImage: string}, wrapped in ```json tags.";
                context = languageModel.CreateContext(systemPrompt);

                _loraModel = new LanguageModelExperimental(languageModel);
            }
            catch (Exception ex)
            {
                InitTxt.Text = "Error: " + ex.Message;
                Console.WriteLine(ex);
            }
        }

        private async void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            picker.FileTypeFilter.Add(".safetensors");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                AdapterTextBox.Text = file.Path;
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["AdapterFilePath"] = file.Path;
            }
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (_loraModel == null)
            {
                InitTxt.Text = "Language model not initialized";
                return;
            }

            InitTxt.Text = string.Empty;

            var adapterFilePath = AdapterTextBox.Text;
            prompt = PromptTextBox.Text;

            // Load the adapter
            var loraAdapter = _loraModel.LoadAdapter(adapterFilePath);

            // Set the adapter in LanguageModelOptions
            LanguageModelOptionsExperimental options = new LanguageModelOptionsExperimental
            {
                LoraAdapter = loraAdapter
            };

            LoraTxt.Text = string.Empty;
            NoLoraTxt.Text = string.Empty;

            try
            {
                await GetResponse(_loraModel, LoraTxt, InitTxt, new LanguageModelOptionsExperimental());
            }
            catch (Exception ex)
            {
                InitTxt.Text = "Error not using adapter: " + ex.Message;
                Console.WriteLine(ex);
            }

            try
            {
                await GetResponse(_loraModel, NoLoraTxt, InitTxtNoLora, options);
            }
            catch (Exception ex)
            {
                InitTxt.Text = "Error with adapter: " + ex.Message;
                Console.WriteLine(ex);
            }
        }

        async Task GetResponse(LanguageModelExperimental session, TextBlock textBox, TextBlock statusBlock, LanguageModelOptionsExperimental options)
        {
            var op = session.GenerateResponseAsync(context, prompt, options);

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
                DispatcherQueue.TryEnqueue(() => textBox.Text += delta);
                tokenCount++;
            };

            sw.Start();
            var response = await op;
            sw.Stop();
            DispatcherQueue.TryEnqueue(() => statusBlock.Text += $"TTFT: {ttft}ms Tokens: {tokenCount}  Time: {sw.ElapsedMilliseconds}ms  TPS: {1000.0 * (tokenCount - 1) / sw.ElapsedMilliseconds:F2}   ");
            Console.WriteLine();
            Console.WriteLine($"TTFT: {ttft}ms Tokens: {tokenCount}  Time: {sw.ElapsedMilliseconds}ms  TPS: {1000.0 * (tokenCount - 1) / sw.ElapsedMilliseconds:F2}");
            return;
        }
    }
}
