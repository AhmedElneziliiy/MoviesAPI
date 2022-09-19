using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //handle image size and type

        private new List<string> _allowExtensions = new List<string>
        {
            ".jpg",".png"
        };

        private long _maxAllowedPosterSize = 6291456 ; //6 MB

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies
                .OrderByDescending(m=>m.Rate)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsDto { 
                    Id = m.Id,
                    GenreId = m.GenreId,
                    GenreName=m.Genre.Name,
                    Poster=m.Poster,
                    Rate=m.Rate,
                    Storeline=m.Storeline,
                    Title=m.Title,
                    Year=m.Year
                })
                .ToListAsync();
            return Ok(movies);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie=await _context.Movies.Include(m=>m.Genre).SingleOrDefaultAsync(m=>m.Id==id);

            if (movie==null)
            {
                return NotFound();
            }



            var dto = new MovieDetailsDto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre.Name,
                Poster = movie.Poster,
                Rate = movie.Rate,
                Storeline = movie.Storeline,
                Title = movie.Title,
                Year = movie.Year
            };
            return Ok(dto);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _context.Movies
               .Where(m=>m.GenreId==genreId)
               .OrderByDescending(m => m.Rate)
               .Include(m => m.Genre)
               .Select(m => new MovieDetailsDto
               {
                   Id = m.Id,
                   GenreId = m.GenreId,
                   GenreName = m.Genre.Name,
                   Poster = m.Poster,
                   Rate = m.Rate,
                   Storeline = m.Storeline,
                   Title = m.Title,
                   Year = m.Year
               })
               .ToListAsync();
            return Ok(movies);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (!_allowExtensions.Contains(Path.GetExtension(dto.Poster.FileName.ToLower())))
                return BadRequest(error: "only .jpg and .png extension allowed");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for Poster 6 Megabyte");
            
            //checking if genre id is exist or not
            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

            if (!isValidGenre)
                return BadRequest("invalid genre ID !");

            //handle movies poster to store in byte array
            using var dataStream = new MemoryStream();
            await dto.Poster.CopyToAsync(dataStream);


            var movie =new Movie { 
            GenreId= dto.GenreId,
            Title=dto.Title,
            Poster=dataStream.ToArray(),
            Rate=dto.Rate,
            Storeline = dto.Storeline,
            Year=dto.Year,
            };
            
            await _context.AddAsync(movie);
            _context.SaveChanges();
            
            return Ok(movie);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie is null)
            {
                return NotFound($"no movie with than ID:{id}"); 
            }
            _context.Remove(movie);
            _context.SaveChanges();
            return Ok(movie);
        }
    }
}
