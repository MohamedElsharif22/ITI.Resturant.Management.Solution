using Byway.Application.Contracts.ExternalServices;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["Authentication:Google:ClientId"];

                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogError("Google ClientId is not configured");
                    return null;
                }

                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

                _logger.LogInformation("Successfully validated Google token for email: {Email}", payload.Email);

                return new GoogleUserInfo
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    Picture = payload.Picture
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid JWT token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Google token");
                return null;
            }
        }
    }
}
