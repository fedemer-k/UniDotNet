-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema inmobiliariaulp
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema inmobiliariaulp
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `inmobiliariaulp` DEFAULT CHARACTER SET utf8 ;
USE `inmobiliariaulp` ;

-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`personas`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`personas` (
  `id_persona` INT NOT NULL AUTO_INCREMENT,
  `dni` VARCHAR(45) NOT NULL,
  `apellido` VARCHAR(45) NOT NULL,
  `nombre` VARCHAR(45) NOT NULL,
  `telefono` VARCHAR(45) NOT NULL,
  `email` VARCHAR(45) NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`id_persona`),
  UNIQUE INDEX `dni_UNIQUE` (`dni` ASC),
  UNIQUE INDEX `email_UNIQUE` (`email` ASC))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`propietarios`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`propietarios` (
  `id_propietario` INT NOT NULL AUTO_INCREMENT,
  `id_persona` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`id_propietario`),
  INDEX `fk_Propietario_Persona1_idx` (`id_persona` ASC),
  CONSTRAINT `fk_Propietario_Persona1`
    FOREIGN KEY (`id_persona`)
    REFERENCES `inmobiliariaulp`.`personas` (`id_persona`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`tipos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`tipos` (
  `id_tipo` INT NOT NULL AUTO_INCREMENT,
  `descripcion` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`id_tipo`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`inmuebles`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`inmuebles` (
  `id_inmueble` INT NOT NULL AUTO_INCREMENT,
  `direccion` VARCHAR(200) NOT NULL,
  `uso` ENUM('comercial', 'residencial') NOT NULL,
  `ambientes` INT NOT NULL,
  `coordenadas` VARCHAR(150) NOT NULL,
  `precio_base` DECIMAL NOT NULL,
  `estado` TINYINT NOT NULL,
  `id_propietario` INT NOT NULL,
  `id_tipo` INT NOT NULL,
  PRIMARY KEY (`id_inmueble`),
  INDEX `fk_Inmueble_Propietario1_idx` (`id_propietario` ASC),
  INDEX `fk_Inmueble_Tipo1_idx` (`id_tipo` ASC),
  UNIQUE INDEX `direccion_UNIQUE` (`direccion` ASC),
  UNIQUE INDEX `coordenadas_UNIQUE` (`coordenadas` ASC),
  CONSTRAINT `fk_Inmueble_Propietario1`
    FOREIGN KEY (`id_propietario`)
    REFERENCES `inmobiliariaulp`.`propietarios` (`id_propietario`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Inmueble_Tipo1`
    FOREIGN KEY (`id_tipo`)
    REFERENCES `inmobiliariaulp`.`tipos` (`id_tipo`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`empleados`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`empleados` (
  `id_empleado` INT NOT NULL AUTO_INCREMENT,
  `id_persona` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`id_empleado`),
  INDEX `fk_Empleado_Persona1_idx` (`id_persona` ASC),
  CONSTRAINT `fk_Empleado_Persona1`
    FOREIGN KEY (`id_persona`)
    REFERENCES `inmobiliariaulp`.`personas` (`id_persona`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`usuarios`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`usuarios` (
  `id_usuario` INT NOT NULL AUTO_INCREMENT,
  `id_empleado` INT NOT NULL,
  `password` VARCHAR(45) NOT NULL,
  `rol` ENUM('administrador', 'empleado') NOT NULL,
  `avatar` VARCHAR(45) NULL,
  PRIMARY KEY (`id_usuario`),
  INDEX `fk_usuarios_empleados1_idx` (`id_empleado` ASC),
  CONSTRAINT `fk_usuarios_empleados1`
    FOREIGN KEY (`id_empleado`)
    REFERENCES `inmobiliariaulp`.`empleados` (`id_empleado`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`inquilinos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`inquilinos` (
  `id_inquilino` INT NOT NULL AUTO_INCREMENT,
  `id_persona` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`id_inquilino`),
  INDEX `fk_inquilinos_personas1_idx` (`id_persona` ASC),
  CONSTRAINT `fk_inquilinos_personas1`
    FOREIGN KEY (`id_persona`)
    REFERENCES `inmobiliariaulp`.`personas` (`id_persona`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`contratos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`contratos` (
  `id_contrato` INT NOT NULL AUTO_INCREMENT,
  `id_inmueble` INT NOT NULL,
  `id_inquilino` INT NOT NULL,
  `id_usuario` INT NOT NULL,
  `fecha_inicio` DATE NOT NULL,
  `fecha_fin` DATE NOT NULL,
  `monto_mensual` DECIMAL NOT NULL,
  `fecha_finalizacion_anticipada` DATE NULL,
  `multa` DECIMAL NULL,
  `estado` ENUM('vigente', 'finalizado', 'rescindido') NOT NULL,
  PRIMARY KEY (`id_contrato`),
  INDEX `fk_Contrato_Inquilino1_idx` (`id_inquilino` ASC),
  INDEX `fk_Contrato_Inmueble1_idx` (`id_inmueble` ASC),
  INDEX `fk_contratos_usuarios1_idx` (`id_usuario` ASC),
  CONSTRAINT `fk_Contrato_Inquilino1`
    FOREIGN KEY (`id_inquilino`)
    REFERENCES `inmobiliariaulp`.`inquilinos` (`id_inquilino`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Contrato_Inmueble1`
    FOREIGN KEY (`id_inmueble`)
    REFERENCES `inmobiliariaulp`.`inmuebles` (`id_inmueble`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_contratos_usuarios1`
    FOREIGN KEY (`id_usuario`)
    REFERENCES `inmobiliariaulp`.`usuarios` (`id_usuario`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`pagos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`pagos` (
  `id_pago` INT NOT NULL AUTO_INCREMENT,
  `id_contrato` INT NOT NULL,
  `id_usuario` INT NOT NULL,
  `fecha_pago` DATE NOT NULL,
  `numero_pago` VARCHAR(45) NOT NULL,
  `importe` DECIMAL NOT NULL,
  `concepto` VARCHAR(100) NOT NULL,
  `estadoPago` ENUM('aprobado', 'anulado') NOT NULL,
  PRIMARY KEY (`id_pago`),
  INDEX `fk_Pago_Contrato_idx` (`id_contrato` ASC),
  INDEX `fk_pagos_usuarios1_idx` (`id_usuario` ASC),
  CONSTRAINT `fk_Pago_Contrato`
    FOREIGN KEY (`id_contrato`)
    REFERENCES `inmobiliariaulp`.`contratos` (`id_contrato`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_pagos_usuarios1`
    FOREIGN KEY (`id_usuario`)
    REFERENCES `inmobiliariaulp`.`usuarios` (`id_usuario`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
