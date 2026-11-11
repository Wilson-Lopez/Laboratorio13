-- VeterinariaDB: Propietarios (1) -> (N) Mascotas + datos de ejemplo
-- Crea la base si no existe. Ejecutarlo de nuevo reinicia las tablas y sus datos.
-- sqlcmd -S .\SQLEXPRESS -E -C -f 65001 -i VeterinariaDB.sql

IF DB_ID('VeterinariaDB') IS NULL
    CREATE DATABASE VeterinariaDB;
GO

USE VeterinariaDB;
GO

DROP TABLE IF EXISTS Mascotas;
DROP TABLE IF EXISTS Propietarios;
GO

CREATE TABLE Propietarios (
    Id        INT IDENTITY(1,1) NOT NULL,
    Nombres   NVARCHAR(100) NOT NULL,
    Dni       NVARCHAR(8)   NOT NULL,
    Telefono  NVARCHAR(15)  NOT NULL,
    Correo    NVARCHAR(100) NULL,
    CONSTRAINT PK_Propietarios PRIMARY KEY (Id),
    CONSTRAINT UQ_Propietarios_Dni UNIQUE (Dni)
);

CREATE TABLE Mascotas (
    Id               INT IDENTITY(1,1) NOT NULL,
    Nombre           NVARCHAR(50)  NOT NULL,
    Especie          NVARCHAR(30)  NOT NULL,
    Raza             NVARCHAR(50)  NULL,
    FechaNacimiento  DATE          NOT NULL,
    Peso             DECIMAL(5,2)  NOT NULL,
    PropietarioId    INT           NOT NULL,
    CONSTRAINT PK_Mascotas PRIMARY KEY (Id),
    CONSTRAINT FK_Mascotas_Propietarios FOREIGN KEY (PropietarioId) REFERENCES Propietarios(Id),
    CONSTRAINT UQ_Mascotas_Propietario_Nombre UNIQUE (PropietarioId, Nombre),
    CONSTRAINT CK_Mascotas_Peso CHECK (Peso > 0)
);
GO

INSERT INTO Propietarios (Nombres, Dni, Telefono, Correo) VALUES
(N'Lucía Fernanda Rojas Paredes',  '45879632', '987123456', 'lucia.rojas@correo.pe'),
(N'Miguel Ángel Castillo Vega',    '70321458', '954781236', 'miguel.castillo@correo.pe'),
(N'Carmen Rosa Huamán Quispe',     '41526398', '965412378', NULL),
(N'Diego Alonso Salazar Flores',   '72894561', '912874563', 'diego.salazar@correo.pe'),
(N'Patricia Elena Medina Torres',  '46985231', '936258147', 'patricia.medina@correo.pe');

INSERT INTO Mascotas (Nombre, Especie, Raza, FechaNacimiento, Peso, PropietarioId) VALUES
(N'Firulais', N'Perro',  N'Labrador',       '2019-03-15', 28.50, 1),
(N'Michi',    N'Gato',   N'Siamés',         '2021-07-02',  4.20, 1),
(N'Rocky',    N'Perro',  N'Pastor Alemán',  '2018-11-20', 32.00, 2),
(N'Luna',     N'Gato',   N'Persa',          '2022-01-10',  3.80, 3),
(N'Kiara',    N'Perro',  N'Shih Tzu',       '2020-05-25',  6.30, 3),
(N'Copito',   N'Conejo', N'Angora',         '2022-09-05',  2.10, 4),
(N'Toby',     N'Perro',  N'Beagle',         '2017-06-30', 12.40, 4),
(N'Pelusa',   N'Gato',   NULL,              '2023-04-18',  3.10, 4);
GO

SELECT 'Propietarios' AS Tabla, COUNT(*) AS Filas FROM Propietarios
UNION ALL
SELECT 'Mascotas', COUNT(*) FROM Mascotas;
GO
