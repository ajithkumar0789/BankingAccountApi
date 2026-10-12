using BankingAccountApi.Data1;
using BankingAccountApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BankingAccountApi.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/accounts
    // GET: /api/accounts?accountType=Savings
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? accountType)
    {
        var query = _context.Accounts
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(accountType))
        {
            if (accountType != "Savings" &&
                accountType != "Current")
            {
                return BadRequest(
                    "Account type must be Savings or Current.");
            }

            query = query.Where(
                a => a.AccountType == accountType);
        }

        var accounts = await query
            .OrderBy(a => a.Id)
            .ToListAsync();

        return Ok(accounts);
    }

    // GET: /api/accounts/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            return NotFound("Account not found.");
        }

        return Ok(account);
    }

    // POST: /api/accounts
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAccountRequest request)
    {
        string holderName = request.AccountHolderName.Trim();
        string accountNumber = request.AccountNumber.Trim();

        if (string.IsNullOrWhiteSpace(holderName) ||
            string.IsNullOrWhiteSpace(accountNumber))
        {
            return BadRequest(
                "Account holder name and account number are required.");
        }

        bool exists = await _context.Accounts.AnyAsync(
            a => a.AccountNumber == accountNumber);

        if (exists)
        {
            return Conflict("Account number already exists.");
        }

        var account = new BankAccount
        {
            AccountHolderName = holderName,
            AccountNumber = accountNumber,
            Balance = request.Balance,
            AccountType = request.AccountType
        };

        _context.Accounts.Add(account);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627))
        {
            return Conflict("Account number already exists.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id },
            account);
    }

    // PUT: /api/accounts/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateAccountRequest request)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound("Account not found.");
        }

        if (string.IsNullOrWhiteSpace(
            request.AccountHolderName))
        {
            return BadRequest(
                "Account holder name is required.");
        }

        account.AccountHolderName =
            request.AccountHolderName.Trim();

        account.AccountType = request.AccountType;

        await _context.SaveChangesAsync();

        return Ok(account);
    }

    // DELETE: /api/accounts/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound("Account not found.");
        }

        _context.Accounts.Remove(account);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: /api/accounts/1/deposit
    [HttpPost("{id:int}/deposit")]
    public async Task<IActionResult> Deposit(
        int id, AmountRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(
                "Deposit amount must be greater than zero.");
        }

        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound("Account not found.");
        }

        try
        {
            account.Balance =
                checked(account.Balance + request.Amount);
        }
        catch (OverflowException)
        {
            return BadRequest("The resulting balance is too large.");
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "Could not save the updated balance.");
        }

        return Ok(account);
    }

    // POST: /api/accounts/1/withdraw
    [HttpPost("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(
        int id, AmountRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(
                "Withdrawal amount must be greater than zero.");
        }

        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound("Account not found.");
        }

        if (request.Amount > account.Balance)
        {
            return BadRequest("Insufficient balance.");
        }

        account.Balance -= request.Amount;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "Could not save the updated balance.");
        }

        return Ok(account);
    }
}