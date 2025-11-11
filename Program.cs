using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using ParkSquare.Discogs;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;

class Program
{
    private static EmbedIOAuthServer _server;

    private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        await _server.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var tokenResponse = await new OAuthClient(config).RequestToken(
          new AuthorizationCodeTokenRequest(
            "8ef7940ed411467eb151ddacecd9284b", "d80dfd965a4c4357b1423067b12af548", response.Code, new Uri("http://127.0.0.1:5543/callback")
          )
        );

        var spotify = new SpotifyClient(tokenResponse.AccessToken);
        // do calls with Spotify and save token?

        try
        {
            // 🎵 Lekérdezzük az aktuálisan játszott zenét
            var currentlyPlaying = await spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

            if (currentlyPlaying?.Item is FullTrack track)
            {
                Console.WriteLine($"Most játszott szám: {track.Name}");
                Console.WriteLine($"Előadó: {string.Join(", ", track.Artists.Select(a => a.Name))}");
            }
            else
            {
                Console.WriteLine("Jelenleg nem játszik semmit a Spotify fiókban.");
            }
        }
        catch (APIUnauthorizedException)
        {
            Console.WriteLine("A token lejárt vagy nincs megfelelő engedély.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba történt: {ex.Message}");
        }

        Console.WriteLine("Program vége, nyomj meg egy gombot...");
        Console.ReadKey();
    }

    private static async Task OnErrorReceived(object sender, string error, string state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        await _server.Stop();
    }

    static async Task Main()
    {
        //var client = new DiscogsClient(
        //    new HttpClient(),
        //    new ApiQueryBuilder(new HardCodedClientConfig())
        //);

        //string artist = "Illés";
        //string track = "Még fáj minden csók";

        //int? earliestYear = DiscogsService.GetEarliestReleaseYear(client, artist, track);

        //if (earliestYear.HasValue)
        //    Console.WriteLine($"Legkorábbi megjelenés: {earliestYear.Value}");
        //else
        //    Console.WriteLine("Nincs találat vagy nincs elérhető évszám.");


        // Make sure "http://localhost:5543/callback" is in your spotify application as redirect uri!
        _server = new EmbedIOAuthServer(new Uri("http://127.0.0.1:5543/callback"), 5543);
        await _server.Start();

        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        var request = new LoginRequest(_server.BaseUri, "8ef7940ed411467eb151ddacecd9284b", LoginRequest.ResponseType.Code)
        {
            Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserReadCurrentlyPlaying }
        };
        BrowserUtil.Open(request.ToUri());

        // -----------------------------------------------------------------
        // ITT KELL VÁRAKOZNIA!
        // -----------------------------------------------------------------
        // Ez megakadályozza, hogy a Main metódus (és vele a program) azonnal leálljon.
        // A program itt fog várakozni egy Enter lenyomására.
        Console.WriteLine("A program fut és vár a böngészős bejelentkezésre...");
        Console.WriteLine("Miután a böngészőben engedélyezted az alkalmazást, a program folytatódni fog.");
        Console.WriteLine("A program leállításához itt nyomj egy Entert (bár a callback után magától is leáll).");
        Console.ReadLine();
    }
}

// Konfiguráció a Discogs klienshez
//public class HardCodedClientConfig : IClientConfig
//{
//    public string AuthToken => "PpAovVHPOzKOyXBSgTYNmfCYLtfSRBlbdTuGULye";
//    public string BaseUrl => "https://api.discogs.com";
//}

