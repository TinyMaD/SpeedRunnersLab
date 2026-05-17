using SpeedRunners;

namespace SpeedRunners.Tests
{
    public class AdminHelperTests
    {
        public AdminHelperTests()
        {
            AdminHelper.OverrideAdminIdsForTesting(new[] { "76561198000000001", "76561198000000002" });
        }

        [Fact]
        public void IsAdmin_ReturnsFalse_WhenIdIsNull()
        {
            Assert.False(AdminHelper.IsAdmin(null));
        }

        [Fact]
        public void IsAdmin_ReturnsFalse_WhenIdIsEmpty()
        {
            Assert.False(AdminHelper.IsAdmin(""));
        }

        [Fact]
        public void IsAdmin_ReturnsFalse_WhenIdNotInList()
        {
            Assert.False(AdminHelper.IsAdmin("76561198999999999"));
        }

        [Fact]
        public void IsAdmin_ReturnsTrue_WhenIdMatchesAnyAdmin()
        {
            Assert.True(AdminHelper.IsAdmin("76561198000000001"));
            Assert.True(AdminHelper.IsAdmin("76561198000000002"));
        }

        [Fact]
        public void IsAdmin_IsCaseInsensitive()
        {
            AdminHelper.OverrideAdminIdsForTesting(new[] { "AbC123" });
            Assert.True(AdminHelper.IsAdmin("abc123"));
            Assert.True(AdminHelper.IsAdmin("ABC123"));
        }
    }
}
