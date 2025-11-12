using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using ParkSquare.Discogs;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;
using MusicQuiz;

class Program
{
    private static EmbedIOAuthServer _server;

    static async Task Main()
    {
        //* ------------ Spotify Auth ------------ *//
        await SpotifyService.Authenticathion(_server);
        var spotify = SpotifyService.GetClient();
        DiscogsService discogsService = new DiscogsService();

        string? cmd;

        do
        {
            Console.WriteLine("Válassz egy opciót:");
            Console.WriteLine("1 - Aktuális zene évszáma");
            Console.WriteLine("2 - Zene megállítása / elindítása");
            Console.WriteLine("3 - Következő zene");
            Console.WriteLine("exit - Kilépés");
            Console.Write("> ");

            cmd = Console.ReadLine();

            switch (cmd)
            {
                case "1":
                    {
                        // Lekérjük az aktuális dalt
                        string[]? artistAndTrackName = await SpotifyService.GiveBackArtistAndTrackName();
                        if (artistAndTrackName != null)
                        {
                            // Lekérjük a legrégebbi megjelenés évét a Discogs API-val
                            int? earliestYear = await DiscogsService.GetEarliestReleaseYear(discogsService._client, artistAndTrackName[0], artistAndTrackName[1]);
                            if (earliestYear.HasValue)
                                Console.WriteLine($"🎵 {artistAndTrackName[0]} - {artistAndTrackName[1]} ({earliestYear.Value})");
                            else
                                Console.WriteLine("❌ Nem található megjelenési év.");
                        }
                        break;
                    }
                case "2":
                    {
                        // Toggle play / pause
                        await SpotifyService.Play();
                        break;
                    }
                case "3":
                    {
                        // Következő dal
                        await SpotifyService.NextSong();
                        break;
                    }
                case "exit":
                    {
                        Console.WriteLine("Kilépés...");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("❌ Érvénytelen parancs.");
                        break;
                    }
            }

        } while (cmd != "exit");

    }
}

// Configuration for discogs client
public class HardCodedClientConfig : IClientConfig
{
    public string AuthToken => "PpAovVHPOzKOyXBSgTYNmfCYLtfSRBlbdTuGULye";
    public string BaseUrl => "https://api.discogs.com";
}

