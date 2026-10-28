using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using BankingProject.Data;

namespace BankingProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly DbHelper _db;

        public TransactionsController(DbHelper db)
        {
            _db = db;
        }
        private bool IsAccountOwnedByUser(int userId, string accountNumber)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();

            string query = @"SELECT COUNT(*)
                            FROM Accounts a
                            INNER JOIN Customers c
                            ON a.CustomerId = c.CustomerId
                            WHERE a.AccountNumber = @AccountNumber
                            AND c.UserId = @UserId";

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar, 20)
                .Value = accountNumber;

            cmd.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = userId;

            return (int)cmd.ExecuteScalar() > 0;
        }
        // GET: api/Transactions
        // Get all transaction history
        [HttpGet]
        public IActionResult GetAllTransactions()
        {
            string? userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized("Invalid user token.");

            try
            {
                List<object> transactions = new List<object>();

                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"
                    SELECT
                        t.TransactionId,
                        a.AccountNumber,
                        t.TransactionType,
                        t.Amount,
                        t.Description,
                        t.TransactionDate
                    FROM Transactions t
                    INNER JOIN Accounts a
                        ON t.AccountId = a.AccountId
                    INNER JOIN Customers c
                        ON a.CustomerId = c.CustomerId
                    WHERE c.UserId = @UserId
                    ORDER BY
                        t.TransactionDate DESC,
                        t.TransactionId DESC";

                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    transactions.Add(new
                    {
                        TransactionId = reader.GetInt32(0),
                        AccountNumber = reader.GetString(1),
                        TransactionType = reader.GetString(2),
                        Amount = reader.GetDecimal(3),
                        Description = reader.IsDBNull(4)
                            ? null
                            : reader.GetString(4),
                        TransactionDate = reader.GetDateTime(5)
                    });
                }

                return Ok(transactions);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unable to retrieve transaction history.");
            }
        }

        // GET: api/Transactions/account/ACC10001
        // Get transaction history for one account
        [HttpGet("account/{accountNumber}")]
        public IActionResult GetAccountTransactions(string accountNumber)
        {
            string? userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized("Invalid user token.");

            try
            {
                List<object> transactions = new List<object>();

                using SqlConnection con = _db.GetConnection();
                con.Open();

                string query = @"
                    SELECT
                        t.TransactionId,
                        a.AccountNumber,
                        t.TransactionType,
                        t.Amount,
                        t.Description,
                        t.TransactionDate
                    FROM Transactions t
                    INNER JOIN Accounts a
                        ON t.AccountId = a.AccountId
                    INNER JOIN Customers c
                        ON a.CustomerId = c.CustomerId
                    WHERE a.AccountNumber = @AccountNumber
                        AND c.UserId = @UserId
                    ORDER BY
                        t.TransactionDate DESC,
                        t.TransactionId DESC";

                using SqlCommand cmd = new SqlCommand(query, con);


                cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar, 20)
                    .Value = accountNumber;

                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    transactions.Add(new
                    {
                        TransactionId = reader.GetInt32(0),
                        AccountNumber = reader.GetString(1),
                        TransactionType = reader.GetString(2),
                        Amount = reader.GetDecimal(3),
                        Description = reader.IsDBNull(4)
                            ? null
                            : reader.GetString(4),
                        TransactionDate = reader.GetDateTime(5)
                    });
                }

                return Ok(transactions);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unable to retrieve account transaction history.");
            }
        }

        // POST: api/Transactions/deposit
        [HttpPost("deposit")]
        public IActionResult Deposit(MoneyRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            string? userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized("Invalid user token.");

            if (!IsAccountOwnedByUser(userId, request.AccountNumber))
                return NotFound("Account not found.");

            try
            {
                using SqlConnection con = _db.GetConnection();
                using SqlCommand cmd = new SqlCommand("Sp_DepositMoney", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar, 20)
                    .Value = request.AccountNumber;
                cmd.Parameters.Add("@DepositAmount", SqlDbType.Decimal)
                    .Value = request.Amount;

                cmd.Parameters["@DepositAmount"].Precision = 18;
                cmd.Parameters["@DepositAmount"].Scale = 2;

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Money deposited successfully.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        // POST: api/Transactions/withdraw
        [HttpPost("withdraw")]
        public IActionResult Withdraw(MoneyRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            string? userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized("Invalid user token.");

            if (!IsAccountOwnedByUser(userId, request.AccountNumber))
                return NotFound("Account not found.");

            try
            {
                using SqlConnection con = _db.GetConnection();
                using SqlCommand cmd = new SqlCommand("Sp_WithdrawMoney", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar, 20)
                    .Value = request.AccountNumber;
                cmd.Parameters.Add("@Amount", SqlDbType.Decimal)
                    .Value = request.Amount;

                cmd.Parameters["@Amount"].Precision = 18;
                cmd.Parameters["@Amount"].Scale = 2;

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Money withdrawn successfully.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        // POST: api/Transactions/transfer
        [HttpPost("transfer")]
        public IActionResult Transfer(TransferRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            if (request.FromAccountNumber == request.ToAccountNumber)
                return BadRequest("Source and destination accounts must differ.");

            string? userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized("Invalid user token.");

            if (!IsAccountOwnedByUser(userId, request.FromAccountNumber))
                return NotFound("Source account not found.");

            try
            {
                using SqlConnection con = _db.GetConnection();
                using SqlCommand cmd = new SqlCommand("Sp_TransferMoney", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FromAccountNumber", SqlDbType.NVarChar, 20)
                    .Value = request.FromAccountNumber;
                cmd.Parameters.Add("@ToAccountNumber", SqlDbType.NVarChar, 20)
                    .Value = request.ToAccountNumber;
                cmd.Parameters.Add("@Amount", SqlDbType.Decimal)
                    .Value = request.Amount;

                cmd.Parameters["@Amount"].Precision = 18;
                cmd.Parameters["@Amount"].Scale = 2;

                con.Open();
                cmd.ExecuteNonQuery();

                return Ok("Money transferred successfully.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}

