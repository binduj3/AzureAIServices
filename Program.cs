using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Azure.Search.Documents.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AzureAIMiniExamples
{
    class Program
    {
        // Replace with your keys & endpoints
        static string visionEndpoint = "https://<your-vision-endpoint>.cognitiveservices.azure.com/";
        static string visionKey = "<YOUR_VISION_KEY>";
        static string speechEndpoint = "https://<your-speech-endpoint>.cognitiveservices.azure.com/";
        static string speechKey = "<YOUR_SPEECH_KEY>";
        static string textAnalyticsEndpoint = "https://<your-textanalytics-endpoint>.cognitiveservices.azure.com/";
        static string textAnalyticsKey = "<YOUR_TEXTANALYTICS_KEY>";
        static string translatorEndpoint = "https://api.cognitive.microsofttranslator.com/";
        static string translatorKey = "<YOUR_TRANSLATOR_KEY>";
        static string searchEndpoint = "https://<your-search-service>.search.windows.net/";
        static string searchKey = "<YOUR_SEARCH_KEY>";
        static string docIntelligenceEndpoint = "https://<your-docintelligence-endpoint>.cognitiveservices.azure.com/";
        static string docIntelligenceKey = "<YOUR_DOCINT_KEY>";
        static string contentSafetyEndpoint = "https://<your-contentsafety-endpoint>.cognitiveservices.azure.com/";
        static string contentSafetyKey = "<YOUR_CONTENTSAFETY_KEY>";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Azure AI Mini Examples");

            await VisionExample();
            await SpeechExample();
            await LanguageExample();
            await ContentSafetyExample();
            await TranslatorExample();
            await SearchExample();
            await DocumentIntelligenceExample();

            Console.WriteLine("Done.");
        }

        static async Task VisionExample()
        {
            Console.WriteLine("\n--- Vision Example ---");
            // Example: analyze image to extract text (OCR) & description
            ImageAnalysisClient client = new ImageAnalysisClient(
             new Uri(visionEndpoint),
             new AzureKeyCredential(visionKey));

            var resultResponse = await client.AnalyzeAsync(
                new Uri("https://learn.microsoft.com/azure/ai-services/computer-vision/media/quickstarts/presentation.png"),
                VisualFeatures.Caption | VisualFeatures.Read,
                new ImageAnalysisOptions { GenderNeutralCaption = true });

            var result = resultResponse.Value;

            Console.WriteLine("Image analysis results:");
            Console.WriteLine(" Caption:");
            Console.WriteLine($"   '{result.Caption.Text}', Confidence {result.Caption.Confidence:F4}");

            Console.WriteLine(" Read:");
            foreach (DetectedTextBlock block in result.Read.Blocks)
                foreach (DetectedTextLine line in block.Lines)
                {
                    Console.WriteLine($"   Line: '{line.Text}', Bounding Polygon: [{string.Join(" ", line.BoundingPolygon)}]");
                    foreach (DetectedTextWord word in line.Words)
                    {
                        Console.WriteLine($"     Word: '{word.Text}', Confidence {word.Confidence.ToString("#.####")}, Bounding Polygon: [{string.Join(" ", word.BoundingPolygon)}]");
                    }
                }
        }

        /// <summary>
        /// Synthesizes sample text to an audio file using the Azure AI Speech.
        /// </summary>
        static async Task SpeechExample()
        {
            Console.WriteLine("\n---Speech Example ---text to speech---");
            var config = SpeechConfig.FromEndpoint(new Uri(speechEndpoint), speechKey);
            config.SpeechRecognitionLanguage = "en-US";
            config.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

            var audioConfig = AudioConfig.FromWavFileOutput("outputaudio.wav");

            using (var speechSynthesizer = new SpeechSynthesizer(config, audioConfig))
            {
                string text = "This is some sample text for testing it worked";

                var result = await speechSynthesizer.SpeakTextAsync(text);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine(@$"Speech synthesis completed. Audio saved to: outputaudio.wav, bin\Debug\net9.0 folder");
                }
                else
                {
                    Console.WriteLine($"Speech synthesis failed: {result.Reason}");
                    if (result.Reason == ResultReason.Canceled)
                    {
                        Console.WriteLine($"Speech synthesis failed");
                    }
                }
            }
        }


        /// <summary>
        /// Extracts key phrases from sample text using the Azure Text Analytics service.
        /// </summary>
        static async Task LanguageExample()
        {
            Console.WriteLine("\n--- Text Analytics Example ---");

            var client = new Azure.AI.TextAnalytics.TextAnalyticsClient(
                new Uri(languageEndpoint),
                new Azure.AzureKeyCredential(languageKey));

            var doc = "I had a wonderful experience! The rooms were clean and the staff was friendly.";
            var response = await client.ExtractKeyPhrasesAsync(doc);


            foreach (var phrase in response.Value)
            {
                Console.WriteLine($" Key Phrase: {phrase}");
            }
        }


        /// <summary>
        ///Translates text to hindi.
        /// </summary>
        static async Task TranslatorExample()
        {
            Console.WriteLine("\n--- Translator Example ---");
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var client = new Azure.AI.Translation.Text.TextTranslationClient(
                new Azure.AzureKeyCredential(translatorKey),
                new Uri(translatorEndpoint));

            var result = await client.TranslateAsync("hi", ["Hello please come"]);
            foreach (var doc in result.Value)
            {
                foreach (var trans in doc.Translations)
                {
                    Console.WriteLine($"Translated: {trans.Text} (to {trans.TargetLanguage})");
                }
            }
        }

        /// <summary>
        /// Performs a simple search query against an Azure search index and writes matching documents to the console.
        /// </summary>
        static async Task SearchExample()
        {
            Console.WriteLine("\n--- Search Example ---");
            // Example: simple search query over pre-existing index
            var client = new Azure.Search.Documents.SearchClient(
                new Uri(searchEndpoint),
                "hotels-sample-index",
                new Azure.AzureKeyCredential(searchKey));

            var response = await client.SearchAsync<SearchDocument>("luxury");
            await foreach (var doc in response.Value.GetResultsAsync())
            {
                Console.WriteLine($"Found document: {doc.Document["HotelId"]}  {doc.Document["HotelName"]}");
            }
        }

        /// <summary>
        /// Extracts fields from document using prebuilt model.
        /// </summary>
        static async Task DocumentIntelligenceExample()
        {
            Console.WriteLine("\n--- Document Intelligence Example ---");

            var client = new Azure.AI.DocumentIntelligence.DocumentIntelligenceClient(
                new Uri(docIntelligenceEndpoint),
                new Azure.AzureKeyCredential(docIntelligenceKey));

            var operation = await client.AnalyzeDocumentAsync(
                Azure.WaitUntil.Completed,
                "prebuilt-read",
                new Uri("https://storagedev23.blob.core.windows.net/aiservices/test.pdf?sp=r&st=2025-10-30T13:16:04Z&se=2025-10-30T21:31:04Z&spr=https&sv=2024-11-04&sr=b&sig=qlyepzOtAQ%2FQcAqW2KxJnfdqxoT%2FkZib9CbUKaG1RZk%3D"));

            var result = operation.Value;
            foreach (var page in result.Pages)
            {
                Console.WriteLine($"Page {page.PageNumber}:");
                foreach (var line in page.Lines)
                {
                    Console.WriteLine($"  {line.Content}");
                }
            }
        }

        /// <summary>
        /// Analyzes input text for potentially harmful or unsafe content using the Content Safety API.
        /// </summary>
        static async Task ContentSafetyExample()
        {
            Console.WriteLine("\n--- Content Safety Example ---");
            // Example: check text for harmful content
            var client = new Azure.AI.ContentSafety.ContentSafetyClient(
                new Uri(contentSafetyEndpoint),
                new Azure.AzureKeyCredential(contentSafetyKey));

            var response = await client.AnalyzeTextAsync("I will hurt you badly!");

            for(var i =0; i< response.Value.CategoriesAnalysis.Count;i++)
            {
                var output = response.Value.CategoriesAnalysis[i];
                Console.WriteLine($" Category: {output.Category}, severity: {output.Severity}");
            }
        }
    }
}
