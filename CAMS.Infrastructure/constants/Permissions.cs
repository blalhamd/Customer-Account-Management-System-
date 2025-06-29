namespace CAMS.Infrastructure.constants
{
    public static class Permissions
    {
        public static string Type { get; } = "permissions";

        public static class Users
        {
            public const string View = "Permissions.Users.View"; 
            public const string ViewById = "Permissions.Users.ViewUserById";
            public const string Create = "Permissions.Users.Create"; 
            public const string CreateAdmin = "Permissions.Users.CreateAdmin"; 
            public const string Edit = "Permissions.Users.Edit"; 
            public const string Delete = "Permissions.Users.Delete"; 
            public const string UnLock = "Permissions.Users.UnLock"; 
            public const string Disable = "Permissions.Users.Disable"; 
            public const string Enable = "Permissions.Users.Enable"; 
            public const string AssignRoles = "Permissions.Users.AssignRoles"; 
            public const string ViewAuditTrail = "Permissions.Users.ViewAuditTrail"; 
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View"; 
            public const string ViewById = "Permissions.Roles.ViewRoleById"; 
            public const string Create = "Permissions.Roles.Create"; 
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
            public const string AssignPermissionsToRole = "Permissions.Roles.AssignPermissionsToRole"; 
        }

        public static class Accounts
        {
            public const string ViewAccountsForClient = "Permissions.Accounts.ViewAccountsForClient";
            public const string ViewById = "Permissions.Accounts.ViewById";
            public const string ChangeAccountStatus = "Permissions.Accounts.ChangeAccountStatus";
            public const string FlagAccountSigned = "Permissions.Accounts.FlagAccountSigned";
            public const string CloseAccount = "Permissions.Accounts.CloseAccount";
        }

        public static class Clients
        {
            public const string RegisterClient = "Permissions.Clients.RegisterClient";
            public const string ViewById = "Permissions.Clients.ViewById";
            public const string View = "Permissions.Clients.View";
            public const string Edit = "Permissions.Clients.Edit";
            public const string Soft = "Permissions.Clients.Soft";
            public const string Restore = "Permissions.Clients.Restore";
        }

        public static class Currents
        {
            public const string OpenCurrent = "Permissions.Currents.OpenCurrent";
            public const string View = "Permissions.Currents.View";
            public const string ViewById = "Permissions.Currents.ViewById";
        } 
        
        public static class FixedDeposits
        {
            public const string OpenFixedDeposit = "Permissions.FixedDeposits.OpenFixedDeposit";
            public const string View = "Permissions.FixedDeposits.View";
        }
        
        public static class JointAccounts
        {
            public const string OpenJointAccount = "Permissions.JointAccounts.OpenJointAccount";
            public const string AddSecondary = "Permissions.JointAccounts.AddSecondary";
            public const string RemoveSecondary = "Permissions.JointAccounts.RemoveSecondary";
            public const string View = "Permissions.JointAccounts.View";
        }

        public static class Loans
        {
            public const string Apply = "Permissions.Loans.Apply";
            public const string Approve = "Permissions.Loans.Approve";
            public const string Installment = "Permissions.Loans.Installment";
            public const string View = "Permissions.Loans.View";
            public const string ViewById = "Permissions.Loans.ViewById";
        }

        public static class Savings
        {
            public const string Open = "Permissions.Savings.Open";
            public const string EnableOverdraft = "Permissions.Savings.EnableOverdraft";
            public const string View = "Permissions.Savings.View";
            public const string ViewById = "Permissions.Savings.ViewById";
        } 
        
        public static class Transactions
        {
            public const string Create = "Permissions.Transactions.Create";
            public const string View = "Permissions.Transactions.View";
            public const string ViewById = "Permissions.Transactions.ViewById";
        }

        /// <summary>
        /// Retrieves a list of all permission constants.
        /// </summary>
        public static IList<string> GetPermissions()
        {
            return typeof(Permissions)
                .GetNestedTypes()
                .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Select(f => f.GetValue(null) as string))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList()!;
        }
    }
}
