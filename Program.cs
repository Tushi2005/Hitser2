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

        using var db = new MusicDbContext();
        DiscogsService discogsService = new DiscogsService();
        var musicService = new MusicService(db, discogsService);

        string? cmd;
        do
        {
            Console.WriteLine("\nVálassz egy opciót:");
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
                        // Csak itt kérdezzük le az aktuális számot
                        string[]? artistAndTrack = await SpotifyService.GiveBackArtistAndTrackName();
                        if (artistAndTrack != null)
                        {
                            string artist = artistAndTrack[0];
                            string title = artistAndTrack[1];

                            // Lekérjük az adatbázisból vagy Discogs API-ból
                            int? year = await musicService.GetOrFetchSongAsync(title, artist);
                            
                            if (year.HasValue)
                                Console.WriteLine($"{artist} - {title} ({year.Value})");
                            else
                                Console.WriteLine($"{artist} - {title} (Megjelenési év nem található)");
                        }
                        else
                        {
                            Console.WriteLine("Nincs elérhető Spotify zene.");
                        }
                        break;
                    }
                case "2":
                    await SpotifyService.Play();
                    break;
                case "3":
                    await SpotifyService.NextSong();
                    break;
                case "exit":
                    break;
                default:
                    Console.WriteLine("Érvénytelen parancs.");
                    break;
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



