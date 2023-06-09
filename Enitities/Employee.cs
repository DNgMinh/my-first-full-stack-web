namespace misa.web.api.Enitities
{
    public class Employee
    {
        public Guid EmployeeID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string? IdentityNumber { get; set; }
        public string? IdentityPlace { get; set; }
        public string? IdentityDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Office { get; set; }
        public string? Unit { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
