using System;
using System.Threading;
using System.Threading.Tasks;
using Dopamine.Services.Entities;
using Dopamine.Services.Playback;
using Global;
using Infra.App;
using Infra.Config;
using Infra.Topic.Support;
using Infra.Trace;
using Solarena.Topics.Midi;

namespace TheProgram
{
    [TopicPublisher( typeof( MidiDataTopic ))]
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

            m_playbackService.PlaybackVolumeChanged += PlaybackVolumeChangedCallBack;
            m_playbackService.PlaybackSuccess += PlaybackService_PlaybackSuccess;
        }

        private static void PlaybackService_PlaybackSuccess( object sender, PlaybackSuccessEventArgs e )
        {
            TrackViewModel track = m_playbackService.CurrentTrack;
            TrackControlTopic.g.CurrentTrack( track.TrackTitle, track.ArtistName, track.AlbumTitle );
        }

        private static void PlaybackVolumeChangedCallBack(object sender, PlaybackVolumeChangedEventArgs e)
        {
            float fVolume = m_playbackService.Volume;
//L            Tracer.Info( $"fVolume={fVolume}" );

            double dMidiVolume = fVolume * 127.0;
            MidiDataTopic.g.NamedValue( "Volume", dMidiVolume, "Dopamine" );
        }

        [TopicHandler( typeof(MidiDataTopic))]
        public static void RawValue( string SubTopic, double Raw, double RealValueForCompare )
        {
        }

        [TopicHandler( typeof(MidiDataTopic))]
        public static void NamedValue( string SubTopic, double Value, string Origin )
        {
            if (SubTopic != "Volume"  ) return;
            if (Origin   == "Dopamine") return;
            if (m_playbackService == null) {  Tracer.Error( "playback is null." ); return; }

            float fVolume = (float)(Value / 127.0);
            m_playbackService.Volume = (1000.0f + fVolume);  // Flag as midi set to avoid change event.
        }

        [TopicHandler(typeof(MidiDataTopic))]
        public static void OnPrime( uint Spoke )  // spokeFrom
        {
            PlaybackVolumeChangedCallBack( null, null );
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

        [TopicHandler( typeof(TrackControlTopic))]
        public static async void RestartTrack( int RestartDelay ) 
        {
            if (m_playbackService == null) {  Tracer.Error( "playback is null." ); return; }
            m_playbackService.RestartTrack( RestartDelay );
        }

        [TopicHandler( typeof(TrackControlTopic))]
        public static void CurrentTrack( string TrackTitle, string ArtistName, string AlbumTitle )
        {
        }
    }

    [TopicPublisher( typeof( TrackControlTopic ))]
    static class TopicPub { }
}
