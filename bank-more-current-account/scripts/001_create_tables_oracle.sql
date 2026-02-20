
WHENEVER SQLERROR CONTINUE

BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE contacorrente (
      idcontacorrente VARCHAR2(37) PRIMARY KEY,
      numero NUMBER(10) NOT NULL UNIQUE,
      nome VARCHAR2(100) NOT NULL,
      ativo NUMBER(1) DEFAULT 0 NOT NULL,
      senha VARCHAR2(100) NOT NULL,
      salt VARCHAR2(100) NOT NULL,
      cpf VARCHAR2(14) NOT NULL UNIQUE,
      CONSTRAINT chk_contacorrente_ativo CHECK (ativo IN (0, 1))
    )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955) THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE movimento (
      idmovimento VARCHAR2(37) PRIMARY KEY,
      idcontacorrente VARCHAR2(37) NOT NULL,
      datamovimento VARCHAR2(25) NOT NULL,
      tipomovimento VARCHAR2(1) NOT NULL,
      valor NUMBER(18,2) NOT NULL,
      CONSTRAINT fk_movimento_conta FOREIGN KEY (idcontacorrente) REFERENCES contacorrente(idcontacorrente),
      CONSTRAINT chk_movimento_tipo CHECK (tipomovimento IN (''C'', ''D''))
    )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955) THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX idx_movimento_idcontacorrente ON movimento(idcontacorrente)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955, -1408) THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE idempotencia_kafka (
      message_id VARCHAR2(255) PRIMARY KEY,
      processed_at TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL
    )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955) THEN RAISE; END IF;
END;
/

WHENEVER SQLERROR EXIT 1
