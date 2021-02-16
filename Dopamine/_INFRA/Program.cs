using System;
using System.Threading.Tasks;
using Global;
using Infra.App;
using Infra.Config;
using Infra.Trace;

namespace TheProgram
{
    static partial class Program
    {
        internal static string   g_szAppName = "Dopamine";  // NOTE: simple static global for Design Mode.  Also locale folder.

        internal static void StartInfra()
        {
            Task taskStartInfra = new Task(() => PseudoMain(), TaskCreationOptions.LongRunning );
            taskStartInfra.Start();
        }

        static void PseudoMain()
        {
            Tracer.SetFilterTraceLevel( CommandLine.Value( "LogLevel" ) );

            using (InfraConnection.g = new InfraConnection( g_szAppName )) {
                Tracer.SwitchToTcp( InfraConnection.g );

                InfraConnection.g.Start( CommandLine.Value( "-infrahost" ), CommandLine.nValue( "port", GLOBAL2.TCP_HUB_PORT )); // 8368 

                InfraBase_NoGui appNoGui = new InfraBase_NoGui( g_szAppName );
                InfraConnection.g.SaveCacheToXml( AppDomain.CurrentDomain.BaseDirectory + "/TopicInfo.xml" );  // Exe folder
                appNoGui.Run();
            }
        }
    }
}
