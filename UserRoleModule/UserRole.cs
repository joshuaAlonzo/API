using System;

namespace Api.UserRoleModule
{
    public class Userrole : IEquatable<Userrole>
    {
        // Maps to: roles(role_id, role_name)
        public int    UserRoleId { get; set; }   // role_id
        public string UserRole   { get; set; } = string.Empty; // role_name

        public Userrole() { }

        public Userrole(int userRoleId, string userRole)
        {
            UserRoleId = userRoleId;
            UserRole   = userRole;
        }

        public bool Equals(Userrole? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (UserRoleId > 0 || other.UserRoleId > 0) return UserRoleId == other.UserRoleId;
            return string.Equals(UserRole, other.UserRole, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Userrole);

        public override int GetHashCode()
        {
            if (UserRoleId > 0) return UserRoleId.GetHashCode();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(UserRole);
        }

        public override string ToString() =>
            $"Userrole[RoleId={UserRoleId}, RoleName={UserRole}]";
    }
}
