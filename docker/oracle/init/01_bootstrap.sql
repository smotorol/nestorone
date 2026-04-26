WHENEVER OSERROR EXIT FAILURE ROLLBACK;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;

SET DEFINE OFF;
SET SERVEROUTPUT ON;
SET ECHO ON;

ALTER SESSION SET CONTAINER = FREEPDB1;
ALTER SESSION SET CURRENT_SCHEMA = APP_USER;

PROMPT [BOOTSTRAP] Running 01_tables.sql
@@/opt/oracle/scripts/app-db/schema/01_tables.sql
PROMPT [BOOTSTRAP] Running 02_sequences.sql
@@/opt/oracle/scripts/app-db/schema/02_sequences.sql
PROMPT [BOOTSTRAP] Running 03_indexes.sql
@@/opt/oracle/scripts/app-db/schema/03_indexes.sql
PROMPT [BOOTSTRAP] Running 04_partitioning.sql
@@/opt/oracle/scripts/app-db/schema/04_partitioning.sql

PROMPT Bootstrap completed successfully.
EXIT SUCCESS;
