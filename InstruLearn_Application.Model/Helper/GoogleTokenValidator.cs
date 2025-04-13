using Google.Apis.Auth;
using InstruLearn_Application.Model.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Helper
{
    public class GoogleTokenValidator
    {
        private readonly GoogleAuthSettings _googleAuthSettings;

        public GoogleTokenValidator(GoogleAuthSettings googleAuthSettings)
        {
            _googleAuthSettings = googleAuthSettings;
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleAuthSettings.ClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch
            {
                return null;
            }
        }
    }
}