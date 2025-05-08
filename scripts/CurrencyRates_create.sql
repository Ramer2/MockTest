-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2025-05-08 17:30:03.211

-- tables
-- Table: Country
CREATE TABLE Country (
    Id int IDENTITY(1,1) NOT NULL,
    Name varchar(100)  NOT NULL,
    CONSTRAINT Country_pk PRIMARY KEY  (Id)
);

-- Table: Currency
CREATE TABLE Currency (
    Id int IDENTITY(1,1) NOT NULL,
    Name varchar(100)  NOT NULL,
    Rate float(3)  NOT NULL,
    CONSTRAINT Currency_pk PRIMARY KEY  (Id)
);

-- Table: Currency_Country
CREATE TABLE Currency_Country (
    Country_Id int  NOT NULL,
    Currency_Id int  NOT NULL,
    CONSTRAINT Currency_Country_pk PRIMARY KEY  (Country_Id,Currency_Id)
);

-- foreign keys
-- Reference: Table_2_Country (table: Currency_Country)
ALTER TABLE Currency_Country ADD CONSTRAINT Table_2_Country
    FOREIGN KEY (Country_Id)
    REFERENCES Country (Id);

-- Reference: Table_2_Currency (table: Currency_Country)
ALTER TABLE Currency_Country ADD CONSTRAINT Table_2_Currency
    FOREIGN KEY (Currency_Id)
    REFERENCES Currency (Id);

-- End of file.

