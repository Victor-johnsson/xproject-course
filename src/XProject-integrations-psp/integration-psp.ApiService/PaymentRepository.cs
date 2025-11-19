using Dapper;
using Models;
using Npgsql;

namespace Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();

    Task<Payment> GetByIdAsync(int id);

    Task<int> CreateAsync();

    Task<bool> UpdateAsync(Payment payment);

    Task<bool> DeleteAsync(int id);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly NpgsqlDataSource _datasource;

    public PaymentRepository(NpgsqlDataSource datasource)
    {
        _datasource = datasource;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        using var conn = _datasource.CreateConnection();
        return await conn.QueryAsync<Payment>("SELECT * FROM payments");
    }

    public async Task<Payment> GetByIdAsync(int id)
    {
        using var conn = _datasource.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Payment>(
            "SELECT * FROM payments WHERE id = @Id",
            new { Id = id }
        );
    }

    public async Task<int> CreateAsync()
    {
        using var conn = _datasource.CreateConnection();
        var sql =
            @"
                INSERT INTO Payments (paymentCompleted) values (false)
                RETURNING id";

        return await conn.ExecuteScalarAsync<int>(sql);
    }

    public async Task<bool> UpdateAsync(Payment payment)
    {
        using var conn = _datasource.CreateConnection();
        var sql =
            @"
                UPDATE payments
                SET paymentCompleted = @PaymentCompleted
                WHERE id = @Id";

        var affected = await conn.ExecuteAsync(sql, payment);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _datasource.CreateConnection();
        var affected = await conn.ExecuteAsync(
            "DELETE FROM payments WHERE id = @Id",
            new { Id = id }
        );

        return affected > 0;
    }
}
