-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: inmobiliariaulp
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `contratos`
--

DROP TABLE IF EXISTS `contratos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `contratos` (
  `id_contrato` int NOT NULL AUTO_INCREMENT,
  `id_inmueble` int NOT NULL,
  `id_inquilino` int NOT NULL,
  `id_usuario` int NOT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_fin` date NOT NULL,
  `monto_mensual` decimal(10,0) NOT NULL,
  `fecha_finalizacion_anticipada` date DEFAULT NULL,
  `multa` decimal(10,0) DEFAULT NULL,
  `estado` enum('vigente','finalizado','rescindido') NOT NULL,
  PRIMARY KEY (`id_contrato`),
  KEY `fk_Contrato_Inquilino1_idx` (`id_inquilino`),
  KEY `fk_Contrato_Inmueble1_idx` (`id_inmueble`),
  KEY `fk_contratos_usuarios1_idx` (`id_usuario`),
  CONSTRAINT `fk_Contrato_Inmueble1` FOREIGN KEY (`id_inmueble`) REFERENCES `inmuebles` (`id_inmueble`),
  CONSTRAINT `fk_Contrato_Inquilino1` FOREIGN KEY (`id_inquilino`) REFERENCES `inquilinos` (`id_inquilino`),
  CONSTRAINT `fk_contratos_usuarios1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contratos`
--

LOCK TABLES `contratos` WRITE;
/*!40000 ALTER TABLE `contratos` DISABLE KEYS */;
/*!40000 ALTER TABLE `contratos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `empleados`
--

DROP TABLE IF EXISTS `empleados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empleados` (
  `id_empleado` int NOT NULL AUTO_INCREMENT,
  `id_persona` int NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_empleado`),
  KEY `fk_Empleado_Persona1_idx` (`id_persona`),
  CONSTRAINT `fk_Empleado_Persona1` FOREIGN KEY (`id_persona`) REFERENCES `personas` (`id_persona`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empleados`
--

LOCK TABLES `empleados` WRITE;
/*!40000 ALTER TABLE `empleados` DISABLE KEYS */;
/*!40000 ALTER TABLE `empleados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inmuebles`
--

DROP TABLE IF EXISTS `inmuebles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inmuebles` (
  `id_inmueble` int NOT NULL AUTO_INCREMENT,
  `direccion` varchar(200) NOT NULL,
  `uso` enum('comercial','residencial') NOT NULL,
  `ambientes` int NOT NULL,
  `coordenadas` varchar(150) NOT NULL,
  `precio_base` decimal(10,0) NOT NULL,
  `estado` tinyint NOT NULL,
  `id_propietario` int NOT NULL,
  `id_tipo` int NOT NULL,
  PRIMARY KEY (`id_inmueble`),
  UNIQUE KEY `direccion_UNIQUE` (`direccion`),
  UNIQUE KEY `coordenadas_UNIQUE` (`coordenadas`),
  KEY `fk_Inmueble_Propietario1_idx` (`id_propietario`),
  KEY `fk_Inmueble_Tipo1_idx` (`id_tipo`),
  CONSTRAINT `fk_Inmueble_Propietario1` FOREIGN KEY (`id_propietario`) REFERENCES `propietarios` (`id_propietario`),
  CONSTRAINT `fk_Inmueble_Tipo1` FOREIGN KEY (`id_tipo`) REFERENCES `tipos` (`id_tipo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inmuebles`
--

LOCK TABLES `inmuebles` WRITE;
/*!40000 ALTER TABLE `inmuebles` DISABLE KEYS */;
/*!40000 ALTER TABLE `inmuebles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inquilinos`
--

DROP TABLE IF EXISTS `inquilinos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inquilinos` (
  `id_inquilino` int NOT NULL AUTO_INCREMENT,
  `id_persona` int NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_inquilino`),
  KEY `fk_inquilinos_personas1_idx` (`id_persona`),
  CONSTRAINT `fk_inquilinos_personas1` FOREIGN KEY (`id_persona`) REFERENCES `personas` (`id_persona`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inquilinos`
--

LOCK TABLES `inquilinos` WRITE;
/*!40000 ALTER TABLE `inquilinos` DISABLE KEYS */;
INSERT INTO `inquilinos` VALUES (1,1,0),(2,3,1),(4,2,1),(5,4,1),(6,5,1),(7,6,1),(8,9,1),(9,10,1),(10,8,1),(11,33,1),(12,34,1),(13,31,1),(14,21,1),(15,12,1),(16,13,1),(17,14,1),(18,15,1),(19,16,1),(20,17,1),(21,19,1),(22,26,1),(23,25,1),(24,27,1),(25,28,1),(26,29,1);
/*!40000 ALTER TABLE `inquilinos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pagos`
--

DROP TABLE IF EXISTS `pagos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pagos` (
  `id_pago` int NOT NULL AUTO_INCREMENT,
  `id_contrato` int NOT NULL,
  `id_usuario` int NOT NULL,
  `fecha_pago` date NOT NULL,
  `numero_pago` varchar(45) NOT NULL,
  `importe` decimal(10,0) NOT NULL,
  `concepto` varchar(100) NOT NULL,
  `estadoPago` enum('aprobado','anulado') NOT NULL,
  PRIMARY KEY (`id_pago`),
  KEY `fk_Pago_Contrato_idx` (`id_contrato`),
  KEY `fk_pagos_usuarios1_idx` (`id_usuario`),
  CONSTRAINT `fk_Pago_Contrato` FOREIGN KEY (`id_contrato`) REFERENCES `contratos` (`id_contrato`),
  CONSTRAINT `fk_pagos_usuarios1` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pagos`
--

LOCK TABLES `pagos` WRITE;
/*!40000 ALTER TABLE `pagos` DISABLE KEYS */;
/*!40000 ALTER TABLE `pagos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personas`
--

DROP TABLE IF EXISTS `personas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personas` (
  `id_persona` int NOT NULL AUTO_INCREMENT,
  `dni` varchar(45) NOT NULL,
  `apellido` varchar(45) NOT NULL,
  `nombre` varchar(45) NOT NULL,
  `telefono` varchar(45) NOT NULL,
  `email` varchar(45) NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_persona`),
  UNIQUE KEY `dni_UNIQUE` (`dni`),
  UNIQUE KEY `email_UNIQUE` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personas`
--

LOCK TABLES `personas` WRITE;
/*!40000 ALTER TABLE `personas` DISABLE KEYS */;
INSERT INTO `personas` VALUES (1,'35475532','Soria','Cristian ','26644','correo@correo.com',1),(2,'43216348','Gonzalez','Joniti','2554','correo2@correo.com',0),(3,'43214434','Di Gangi','Yamila Chola','2543','lachola@correo.com',1),(4,'21372783','Gutierrez','Romina','2664 3536528','correo4@correo.com',1),(5,'12345678','González','María','2664001001','maria.gonzalez@example.com',1),(6,'23456789','Pérez','Juan','2664001002','juan.perez@example.com',0),(7,'34567890','Rodríguez','Lucía','2664001003','lucia.rodriguez@example.com',1),(8,'45678901','Fernández','Carlos','2664001004','carlos.fernandez@example.com',1),(9,'56789012','López','Ana','2664001005','ana.lopez@example.com',1),(10,'67890123','Martínez','Diego','2664001006','diego.martinez@example.com',1),(11,'78901234','Gómez','Sofía','2664001007','sofia.gomez@example.com',1),(12,'89012345','Díaz','Javier','2664001008','javier.diaz@example.com',1),(13,'90123456','Torres','Laura','2664001009','laura.torres@example.com',1),(14,'11223344','Sánchez','Mateo','2664001010','mateo.sanchez@example.com',1),(15,'22334455','Romero','Valentina','2664001011','valentina.romero@example.com',1),(16,'33445566','Álvarez','Tomás','2664001012','tomas.alvarez@example.com',1),(17,'44556677','Ruiz','Camila','2664001013','camila.ruiz@example.com',1),(18,'55667788','Ramírez','Nicolás','2664001014','nicolas.ramirez@example.com',1),(19,'66778899','Flores','Julieta','2664001015','julieta.flores@example.com',1),(20,'77889900','Acosta','Ignacio','2664001016','ignacio.acosta@example.com',1),(21,'88990011','Benítez','Martina','2664001017','martina.benitez@example.com',1),(22,'99001122','Molina','Facundo','2664001018','facundo.molina@example.com',1),(23,'10111213','Castro','Victoria','2664001019','victoria.castro@example.com',1),(24,'12131415','Ortiz','Gabriel','2664001020','gabriel.ortiz@example.com',1),(25,'13141516','Silva','Paula','2664001021','paula.silva@example.com',1),(26,'14151617','Suárez','Leandro','2664001022','leandro.suarez@example.com',1),(27,'15161718','Herrera','Agustina','2664001023','agustina.herrera@example.com',1),(28,'16171819','Medina','Emiliano','2664001024','emiliano.medina@example.com',1),(29,'17181920','Castillo','Florencia','2664001025','florencia.castillo@example.com',1),(30,'18192021','Ramos','Federico','2664001026','federico.ramos@example.com',1),(31,'19202122','Moreno','Pilar','2664001027','pilar.moreno@example.com',1),(32,'20212223','Aguilar','Santiago','2664001028','santiago.aguilar@example.com',1),(33,'21222324','Vega','Bianca','2664001029','bianca.vega@example.com',1),(34,'22232425','Jiménez','Sebastián','2664001030','sebastian.jimenez@example.com',1);
/*!40000 ALTER TABLE `personas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `propietarios`
--

DROP TABLE IF EXISTS `propietarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `propietarios` (
  `id_propietario` int NOT NULL AUTO_INCREMENT,
  `id_persona` int NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_propietario`),
  KEY `fk_Propietario_Persona1_idx` (`id_persona`),
  CONSTRAINT `fk_Propietario_Persona1` FOREIGN KEY (`id_persona`) REFERENCES `personas` (`id_persona`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `propietarios`
--

LOCK TABLES `propietarios` WRITE;
/*!40000 ALTER TABLE `propietarios` DISABLE KEYS */;
INSERT INTO `propietarios` VALUES (1,1,1),(2,2,1),(3,3,0),(6,4,1),(7,6,1),(8,7,1),(9,9,1),(10,8,1),(11,33,1),(12,32,1),(13,31,1),(14,11,1),(15,14,1),(16,16,1),(17,17,1),(18,18,1),(19,20,1),(20,22,1),(21,23,1),(22,24,1),(23,25,1),(24,28,1),(25,30,1);
/*!40000 ALTER TABLE `propietarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos`
--

DROP TABLE IF EXISTS `tipos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipos` (
  `id_tipo` int NOT NULL AUTO_INCREMENT,
  `descripcion` varchar(45) NOT NULL,
  PRIMARY KEY (`id_tipo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos`
--

LOCK TABLES `tipos` WRITE;
/*!40000 ALTER TABLE `tipos` DISABLE KEYS */;
/*!40000 ALTER TABLE `tipos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id_usuario` int NOT NULL AUTO_INCREMENT,
  `id_empleado` int NOT NULL,
  `password` varchar(45) NOT NULL,
  `rol` enum('administrador','empleado') NOT NULL,
  `avatar` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id_usuario`),
  KEY `fk_usuarios_empleados1_idx` (`id_empleado`),
  CONSTRAINT `fk_usuarios_empleados1` FOREIGN KEY (`id_empleado`) REFERENCES `empleados` (`id_empleado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-09-05 15:42:24
