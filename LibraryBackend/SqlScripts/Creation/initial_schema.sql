-- Database Versioning Table
CREATE TABLE IF NOT EXISTS "DbVersion" (
    "Version" INTEGER NOT NULL PRIMARY KEY,
    "AppliedDate" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Categories Table
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

-- Books Table
CREATE TABLE IF NOT EXISTS "Books" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Author" TEXT NOT NULL,
    "ISBN" TEXT NOT NULL,
    "IsAvailable" INTEGER NOT NULL,
    "CategoryId" INTEGER NULL,
    CONSTRAINT "FK_Books_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id")
);

-- Clients Table
CREATE TABLE IF NOT EXISTS "Clients" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Role" TEXT NOT NULL DEFAULT 'User'
);

-- Loans Table
CREATE TABLE IF NOT EXISTS "Loans" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "BookId" INTEGER NOT NULL,
    "ClientId" INTEGER NOT NULL,
    "LoanDate" TEXT NOT NULL,
    "ReturnDate" TEXT NULL,
    CONSTRAINT "FK_Loans_Books_BookId" FOREIGN KEY ("BookId") REFERENCES "Books" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Loans_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE
);

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_Books_CategoryId" ON "Books" ("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_Loans_BookId" ON "Loans" ("BookId");
CREATE INDEX IF NOT EXISTS "IX_Loans_ClientId" ON "Loans" ("ClientId");

-- Set Initial Version
INSERT INTO "DbVersion" ("Version", "AppliedDate") VALUES (2, CURRENT_TIMESTAMP);

-- Create admin user
INSERT INTO "Clients" ("Id", "Name", "Email", "Role") VALUES (1, "admin", "MarioGomes@criticalmanufacturing.com", "Admin");
