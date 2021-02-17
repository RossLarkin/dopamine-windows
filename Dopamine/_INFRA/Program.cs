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
        private static IPlaybackService m_playbackService;


        internal static void StartInfra()
        {
            Tracer.SetFilterTraceLevel( CommandLine.Value( "LogLevel" ) );
            Tracer.Info2( InfraColor.White, InfraColor.DarkBlue, "**** Starting Infra ******" );

            InfraConnection.g = new InfraConnection( g_szAppName );
            InfraConnection.g.Start( CommandLine.Value( "-infrahost" ), CommandLine.nValue( "port", GLOBAL2.TCP_HUB_PORT )); // 8368 
        }

        internal static void SetPlaybackService( IPlaybackService playbackService )
        {
            Tracer.Info2( InfraColor.White, InfraColor.DarkBlue, "Have playback service instance." );
            m_playbackService = playbackService;
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
