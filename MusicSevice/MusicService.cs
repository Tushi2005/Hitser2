using Microsoft.EntityFrameworkCore;

namespace MusicQuiz
{
    internal class MusicService
    {
        private readonly MusicDbContext _db;
        private readonly DiscogsService discogsService;

        public MusicService(MusicDbContext dbContext, DiscogsService discogsService)
        {
            _db = dbContext;
            this.discogsService = discogsService;
        }

        public void AddSong(string title, string artistName, int releaseYear)
        {
            // Ellenőrizzük, hogy létezik-e az előadó
            var artist = _db.Artists.FirstOrDefault(a => a.Name == artistName);
            if (artist == null)
            {
                artist = new Artist { Name = artistName };
                _db.Artists.Add(artist);
                _db.SaveChanges(); // először mentjük az előadót, hogy legyen Id-ja
            }

            // Új dal létrehozása
            var song = new Song
            {
                Title = title,
                ReleaseYear = releaseYear,
                ArtistId = artist.Id
            };

            _db.Songs.Add(song);
            _db.SaveChanges();
        }


        public async Task<int?> GetOrFetchSongAsync(string title, string artistName)
        {
            // Ellenőrizzük az adatbázist
            var song = _db.Songs
                .Include(s => s.Artist)
                .FirstOrDefault(s => s.Title == title && s.Artist.Name == artistName);

            if (song != null)
                return song.ReleaseYear;

            // Ha nincs, hívjuk az API-t (itt a te DiscogsService-edet)
            int? year = await discogsService.GetEarliestReleaseYear(artistName, title);

            if (!year.HasValue || year < 1900)
                return null;

            // Mentés az adatbázisba
            var artist = _db.Artists.FirstOrDefault(a => a.Name == artistName);
            if (artist == null)
            {
                artist = new Artist { Name = artistName };
                _db.Artists.Add(artist);
                _db.SaveChanges();
            }

            song = new Song
            {
                Title = title,
                ReleaseYear = year.Value,
                ArtistId = artist.Id
            };
            _db.Songs.Add(song);
            _db.SaveChanges();

            return year;
        }
    }
}
