CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    registration_date DATETIME DEFAULT GETDATE()
);

CREATE TABLE PrimeCheckHistory (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT,
    input_number BIGINT NOT NULL,
    is_prime BIT NOT NULL,
    check_date DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);