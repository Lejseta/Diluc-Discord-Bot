using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Threading;
using System;
using System.Linq;
using System.IO;
using System.Drawing;

namespace Bot.Commands
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        static AudioManager audio;
        static IVoiceChannel connectedChannel;
        static CancellationTokenSource cancelSource;
        static ImageProcessing imageProcessing;

        static Utility()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, arg) => {
                if (connectedChannel != null)
                    connectedChannel.DisconnectAsync().Wait();
            };
        }

        [Command("clear", RunMode = RunMode.Async)]
        public async Task Clear(int i)
        {   
            await Context.Message.DeleteAsync();
            var messages = Context.Channel.GetMessagesAsync(i, CacheMode.AllowDownload).FlattenAsync().Result;

            var en = messages.GetEnumerator();
            while (en.MoveNext())
            {
                try
                {
                    en.Current.DeleteAsync();
                }
                catch { }
            }

            await ReplyAsync("Clearing completed");
        }

        [Command("imageProcessing", RunMode = RunMode.Async)]
        public async Task ConvertToGray()
        {
            if (Context.Message.Attachments.Count != 0)
            {
                await Context.Channel.SendMessageAsync("Image attached.");
                string URL = Context.Message.Attachments.ElementAt(0).Url;
                Console.WriteLine("url " + URL);
                try
                {
                    var stream = ImageProcessing.DownloadStreamImage(URL).Result;
                    Bitmap copy = new Bitmap(stream);
                    var newImage = ImageProcessing.ConvertToGray(copy);

                    var memoryStream = new MemoryStream();
                    newImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                    await Context.Channel.SendFileAsync(new FileAttachment(stream: memoryStream, fileName: "zdj.png"));
                }
                catch (Exception e) { Console.WriteLine("error" + e); };
            }
        }
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            if (channel == null) {
                await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            try
            {
                cancelSource = new CancellationTokenSource();
                await ConnectAndListen(channel, cancelSource.Token);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task ConnectAndListen(IVoiceChannel channel, CancellationToken token)
        {
            connectedChannel = channel;
            var audioClient = await connectedChannel.ConnectAsync();
            audio = new AudioManager(audioClient);
            audio.StartAudioManager();

            if(!audio.initialized)
            {
                await ReplyAsync("Audio does not work.");
                await connectedChannel.DisconnectAsync();
                return;
            }
            do
            {
                await Task.Delay(1000);
            }
            while (!token.IsCancellationRequested);
        }

        [Command("disconnect")]
        public async Task DisconnectChannel(IVoiceChannel channel = null)
        {
            if (connectedChannel is null) return;

            cancelSource.Cancel();
            await connectedChannel.DisconnectAsync();
        }
    }
}
