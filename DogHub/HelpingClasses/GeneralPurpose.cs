using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using DogHub.BL;
using DogHub.Models;

namespace DogHub.HelpingClasses
{
    public class GeneralPurpose
    {
        DogHubEntities db = new DogHubEntities();

        public User ValidateLoggedinUser()
        {
            try
            {
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var userId = identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return null;

                int id = Convert.ToInt32(userId);
                return new UserBL().GetUserById(id, db);
            }
            catch
            {
                return null;
            }
        }

        public bool ValidateEmail(string email, int id = -1)
        {
            int count = 0;
            if (id != -1)
            {
                count = new UserBL().GetActiveUsers(db)
                    .Count(x => x.Email.ToLower() == email.ToLower() && x.PK_UserId != id && x.IsActive == true);
            }
            else
            {
                count = new UserBL().GetActiveUsers(db)
                    .Count(x => x.Email.ToLower() == email.ToLower() && x.IsActive == true);
            }

            return count == 0;
        }
        public bool ISDogBreedExist(string dogbreed, int id = -1)
        {
            int count = 0;

            if (string.IsNullOrWhiteSpace(dogbreed))
            {
                return false;
            }

            dogbreed = dogbreed.Trim();

            if (id != -1)
            {
                count = new DogBreedBL().GetAllDogBreeds(db)
                    .Count(x => x.DogName.ToLower() == dogbreed.ToLower() && x.PK_DogBreedId != id && x.IsDeleted == false);
            }
            else
            {
                count = new DogBreedBL().GetAllDogBreeds(db)
                    .Count(x => x.DogName.ToLower() == dogbreed.ToLower() && x.IsDeleted == false);
            }

            return count == 0;
        }

        public bool ISDogNameANDParentSame(string dogbreed, int? Parentid = -1)
        {
            DogBreed _parentDogBreed = null;

            if (string.IsNullOrWhiteSpace(dogbreed))
            {
                return false;
            }

            dogbreed = dogbreed.Trim();

            if (Parentid != -1)
            {
                _parentDogBreed = new DogBreedBL().GetDogBreedById(Parentid.Value, db);
                if (_parentDogBreed.DogName == dogbreed) {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

    }
}
