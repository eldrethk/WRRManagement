using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class RefreshTokenRepository : DapperRepository, IRefreshTokenRepository
    {
        public RefreshTokenRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public  async Task<int> CreateAsync(RefreshToken token)
        {
            var parameters = new
            {
                UserId = token.UserId,
                Token = token.Token,
                ExpiresAt = token.ExpiresAt
            };

            return await ExecuteScalarIntAsync("dbo.authInsRefreshToken", parameters);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
        {
            var parameters = new {Token =  tokenHash};
            return await QueryFirstOrDefaultAsync<RefreshToken>("dbo.authSelRefreshToken", parameters);
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            var parameters = new { userId = userId };
            await ExecuteAsync("dbo.authRevokeAllUserTokens", parameters);
        }

        public async Task RevokeAsync(string tokenHash, string? replacedByTokenHash = null)
        {
            var parameters = new {Token = tokenHash, ReplacedByToken =  replacedByTokenHash};
            await ExecuteAsync("dbo.authRevokeRefreshToken", parameters);
        }
    }
}
