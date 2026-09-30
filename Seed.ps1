$dbPath = "c:\3rd\Api\pharmacy.db"
$dllPath = (Get-ChildItem -Path "c:\3rd\Api\bin\Debug\net10.0" -Recurse -Filter "Microsoft.Data.Sqlite.dll" | Select-Object -First 1).FullName

if (-not $dllPath) {
    Write-Host "SQLite DLL not found!"
    exit 1
}

Add-Type -Path $dllPath

$connStr = "Data Source=$dbPath"
$conn = New-Object Microsoft.Data.Sqlite.SqliteConnection($connStr)
$conn.Open()

$trans = $conn.BeginTransaction()

try {
    # 1. Ensure Customer Role
    $cmd = $conn.CreateCommand()
    $cmd.Transaction = $trans
    $cmd.CommandText = "INSERT OR IGNORE INTO user_role (role_id, role_name, description) VALUES (3, 'User', 'Customer role');"
    $cmd.ExecuteNonQuery() | Out-Null

    # 2. Add customer account
    $cmd.CommandText = "SELECT COUNT(*) FROM user WHERE username = 'customer';"
    $count = $cmd.ExecuteScalar()
    if ($count -eq 0) {
        # Using a pre-hashed bcrypt for 'password123'
        $hashed = "`$2a`$11`$6u9u/QZc5Zq.O7.k4v1x..D7L1jUj1.fJ4.kY3.Q6Y4./8.v4.v4." 
        $cmd.CommandText = "INSERT INTO user (role_id, username, password, first_name, last_name, email) VALUES (3, 'customer', '$hashed', 'Test', 'Customer', 'customer@medistock.com');"
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Created 'customer' account."
    }

    # 3. Add categories
    $cmd.CommandText = @"
        INSERT OR IGNORE INTO category (category_id, name, description) VALUES 
        (1, 'Pain Reliever', 'Medicines for pain and fever'),
        (2, 'Antibiotic', 'Medicines for bacterial infections'),
        (3, 'Antihistamine', 'Medicines for allergies'),
        (4, 'Vitamins', 'Nutritional supplements'),
        (5, 'Cough & Cold', 'Medicines for respiratory relief');
"@
    $cmd.ExecuteNonQuery() | Out-Null

    # 4. Add medicines
    $cmd.CommandText = "SELECT COUNT(*) FROM medicine;"
    $medCount = $cmd.ExecuteScalar()
    if ($medCount -eq 0) {
        $now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
        $cmd.CommandText = @"
            INSERT INTO medicine (brand_name, generic_name, category_id, dosage, price, stock_qty, status, created_at) VALUES 
            ('Biogesic', 'Paracetamol', 1, '500mg', 5.50, 100, 'Available', '$now'),
            ('Amoxil', 'Amoxicillin', 2, '500mg', 12.00, 50, 'Available', '$now'),
            ('Alerta', 'Loratadine', 3, '10mg', 15.00, 75, 'Available', '$now'),
            ('Enervon', 'Multivitamins', 4, 'Tablet', 7.50, 200, 'Available', '$now'),
            ('Neozep', 'Phenylephrine', 5, 'Tablet', 6.00, 150, 'Available', '$now'),
            ('Tempra', 'Paracetamol Drops', 1, '100mg/mL', 120.00, 20, 'Available', '$now'),
            ('Kremil-S', 'Aluminum Hydroxide', 1, 'Tablet', 8.00, 0, 'Out of Stock', '$now');
"@
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Inserted Philippine-market medicines."
    }

    $trans.Commit()
    Write-Host "Success! Data seeded into pharmacy.db."
} catch {
    $trans.Rollback()
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    $conn.Close()
}
