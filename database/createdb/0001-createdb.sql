DROP DATABASE IF EXISTS poudrierec2;
DROP ROLE IF EXISTS poudriereadmin;

CREATE ROLE poudriereadmin WITH
     CREATEDB;
COMMENT ON ROLE poudriereadmin IS E'Owner role for the Poudriere database';
GRANT poudriereadmin to CURRENT_USER;

CREATE DATABASE poudrierec2
     ENCODING = 'UTF8'
     OWNER = poudriereadmin;
