using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CopilotApiProject.Middleware
{
    /// <summary>
    /// Middleware to validate JWT tokens and ensure only authorized users can access protected endpoints
    /// </summary>
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip token validation for certain endpoints
            if (ShouldSkipValidation(context.Request.Path))
            {
                await _next(context);
                return;
            }

            try
            {
                var token = ExtractTokenFromRequest(context.Request);
                
                if (string.IsNullOrEmpty(token))
                {
                    await HandleUnauthorizedAsync(context, "Missing authorization token");
                    return;
                }

                var principal = await ValidateTokenAsync(token);
                
                if (principal == null)
                {
                    await HandleUnauthorizedAsync(context, "Invalid authorization token");
                    return;
                }

                // Set the user principal for the current request
                context.User = principal;
                
                // Log successful authentication
                _logger.LogInformation("User authenticated successfully: {UserId} for {Method} {Path}",
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);
            }
            catch (SecurityTokenExpiredException)
            {
                await HandleUnauthorizedAsync(context, "Token has expired");
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning("Token validation failed: {Error}", ex.Message);
                await HandleUnauthorizedAsync(context, "Invalid token format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                await HandleUnauthorizedAsync(context, "Authentication error occurred");
            }
        }

        private bool ShouldSkipValidation(PathString path)
        {
            var publicEndpoints = new[]
            {
                "/health",
                "/api",
                "/swagger",
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh"
            };

            return publicEndpoints.Any(endpoint => path.StartsWithSegments(endpoint, StringComparison.OrdinalIgnoreCase));
        }

        private string? ExtractTokenFromRequest(HttpRequest request)
        {
            // Try to get token from Authorization header (Bearer token)
            var authorizationHeader = request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            // Try to get token from query parameter (for websocket connections, etc.)
            if (request.Query.ContainsKey("access_token"))
            {
                return request.Query["access_token"].FirstOrDefault();
            }

            // Try to get token from cookie
            if (request.Cookies.ContainsKey("auth_token"))
            {
                return request.Cookies["auth_token"];
            }

            return null;
        }

        private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("JWT SecretKey is not configured");
                    return null;
                }

                var key = Encoding.UTF8.GetBytes(secretKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5), // Allow 5 minutes clock skew
                    RequireExpirationTime = true
                };

                var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                // Additional custom validations
                if (!IsTokenFormatValid(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Token validation failed: {Error}", ex.Message);
                return null;
            }
        }

        private bool IsTokenFormatValid(SecurityToken token)
        {
            // Ensure it's a JWT token
            if (token is not JwtSecurityToken jwtToken)
            {
                return false;
            }

            // Check algorithm (ensure it's HMAC SHA256)
            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm: {Algorithm}", jwtToken.Header.Alg);
                return false;
            }

            // Ensure required claims exist
            var requiredClaims = new[] { ClaimTypes.NameIdentifier, ClaimTypes.Email };
            foreach (var requiredClaim in requiredClaims)
            {
                if (!jwtToken.Claims.Any(c => c.Type == requiredClaim))
                {
                    _logger.LogWarning("Missing required claim: {Claim}", requiredClaim);
                    return false;
                }
            }

            return true;
        }

        private async Task HandleUnauthorizedAsync(HttpContext context, string message)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}. Request: {Method} {Path} from {RemoteIP}",
                message,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized access",
                message = message,
                statusCode = 401,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value,
                method = context.Request.Method,
                traceId = context.TraceIdentifier
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Extension method to register the token validation middleware
    /// </summary>
    public static class TokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidationMiddleware>();
        }
    }

    /// <summary>
    /// Helper service for generating JWT tokens (for authentication endpoints)
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string GenerateToken(string userId, string email, IEnumerable<string>? roles = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "24");

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                
                if (string.IsNullOrEmpty(secretKey))
                {
                    return null;
                }
                
                var key = Encoding.UTF8.GetBytes(secretKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}