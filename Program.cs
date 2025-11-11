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

     

        // curretnly played song artist name and track name
        string[]? artistAndTrackName = await SpotifyService.GiveBackArtistAndTrackName(spotify);

        //* ------------ Discgos ------------ *//

        DiscogsService discogsService = new DiscogsService();

        int? earliestYear = DiscogsService.GetEarliestReleaseYear(discogsService.Client , artistAndTrackName[0], artistAndTrackName[1]);

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

