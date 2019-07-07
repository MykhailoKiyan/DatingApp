using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AutoMapper;

using DatingApp.API.Data;
using DatingApp.API.Helpers;
using CloudinaryDotNet;
using DatingApp.API.Dtos;
using CloudinaryDotNet.Actions;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Controllers {
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private IDatingRepository repository;

        private IMapper mapper;

        private IOptions<CloudinarySettings> cloudinaryOptions;

        private Cloudinary cloudinary;

        public PhotosController(
                IDatingRepository repository,
                IMapper mapper,
                IOptions<CloudinarySettings> cloudinaryOptions) {
            this.repository = repository;
            this.mapper = mapper;
            this.cloudinaryOptions = cloudinaryOptions;

            Account account = new Account {
                Cloud = this.cloudinaryOptions.Value.CloudName,
                ApiKey = this.cloudinaryOptions.Value.ApiKey,
                ApiSecret = this.cloudinaryOptions.Value.ApiSecret
            };

            this.cloudinary = new Cloudinary(account);
        }

		[HttpGet("{id}", Name = "GetPhoto")]
		public async Task<IActionResult> GetPhoto(int id) {
			var photo = await this.repository.GetPhoto(id);

			var photoDto = this.mapper.Map<PhotoForReturnDto>(photo);

			return Ok(photoDto);
		}

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(
				int userId,
				[FromForm]PhotoForCreationDto photoDto) {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userId != currentUserId) {
                return Unauthorized();
            }

            var user = await this.repository.GetUser(userId);

            var file = photoDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0) {
                using (var stream = file.OpenReadStream()) {
                    var uploadParams = new ImageUploadParams {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = this.cloudinary.Upload(uploadParams);
                }
            }

            photoDto.Url = uploadResult.Uri.ToString();
            photoDto.PublicId = uploadResult.PublicId;

            var photo = this.mapper.Map<Photo>(photoDto);

            if (!user.Photos.Any(u => u.IsMain)) {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

			if (await this.repository.SaveAll()) {
				var photoToReturn = this.mapper.Map<PhotoForReturnDto>(photo);
				return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            } else {
                return BadRequest("Could not add a photo!");
            }
        }
    }
}