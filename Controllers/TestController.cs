using Microsoft.AspNetCore.Mvc;
using CopilotApiProject.Middleware;

namespace CopilotApiProject.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<TestController> _logger;

        public TestController(JwtTokenService jwtTokenService, ILogger<TestController> logger)
        {
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a JWT token for testing purposes (Development only)
        /// </summary>
        [HttpPost("generate-token")]
        public IActionResult GenerateToken([FromBody] TokenRequest request)
        {
            try
            {
                var token = _jwtTokenService.GenerateToken(
                    request.UserId,
                    request.Email,
                    request.Roles
                );

                var response = new
                {
                    token = token,
                    expiresAt = DateTime.UtcNow.AddHours(2), // Development expiration
                    user = new
                    {
                        id = request.UserId,
                        email = request.Email,
                        roles = request.Roles
                    }
                };

                _logger.LogInformation("Test token generated for user {UserId} with email {Email}", 
                    request.UserId, request.Email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate test token for user {UserId}", request.UserId);
                return BadRequest(new { error = "Token generation failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Validate a JWT token for testing purposes
        /// </summary>
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var principal = _jwtTokenService.ValidateToken(request.Token);

                if (principal == null)
                {
                    return BadRequest(new { error = "Invalid token", isValid = false });
                }

                var claims = principal.Claims.Select(c => new
                {
                    type = c.Type,
                    value = c.Value
                }).ToList();

                var response = new
                {
                    isValid = true,
                    userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                    roles = principal.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray(),
                    claims = claims
                };

                _logger.LogInformation("Token validated successfully for user {UserId}", response.userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return BadRequest(new { error = "Token validation failed", message = ex.Message, isValid = false });
            }
        }

        /// <summary>
        /// Test endpoint that requires authentication
        /// </summary>
        [HttpGet("protected")]
        public IActionResult ProtectedEndpoint()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

            return Ok(new
            {
                message = "Access granted to protected endpoint",
                user = new
                {
                    id = userId,
                    email = email,
                    roles = roles,
                    isAuthenticated = User.Identity?.IsAuthenticated ?? false
                },
                timestamp = DateTime.UtcNow,
                endpoint = "/api/test/protected"
            });
        }

        /// <summary>
        /// Get current user information from JWT claims
        /// </summary>
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            var userInfo = new
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray(),
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                authenticationType = User.Identity?.AuthenticationType,
                allClaims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToArray()
            };

            return Ok(userInfo);
        }
    }

    public class TokenRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[]? Roles { get; set; }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}