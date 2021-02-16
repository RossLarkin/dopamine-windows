using System;
using System.Threading;
using System.Threading.Tasks;
using Dopamine.Services.Playback;
using Global;
using Infra.App;
using Infra.Config;
using Infra.Topic.Support;
using Infra.Trace;
using Solarena.Topics.Midi;

namespace TheProgram
{
    static partial class Program
    {
        internal static string   g_szAppName = "Dopamine";  // NOTE: simple static global for Design Mode.  Also locale folder.
        internal static IPlaybackService m_playbackService;


        internal static void StartInfra()
        {
            Tracer.SetFilterTraceLevel( CommandLine.Value( "LogLevel" ) );

            InfraConnection.g = new InfraConnection( g_szAppName );
            Tracer.SwitchToTcp( InfraConnection.g );

            InfraConnection.g.Start( CommandLine.Value( "-infrahost" ), CommandLine.nValue( "port", GLOBAL2.TCP_HUB_PORT )); // 8368 

            InfraBase_NoGui appNoGui = new InfraBase_NoGui( g_szAppName );
            InfraConnection.g.SaveCacheToXml( AppDomain.CurrentDomain.BaseDirectory + "/TopicInfo.xml" );  // Exe folder

            Task taskStartInfra = new Task(() => appNoGui.Run(), TaskCreationOptions.LongRunning );
            taskStartInfra.Start();
        }



        [TopicHandler( typeof(TrackControlTopic))]
        public static async void NextTrack() 
        {
            if (m_playbackService == null) {  Tracer.Error( "playback is null." ); return; }
            await m_playbackService.PlayNextAsync();
        }

        [TopicHandler( typeof(TrackControlTopic))]
        public static async void PreviousTrack() 
        {
            if (m_playbackService == null) {  Tracer.Error( "playback is null." ); return; }
            await m_playbackService.PlayPreviousAsync();
        }

        [TopicHandler( typeof(TrackControlTopic))]
        public static void Stop() 
        {
        }

        [TopicHandler( typeof(TrackControlTopic))]
        public static void Pause() 
        {
            if (m_playbackService == null) {  Tracer.Error( "playback is null." ); return; }
            m_playbackService.PlayOrPauseAsync();
        }

        [TopicHandler( typeof(TrackControlTopic))]
        public static async void Play() 
        {
            if (m_playbackService == null) {  Tracer.Error( "playback is null." ); return; }
            m_playbackService.PlayOrPauseAsync();
        }

    }
}
