BEGIN TRANSACTION;

-- 1. Ensure Customer Role exists
INSERT OR IGNORE INTO roles (role_id, role_name) VALUES (3, 'User');

-- 2. Create customer account
INSERT INTO users (role_id, username, password_hash, first_name, last_name, email)
SELECT 3, 'customer', '$2a$11$6u9u/QZc5Zq.O7.k4v1x..D7L1jUj1.fJ4.kY3.Q6Y4./8.v4.v4.', 'Test', 'Customer', 'customer@medistock.com'
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = 'customer');

-- 3. Add categories
INSERT OR IGNORE INTO categories (category_id, category_name) VALUES 
(1, 'Pain Reliever'),
(2, 'Antibiotic'),
(3, 'Antihistamine'),
(4, 'Vitamins'),
(5, 'Cough & Cold');

-- 4. Add medicines
INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Biogesic', 'Paracetamol', 1, '500mg', 'Unilab', 5.50, 100, 'Available', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Biogesic');

INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Amoxil', 'Amoxicillin', 2, '500mg', 'GSK', 12.00, 50, 'Available', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Amoxil');

INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Alerta', 'Loratadine', 3, '10mg', 'Unilab', 15.00, 75, 'Available', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Alerta');

INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Enervon', 'Multivitamins', 4, 'Tablet', 'Unilab', 7.50, 200, 'Available', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Enervon');

INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Neozep', 'Phenylephrine', 5, 'Tablet', 'Unilab', 6.00, 150, 'Available', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Neozep');

INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Tempra', 'Paracetamol Drops', 1, '100mg/mL', 'Taisho', 120.00, 20, 'Available', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Tempra');

INSERT INTO medicines (brand_name, generic_name, category_id, dosage, manufacturer, price, stock_qty, status, created_at)
SELECT 'Kremil-S', 'Aluminum Hydroxide', 1, 'Tablet', 'Unilab', 8.00, 0, 'Out of Stock', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE brand_name = 'Kremil-S');

COMMIT;
