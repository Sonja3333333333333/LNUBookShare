CREATE TYPE image_type_enum AS ENUM ('book_cover', 'avatar');
CREATE TYPE book_status_enum AS ENUM ('available', 'issued');

CREATE TABLE Faculty (
    faculty_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE Category (
    category_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE Image (
    image_id SERIAL PRIMARY KEY,
    image_path VARCHAR(255) NOT NULL,
    uploaded_at TIMESTAMP NOT NULL DEFAULT NOW(),
    image_type image_type_enum NOT NULL
);

CREATE TABLE "User" (
    user_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE CHECK (email LIKE '%@lnu.edu.ua'),
    password_hash VARCHAR(255) NOT NULL,
    faculty_id INT NOT NULL REFERENCES Faculty(faculty_id) ON DELETE CASCADE,
    avatar_id INT REFERENCES Image(image_id) ON DELETE SET NULL,
    is_email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP
);

CREATE TABLE Book (
    book_id SERIAL PRIMARY KEY,
    title VARCHAR(150) NOT NULL,
    author VARCHAR(100) NOT NULL,
    isbn VARCHAR(20),
    year INT,
    publisher VARCHAR(100),
    language VARCHAR(50),
    category_id INT NOT NULL REFERENCES Category(category_id) ON DELETE CASCADE,
    owner_id INT NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    status book_status_enum NOT NULL DEFAULT 'available',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP,
    cover_id INT REFERENCES Image(image_id) ON DELETE SET NULL
);

CREATE TABLE Favorite (
    favorite_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    book_id INT NOT NULL REFERENCES Book(book_id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT unique_favorite UNIQUE (user_id, book_id)
);

CREATE TABLE EmailConfirmation (
    confirmation_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL UNIQUE REFERENCES "User"(user_id) ON DELETE CASCADE,
    confirmation_token VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP NOT NULL
);
