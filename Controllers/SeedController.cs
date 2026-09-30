using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Api.Main;
using System;
using Microsoft.Data.Sqlite;
using BCrypt.Net;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly MyCon _myCon;

        public SeedController(MyCon myCon)
        {
            _myCon = myCon;
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunSeed()
        {
            try
            {
                using var conn = _myCon.GetConnection();
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                try
                {
                    // 1. Create Customer Role if it doesn't exist
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT OR IGNORE INTO user_role (role_id, role_name, description) VALUES (3, 'User', 'Customer role');";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 2. Create a test customer account if missing
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM user WHERE username = 'customer';";
                        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count == 0)
                        {
                            var hashed = BCrypt.Net.BCrypt.HashPassword("password123");
                            var insertUser = conn.CreateCommand();
                            insertUser.CommandText = $@"
                                INSERT INTO user (role_id, username, password, first_name, last_name, email)
                                VALUES (3, 'customer', '{hashed}', 'Test', 'Customer', 'customer@medistock.com');";
                            await insertUser.ExecuteNonQueryAsync();
                        }
                    }

                    // 3. Insert Categories
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            INSERT OR IGNORE INTO category (category_id, name, description) VALUES 
                            (1, 'Pain Reliever', 'Medicines for pain and fever'),
                            (2, 'Antibiotic', 'Medicines for bacterial infections'),
                            (3, 'Antihistamine', 'Medicines for allergies'),
                            (4, 'Vitamins', 'Nutritional supplements'),
                            (5, 'Cough & Cold', 'Medicines for respiratory relief');";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 4. Insert Medicines
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM medicine;";
                        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count == 0)
                        {
                            var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                            var insertMeds = conn.CreateCommand();
                            insertMeds.CommandText = $@"
                                INSERT INTO medicine (brand_name, generic_name, category_id, dosage, price, stock_qty, status, created_at) VALUES 
                                ('Biogesic', 'Paracetamol', 1, '500mg', 5.50, 100, 'Available', '{now}'),
                                ('Amoxil', 'Amoxicillin', 2, '500mg', 12.00, 50, 'Available', '{now}'),
                                ('Alerta', 'Loratadine', 3, '10mg', 15.00, 75, 'Available', '{now}'),
                                ('Enervon', 'Multivitamins', 4, 'Tablet', 7.50, 200, 'Available', '{now}'),
                                ('Neozep', 'Phenylephrine', 5, 'Tablet', 6.00, 150, 'Available', '{now}'),
                                ('Tempra', 'Paracetamol Drops', 1, '100mg/mL', 120.00, 20, 'Available', '{now}'),
                                ('Kremil-S', 'Aluminum Hydroxide', 1, 'Tablet', 8.00, 0, 'Out of Stock', '{now}');";
                            await insertMeds.ExecuteNonQueryAsync();
                        }
                    }

                    transaction.Commit();
                    return Ok(new { message = "Seeded successfully! Customer account: 'customer' / 'password123'" });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
