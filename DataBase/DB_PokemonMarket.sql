CREATE DATABASE IF NOT EXISTS PokemonMarket;
USE PokemonMarket;


CREATE TABLE Usuario (
    ID_Usuarios INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Contraseña VARCHAR(255) NOT NULL, 
    Correo VARCHAR(150) NOT NULL UNIQUE,
    Telefono VARCHAR(20),
    Estado TINYINT DEFAULT 1, 
    Rol VARCHAR(20) DEFAULT 'Usuario',
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    Calificacion DECIMAL(3,2) DEFAULT 0.00,
    IMG_Perfil VARCHAR(500)
);


CREATE TABLE Inventario (
    ID_Item INT AUTO_INCREMENT PRIMARY KEY,
    ID_Usuarios INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL, -- Nombre de la carta
    Estado VARCHAR(50), -- Mint, Played, etc.
    Rareza VARCHAR(50),
    Edicion VARCHAR(100),
    IMG_Link VARCHAR(500), -- Link de la PokeAPI
    CONSTRAINT FK_Inventario_Usuario FOREIGN KEY (ID_Usuarios) 
        REFERENCES Usuario(ID_Usuarios) ON DELETE CASCADE
);


CREATE TABLE Guardados (
    ID_Lista INT AUTO_INCREMENT PRIMARY KEY,
    ID_Usuario INT NOT NULL,
    ID_Item INT NOT NULL,
    Fecha_Guardado DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Guardados_Usuario FOREIGN KEY (ID_Usuario) 
        REFERENCES Usuario(ID_Usuarios) ON DELETE CASCADE,
    CONSTRAINT FK_Guardados_Inventario FOREIGN KEY (ID_Item) 
        REFERENCES Inventario(ID_Item) ON DELETE CASCADE
);


CREATE TABLE Mensajes (
    ID_Mensaje INT AUTO_INCREMENT PRIMARY KEY,
    ID_Remitente INT NOT NULL,
    ID_Destinatario INT NOT NULL,
    ID_Item INT, -- Para saber por qué carta preguntan
    Texto TEXT NOT NULL,
    Estado TINYINT(1) DEFAULT 0, -- 0: No leído, 1: Leído
    Fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Mensaje_Remitente FOREIGN KEY (ID_Remitente) 
        REFERENCES Usuario(ID_Usuarios),
    CONSTRAINT FK_Mensaje_Destinatario FOREIGN KEY (ID_Destinatario) 
        REFERENCES Usuario(ID_Usuarios),
    CONSTRAINT FK_Mensaje_Item FOREIGN KEY (ID_Item) 
        REFERENCES Inventario(ID_Item) ON DELETE SET NULL
);


CREATE TABLE Reseñas (
    Reseña_id INT AUTO_INCREMENT PRIMARY KEY,
    ID_Usuario INT NOT NULL, -- Quién escribe
    ID_Item INT NOT NULL,    -- Qué carta/vendedor califica
    Texto TEXT,
    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Reseña_Usuario FOREIGN KEY (ID_Usuario) 
        REFERENCES Usuario(ID_Usuarios),
    CONSTRAINT FK_Reseña_Item FOREIGN KEY (ID_Item) 
        REFERENCES Inventario(ID_Item) ON DELETE CASCADE
);


CREATE TABLE Notificaciones (
    ID_Notificaciones INT AUTO_INCREMENT PRIMARY KEY,
    ID_User_Destinatario INT NOT NULL,
    Asunto VARCHAR(100),
    Contenido TEXT,
    Leido TINYINT(1) DEFAULT 0,
    Fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Notificacion_Usuario FOREIGN KEY (ID_User_Destinatario) 
        REFERENCES Usuario(ID_Usuarios) ON DELETE CASCADE
);

-- 7. Tabla Top Registros (Como tabla de caché)
CREATE TABLE Top_Registros (
    ID_Top INT AUTO_INCREMENT PRIMARY KEY,
    ID_Item INT NOT NULL,
    Posicion INT,
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Top_Item FOREIGN KEY (ID_Item) 
        REFERENCES Inventario(ID_Item) ON DELETE CASCADE
);