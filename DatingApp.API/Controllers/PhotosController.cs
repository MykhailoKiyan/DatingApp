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
        private readonly IDatingRepository repository;

        private readonly IMapper mapper;

        private readonly IOptions<CloudinarySettings> cloudinaryOptions;

        private readonly Cloudinary cloudinary;

        public PhotosController(
                IDatingRepository repository,
                IMapper mapper,
                IOptions<CloudinarySettings> cloudinaryOptions) {
            this.repository = repository;
            this.mapper = mapper;
            this.cloudinaryOptions = cloudinaryOptions;

            var account = new Account {
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

			return this.Ok(photoDto);
		}

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(
				int userId,
				[FromForm]PhotoForCreationDto photoDto) {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userId != currentUserId) {
                return this.Unauthorized();
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

			if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK) {
				return this.BadRequest("Could not upload a photo to a storage!");
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
				return this.CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            } else {
                return this.BadRequest("Could not add a photo!");
            }
        }

		[HttpPost("{id}/setMain")]
		public async Task<IActionResult> SetMainPhoto(int userId, int id) {
			int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) {
				return this.Unauthorized();
			}

			var user = await this.repository.GetUser(userId);

			if (!user.Photos.Any(p => p.Id == id)) {
				return this.Unauthorized();
			}

			var photo = await this.repository.GetPhoto(id);

			if (photo.IsMain) {
				return this.BadRequest("The photo is already main photo!");
			}

			var currentMainPhoto = await this.repository.GetMainPhotoForUser(userId);
			if (currentMainPhoto != null) {
				currentMainPhoto.IsMain = false;
			}

			photo.IsMain = true;

			if (await this.repository.SaveAll()) {
				return this.NoContent();
			}

			return this.BadRequest("Could not set photo to main!");
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePhoto(int userId, int id) {
			int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) {
				return this.Unauthorized();
			}

			var user = await this.repository.GetUser(userId);

			if (!user.Photos.Any(p => p.Id == id)) {
				return this.Unauthorized();
			}

			var photo = await this.repository.GetPhoto(id);

			if (photo.IsMain) {
				return this.BadRequest("You can not delete your main photo!");
			}

			if (photo.PublicId != null) {
				var deletionParams = new DeletionParams(photo.PublicId);

				var result = this.cloudinary.Destroy(deletionParams);

				if (result.Result == "ok") {
					this.repository.Delete(photo);
				}

			} else {
				this.repository.Delete(photo);
			}

			if (await this.repository.SaveAll()) {
				return this.Ok();
			} else {
				return this.BadRequest("Failed to delete the photo!");
			}
		}
	}
}