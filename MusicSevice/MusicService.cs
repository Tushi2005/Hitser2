using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicQuiz
{
    internal class MusicService
    {
        private readonly MusicDbContext _db;

        public MusicService(MusicDbContext dbContext)
        {
            _db = dbContext;
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
    }
}
