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
    public class AccountsController : ControllerBase
    {
        private readonly DbHelper _db;

        public AccountsController(DbHelper db)
        {
            _db = db;
        }

        // GET: api/Accounts
        [HttpGet]
        public IActionResult GetAccounts()
        {
            List<object> accounts = new();

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"SELECT AccountId, CustomerId, AccountNumber,
                    AccountType, Balance, Status, CreatedAt
                    FROM Accounts";

                using SqlCommand cmd = new(query, con);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    accounts.Add(new
                    {
                        AccountId = reader["AccountId"],
                        CustomerId = reader["CustomerId"],
                        AccountNumber = reader["AccountNumber"],
                        AccountType = reader["AccountType"],
                        Balance = reader["Balance"],
                        Status = reader["Status"],
                        CreatedAt = reader["CreatedAt"]
                    });
                }

                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET: api/Accounts/1
        [HttpGet("{id}")]
        public IActionResult GetAccount(int id)
        {
            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"SELECT AccountId, CustomerId, AccountNumber,
                    AccountType, Balance, Status, CreatedAt
                    FROM Accounts WHERE AccountId = @Id";

                using SqlCommand cmd = new(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                using SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return NotFound("Account not found.");

                var account = new
                {
                    AccountId = reader["AccountId"],
                    CustomerId = reader["CustomerId"],
                    AccountNumber = reader["AccountNumber"],
                    AccountType = reader["AccountType"],
                    Balance = reader["Balance"],
                    Status = reader["Status"],
                    CreatedAt = reader["CreatedAt"]
                };

                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // POST: api/Accounts
        [HttpPost]
        public IActionResult AddAccount(AccountRequest account)
        {
            if (string.IsNullOrWhiteSpace(account.AccountNumber) ||
                string.IsNullOrWhiteSpace(account.AccountType))
                return BadRequest("Account number and type are required.");

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"INSERT INTO Accounts
                    (CustomerId, AccountNumber, AccountType, Balance, Status)
                    OUTPUT INSERTED.AccountId
                    VALUES (@CustomerId, @AccountNumber, @AccountType, 0, 'Active')";

                using SqlCommand cmd = new(query, con);

                cmd.Parameters.AddWithValue("@CustomerId", account.CustomerId);
                cmd.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                cmd.Parameters.AddWithValue("@AccountType", account.AccountType);

                int id = (int)cmd.ExecuteScalar()!;

                return Ok(new
                {
                    Message = "Account created successfully.",
                    AccountId = id
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict("This account number already exists.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return BadRequest("Customer ID does not exist.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("{id}")]
        public IActionResult UpdateAccount(int id, AccountUpdate account)
        {
            if (account.Status != "Active" &&
                account.Status != "Inactive" &&
                account.Status != "Closed")
            {
                return BadRequest("Status must be Active, Inactive, or Closed.");
            }

            try
            {
                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"UPDATE Accounts
                                SET AccountType = @Type,
                                Status = @Status
                                WHERE AccountId = @Id";

                using SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Type", account.AccountType);
                cmd.Parameters.AddWithValue("@Status", account.Status);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Account not found or already closed.");

                return Ok("Account updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}

