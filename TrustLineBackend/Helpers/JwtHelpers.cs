using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AnonymousComplaintsAPI.Models;

namespace AnonymousComplaintsAPI.Helpers
{
    public static class JwtHelpers
    {
        public static IEnumerable<Claim> GetClaims(this UserTokens userAccounts, Guid Id)
        {
            Id = Guid.NewGuid();
            IEnumerable<Claim> claims = new Claim[]
            {
                new Claim("Id", userAccounts.Id.ToString()),
                new Claim(ClaimTypes.Name, userAccounts.UserName),
                new Claim(ClaimTypes.Email, userAccounts.EmailId),
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                //new Claim(ClaimTypes.Role, "View App"),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddDays(1).ToLongDateString())
            };


            //var permissionsList = userAccounts.permissions.SelectMany(app => app.Roles.Select(permission => permission.Permissions.Select(p => p.Name).ToList());


            //var permissionsList = userAccounts.permissions
            //.SelectMany(app => app.Roles
            //    .SelectMany(role => role.Permissions
            //        .Select(permission => permission.Name)))
            //.ToList();




            ////List<string> roles = userAccounts.permissions;

            //foreach (var p in permissionsList)
            //{
            //    claims = claims.Concat(new[] { new Claim(ClaimTypes.Role, p) });
            //    Console.WriteLine(p);
            //}

            return claims;
        }

        public static IEnumerable<Claim> GetClaims(this UserTokens userAccounts, out Guid Id)
        {
            Id = Guid.NewGuid();
            return GetClaims(userAccounts, Id);
        }

        public static UserTokens GenTokenkey(UserTokens model, JwtSettings jwtSettings)
        {
            try
            {
                var UserToken = new UserTokens();
                if (model == null) throw new ArgumentException(nameof(model));

                // Get secret key
                var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey);
                Guid Id = Guid.Empty;
                DateTime expireTime = DateTime.UtcNow.AddDays(7);
                UserToken.Validaty = expireTime.TimeOfDay;

                // Retrieve user role from the database
                // Example code assuming a method `GetUserRoleFromDatabase` is available
                //string role = "Admin Role";//GetUserRoleFromDatabase(model.UserName);

                // Generate access token
                var JWToken = new JwtSecurityToken(
                    issuer: jwtSettings.ValidIssuer,
                    audience: jwtSettings.ValidAudience,
                    claims: GetClaims(model, out Id),
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: new DateTimeOffset(expireTime).DateTime,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                );


                // Generate refresh token
                var refreshExpires = DateTime.UtcNow.AddDays(14);
                var refreshJWToken = new JwtSecurityToken(
                    issuer: jwtSettings.ValidIssuer,
                    audience: jwtSettings.ValidAudience,
                    claims: GetClaims(model, out Id),
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: refreshExpires,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                );

                // Create access token string
                UserToken.Token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                UserToken.UserName = model.UserName;
                UserToken.Name = model.Name;
                UserToken.EmailId = model.EmailId;
                UserToken.Id = model.Id;
                UserToken.GuidId = Id;
                UserToken.permissions = model.permissions;
                UserToken.avatar = model.avatar;


                // Create refresh token string
                UserToken.RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshJWToken);
                UserToken.ExpiredTime = expireTime;

                return UserToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
