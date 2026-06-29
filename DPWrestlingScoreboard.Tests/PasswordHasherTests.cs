using DPWrestlingScoreboard.Services;

namespace DPWrestlingScoreboard.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_And_Verify_Succeeds()
        {
            var hash = PasswordHasher.Hash("secret123");
            Assert.True(PasswordHasher.IsHashed(hash));
            Assert.True(PasswordHasher.Verify("secret123", hash));
            Assert.False(PasswordHasher.Verify("wrong", hash));
        }

        [Fact]
        public void Hash_Produces_Different_Salts()
        {
            var h1 = PasswordHasher.Hash("same");
            var h2 = PasswordHasher.Hash("same");
            Assert.NotEqual(h1, h2);
        }
    }
}
