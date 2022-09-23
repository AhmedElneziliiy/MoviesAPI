using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;
        private readonly IMapper _mapper;



        public MoviesController(IMoviesService moviesService, IGenresService genresService, IMapper mapper)
        {
            _moviesService = moviesService;
            _genresService = genresService;
            _mapper = mapper;
        }

        //handle image size and type

        private new List<string> _allowExtensions = new List<string>
        {
            ".jpg",".png"
        };

        private long _maxAllowedPosterSize = 6291456 ; //6 MB

       

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _moviesService.GetAll();
            //AutoMapper
            var data=_mapper.Map<IEnumerable<MovieDetailsDto>>(movies);

            return Ok(data);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie=await _moviesService.GetById(id);

            if (movie==null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<MovieDetailsDto>(movie);

            return Ok(dto);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _moviesService.GetAll(genreId);
            //todo map ovies to dto

            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);

            return Ok(data);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (dto.Poster is null)
                return BadRequest("poster is required");

            
            if (!_allowExtensions.Contains(Path.GetExtension(dto.Poster.FileName.ToLower())))
                return BadRequest(error: "only .jpg and .png extension allowed");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for Poster 6 Megabyte");

            //checking if genre id is exist or not
            var isValidGenre = await _genresService.IsValidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("invalid genre ID !");

            //handle movies poster to store in byte array
            using var dataStream = new MemoryStream();
            await dto.Poster.CopyToAsync(dataStream);


            var movie = _mapper.Map<Movie>(dto);
            movie.Poster=dataStream.ToArray();
           _moviesService.Add(movie);
            
            return Ok(movie);
        }

        [HttpPut(template:"{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie= await _moviesService.GetById(id);
            if (movie == null)
                return NotFound($"no movie found with that : {id}");

            var isValidGenre = await _genresService.IsValidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("invalid genre ID !");

            if (dto.Poster != null)
            {
                if (!_allowExtensions.Contains(Path.GetExtension(dto.Poster.FileName.ToLower())))
                    return BadRequest(error: "only .jpg and .png extension allowed");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for Poster 6 Megabyte");
                using var dataStream = new MemoryStream();
                await dto.Poster.CopyToAsync(dataStream);
                movie.Poster=dataStream.ToArray();
            }

            movie.Title=dto.Title;
            movie.GenreId=dto.GenreId;
            movie.Year=dto.Year;
            movie.Storeline=dto.Storeline;
            movie.Rate=dto.Rate;
            
            _moviesService.Update(movie);   
            return Ok(movie);
        }





        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesService.GetById(id);
            if (movie is null)
            {
                return NotFound($"no movie with than ID:{id}"); 
            }
            _moviesService.Delete(movie);
            return Ok(movie);
        }
    }
}
