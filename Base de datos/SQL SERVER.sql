create database DBAMONSUL;

USE DBAMONSUL;

CREATE TABLE Faccion (
    ID_Faccion INT PRIMARY KEY IDENTITY(1,1),
    Nombre_Faccion VARCHAR(100) NOT NULL
);

CREATE TABLE Usuario (
    ID_Usuario INT PRIMARY KEY IDENTITY(1,1),
    Nombre_Usuario VARCHAR(50) NOT NULL,
    Primer_Apellido VARCHAR(50) NOT NULL,
    Segundo_Apellido VARCHAR(50),
    Email VARCHAR(100) UNIQUE NOT NULL,
    Contraseña VARCHAR(100) NOT NULL,
    Nick VARCHAR(50) UNIQUE NOT NULL,
    Ciudad VARCHAR(100),
    Fecha_Registro DATE NOT NULL,
    Fecha_Nacimiento DATE NOT NULL,
    ID_Faccion INT,
    CONSTRAINT FK_Usuario_Faccion FOREIGN KEY (ID_Faccion) REFERENCES Faccion(ID_Faccion)
);

CREATE TABLE Rango_Torneo (
    ID_Rango INT PRIMARY KEY IDENTITY(1,1),
    Nombre_Rango VARCHAR(100) NOT NULL,
    Puntos_1 INT NOT NULL,
    Puntos_2 INT NOT NULL,
    Puntos_3 INT NOT NULL,
    Puntos_4 INT NOT NULL,
    Puntos_5 INT NOT NULL,
    Puntos_6 INT NOT NULL,
    Puntos_7 INT NOT NULL,
    Puntos_8 INT NOT NULL,
    Puntos_9 INT NOT NULL,
    Puntos_10 INT NOT NULL
);

CREATE TABLE Torneo (
    ID_Torneo INT PRIMARY KEY IDENTITY(1,1),
    ID_Usuario INT NOT NULL,
    Nombre_Torneo VARCHAR(200) NOT NULL,
    Descripcion_Torneo TEXT NOT NULL,
    Limite_Participantes INT,
    Fecha_Inicio_Torneo DATE NOT NULL,
    Fecha_Fin_Torneo DATE NOT NULL,
    Precio_Torneo INT NOT NULL,
    Numero_Partidas INT NOT NULL,
    Puntos_Torneo INT NOT NULL,
    Estado_Torneo VARCHAR(10) CHECK (Estado_Torneo IN ('ESPERANDO', 'LIVE', 'TERMINADO')) NOT NULL,
    Lugar_Torneo VARCHAR(200) NOT NULL,
    Tipo_Torneo VARCHAR(50),
    Es_Privado_Torneo BIT NOT NULL,
    Es_Liga BIT NOT NULL,
    ID_Rango_Torneo INT,
    Es_Matched_Play_Torneo BIT,
    Fecha_Entrega_Listas DATE,
    Fecha_Fin_Inscripcion DATE,
    Bases_Torneo VARBINARY(MAX),
    Cartel_Torneo TEXT,
    Metodos_Pago TEXT,
    Hora_Inicio_Torneo TIME,
    Hora_Fin_Torneo TIME,
    CONSTRAINT FK_Torneo_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT FK_Torneo_Rango_Torneo FOREIGN KEY (ID_Rango_Torneo) REFERENCES Rango_Torneo(ID_Rango)
);

CREATE TABLE Partida_Torneo (
    ID_Partida_Torneo INT PRIMARY KEY IDENTITY(1,1),
    ID_Torneo INT,
    ID_Usuario1 INT,
    ID_Usuario2 INT,
    Resultado_Usuario1 INT,
    Resultado_Usuario2 INT,
    Fecha_Partida DATETIME,
    Es_Matched_Play_Partida BIT,
    Escenario_Partida VARCHAR(150),
    Puntos_Partida INT,
    Ganador_Partida_Torneo INT,
    CONSTRAINT FK_Partida_Torneo_Torneo FOREIGN KEY (ID_Torneo) REFERENCES Torneo(ID_Torneo),
    CONSTRAINT FK_Partida_Torneo_Usuario1 FOREIGN KEY (ID_Usuario1) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT FK_Partida_Torneo_Usuario2 FOREIGN KEY (ID_Usuario2) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT CHK_Partida_Torneo_Resultados CHECK (Resultado_Usuario1 >= 0 AND Resultado_Usuario2 >= 0),
    CONSTRAINT CHK_Partida_Torneo_Ganador CHECK (Ganador_Partida_Torneo IN (ID_Usuario1, ID_Usuario2))
);

CREATE TABLE Clasificacion_Torneo (
    ID_Clasificacion_Torneo INT PRIMARY KEY IDENTITY(1,1),
    ID_Torneo INT,
    ID_Usuario INT,
    Posicion_Final INT,
    Puntos_Torneo INT,
    Puntos_Favor INT,
    Puntos_contra INT,
    Puntos_General INT,
    CONSTRAINT FK_Clasificacion_Torneo_Torneo FOREIGN KEY (ID_Torneo) REFERENCES Torneo(ID_Torneo),
    CONSTRAINT FK_Clasificacion_Torneo_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
);

CREATE TABLE Inscripcion_Torneo (
    ID_Inscripcion INT PRIMARY KEY IDENTITY(1,1),
    ID_Torneo INT,
    ID_Usuario INT,
    Estado_Inscripcion VARCHAR(20) CHECK (Estado_Inscripcion IN ('ESPERANDO CONFIRMACION', 'CONFIRMADA', 'PAGADA')),
    Fecha_Inscripcion DATE,
    Estado_Lista VARCHAR(20) CHECK (Estado_Lista IN ('NO ENTREGADA', 'ENTREGADA', 'ILEGAL', 'OK')),
    Fecha_Entrega_Lista DATE,
    Es_Pago BIT,
    CONSTRAINT FK_Inscripcion_Torneo_Torneo FOREIGN KEY (ID_Torneo) REFERENCES Torneo(ID_Torneo),
    CONSTRAINT FK_Inscripcion_Torneo_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
);

CREATE TABLE Lista (
    ID_Lista INT PRIMARY KEY IDENTITY(1,1),
    ID_Inscripcion INT,
    Lista TEXT,
    Fecha_Entrega DATE,
    CONSTRAINT FK_Lista_Inscripcion FOREIGN KEY (ID_Inscripcion) REFERENCES Inscripcion_Torneo(ID_Inscripcion)
);

CREATE TABLE Comentario (
    ID_Comentario INT PRIMARY KEY IDENTITY(1,1),
    ID_Torneo INT,
    ID_Usuario INT,
    Puntuar_Torneo INT CHECK (Puntuar_Torneo BETWEEN 1 AND 10),
    Comentario TEXT,
    Fecha_Comentario DATE,
    CONSTRAINT FK_Comentario_Torneo FOREIGN KEY (ID_Torneo) REFERENCES Torneo(ID_Torneo),
    CONSTRAINT FK_Comentario_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
);

CREATE TABLE Elo (
    ID_Elo INT PRIMARY KEY IDENTITY(1,1),
    ID_Usuario INT,
    Puntuacion_Elo INT,
    CONSTRAINT FK_Elo_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
);

CREATE TABLE Clasificacion_General (
    ID_Clasificacion INT PRIMARY KEY IDENTITY(1,1),
    ID_Usuario INT,
    Puntuacion_Total INT,
    ID_Puntuacion_Elo INT,
    CONSTRAINT FK_Clasificacion_General_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT FK_Clasificacion_General_Elo FOREIGN KEY (ID_Puntuacion_Elo) REFERENCES Elo(ID_Elo)
);

CREATE TABLE Ronda (
    ID_Ronda INT PRIMARY KEY IDENTITY(1,1),
    ID_Torneo INT,
    Numero_Ronda INT,
    Estado_Ronda VARCHAR(15) CHECK (Estado_Ronda IN ('ESPERANDO', 'LIVE', 'TERMINADA')),
    Fecha_Inicio_Ronda DATE,
    Fecha_Fin_Ronda DATE,
    Hora_Inicio_Ronda TIME,
    Duracion_Ronda INT,
    Escenario_Ronda VARCHAR(100),
    CONSTRAINT FK_Ronda_Torneo FOREIGN KEY (ID_Torneo) REFERENCES Torneo(ID_Torneo)
);

CREATE TABLE Partida_Amistosa (
    ID_Partida_Amistosa INT PRIMARY KEY IDENTITY(1,1),
    ID_Usuario1 INT,
    ID_Usuario2 INT,
    Resultado_Usuario1 INT,
    Resultado_Usuario2 INT,
    Fecha_Partida DATE,
    Es_Matched_Play_Partida BIT,
    Escenario_Partida VARCHAR(100),
    Puntos_Partida INT,
    Ganador_Partida INT,
    Es_Elo BIT,
    CONSTRAINT FK_Partida_Amistosa_Usuario1 FOREIGN KEY (ID_Usuario1) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT FK_Partida_Amistosa_Usuario2 FOREIGN KEY (ID_Usuario2) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT FK_Partida_Amistosa_Ganador FOREIGN KEY (Ganador_Partida) REFERENCES Usuario(ID_Usuario),
    CONSTRAINT CHK_Partida_Amistosa_Resultados CHECK (Resultado_Usuario1 >= 0 AND Resultado_Usuario2 >= 0),
    CONSTRAINT CHK_Partida_Amistosa_Ganador CHECK (Ganador_Partida IN (ID_Usuario1, ID_Usuario2))
);

CREATE TABLE Historico_Login (
    ID_Login INT PRIMARY KEY IDENTITY(1,1),
    ID_Usuario INT,
    Fecha_Login DATE,
    CONSTRAINT FK_Historico_Login_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
);

ALTER TABLE Usuario ADD Telefono VARCHAR(9);

ALTER TABLE Historico_Login ALTER COLUMN Fecha_Login DATE;

ALTER TABLE Partida_Torneo ALTER COLUMN Fecha_Partida DATE;

DROP TABLE IF EXISTS Historico_Login;

EXEC sp_rename 'Lista.Lista', 'Lista_Data', 'COLUMN';


INSERT INTO Faccion (Nombre_Faccion) VALUES
('Domadores de Mumaks'),
('Los Cazadores de Alicante'),
('La Legion del Turia');

Select * from faccion;

INSERT INTO Usuario (Nombre_Usuario, Primer_Apellido, Segundo_Apellido, Email, Contraseña, Nick, Ciudad, Fecha_Registro, Fecha_Nacimiento, ID_Faccion) VALUES
('Jose Antonio', 'Sanchez', 'Molina', 'jose.a.sanchez.molina@gmail.com', '29101988aA..', 'SanUchiha', 'Murcia', '2024-04-20', '1988-10-29', 1),
('Hector', 'Valor', '', 'hector@example.com', 'hector@example.com', 'Dym21', 'Alicante', '2024-04-21', '1985-09-10', 2),
('Manuel', 'Torrekampeño', '', 'Manuel@example.com', 'Manuel@example.com', 'Torre', 'Jaen', '2024-04-22', '1998-11-25', 3);

Select * from usuario;

INSERT INTO Rango_Torneo (Nombre_Rango, Puntos_1, Puntos_2, Puntos_3, Puntos_4, Puntos_5, Puntos_6, Puntos_7, Puntos_8, Puntos_9, Puntos_10) VALUES
('Oro', 1000, 900, 800, 700, 600, 500, 400, 300, 200, 100),
('Plata', 900, 800, 700, 600, 500, 400, 300, 200, 100, 50),
('Bronce', 800, 700, 600, 500, 400, 300, 200, 100, 50, 25);

Select * from rango_torneo;
-- Supongamos que ya existen 5 usuarios con IDs del 1 al 5 y 5 rangos de torneo con IDs del 1 al 5
-- Aquí se insertarán 5 torneos de muestra

INSERT INTO Torneo (
    ID_Usuario, Nombre_Torneo, Descripcion_Torneo, Limite_Participantes, Fecha_Inicio_Torneo, Fecha_Fin_Torneo, 
    Precio_Torneo, Numero_Partidas, Puntos_Torneo, Estado_Torneo, Lugar_Torneo, Tipo_Torneo, 
    Es_Privado_Torneo, Es_Liga, ID_Rango_Torneo, Es_Matched_Play_Torneo, Fecha_Entrega_Listas, 
    Fecha_Fin_Inscripcion, Bases_Torneo, Cartel_Torneo, Metodos_Pago, Hora_Inicio_Torneo, Hora_Fin_Torneo
) VALUES
(1, 'Torneo de Primavera', 'Un torneo emocionante para dar la bienvenida a la primavera.', 32, '2024-06-01', '2024-06-02', 
 50, 5, 100, 'Esperando', 'Parque Central', 'Eliminación', 0, 1, 1, 1, '2024-05-15', 
 '2024-05-20', 0x, 'Cartel de Primavera', 'PayPal, Transferencia', '10:00:00', '18:00:00'),
(2, 'Torneo Veraniego', 'El torneo más caliente del verano.', 16, '2024-07-10', '2024-07-11', 
 75, 3, 80, 'Esperando', 'Playa Norte', 'Round Robin', 1, 0, 2, 0, '2024-06-30', 
 '2024-07-05', 0x, 'Cartel Veraniego', 'Tarjeta, Efectivo', '09:00:00', '17:00:00'),
(3, 'Torneo Otoñal', 'Un torneo para celebrar la llegada del otoño.', 24, '2024-09-20', '2024-09-21', 
 60, 4, 90, 'Esperando', 'Bosque Encantado', 'Eliminación', 0, 1, 3, 1, '2024-09-05', 
 '2024-09-10', 0x, 'Cartel Otoñal', 'PayPal, Tarjeta', '11:00:00', '19:00:00'),
(1, 'Torneo Invernal', 'Un torneo frío pero emocionante.', 20, '2024-12-05', '2024-12-06', 
 100, 6, 120, 'Esperando', 'Montaña Nevada', 'Round Robin', 1, 0, 1, 0, '2024-11-20', 
 '2024-11-25', 0x, 'Cartel Invernal', 'Efectivo, Transferencia', '08:00:00', '16:00:00'),
(2, 'Torneo de Año Nuevo', 'El primer torneo del año.', 40, '2025-01-15', '2025-01-16', 
 120, 7, 150, 'Esperando', 'Estadio Central', 'Eliminación', 0, 1,1, 1, '2025-01-01', 
 '2025-01-05', 0x, 'Cartel de Año Nuevo', 'PayPal, Efectivo', '10:00:00', '20:00:00');

Select * from Torneo;

-- Supongamos que ya existen 3 usuarios con IDs 1, 2 y 3

INSERT INTO Partida_Amistosa (
    ID_Usuario1, ID_Usuario2, Resultado_Usuario1, Resultado_Usuario2, 
    Fecha_Partida, Es_Matched_Play_Partida, Escenario_Partida, 
    Puntos_Partida, Ganador_Partida, Es_Elo
) VALUES
(1, 2, 30, 25, '2024-05-01', 1, 'Escenario 1', 5, 1, 1),
(2, 3, 28, 30, '2024-05-02', 0, 'Escenario 2', 5, 3, 0),
(3, 1, 22, 20, '2024-05-03', 1, 'Escenario 3', 5, 3, 1),
(1, 3, 25, 18, '2024-05-04', 0, 'Escenario 4', 5, 1, 0),
(2, 1, 30, 28, '2024-05-05', 1, 'Escenario 5', 5, 2, 1),
(3, 2, 27, 25, '2024-05-06', 0, 'Escenario 6', 5, 3, 0),
(1, 2, 24, 22, '2024-05-07', 1, 'Escenario 7', 5, 1, 1),
(2, 3, 29, 27, '2024-05-08', 0, 'Escenario 8', 5, 2, 0),
(3, 1, 21, 19, '2024-05-09', 1, 'Escenario 9', 5, 3, 1),
(1, 3, 26, 23, '2024-05-10', 0, 'Escenario 10', 5, 1, 0);

select * from Partida_Amistosa;