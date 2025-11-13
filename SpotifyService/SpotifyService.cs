using Microsoft.AspNetCore.Hosting.Server;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicQuiz
{
    public class SpotifyService
    {
        private static SpotifyClient? _spotify;
        private static List<IPlayableItem> totalTrack = new();

        // Authentication
        public static async Task Authenticathion(EmbedIOAuthServer server)
        {
            server = new EmbedIOAuthServer(new Uri("http://127.0.0.1:5543/callback"), 5543);
            await server.Start();

            server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            server.ErrorReceived += OnErrorReceived;

            var request = new LoginRequest(server.BaseUri, "8ef7940ed411467eb151ddacecd9284b", LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserReadCurrentlyPlaying, Scopes.UserModifyPlaybackState, Scopes.PlaylistModifyPublic, Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative, Scopes.PlaylistModifyPrivate}
            };
            BrowserUtil.Open(request.ToUri());

            Console.WriteLine("Jelentkezz be a Spotify-fiókoddal, majd térj vissza ide.");
            Console.ReadLine();
        }

        // Get the spotify client
        public static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            if (sender is EmbedIOAuthServer server)
                await server.Stop();

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
                new AuthorizationCodeTokenRequest(
                    "8ef7940ed411467eb151ddacecd9284b",
                    "d80dfd965a4c4357b1423067b12af548",
                    response.Code,
                    new Uri("http://127.0.0.1:5543/callback")
                )
            );

            _spotify = new SpotifyClient(tokenResponse.AccessToken);
            Console.WriteLine("Bejelentkezés sikeres!");
            await FillTotalTrack();
        }

        public static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Hiba az engedélyezés során: {error}");
            if (sender is EmbedIOAuthServer server)
                await server.Stop();
        }

        public static SpotifyClient? GetClient() => _spotify;

        // Get the currently played track's name and the artist's name
        public static async Task<string[]?> GiveBackArtistAndTrackName()
        {
            try
            {
                var currentlyPlaying = await _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

                if (currentlyPlaying?.Item is FullTrack track)
                {
                    return new string[]
                    {
                    track.Artists.FirstOrDefault().Name,
                    Regex.Replace(track.Name, @"\s*\([^)]*\)", ""),
                    };
                }
                else
                {
                    Console.WriteLine("Jelenleg nem játszik semmit a Spotify.");
                    return null;
                }
            }
            catch (APIUnauthorizedException)
            {
                Console.WriteLine("A token lejárt vagy nincs megfelelő engedély.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
                return null;
            }
        }

        public static async Task Play()
        {
            try
            {
                // Lekérjük, hogy mi játszik éppen
                var currentlyPlaying = await _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

                if (currentlyPlaying?.IsPlaying == true)
                {
                    // Ha éppen megy a zene, megállítjuk
                    await _spotify.Player.PausePlayback();
                    Console.WriteLine("Zene megállítva.");
                }
                else
                {
                    // Ha nem megy a zene, elindítjuk
                    await _spotify.Player.ResumePlayback();
                    Console.WriteLine("Zene elindítva.");
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
        }

        private static async Task FillTotalTrack()
        {
            var playlistId = "1HTknRUz8uGFFgQsnHlXuZ";
            var playlist = await _spotify.Playlists.GetItems(playlistId);
            var trackCount = playlist.Total;

            int offset = 0;
            int limit = 100;

            while (offset < trackCount)
            {
                // Lekérjük az aktuális "oldalt"
                var page = await _spotify.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest
                {
                    Limit = limit,
                    Offset = offset
                });

                // Hozzáadjuk a track-eket a listához
                foreach (var item in page.Items)
                {
                    totalTrack.Add(item.Track); // item.Track implementálja IPlayableItem-et
                }

                // Offset növelése a következő oldalra
                offset += limit;
            }
        }

        public static async Task NextSong()
        {
            try
            {
                // Random index kiválasztása
                var random = new Random();
                int x = random.Next(totalTrack.Count); // 0 .. trackCount-1

                // A track kinyerése a listából
                var randomTrack = totalTrack[x] as FullTrack;       // PlaylistTrack object

                // Track hozzáadása a queue-hoz
                await _spotify.Player.AddToQueue(new PlayerAddToQueueRequest(randomTrack.Uri));

                // SkipNext, hogy rögtön ez a track jöjjön
                await _spotify.Player.SkipNext();
            }
            catch (APIUnauthorizedException)
            {
                Console.WriteLine("A token lejárt vagy nincs megfelelő engedély.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
            }
        }
    }
}
