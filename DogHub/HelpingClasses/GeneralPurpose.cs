using System;
using System.Collections.Generic;
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
            if (Parentid == null) { Parentid = -1; }

            if (string.IsNullOrWhiteSpace(dogbreed))
            {
                return false;
            }

            dogbreed = dogbreed.Trim();

            if (Parentid != -1)
            {
                _parentDogBreed = new DogBreedBL().GetDogBreedById(Parentid.Value, db);
                if (_parentDogBreed.DogName == dogbreed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public List<string> GetAllCountries()
        {
            return new List<string>
        {
            "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina",
            "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados",
            "Belarus", "Belgium", "Belize", "Benin", "Bhutan", "Bolivia", "Bosnia and Herzegovina",
            "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia",
            "Cameroon", "Canada", "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros",
            "Congo (Congo-Brazzaville)", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic",
            "Democratic Republic of the Congo", "Denmark", "Djibouti", "Dominica", "Dominican Republic",
            "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini",
            "Ethiopia", "Fiji", "Finland", "France", "Gabon", "Gambia", "Georgia", "Germany", "Ghana",
            "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti", "Honduras",
            "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Israel", "Italy",
            "Ivory Coast", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kuwait",
            "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein",
            "Lithuania", "Luxembourg", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta",
            "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco",
            "Mongolia", "Montenegro", "Morocco", "Mozambique", "Myanmar (Burma)", "Namibia", "Nauru",
            "Nepal", "Netherlands", "New Zealand", "Nicaragua", "Niger", "Nigeria", "North Korea",
            "North Macedonia", "Norway", "Oman", "Pakistan", "Palau", "Palestine State", "Panama",
            "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar",
            "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia",
            "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe",
            "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia",
            "Slovenia", "Solomon Islands", "Somalia", "South Africa", "South Korea", "South Sudan", "Spain",
            "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Taiwan", "Tajikistan",
            "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia",
            "Turkey", "Turkmenistan", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates",
            "United Kingdom", "United States of America", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City",
            "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe"
        };
        }

    }
}
