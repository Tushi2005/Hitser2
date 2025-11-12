using MusicQuiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicQuiz
{
    internal class SongTracker
    {
        private readonly MusicService _musicService;

        public string? CurrentTitle { get; private set; }
        public string? CurrentArtist { get; private set; }
        public int? CurrentYear { get; private set; }

        public SongTracker(MusicService musicService)
        {
            _musicService = musicService;
        }

        /// <summary>
        /// Ellenőrzi az aktuális Spotify számot, ha változott, frissíti az adatbázist.
        /// </summary>
        public async Task CheckCurrentSongAsync()
        {
            string[]? artistAndTrack = await SpotifyService.GiveBackArtistAndTrackName();
            if (artistAndTrack == null) return;

            string artist = artistAndTrack[0];
            string title = artistAndTrack[1];

            // Ha ugyanaz a dal, nem csinálunk semmit
            if (title == CurrentTitle && artist == CurrentArtist) return;

            // Új dal
            CurrentArtist = artist;
            CurrentTitle = title;
            CurrentYear = await _musicService.GetOrFetchSongAsync(title, artist);
        }
    }
}
