using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BankingProject.Data;
using BankingProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace BankingProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly DbHelper _db;

        public CustomersController(DbHelper db)
        {
            _db = db;
        }

        // GET: api/Customers
        [HttpGet]
        public IActionResult GetCustomers()
        {
            List<object> customers = new();

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"SELECT CustomerId, UserId, FullName,
                    DateOfBirth, Gender, Phone, Email,
                    Cust_Address, AadhaarNumber, CreatedAt
                    FROM Customers";

                using SqlCommand cmd = new(query, con);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    customers.Add(new
                    {
                        CustomerId = reader["CustomerId"],
                        UserId = reader["UserId"],
                        FullName = reader["FullName"],
                        DateOfBirth = reader["DateOfBirth"],
                        Gender = reader["Gender"],
                        Phone = reader["Phone"],
                        Email = reader["Email"],
                        Address = reader["Cust_Address"],
                        AadhaarNumber = reader["AadhaarNumber"],
                        CreatedAt = reader["CreatedAt"]
                    });
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET: api/Customers/1
        [HttpGet("{id}")]
        public IActionResult GetCustomer(int id)
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"SELECT CustomerId, UserId, FullName,
                    DateOfBirth, Gender, Phone, Email,
                    Cust_Address, AadhaarNumber, CreatedAt
                    FROM Customers WHERE CustomerId = @Id";

                using SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                using SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return NotFound("Customer not found.");

                var customer = new
                {
                    CustomerId = reader["CustomerId"],
                    UserId = reader["UserId"],
                    FullName = reader["FullName"],
                    DateOfBirth = reader["DateOfBirth"],
                    Gender = reader["Gender"],
                    Phone = reader["Phone"],
                    Email = reader["Email"],
                    Address = reader["Cust_Address"],
                    AadhaarNumber = reader["AadhaarNumber"],
                    CreatedAt = reader["CreatedAt"]
                };

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // POST: api/Customers
        [HttpPost]
        public IActionResult AddCustomer(CustomerRequest customer)
        {
            if (customer.DateOfBirth.Date >= DateTime.Today)
                return BadRequest("Date of birth must be in the past.");

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"INSERT INTO Customers
                    (UserId, FullName, DateOfBirth, Gender, Phone,
                     Email, Cust_Address, AadhaarNumber)
                    OUTPUT INSERTED.CustomerId
                    VALUES
                    (@UserId, @FullName, @DateOfBirth, @Gender,
                     @Phone, @Email, @Address, @AadhaarNumber)";

                using SqlCommand cmd = new(query, con);

                cmd.Parameters.AddWithValue("@UserId",
                    (object?)customer.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", customer.FullName);
                cmd.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth.Date);
                cmd.Parameters.AddWithValue("@Gender", customer.Gender);
                cmd.Parameters.AddWithValue("@Phone", customer.Phone);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Address", customer.Address);
                cmd.Parameters.AddWithValue("@AadhaarNumber", customer.AadhaarNumber);

                int id = (int)cmd.ExecuteScalar()!;

                return Ok(new
                {
                    Message = "Customer added successfully.",
                    CustomerId = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/Customers/1
        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, CustomerRequest customer)
        {
            if (customer.DateOfBirth.Date >= DateTime.Today)
                return BadRequest("Date of birth must be in the past.");

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"UPDATE Customers SET
                    UserId = @UserId,
                    FullName = @FullName,
                    DateOfBirth = @DateOfBirth,
                    Gender = @Gender,
                    Phone = @Phone,
                    Email = @Email,
                    Cust_Address = @Address,
                    AadhaarNumber = @AadhaarNumber
                    WHERE CustomerId = @Id";

                using SqlCommand cmd = new(query, con);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UserId",
                    (object?)customer.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", customer.FullName);
                cmd.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth.Date);
                cmd.Parameters.AddWithValue("@Gender", customer.Gender);
                cmd.Parameters.AddWithValue("@Phone", customer.Phone);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Address", customer.Address);
                cmd.Parameters.AddWithValue("@AadhaarNumber", customer.AadhaarNumber);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Customer not found.");

                return Ok("Customer updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE: api/Customers/1
        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query =
                    "DELETE FROM Customers WHERE CustomerId = @Id";

                using SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Customer not found.");

                return Ok("Customer deleted successfully.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return Conflict(
                    "Customer cannot be deleted because related records exist.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

