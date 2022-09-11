using System;
using System.Speech.Recognition;
using System.IO;
using Discord.Audio;
using NAudio.Wave;
using System.Speech.AudioFormat;
using System.Threading;
using NAudio.CoreAudioApi;
using System.Threading.Tasks;

namespace Bot
{
    public class AudioManager
    {
        public static event Action<Stream, WaveFormat> StreamFilled;

        public bool initialized { get; private set; } = false;

        SpeechRecognitionEngine speechEngine;
        Choices choice;
        Grammar grammar;
        
        bool speechOn = true;
        bool subscribed = false;

        IAudioClient audioClient;

        public AudioManager(IAudioClient audioClient)
        {
            this.audioClient = audioClient;

            initialized = LoadAudioManager();
            if (initialized)
                Console.WriteLine("AudioManager loaded.");
            else
                Console.WriteLine("AudioManager not loaded.");
        }

        public bool LoadAudioManager()
        {
            if (initialized) return true;

            Console.WriteLine("Loeading speechEngine...");
            speechEngine = new SpeechRecognitionEngine();
            try
            {
              for(int x = 0; x < WaveIn.DeviceCount; x++)
                {
                    var inp = WaveIn.GetCapabilities(x);
                    Console.WriteLine($"{x}: {inp.NameGuid}, {inp.ProductName}");
                }
              
                for (int x = 0; x < WaveOut.DeviceCount; x++)
                {
                    var ou = WaveOut.GetCapabilities(x);
                    Console.WriteLine($"{x}: {ou.NameGuid}, {ou.ProductName}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Does not recignize device " + ex);
            }


            string path = @"./Dictionary.txt";
            var fullPath = Path.GetFullPath(path);

            if (File.Exists(fullPath))
            {
                string[] words = File.ReadAllLines(path);
                choice = new Choices(words);
                GrammarBuilder gb = new GrammarBuilder(choice) {
                    Culture = System.Globalization.CultureInfo.GetCultureInfo("en-GB")
                };

                grammar = new Grammar(gb);
                speechEngine.LoadGrammar(grammar);
                return true;
            }
            Console.WriteLine("Problem with loading gramma");
            return false;
        }

        CancellationTokenSource audioCancellation;
        public void StartAudioManager()
        {
            audioCancellation = new CancellationTokenSource();

            var backupStream = new MemoryStream();
            var stream = new BinaryWriter(backupStream);

            try
            {
                Task.Run(() => AudioRecording(stream, audioCancellation.Token));

                bool created = false;
                StreamFilled += (stream, audioSettings) =>
                {
                    if (created) return;

                    speechEngine.SetInputToAudioStream(backupStream, new SpeechAudioFormatInfo(audioSettings.SampleRate, AudioBitsPerSample.Sixteen, (audioSettings.Channels == 1 ? AudioChannel.Mono : AudioChannel.Stereo)));
                    SubscribeEvent();
                    speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                    created = true;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task AudioRecording(BinaryWriter output, CancellationToken token)
        {
            var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var silence = new SilenceProvider(device.AudioClient.MixFormat);

            using (var wasapiOut = new WasapiOut(device, AudioClientShareMode.Shared, false, 250))
            {
                wasapiOut.Init(silence);
                while (!token.IsCancellationRequested)
                {
                    wasapiOut.Play();
                       
                    await Task.Delay(1500);
                    try
                    {
                        long position = output.BaseStream.Position;
                        wasapiOut.OutputWaveFormat.Serialize(output);
                        long moveBy = output.BaseStream.Position;
                        // Reset position of stream
                        output.BaseStream.Position = position;
                        StreamFilled?.Invoke(output.BaseStream, device.AudioClient.MixFormat);
                        // Move again position of stream
                        output.BaseStream.Position = moveBy;
                        wasapiOut.Stop();
                    }
                    catch(Exception e) { Console.WriteLine(e); }
                }
            };
        }

        public void StopAudioManager()
        {
            if (audioCancellation != null) 
                audioCancellation.Cancel();
            speechEngine.RecognizeAsyncStop();
            UnsubscribeEvent();
        }

        void SubscribeEvent()
        {
            if (subscribed) return;
            subscribed = true;
            speechEngine.SpeechRecognized += SpeechRecognizedHandler;
            speechEngine.SpeechDetected += SpeechDetected;
        }

        void UnsubscribeEvent()
        {
            if (!subscribed) return;
            subscribed = false;
            speechEngine.SpeechRecognized -= SpeechRecognizedHandler;
            speechEngine.SpeechDetected -= SpeechDetected;
        }

        void SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine("Speech Detected");
        }

        void SpeechRecognizedHandler(object sender, SpeechRecognizedEventArgs e)
        {
           
            if (e.Result == null)
            {
                Console.WriteLine("Voice not recognized");
                speechOn = false;
            }
            if (e.Result.Text == "what")
            {
                speechOn = true;
                Console.WriteLine("Recognized" + e.Result.Text);
            }

        }
    }   
}
