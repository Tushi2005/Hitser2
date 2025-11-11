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
        _server = new EmbedIOAuthServer(new Uri("http://127.0.0.1:5543/callback"), 5543);
        await _server.Start();

        _server.AuthorizationCodeReceived += SpotifyService.OnAuthorizationCodeReceived;
        _server.ErrorReceived += SpotifyService.OnErrorReceived;

        // login kérés
        var request = new LoginRequest(_server.BaseUri, "8ef7940ed411467eb151ddacecd9284b", LoginRequest.ResponseType.Code)
        {
            Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserReadCurrentlyPlaying }
        };
        BrowserUtil.Open(request.ToUri());

        Console.WriteLine("Jelentkezz be a Spotify-fiókoddal, majd térj vissza ide.");
        Console.ReadLine(); // várunk, amíg a user befejezi az auth-ot

        // ekkor már a kliens létrejött
        var spotify = SpotifyService.GetClient();

        if (spotify != null)
        {
            var artistAndTrack = await SpotifyService.GiveBackArtistAndTrackName(spotify);
            if (artistAndTrack != null)
            {
                Console.WriteLine($"🎵 {artistAndTrack[1]} — {artistAndTrack[0]}");
            }
        }
        else
        {
            Console.WriteLine("❌ Sikertelen autentikáció.");
        }

        // curretnly played song artist name and track name
        string[]? artistAndTrackName = await SpotifyService.GiveBackArtistAndTrackName(spotify);

        //* ------------ Discgos ------------ *//
        var client = new DiscogsClient(
            new HttpClient(),
            new ApiQueryBuilder(new HardCodedClientConfig())
        );

        int? earliestYear = DiscogsService.GetEarliestReleaseYear(client, artistAndTrackName[0], artistAndTrackName[1]);

        if (earliestYear.HasValue)
            Console.WriteLine($"Legkorábbi megjelenés: {earliestYear.Value}");
        else
            Console.WriteLine("Nincs találat vagy nincs elérhető évszám.");
    }
}

// Configuration for discogs client
public class HardCodedClientConfig : IClientConfig
{
    public string AuthToken => "PpAovVHPOzKOyXBSgTYNmfCYLtfSRBlbdTuGULye";
    public string BaseUrl => "https://api.discogs.com";
}

