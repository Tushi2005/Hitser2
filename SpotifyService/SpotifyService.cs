using Microsoft.AspNetCore.Hosting.Server;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicQuiz
{
    public class SpotifyService
    {
        private static SpotifyClient? _spotify;

        // Authentication
        public static async Task Authenticathion(EmbedIOAuthServer server)
        {
            server = new EmbedIOAuthServer(new Uri("http://127.0.0.1:5543/callback"), 5543);
            await server.Start();

            server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            server.ErrorReceived += OnErrorReceived;

            var request = new LoginRequest(server.BaseUri, "8ef7940ed411467eb151ddacecd9284b", LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserReadCurrentlyPlaying, Scopes.UserModifyPlaybackState }
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
            Console.WriteLine("✅ Bejelentkezés sikeres!");
        }

        public static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"⚠️ Hiba az engedélyezés során: {error}");
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
                        string.Join(", ", track.Artists.Select(a => a.Name)),
                        track.Name
                    };
                }
                else
                {
                    Console.WriteLine("🎵 Jelenleg nem játszik semmit a Spotify.");
                    return null;
                }
            }
            catch (APIUnauthorizedException)
            {
                Console.WriteLine("⚠️ A token lejárt vagy nincs megfelelő engedély.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hiba történt: {ex.Message}");
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
                    Console.WriteLine("⏸️ Zene megállítva.");
                }
                else
                {
                    // Ha nem megy a zene, elindítjuk
                    await _spotify.Player.ResumePlayback();
                    Console.WriteLine("▶️ Zene elindítva.");
                }
            }
            catch (APIUnauthorizedException)
            {
                Console.WriteLine("⚠️ A token lejárt vagy nincs megfelelő engedély.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hiba történt: {ex.Message}");
            }
        }

        public static async Task NextSong()
        {
            try
            {
                await _spotify.Player.SkipNext();
            }
            catch (APIUnauthorizedException)
            {
                Console.WriteLine("⚠️ A token lejárt vagy nincs megfelelő engedély.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hiba történt: {ex.Message}");
            }
        }
    }
}
