DROP ROLE IF EXISTS poudriereadmin;
CREATE ROLE poudriereadmin WITH
     CREATEDB;
COMMENT ON ROLE poudriereadmin IS E'Owner role for the Poudriere database';

DROP DATABASE IF EXISTS poudrierec2;
CREATE DATABASE poudrierec2
     ENCODING = 'UTF8'
     OWNER = poudriereadmin;

