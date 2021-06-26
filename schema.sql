-- Database generated with pgModeler (PostgreSQL Database Modeler).
-- pgModeler  version: 0.9.3
-- PostgreSQL version: 12.0
-- Project Site: pgmodeler.io
-- Model Author: ---
-- -- object: bsa3 | type: ROLE --
-- -- DROP ROLE IF EXISTS bsa3;
-- CREATE ROLE bsa3 WITH 
-- 	CREATEROLE;
-- -- ddl-end --
-- 
-- object: poudriereadmin | type: ROLE --
-- DROP ROLE IF EXISTS poudriereadmin;
CREATE ROLE poudriereadmin WITH 
	CREATEDB;
-- ddl-end --
COMMENT ON ROLE poudriereadmin IS E'Owner role for the Poudriere database';
-- ddl-end --


-- Database creation must be performed outside a multi lined SQL file. 
-- These commands were put in this file only as a convenience.
-- 
-- object: poudrierec2 | type: DATABASE --
-- DROP DATABASE IF EXISTS poudrierec2;
CREATE DATABASE poudrierec2
	ENCODING = 'UTF8';
-- ddl-end --


SET check_function_bodies = false;
-- ddl-end --

-- object: poudrierec2 | type: SCHEMA --
-- DROP SCHEMA IF EXISTS poudrierec2 CASCADE;
CREATE SCHEMA poudrierec2;
-- ddl-end --
ALTER SCHEMA poudrierec2 OWNER TO poudriereadmin;
-- ddl-end --

SET search_path TO pg_catalog,public,poudrierec2;
-- ddl-end --

-- object: poudrierec2.jobconfigs | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.jobconfigs CASCADE;
CREATE TABLE poudrierec2.jobconfigs (
	id uuid NOT NULL DEFAULT gen_random_uuid(),
	title text NOT NULL,
	porttree text NOT NULL,
	portset text NOT NULL,
	jail text NOT NULL,
	CONSTRAINT configs_pk PRIMARY KEY (id)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.jobconfigs IS E'Run configurations';
-- ddl-end --
ALTER TABLE poudrierec2.jobconfigs OWNER TO postgres;
-- ddl-end --

-- object: poudrierec2.configoptions | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.configoptions CASCADE;
CREATE TABLE poudrierec2.configoptions (
	configfile uuid NOT NULL,
	name text NOT NULL,
	value text NOT NULL,
	CONSTRAINT configoptions_pk PRIMARY KEY (configfile,name)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.configoptions IS E'The options set in a configuration';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.configoptions.configfile IS E'The configuration this references.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.configoptions.name IS E'The name of the option to set';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.configoptions.value IS E'The value of the option to set';
-- ddl-end --
ALTER TABLE poudrierec2.configoptions OWNER TO postgres;
-- ddl-end --

-- object: poudrierec2.packageoptions | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.packageoptions CASCADE;
CREATE TABLE poudrierec2.packageoptions (
	configfile uuid NOT NULL,
	category text,
	package text,
	set text[] NOT NULL,
	unset text[] NOT NULL,
	CONSTRAINT pkgoptions_valid_spec CHECK ((category IS NULL AND package IS NULL) OR
(category IS NOT NULL))

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.packageoptions IS E'Package options set';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.packageoptions.category IS E'The category to which these options apply';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.packageoptions.package IS E'The package name to which these options apply';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.packageoptions.set IS E'Options that should be set.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.packageoptions.unset IS E'Options that should be unset.';
-- ddl-end --
COMMENT ON CONSTRAINT pkgoptions_valid_spec ON poudrierec2.packageoptions  IS E'package may only be non-null if category is non-null';
-- ddl-end --
ALTER TABLE poudrierec2.packageoptions OWNER TO postgres;
-- ddl-end --

-- object: poudrierec2.availableoptions | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.availableoptions CASCADE;
CREATE TABLE poudrierec2.availableoptions (
	name text NOT NULL,
	description text,
	required bool NOT NULL DEFAULT false,
	defaultvalue text NOT NULL,
	configtype text NOT NULL,
	CONSTRAINT name_valid CHECK (name ~ '^[A-Z0-9_]+$'),
	CONSTRAINT availableoptions_pk PRIMARY KEY (name)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.availableoptions IS E'Options that can be set in poudriere.conf.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.availableoptions.name IS E'Name of the option that can be set.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.availableoptions.description IS E'A freeform description of the configuration option.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.availableoptions.required IS E'Whether the option is required for poudriere to function.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.availableoptions.defaultvalue IS E'The option''s default value.';
-- ddl-end --
ALTER TABLE poudrierec2.availableoptions OWNER TO postgres;
-- ddl-end --

-- object: poudrierec2.builds | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.builds CASCADE;
CREATE TABLE poudrierec2.builds (
	config uuid NOT NULL,
	started timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
	stopped timestamptz,
	vm uuid NOT NULL,
	CONSTRAINT builds_pk PRIMARY KEY (config,started)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.builds IS E'Bulk builds.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.builds.config IS E'The configuration used.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.builds.started IS E'The start time of this build.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.builds.stopped IS E'The stop time of this build.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.builds.vm IS E'The virtual machine assigned to this build.';
-- ddl-end --
ALTER TABLE poudrierec2.builds OWNER TO postgres;
-- ddl-end --

-- object: poudrierec2.virtualmachines | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.virtualmachines CASCADE;
CREATE TABLE poudrierec2.virtualmachines (
	id integer NOT NULL GENERATED ALWAYS AS IDENTITY ,
	azuuid uuid NOT NULL,
	created timestamptz NOT NULL,
	started timestamptz,
	stopped timestamptz,
	deleted timestamptz,
	CONSTRAINT virtualmachines_pk PRIMARY KEY (id),
	CONSTRAINT vm_uuid_unique UNIQUE (azuuid)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.virtualmachines IS E'Virtual machines used by this system.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.virtualmachines.id IS E'A serial number for this VM.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.virtualmachines.azuuid IS E'Azure-generated UUID for this VM.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.virtualmachines.created IS E'Time VM created';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.virtualmachines.started IS E'Time VM started.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.virtualmachines.stopped IS E'Time VM stopped (deallocated)';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.virtualmachines.deleted IS E'Time VM deleted.';
-- ddl-end --
ALTER TABLE poudrierec2.virtualmachines OWNER TO postgres;
-- ddl-end --

-- object: index_vm_azuuid | type: INDEX --
-- DROP INDEX IF EXISTS poudrierec2.index_vm_azuuid CASCADE;
CREATE INDEX index_vm_azuuid ON poudrierec2.virtualmachines
	USING btree
	(
	  azuuid
	);
-- ddl-end --

-- object: poudrierec2.heartbeats | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.heartbeats CASCADE;
CREATE TABLE poudrierec2.heartbeats (
	config uuid NOT NULL,
	started timestamptz NOT NULL,
	loadaverage float[] NOT NULL,
	CONSTRAINT hb_loadavg_length CHECK (ARRAY_LENGTH(loadaverage, 1) = 3)

);
-- ddl-end --
ALTER TABLE poudrierec2.heartbeats OWNER TO poudriereadmin;
-- ddl-end --

-- object: builds_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.heartbeats DROP CONSTRAINT IF EXISTS builds_fk CASCADE;
ALTER TABLE poudrierec2.heartbeats ADD CONSTRAINT builds_fk FOREIGN KEY (config,started)
REFERENCES poudrierec2.builds (config,started) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: poudrierec2.configfiles | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.configfiles CASCADE;
CREATE TABLE poudrierec2.configfiles (
	id uuid NOT NULL,
	deleted bool NOT NULL DEFAULT false,
	name text NOT NULL,
	portset text,
	porttree text,
	jail text,
	configtype text NOT NULL,
	CONSTRAINT configfiles_pk PRIMARY KEY (id),
	CONSTRAINT poudriereconf_no_portset CHECK (configtype <> 'poudriereconf' OR portset IS NULL)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.configfiles IS E'A configuration file that can be used in a job configuration.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.configfiles.name IS E'Human-readable name of this file.';
-- ddl-end --
COMMENT ON CONSTRAINT poudriereconf_no_portset ON poudrierec2.configfiles  IS E'A poudriere.conf file does not have a portset.';
-- ddl-end --
ALTER TABLE poudrierec2.configfiles OWNER TO poudriereadmin;
-- ddl-end --

-- object: poudrierec2.portstrees | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.portstrees CASCADE;
CREATE TABLE poudrierec2.portstrees (
	name text NOT NULL,
	method text NOT NULL,
	url text,
	CONSTRAINT portstrees_pk PRIMARY KEY (name),
	CONSTRAINT portstrees_url_presence CHECK (method = 'null' OR url IS NOT NULL)

);
-- ddl-end --
COMMENT ON COLUMN poudrierec2.portstrees.name IS E'The name of the ports tree';
-- ddl-end --
COMMENT ON CONSTRAINT portstrees_url_presence ON poudrierec2.portstrees  IS E'URL must be present if "method" is not "null".';
-- ddl-end --
ALTER TABLE poudrierec2.portstrees OWNER TO poudriereadmin;
-- ddl-end --

-- object: poudrierec2.portstree_methods | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.portstree_methods CASCADE;
CREATE TABLE poudrierec2.portstree_methods (
	name text NOT NULL,
	isdefault bool NOT NULL DEFAULT false,
	CONSTRAINT porttree_methods_pk PRIMARY KEY (name)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.portstree_methods IS E'Methods available for acquiring a ports tree.';
-- ddl-end --
ALTER TABLE poudrierec2.portstree_methods OWNER TO poudriereadmin;
-- ddl-end --

-- Appended SQL commands --
ALTER TABLE porttree_methods
ADD CONSTRAINT pt_methods_onlyonedefault
EXCLUDE (isdefault WITH =) WHERE (isdefault);
-- ddl-end --

INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'none', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'git', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'git+http', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'git+https', E'true');
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'git+file', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'git+ssh', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'svn', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'svn+http', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'svn+https', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'svn+file', DEFAULT);
-- ddl-end --
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'svn+ssh', DEFAULT);
-- ddl-end --

-- object: portstree_methods_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.portstrees DROP CONSTRAINT IF EXISTS portstree_methods_fk CASCADE;
ALTER TABLE poudrierec2.portstrees ADD CONSTRAINT portstree_methods_fk FOREIGN KEY (method)
REFERENCES poudrierec2.portstree_methods (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: portstrees_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.jobconfigs DROP CONSTRAINT IF EXISTS portstrees_fk CASCADE;
ALTER TABLE poudrierec2.jobconfigs ADD CONSTRAINT portstrees_fk FOREIGN KEY (porttree)
REFERENCES poudrierec2.portstrees (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: configfiles_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.packageoptions DROP CONSTRAINT IF EXISTS configfiles_fk CASCADE;
ALTER TABLE poudrierec2.packageoptions ADD CONSTRAINT configfiles_fk FOREIGN KEY (configfile)
REFERENCES poudrierec2.configfiles (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: poudrierec2.configfile_is_makeconf | type: FUNCTION --
-- DROP FUNCTION IF EXISTS poudrierec2.configfile_is_makeconf() CASCADE;
CREATE FUNCTION poudrierec2.configfile_is_makeconf ()
	RETURNS trigger
	LANGUAGE plpgsql
	VOLATILE 
	CALLED ON NULL INPUT
	SECURITY INVOKER
	COST 1
	AS $$
DECLARE
  configType configfiles.type%TYPE
BEGIN
SELECT c.type
INTO configType
FROM configfiles
WHERE c.id = NEW.id;
IF configType <> 'makeconf' RETURN null;
END

$$;
-- ddl-end --
ALTER FUNCTION poudrierec2.configfile_is_makeconf() OWNER TO poudriereadmin;
-- ddl-end --
COMMENT ON FUNCTION poudrierec2.configfile_is_makeconf() IS E'Validate that the config file referenced by this row is a make.conf.';
-- ddl-end --

-- object: poudrierec2.portsets | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.portsets CASCADE;
CREATE TABLE poudrierec2.portsets (
	name text NOT NULL,
	CONSTRAINT portsets_pk PRIMARY KEY (name)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.portsets IS E'Sets of ports.';
-- ddl-end --
ALTER TABLE poudrierec2.portsets OWNER TO poudriereadmin;
-- ddl-end --

-- object: poudrierec2.portset_members | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.portset_members CASCADE;
CREATE TABLE poudrierec2.portset_members (
	portset text NOT NULL,
	portname text NOT NULL,
	CONSTRAINT portset_members_pk PRIMARY KEY (portset,portname)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.portset_members IS E'Members of a port set';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.portset_members.portname IS E'Origin (with optional flavor) to build.';
-- ddl-end --
ALTER TABLE poudrierec2.portset_members OWNER TO poudriereadmin;
-- ddl-end --

-- object: portsets_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.jobconfigs DROP CONSTRAINT IF EXISTS portsets_fk CASCADE;
ALTER TABLE poudrierec2.jobconfigs ADD CONSTRAINT portsets_fk FOREIGN KEY (portset)
REFERENCES poudrierec2.portsets (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: portsets_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.configfiles DROP CONSTRAINT IF EXISTS portsets_fk CASCADE;
ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT portsets_fk FOREIGN KEY (portset)
REFERENCES poudrierec2.portsets (name) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: poudrierec2.jobconfigs_configfiles | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.jobconfigs_configfiles CASCADE;
CREATE TABLE poudrierec2.jobconfigs_configfiles (
	id_jobconfigs uuid NOT NULL,
	id_configfiles uuid NOT NULL,
	CONSTRAINT jobconfigs_configfiles_pk PRIMARY KEY (id_jobconfigs,id_configfiles)

);
-- ddl-end --

-- object: jobconfigs_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.jobconfigs_configfiles DROP CONSTRAINT IF EXISTS jobconfigs_fk CASCADE;
ALTER TABLE poudrierec2.jobconfigs_configfiles ADD CONSTRAINT jobconfigs_fk FOREIGN KEY (id_jobconfigs)
REFERENCES poudrierec2.jobconfigs (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: configfiles_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.jobconfigs_configfiles DROP CONSTRAINT IF EXISTS configfiles_fk CASCADE;
ALTER TABLE poudrierec2.jobconfigs_configfiles ADD CONSTRAINT configfiles_fk FOREIGN KEY (id_configfiles)
REFERENCES poudrierec2.configfiles (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: portstrees_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.configfiles DROP CONSTRAINT IF EXISTS portstrees_fk CASCADE;
ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT portstrees_fk FOREIGN KEY (porttree)
REFERENCES poudrierec2.portstrees (name) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: poudrierec2.jails | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.jails CASCADE;
CREATE TABLE poudrierec2.jails (
	name text NOT NULL,
	version text NOT NULL,
	architecture text,
	CONSTRAINT jails_pk PRIMARY KEY (name)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.jails IS E'Jail definitions.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.jails.version IS E'The OS version to install in the jail.';
-- ddl-end --
COMMENT ON COLUMN poudrierec2.jails.architecture IS E'The architecture of the jail (e.g. amd64)';
-- ddl-end --
ALTER TABLE poudrierec2.jails OWNER TO poudriereadmin;
-- ddl-end --

-- object: jails_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.configfiles DROP CONSTRAINT IF EXISTS jails_fk CASCADE;
ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT jails_fk FOREIGN KEY (jail)
REFERENCES poudrierec2.jails (name) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;
-- ddl-end --

-- object: jails_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.jobconfigs DROP CONSTRAINT IF EXISTS jails_fk CASCADE;
ALTER TABLE poudrierec2.jobconfigs ADD CONSTRAINT jails_fk FOREIGN KEY (jail)
REFERENCES poudrierec2.jails (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: poudrierec2.configfiletypes | type: TABLE --
-- DROP TABLE IF EXISTS poudrierec2.configfiletypes CASCADE;
CREATE TABLE poudrierec2.configfiletypes (
	name text NOT NULL,
	CONSTRAINT configfiletypes_pk PRIMARY KEY (name)

);
-- ddl-end --
COMMENT ON TABLE poudrierec2.configfiletypes IS E'Types of configuration files that can be used by Poudriere.';
-- ddl-end --
ALTER TABLE poudrierec2.configfiletypes OWNER TO poudriereadmin;
-- ddl-end --

INSERT INTO poudrierec2.configfiletypes (name) VALUES (E'poudriereconf');
-- ddl-end --
INSERT INTO poudrierec2.configfiletypes (name) VALUES (E'makeconf');
-- ddl-end --
INSERT INTO poudrierec2.configfiletypes (name) VALUES (E'srcconf');
-- ddl-end --

-- object: configfiletypes_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.configfiles DROP CONSTRAINT IF EXISTS configfiletypes_fk CASCADE;
ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT configfiletypes_fk FOREIGN KEY (configtype)
REFERENCES poudrierec2.configfiletypes (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: configfiletypes_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.availableoptions DROP CONSTRAINT IF EXISTS configfiletypes_fk CASCADE;
ALTER TABLE poudrierec2.availableoptions ADD CONSTRAINT configfiletypes_fk FOREIGN KEY (configtype)
REFERENCES poudrierec2.configfiletypes (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;
-- ddl-end --

-- object: unique_entry | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.packageoptions DROP CONSTRAINT IF EXISTS unique_entry CASCADE;
ALTER TABLE poudrierec2.packageoptions ADD CONSTRAINT unique_entry UNIQUE (configfile,category,package);
-- ddl-end --
COMMENT ON CONSTRAINT unique_entry ON poudrierec2.packageoptions  IS E'Only one (configfile, category, package) row may exist. Would be a PK but category/package may be null.';
-- ddl-end --


-- object: configfiles_index_undeleted_titles | type: INDEX --
-- DROP INDEX IF EXISTS poudrierec2.configfiles_index_undeleted_titles CASCADE;
CREATE UNIQUE INDEX configfiles_index_undeleted_titles ON poudrierec2.configfiles
	USING btree
	(
	  name
	)
	WHERE (not deleted);
-- ddl-end --

-- object: configfile_id | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.configoptions DROP CONSTRAINT IF EXISTS configfile_id CASCADE;
ALTER TABLE poudrierec2.configoptions ADD CONSTRAINT configfile_id FOREIGN KEY (configfile)
REFERENCES poudrierec2.configfiles (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: build_config_id | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.builds DROP CONSTRAINT IF EXISTS build_config_id CASCADE;
ALTER TABLE poudrierec2.builds ADD CONSTRAINT build_config_id FOREIGN KEY (config)
REFERENCES poudrierec2.jobconfigs (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: build_vm_uuid | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.builds DROP CONSTRAINT IF EXISTS build_vm_uuid CASCADE;
ALTER TABLE poudrierec2.builds ADD CONSTRAINT build_vm_uuid FOREIGN KEY (vm)
REFERENCES poudrierec2.virtualmachines (azuuid) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;
-- ddl-end --

-- object: portset_member_fk | type: CONSTRAINT --
-- ALTER TABLE poudrierec2.portset_members DROP CONSTRAINT IF EXISTS portset_member_fk CASCADE;
ALTER TABLE poudrierec2.portset_members ADD CONSTRAINT portset_member_fk FOREIGN KEY (portset)
REFERENCES poudrierec2.portsets (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE NO ACTION;
-- ddl-end --


