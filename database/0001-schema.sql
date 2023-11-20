SET check_function_bodies = false;

-- This bit obviously requires superuser privileges.
CREATE SCHEMA poudrierec2;
ALTER SCHEMA poudrierec2 OWNER TO poudriereadmin;

-- Everything is contained in the application schema.
SET search_path TO poudrierec2;

CREATE TABLE poudrierec2.packageoptions (
	category text,
	package text,
	set text[] NOT NULL,
	unset text[] NOT NULL,
	configfile uuid NOT NULL,
	CONSTRAINT pkgoptions_valid_spec CHECK ((((category IS NULL) AND (package IS NULL)) OR (category IS NOT NULL))),
	CONSTRAINT unique_entry UNIQUE (configfile,category,package)
);
COMMENT ON TABLE poudrierec2.packageoptions IS E'Package options set';
COMMENT ON COLUMN poudrierec2.packageoptions.category IS E'The category to which these options apply';
COMMENT ON COLUMN poudrierec2.packageoptions.package IS E'The package name to which these options apply';
COMMENT ON COLUMN poudrierec2.packageoptions.set IS E'Options that should be set.';
COMMENT ON COLUMN poudrierec2.packageoptions.unset IS E'Options that should be unset.';
COMMENT ON CONSTRAINT pkgoptions_valid_spec ON poudrierec2.packageoptions IS E'package may only be non-null if category is non-null';
COMMENT ON CONSTRAINT unique_entry ON poudrierec2.packageoptions IS E'Only one (configfile, category, package) row may exist. Would be a PK but category/package may be null.';
ALTER TABLE poudrierec2.packageoptions OWNER TO poudriereadmin;

CREATE TABLE poudrierec2.availableoptions (
	name text NOT NULL,
	description text,
	required boolean NOT NULL DEFAULT false,
	defaultvalue text NOT NULL,
	configtype text NOT NULL,
	CONSTRAINT name_valid CHECK ((name ~ '^[A-Z0-9_]+$'::text)),
	CONSTRAINT availableoptions_pk PRIMARY KEY (name)
);
COMMENT ON TABLE poudrierec2.availableoptions IS E'Options that can be set in poudriere.conf.';
COMMENT ON COLUMN poudrierec2.availableoptions.name IS E'Name of the option that can be set.';
COMMENT ON COLUMN poudrierec2.availableoptions.description IS E'A freeform description of the configuration option.';
COMMENT ON COLUMN poudrierec2.availableoptions.required IS E'Whether the option is required for poudriere to function.';
COMMENT ON COLUMN poudrierec2.availableoptions.defaultvalue IS E'The option''s default value.';
ALTER TABLE poudrierec2.availableoptions OWNER TO poudriereadmin;

CREATE TABLE poudrierec2.virtualmachines (
	azuuid uuid NOT NULL,
	created timestamp with time zone NOT NULL,
	started timestamp with time zone,
	stopped timestamp with time zone,
	deleted timestamp with time zone,
	CONSTRAINT virtualmachines_pk PRIMARY KEY (azuuid)
);
COMMENT ON TABLE poudrierec2.virtualmachines IS E'Virtual machines used by this system.';
COMMENT ON COLUMN poudrierec2.virtualmachines.azuuid IS E'Azure-generated UUID for this VM.';
COMMENT ON COLUMN poudrierec2.virtualmachines.created IS E'Time VM created';
COMMENT ON COLUMN poudrierec2.virtualmachines.started IS E'Time VM started.';
COMMENT ON COLUMN poudrierec2.virtualmachines.stopped IS E'Time VM stopped (deallocated)';
COMMENT ON COLUMN poudrierec2.virtualmachines.deleted IS E'Time VM deleted.';
ALTER TABLE poudrierec2.virtualmachines OWNER TO poudriereadmin;

CREATE INDEX index_vm_azuuid ON poudrierec2.virtualmachines
USING btree(azuuid);

CREATE TABLE poudrierec2.heartbeats (
	timereported timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'::text),
	virtualmachine uuid NOT NULL,
	loadaverage double precision[] NOT NULL,
	CONSTRAINT hb_loadavg_length CHECK ((array_length(loadaverage, 1) = 3)),
	CONSTRAINT heartbeats_pk PRIMARY KEY (timereported,virtualmachine)
);
COMMENT ON COLUMN poudrierec2.heartbeats.virtualmachine IS E'Azure-generated GUID of the virtual machine reporting this heartbeat.';
ALTER TABLE poudrierec2.heartbeats OWNER TO poudriereadmin;

CREATE TABLE poudrierec2.configfiles (
	id uuid NOT NULL,
	deleted boolean NOT NULL DEFAULT false,
	name text NOT NULL,
	portset uuid,
	portstree uuid,
	jail uuid,
	configtype text NOT NULL,
	CONSTRAINT poudriereconf_no_portset CHECK (((configtype <> 'poudriereconf'::text) OR (portset IS NULL))),
	CONSTRAINT configfiles_pk PRIMARY KEY (id)
);
COMMENT ON TABLE poudrierec2.configfiles IS E'A configuration file that can be used in a job configuration.';
COMMENT ON COLUMN poudrierec2.configfiles.name IS E'Human-readable name of this file.';
COMMENT ON CONSTRAINT poudriereconf_no_portset ON poudrierec2.configfiles IS E'A poudriere.conf file does not have a portset.';
ALTER TABLE poudrierec2.configfiles OWNER TO poudriereadmin;

CREATE TABLE poudrierec2.configoptions (
	configfile uuid NOT NULL,
	name text NOT NULL,
	value text NOT NULL,
	CONSTRAINT configoptions_pk PRIMARY KEY (configfile,name)
);
COMMENT ON TABLE poudrierec2.configoptions IS E'The options set in a configuration';
COMMENT ON COLUMN poudrierec2.configoptions.configfile IS E'The configuration this references.';
COMMENT ON COLUMN poudrierec2.configoptions.name IS E'The name of the option to set';
COMMENT ON COLUMN poudrierec2.configoptions.value IS E'The value of the option to set';
ALTER TABLE poudrierec2.configoptions OWNER TO poudriereadmin;

ALTER TABLE poudrierec2.configoptions ADD CONSTRAINT configfile_id FOREIGN KEY (configfile)
REFERENCES poudrierec2.configfiles (id) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;

CREATE TABLE poudrierec2.portstrees (
	id uuid NOT NULL,
	name text NOT NULL,
	portable_name text NOT NULL,
	method text NOT NULL,
	url text,
	CONSTRAINT portstrees_pk PRIMARY KEY (id)
);
COMMENT ON COLUMN poudrierec2.portstrees.name IS E'The name of the ports tree';
ALTER TABLE poudrierec2.portstrees OWNER TO poudriereadmin;

CREATE UNIQUE INDEX portstrees_index_undeleted_names_portable ON poudrierec2.portstrees
USING btree(portable_name);

CREATE FUNCTION poudrierec2.configfile_is_makeconf ()
	RETURNS trigger
	LANGUAGE plpgsql
	PARALLEL SAFE
	CALLED ON NULL INPUT
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
ALTER FUNCTION poudrierec2.configfile_is_makeconf() OWNER TO poudriereadmin;
COMMENT ON FUNCTION poudrierec2.configfile_is_makeconf() IS E'Validate that the config file referenced by this row is a make.conf.';

CREATE TABLE poudrierec2.portsets (
	id uuid NOT NULL,
	name text NOT NULL,
	portable_name text NOT NULL,
	CONSTRAINT portsets_pk PRIMARY KEY (id)
);
COMMENT ON TABLE poudrierec2.portsets IS E'Sets of ports.';
ALTER TABLE poudrierec2.portsets OWNER TO poudriereadmin;

CREATE UNIQUE INDEX portsets_index_undeleted_names_portable ON poudrierec2.portsets
USING btree(portable_name);

CREATE TABLE poudrierec2.portset_members (
	portset uuid NOT NULL,
	portname text NOT NULL,
	CONSTRAINT portset_members_pk PRIMARY KEY (portset,portname)
);
COMMENT ON TABLE poudrierec2.portset_members IS E'Members of a port set';
COMMENT ON COLUMN poudrierec2.portset_members.portname IS E'Origin (with optional flavor) to build.';
ALTER TABLE poudrierec2.portset_members OWNER TO poudriereadmin;

CREATE TABLE poudrierec2.jails (
	id uuid NOT NULL,
	name text NOT NULL,
	portable_name text NOT NULL,
	version text,
	architecture text,
	method text,
	url text,
	path text,
	CONSTRAINT jails_pk PRIMARY KEY (id),
	CONSTRAINT jails_url_or_path CHECK ((url IS NULL) or (path IS NULL))
);
COMMENT ON TABLE poudrierec2.jails IS E'Jail definitions.';
COMMENT ON COLUMN poudrierec2.jails.version IS E'The OS version to install in the jail.';
COMMENT ON COLUMN poudrierec2.jails.architecture IS E'The architecture of the jail (e.g. amd64)';
ALTER TABLE poudrierec2.jails OWNER TO poudriereadmin;

CREATE UNIQUE INDEX jails_index_undeleted_names_portable ON poudrierec2.jails
USING btree(portable_name);

CREATE TABLE poudrierec2.configfiletypes (
	name text NOT NULL,
	CONSTRAINT configfiletypes_pk PRIMARY KEY (name)
);
COMMENT ON TABLE poudrierec2.configfiletypes IS E'Types of configuration files that can be used by Poudriere.';
ALTER TABLE poudrierec2.configfiletypes OWNER TO poudriereadmin;

INSERT INTO poudrierec2.configfiletypes (name) VALUES (E'poudriereconf');
INSERT INTO poudrierec2.configfiletypes (name) VALUES (E'makeconf');
INSERT INTO poudrierec2.configfiletypes (name) VALUES (E'srcconf');

CREATE UNIQUE INDEX configfiles_index_undeleted_names ON poudrierec2.configfiles
USING btree(name)
WHERE (NOT deleted);

CREATE TABLE poudrierec2.jobconfigs (
	id uuid NOT NULL DEFAULT gen_random_uuid(),
	name text NOT NULL,
	poudriereconf uuid NOT NULL REFERENCES poudrierec2.configfiles (id),
	portstree uuid NOT NULL REFERENCES poudrierec2.portstrees (id),
	portset uuid NOT NULL REFERENCES poudrierec2.portsets (id),
	jail uuid NOT NULL REFERENCES poudrierec2.jails (id),
	deleted boolean NOT NULL DEFAULT false,
	CONSTRAINT configs_pk PRIMARY KEY (id)
);
COMMENT ON TABLE poudrierec2.jobconfigs IS E'Run configurations';
ALTER TABLE poudrierec2.jobconfigs OWNER TO poudriereadmin;

CREATE UNIQUE INDEX jobconfigs_unique_undeleted_names ON poudrierec2.jobconfigs
USING btree(name)
WHERE (NOT deleted);

CREATE FUNCTION poudrierec2.jobconfig_poudriereconf_is_one ()
	RETURNS trigger
	LANGUAGE plpgsql
	PARALLEL SAFE
	AS $$
DECLARE
  configType poudrierec2.configfiles.configtype%TYPE;
BEGIN
SELECT cf.configtype
INTO configType
FROM poudrierec2.configfiles cf
WHERE cf.id = NEW.poudriereconf;
IF configType <> 'poudriereconf' THEN
  RAISE EXCEPTION 'poudriere.conf must be a poudriere.conf';
END IF;
RETURN NEW;
END
$$;

COMMENT ON FUNCTION poudrierec2.jobconfig_poudriereconf_is_one() IS 'Validate that this job config''s poudriere.conf is a poudriere.conf.';
ALTER FUNCTION poudrierec2.jobconfig_poudriereconf_is_one() OWNER TO poudriereadmin;
CREATE TRIGGER jobconfig_pc_validate BEFORE INSERT OR UPDATE ON poudrierec2.jobconfigs
	FOR EACH ROW EXECUTE FUNCTION poudrierec2.jobconfig_poudriereconf_is_one();

CREATE TABLE poudrierec2.schedules (
	jobconfig uuid NOT NULL,
	runat text NOT NULL,
	CONSTRAINT schedules_pk PRIMARY KEY (jobconfig, runat) -- rows should be unique
);
COMMENT ON COLUMN poudrierec2.schedules.runat IS E'Crontab string to evaluate for scheduling jobs.';
ALTER TABLE poudrierec2.schedules ADD CONSTRAINT schedules_jobconfig_fk FOREIGN KEY (jobconfig)
REFERENCES poudrierec2.jobconfigs (id) MATCH FULL;
ALTER TABLE poudrierec2.schedules OWNER TO poudriereadmin;

CREATE TABLE poudrierec2.jobruns (
	jobconfig uuid NOT NULL,
	requested timestamp with time zone NOT NULL,
	virtualmachine uuid,
	started timestamp with time zone,
	completed timestamp with time zone,
	CONSTRAINT jobruns_pk PRIMARY KEY (jobconfig, requested),
	CONSTRAINT jobruns_completed_implies_started CHECK ((completed IS NULL) OR (started IS NOT NULL)),
	CONSTRAINT jobruns_started_implies_vm_assigned CHECK ((started IS NULL) OR (virtualmachine IS NOT NULL))
);
COMMENT ON TABLE poudrierec2.jobruns IS E'Historical, current, and scheduled jobs.';
ALTER TABLE poudrierec2.jobruns OWNER TO poudriereadmin;
ALTER TABLE poudrierec2.jobruns ADD CONSTRAINT virtualmachines_fk FOREIGN KEY (virtualmachine)
REFERENCES poudrierec2.virtualmachines (azuuid);
ALTER TABLE poudrierec2.jobruns ADD CONSTRAINT jobruns_jobconfig_fk FOREIGN KEY (jobconfig)
REFERENCES poudrierec2.jobconfigs (id);
CREATE INDEX jobruns_completed ON poudrierec2.jobruns (jobconfig, completed)
WHERE (completed IS NOT NULL);

CREATE VIEW poudrierec2.jobruns_current AS
SELECT *
FROM poudrierec2.jobruns
WHERE virtualmachine IS NOT NULL
AND started IS NOT NULL
AND completed IS NULL;
COMMENT ON VIEW poudrierec2.jobruns_current IS E'Jobs that are currently executing.';

CREATE VIEW poudrierec2.jobruns_mostrecentcompleted AS
SELECT jobconfig, max(completed) AS completed
FROM poudrierec2.jobruns
WHERE completed IS NOT NULL
GROUP BY jobconfig;
COMMENT ON VIEW poudrierec2.jobruns_current IS E'Most recent completed job for each configuration.';

CREATE VIEW poudrierec2.jobs_lastrun_scheduled AS
-- Jobs with whether they are currently executing, last successful run, and schedule.
SELECT jc.id, jc.name, mrc.completed last_completed, c.requested current_requested, s.runat
FROM poudrierec2.jobconfigs jc
LEFT JOIN poudrierec2.jobruns_mostrecentcompleted mrc ON jc.id = mrc.jobconfig
LEFT JOIN poudrierec2.jobruns_current c ON jc.id = c.jobconfig
LEFT JOIN poudrierec2.schedules s ON s.jobconfig = jc.id
WHERE jc.deleted = false
ORDER BY jc.id;

CREATE TABLE poudrierec2.portstree_methods (
	name text NOT NULL,
	isdefault boolean NOT NULL DEFAULT false,
	CONSTRAINT portstree_methods_pk PRIMARY KEY (name),
	CONSTRAINT pt_methods_onlyonedefault EXCLUDE 
	USING btree(isdefault WITH pg_catalog.=) WHERE (isdefault)
);
COMMENT ON TABLE poudrierec2.portstree_methods IS E'Methods available for acquiring a ports tree.';
ALTER TABLE poudrierec2.portstree_methods OWNER TO poudriereadmin;
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'null', DEFAULT);
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'git', true);
INSERT INTO poudrierec2.portstree_methods (name, isdefault) VALUES (E'svn', DEFAULT);

ALTER TABLE poudrierec2.packageoptions ADD CONSTRAINT configfiles_fk FOREIGN KEY (configfile)
REFERENCES poudrierec2.configfiles (id) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE poudrierec2.availableoptions ADD CONSTRAINT configfiletypes_fk FOREIGN KEY (configtype)
REFERENCES poudrierec2.configfiletypes (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE poudrierec2.heartbeats ADD CONSTRAINT heartbeat_virtualmachine_fk FOREIGN KEY (virtualmachine)
REFERENCES poudrierec2.virtualmachines (azuuid) MATCH FULL
ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT portsets_fk FOREIGN KEY (portset)
REFERENCES poudrierec2.portsets (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT portstrees_fk FOREIGN KEY (portstree)
REFERENCES poudrierec2.portstrees (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT jails_fk FOREIGN KEY (jail)
REFERENCES poudrierec2.jails (id) MATCH FULL
ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE poudrierec2.configfiles ADD CONSTRAINT configfiletypes_fk FOREIGN KEY (configtype)
REFERENCES poudrierec2.configfiletypes (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE poudrierec2.portstrees ADD CONSTRAINT portstree_methods_fk FOREIGN KEY (method)
REFERENCES poudrierec2.portstree_methods (name) MATCH FULL
ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE poudrierec2.portset_members ADD CONSTRAINT portset_member_fk FOREIGN KEY (portset)
REFERENCES poudrierec2.portsets (id) MATCH FULL
ON DELETE CASCADE ON UPDATE NO ACTION;
