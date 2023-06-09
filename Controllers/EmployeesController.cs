using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using misa.web.api.Enitities;
using misa.web.api.Enitities.DTO;
using MySqlConnector;
using System.Reflection;

namespace misa.web.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllEmplyees()
        {           
            try
            {
                // Khoi tao ket noi toi DB MySQL
                string connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";
                var mySqlConnection = new MySqlConnection(connectionString);

                // Chuan bi cau lenh truy van
                string GetAllEmployeesCommand = "SELECT * FROM employee;";

                // Thuc hien goi vao DB de chay cau lenh 
                var employees = mySqlConnection.Query<Employee>(GetAllEmployeesCommand);

                // Tra ve ket qua              
                return StatusCode(StatusCodes.Status200OK, employees);      
            }
            catch (Exception exception)
            {
                var response = new
                {
                    ErrorCode = "",
                    UserMsg = "Có lỗi xảy ra, vui lòng liên hệ MISA để được trợ giúp.",
                    DevMsg = exception.Message,
                };
                return StatusCode(StatusCodes.Status400BadRequest, response);
            }               
        }

 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="positionID"></param>
        /// <param name="departmentID"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Filter")]
        public IActionResult FilterEmployees (
            [FromQuery] string? keyword,
            [FromQuery] int limit,
            [FromQuery] int page )
        {
            try
            {
                // Khai báo thông tin database
                var connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";

                // Khởi tạo kết nối đến database
                var mySqlConnection = new MySqlConnection(connectionString);

                // Khai báo câu lệnh 
                var sql = "SELECT * FROM employee WHERE (EmployeeName LIKE @keyword OR EmployeeCode LIKE @keyword OR PhoneNumber LIKE @keyword) ORDER BY 'ModifiedDate' LIMIT @offset, @limit;";
                
                // Khai báo các tham số 
                var parameters = new DynamicParameters();
                parameters.Add("@keyword", "%" + keyword + "%");
                parameters.Add("@limit", limit);
                parameters.Add("@offset", limit * (page - 1) );

                // Thực hiện câu lệnh
                var filterEmployees = mySqlConnection.Query<Employee>(sql, parameters);

                // Trả về kết quả
                return StatusCode(StatusCodes.Status200OK, filterEmployees);
            }
            catch (Exception exception)
            {
                var response = new
                {
                    ErrorCode = "",
                    UserMsg = "Có lỗi xảy ra, vui lòng liên hệ MISA để được trợ giúp.",
                    DevMsg = exception.Message,                
                };
                return StatusCode(StatusCodes.Status400BadRequest, response);
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult InsertEmployee([FromBody] Employee employee)
        {
            try
            {
                var erros = new List<string>();
                // I.Thực hiện validate dữ liệu:
                // 1. Các thông tin bắt buộc nhập:
                //--> Mã nhân viên, họ và tên, số chứng thư....:
                if (string.IsNullOrEmpty(employee.EmployeeCode))
                {
                    erros.Add("Mã nhân viên không được phép để trống.");
                }
                if (string.IsNullOrEmpty(employee.EmployeeName))
                {
                    erros.Add("Họ và tên không được phép để trống.");

                }
                if (string.IsNullOrEmpty(employee.Unit))
                {
                    erros.Add("Tên đơn vị không được phép để trống.");

                }
                // Mã nhân viên không được phép trùng: ---> kiểm tra mã nhân viên đã có hay chưa?
                var isValid = CheckEmployeeCode(employee.EmployeeCode);
                if (!isValid)
                {
                    erros.Add("Mã nhân viên không được phép trùng.");
                }
                // 2. Email phải đúng định dạng
                // 3. NGày sinh không được lớn hơn ngày hiện tại:
                // 4. ....


                // Nếu dữ liệu hợp lệ thì thêm mới:
                // Nếu có lỗi dữ liệu thì trả về thông tin lỗi:
                if (erros.Count() > 0)
                {
                    var response = new
                    {
                        ErrorCode = "", 
                        UserMsg = "Dữ liệu không hợp lệ",
                        DevMsg = "",
                        Errors = erros
                        // Các thông tin khác.
                    };
                    return BadRequest(response);
                }
                // II. Thêm mới vào database: 
                // 1. Khai báo thông tin database:
                var connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";

                // 2. Khởi tạo kết nối đến database:
                var mySqlConnection = new MySqlConnection(connectionString);

                // 3. Khai báo câu lệnh thêm mới dữ liệu:
                var sql = "INSERT employee ( EmployeeID, EmployeeCode, EmployeeName, Office, Unit, Gender, DateOfBirth, IdentityNumber, IdentityDate, IdentityPlace, BankAccount, BankName, BankBranch, Address, Email, PhoneNumber, CreatedDate)" +
                    "VALUES ( @EmployeeID, @EmployeeCode, @EmployeeName, @Office, @Unit, @Gender, @DateOfBirth, @IdentityNumber, @IdentityDate, @IdentityPlace, @BankAccount, @BankName, @BankBranch, @Address, @Email, @PhoneNumber, NOW())";
                // 4. Khai báo các tham số cần thiết cho câu lệnh thêm mới:
                var parameters = new DynamicParameters();
                employee.EmployeeID = Guid.NewGuid();
                parameters.Add("@EmployeeID", employee.EmployeeID);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@Office", employee.Office);
                parameters.Add("@Unit", employee.Unit);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityDate", employee.IdentityDate);
                parameters.Add("@IdentityPlace", employee.IdentityPlace);
                parameters.Add("@BankAccount", employee.BankAccount);
                parameters.Add("@BankName", employee.BankName);
                parameters.Add("@BankBranch", employee.BankBranch);
                parameters.Add("@Address", employee.Address);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@PhoneNumber", employee.PhoneNumber);

                // 5. Thực hiện thêm mới:
                var numberAffectedRow = mySqlConnection.Execute(sql, parameters);

                // Trả về kết quả:
                return StatusCode(StatusCodes.Status201Created, numberAffectedRow);
            }
            catch (Exception exception)
            {
                var response = new
                {
                    ErrorCode = "",
                    UserMsg = "Có lỗi xảy ra, vui lòng liên hệ MISA để được trợ giúp.",
                    DevMsg = exception.Message,
                };
                return StatusCode(StatusCodes.Status400BadRequest, response);
            }
        }

        private static bool CheckEmployeeCode(string employeeCode, Guid? employeeID = null )
        {
            var connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";
            var sqlCheck = "SELECT EmployeeCode FROM employee e WHERE e.EmployeeCode = @EmployeeCode AND e.employeeID != @EmployeeID";
            var paramCheck = new DynamicParameters();
            paramCheck.Add("@EmployeeCode", employeeCode);
            paramCheck.Add("@EmployeeID", employeeID);
            var connectionCheck = new MySqlConnection(connectionString);
            var resCheck = connectionCheck.QueryFirstOrDefault<string>(sqlCheck, paramCheck);
            if (resCheck != null)
            {
                return false;
            }
            return true;
        }


        [HttpPut]
        [Route("{employeeID}")]
        public IActionResult UpdateEmployee([FromBody] Employee employee, [FromRoute] Guid employeeID)
        {
            try
            {
                var erros = new List<string>();
                // I.Thực hiện validate dữ liệu:
                // 1. Các thông tin bắt buộc nhập:
                //--> Mã nhân viên, họ và tên, số chứng thư....:
                if (string.IsNullOrEmpty(employee.EmployeeCode))
                {
                    erros.Add("Mã nhân viên không được phép để trống.");
                }
                if (string.IsNullOrEmpty(employee.EmployeeName))
                {
                    erros.Add("Họ và tên không được phép để trống.");

                }
                if (string.IsNullOrEmpty(employee.Unit))
                {
                    erros.Add("Tên đơn vị không được phép để trống.");

                }
                // Mã nhân viên không được phép trùng: ---> kiểm tra mã nhân viên đã có hay chưa?
                var isValid = CheckEmployeeCode(employee.EmployeeCode, employeeID);
                if (!isValid)
                {
                    erros.Add("Mã nhân viên không được phép trùng.");
                }
                // 2. Email phải đúng định dạng
                // 3. NGày sinh không được lớn hơn ngày hiện tại:
                // 4. ....


                // Nếu dữ liệu hợp lệ thì thêm mới:
                // Nếu có lỗi dữ liệu thì trả về thông tin lỗi:
                if (erros.Count() > 0)
                {
                    var response = new
                    {
                        ErrorCode = "",
                        UserMsg = "Dữ liệu không hợp lệ",
                        DevMsg = "",
                        Errors = erros
                        // Các thông tin khác.
                    };
                    return BadRequest(response);
                }
                // II. Thêm mới vào database: 
                // 1. Khai báo thông tin database:
                var connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";

                // 2. Khởi tạo kết nối đến database:
                var mySqlConnection = new MySqlConnection(connectionString);

                // 3. Khai báo câu lệnh thêm mới dữ liệu:
                var sql = "UPDATE employee " +
                    "SET " +
                    "EmployeeCode = @EmployeeCode, " +
                    "EmployeeName = @EmployeeName, " +
                    "Office = @Office, " +
                    "Unit = @Unit, " +
                    "DateOfBirth = @DateOfBirth, " +
                    "Gender = @Gender, " +
                    "IdentityNumber = @IdentityNumber, " +
                    "IdentityDate = @IdentityDate, " +
                    "IdentityPlace = @IdentityPlace, " +
                    "BankAccount = @BankAccount, " +
                    "BankName = @BankName, " +
                    "BankBranch = @BankBranch, " +
                    "Address = @Address, " +
                    "Email = @Email, " +
                    "PhoneNumber = @PhoneNumber, " +                
                    "ModifiedDate = NOW() " +
                    "WHERE EmployeeID = @EmployeeID; ";
                // 4. Khai báo các tham số
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@Office", employee.Office);
                parameters.Add("@Unit", employee.Unit);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityDate", employee.IdentityDate);
                parameters.Add("@IdentityPlace", employee.IdentityPlace);
                parameters.Add("@BankAccount", employee.BankAccount);
                parameters.Add("@BankName", employee.BankName);
                parameters.Add("@BankBranch", employee.BankBranch);
                parameters.Add("@Address", employee.Address);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@PhoneNumber", employee.PhoneNumber);

                // 5. Thực hiện thêm mới:
                var numberRowAffected = mySqlConnection.Execute(sql, parameters);

                // Trả về kết quả:
                return StatusCode(StatusCodes.Status200OK, numberRowAffected);
            }
            catch (Exception exception)
            {
                var response = new
                {
                    ErrorCode = "",
                    UserMsg = "Có lỗi xảy ra, vui lòng liên hệ MISA để được trợ giúp.",
                    DevMsg = exception.Message,
                };
                return StatusCode(StatusCodes.Status400BadRequest, response);
            }
        }

 
        [HttpDelete]
        [Route("{employeeID}")]
        public IActionResult DeleteEmployee([FromRoute] Guid employeeID)
        {
            try
            {
                // Khoi tao ket noi toi DB MySQL
                string connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";
                var mySqlConnection = new MySqlConnection(connectionString);

                // Chuan bi cau lenh
                string sql = "DELETE FROM employee WHERE EmployeeID = @EmployeeID;";

                // Khai báo các tham số
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);

                // Thuc hien goi vao DB de chay cau lenh 
                var numberRowAffected = mySqlConnection.Execute(sql, parameters);

                return StatusCode(StatusCodes.Status200OK, numberRowAffected);
            }
            catch (Exception exception)
            {
                var response = new
                {
                    ErrorCode = "",
                    UserMsg = "Có lỗi xảy ra, vui lòng liên hệ MISA để được trợ giúp.",
                    DevMsg = exception.Message,
                };
                return StatusCode(StatusCodes.Status400BadRequest, response);
            }
        }

        [HttpGet]
        [Route("NewEmployeeCode")]
        public IActionResult NewEmployeeCode()
        {
            try
            {
                // Khoi tao ket noi toi DB MySQL
                string connectionString = "Host=localhost; Port=3306; Database=daotao.ai.2023.dnminh; User Id=root; Pwd=1234abcd;";
                var mySqlConnection = new MySqlConnection(connectionString);

                // Chuan bi cau lenh
                string sql = "SELECT MAX( EmployeeCode ) FROM employee;";

                // Thuc hien goi vao DB de chay cau lenh 
                string maxEmployeeCode = mySqlConnection.QueryFirstOrDefault<string>(sql);

                // Tao Code moi
                maxEmployeeCode = "NV-" + (Int64.Parse(maxEmployeeCode.Substring(3)) + 1).ToString();
                
                // Tra ve ket qua
                return StatusCode(StatusCodes.Status200OK, maxEmployeeCode);
            }
            catch (Exception exception)
            {
                var response = new
                {
                    ErrorCode = "MISA002",
                    UserMsg = "Có lỗi xảy ra, vui lòng liên hệ MISA để được trợ giúp.",
                    DevMsg = exception.Message,
                };
                return StatusCode(StatusCodes.Status400BadRequest, response);
            }
        }

    }
}