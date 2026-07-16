using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixIsEmailConfirmedForExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix: All existing users (created before email verification was added)
            // should be marked as email confirmed so they can still log in.
            // Users who self-registered via /auth/register will have a VerificationToken
            // (or IsEmailConfirmed already set to true after verifying) — those are left as-is.
            // Everyone else (admin, jockey, owner, referee, vet, google-login) gets confirmed.
            migrationBuilder.Sql(@"
                UPDATE [AppUser]
                SET [IsEmailConfirmed] = 1
                WHERE [IsEmailConfirmed] = 0
                  AND (
                        [VerificationToken] IS NULL
                        OR [PasswordHash] = ''
                      )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot reverse — would incorrectly block users from logging in
        }
    }
}
