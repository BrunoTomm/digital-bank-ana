
WHENEVER SQLERROR CONTINUE

BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE transferencia (
      idtransferencia VARCHAR2(37) PRIMARY KEY,
      idcontacorrente_origem VARCHAR2(37) NOT NULL,
      idcontacorrente_destino VARCHAR2(37) NOT NULL,
      datamovimento VARCHAR2(25) NOT NULL,
      valor NUMBER(18,2) NOT NULL,
      CONSTRAINT fk_transf_conta_origem FOREIGN KEY (idcontacorrente_origem) REFERENCES contacorrente(idcontacorrente),
      CONSTRAINT fk_transf_conta_destino FOREIGN KEY (idcontacorrente_destino) REFERENCES contacorrente(idcontacorrente),
      CONSTRAINT chk_transf_contas_diferentes CHECK (idcontacorrente_origem <> idcontacorrente_destino)
    )';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955) THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX idx_transf_origem ON transferencia(idcontacorrente_origem)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955, -1408) THEN RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'CREATE INDEX idx_transf_destino ON transferencia(idcontacorrente_destino)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE NOT IN (-955, -1408) THEN RAISE; END IF;
END;
/

WHENEVER SQLERROR EXIT 1
