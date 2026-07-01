using SpeedRunners;
using SpeedRunners.BLL;

namespace SpeedRunners.Tests
{
    [Collection("AdminHelper")]
    public class ProfilePrivacyTests
    {
        private const string Owner = "76561198000000100";
        private const string Visitor = "76561198000000200";
        private const string Admin = "76561198000000999";

        public ProfilePrivacyTests()
        {
            AdminHelper.OverrideAdminIdsForTesting(new[] { Admin });
        }

        [Fact]
        public void IsOwnerOrAdmin_ReturnsFalse_WhenVisitorIsNull()
        {
            Assert.False(ProfileBLL.IsOwnerOrAdmin(Owner, null));
        }

        [Fact]
        public void IsOwnerOrAdmin_ReturnsFalse_WhenVisitorIsEmpty()
        {
            Assert.False(ProfileBLL.IsOwnerOrAdmin(Owner, ""));
        }

        [Fact]
        public void IsOwnerOrAdmin_ReturnsTrue_WhenVisitorIsOwner()
        {
            Assert.True(ProfileBLL.IsOwnerOrAdmin(Owner, Owner));
        }

        [Fact]
        public void IsOwnerOrAdmin_ReturnsTrue_WhenVisitorIsAdminButNotOwner()
        {
            Assert.True(ProfileBLL.IsOwnerOrAdmin(Owner, Admin));
        }

        [Fact]
        public void IsOwnerOrAdmin_ReturnsFalse_WhenVisitorIsNeitherOwnerNorAdmin()
        {
            Assert.False(ProfileBLL.IsOwnerOrAdmin(Owner, Visitor));
        }
    }
}
